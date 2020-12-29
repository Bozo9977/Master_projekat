using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.GDA
{
	public enum DeltaOpType : byte
	{
		Insert = 0,
		Update = 1,
		Delete = 2
	}

	[DataContract]
	public class Delta
	{
		[DataMember]
		private List<ResourceDescription> insertOps;
		[DataMember]
		private List<ResourceDescription> deleteOps;
		[DataMember]
		private List<ResourceDescription> updateOps;

		public Delta()
		{
			insertOps = new List<ResourceDescription>();
			deleteOps = new List<ResourceDescription>();
			updateOps = new List<ResourceDescription>();
		}

		public List<ResourceDescription> InsertOperations
		{
			get { return insertOps; }
			set { insertOps = value; }
		}

		public List<ResourceDescription> UpdateOperations
		{
			get { return updateOps; }
			set { updateOps = value; }
		}

		public List<ResourceDescription> DeleteOperations
		{
			get { return deleteOps; }
			set { deleteOps = value; }
		}

		private static ModelResourcesDesc resDesc;

		public static ModelResourcesDesc ResourceDescs
		{
			get
			{
				if(Delta.resDesc == null)
					Delta.resDesc = new ModelResourcesDesc();

				return Delta.resDesc;
			}
			set
			{
				Delta.resDesc = value;
			}
		}

		public Dictionary<long, long> ResolveIds(Dictionary<short, int> typesCounters)
		{
			Dictionary<long, long> d = new Dictionary<long, long>();

			// fix ids in insert operations - generate positive ids
			foreach(ResourceDescription rd in insertOps)
			{
				long oldGid = rd.Id;
				int oldId = ModelCodeHelper.GetEntityIdFromGID(oldGid);

				if(oldId >= 0)
					throw new Exception("Inserted GID " + oldGid + " has a positive entity ID.");

				if(d.ContainsKey(oldGid))
					throw new Exception("Inserted GID " + oldGid + " is a duplicate.");

				short type = ModelCodeHelper.GetTypeFromGID(rd.Id);

				if(!typesCounters.ContainsKey(type))
					throw new Exception("Inserted GID " + oldGid + " has an invalid DMSType.");

				int newId = typesCounters[type]++;

				long newGid = ModelCodeHelper.SetEntityIdInGID(oldGid, newId);
				newGid = ModelCodeHelper.SetSystemIdInGID(newGid, 0);

				d[oldGid] = newGid;
				rd.Id = newGid;
			}

			// change reference ids in insert operations
			foreach(ResourceDescription rd in insertOps)
			{
				foreach(Property p in rd.Properties.Values)
				{
					if(p.Type == PropertyType.Reference)
					{
						long oldGid = ((ReferenceProperty)p).Value;
						int oldId = ModelCodeHelper.GetEntityIdFromGID(oldGid);

						if(oldId < 0)
						{
							if(!d.ContainsKey(oldGid))
								throw new Exception("Referenced inserted GID " + oldGid + " not found.");

							((ReferenceProperty)p).Value = d[oldGid];
						}
					}
					else if(p.Type == PropertyType.ReferenceVector)
					{
						bool changed = false;

						long[] gids = ((ReferencesProperty)p).Value;
						for(int i = 0; i < gids.Length; ++i)
						{
							long oldGid = gids[i];
							int oldId = ModelCodeHelper.GetEntityIdFromGID(oldGid);

							if(oldId < 0)
							{
								if(!d.ContainsKey(oldGid))
									throw new Exception("Referenced inserted GID " + oldGid + " not found.");

								gids[i] = d[oldGid];
								changed = true;
							}
						}

						if(changed)
						{
							((ReferencesProperty)p).Value = gids;
						}
					}
				}
			}

			// change ids and reference ids in update operations
			foreach(ResourceDescription rd in updateOps)
			{
				long oldGid = rd.Id;
				int oldId = ModelCodeHelper.GetEntityIdFromGID(rd.Id);
				if(oldId < 0)
				{
					if(oldId < 0)
					{
						if(!d.ContainsKey(oldGid))
							throw new Exception("Referenced inserted GID " + oldGid + " not found.");

						rd.Id = d[oldGid];
					}
				}

				foreach(Property p in rd.Properties.Values)
				{
					if(p.Type == PropertyType.Reference)
					{
						long gidOldRef = ((ReferenceProperty)p).Value;
						int idOldRef = ModelCodeHelper.GetEntityIdFromGID(gidOldRef);

						if(idOldRef < 0)
						{
							if(!d.ContainsKey(gidOldRef))
								throw new Exception("Referenced inserted GID " + gidOldRef + " not found.");

							((ReferenceProperty)p).Value = d[gidOldRef];
						}
					}
					else if(p.Type == PropertyType.ReferenceVector)
					{
						bool changed = false;

						long[] gids = ((ReferencesProperty)p).Value;
						for(int i = 0; i < gids.Length; ++i)
						{
							long gidOldRef = gids[i];
							int idOldRef = ModelCodeHelper.GetEntityIdFromGID(gidOldRef);

							if(idOldRef < 0)
							{
								if(!d.ContainsKey(gidOldRef))
									throw new Exception("Referenced inserted GID " + gidOldRef + " not found.");

								gids[i] = d[gidOldRef];
								changed = true;
							}
						}

						if(changed)
						{
							((ReferencesProperty)p).Value = gids;
						}
					}
				}
			}

			// change ids in delete operations
			foreach(ResourceDescription rd in deleteOps)
			{
				long oldGid = rd.Id;
				int oldId = ModelCodeHelper.GetEntityIdFromGID(oldGid);
				if(oldId < 0)
				{
					if(!d.ContainsKey(oldGid))
						throw new Exception("Referenced inserted GID " + oldGid + " not found.");

					rd.Id = d[oldGid];
				}
			}

			return d;
		}

		public void SortOperations()
		{
			int insertSorted = 0;

			for(int i = 0; i < Delta.ResourceDescs.TypeIdsInInsertOrder.Count; ++i)
			{
				DMSType type = ModelCodeHelper.GetTypeFromModelCode(Delta.ResourceDescs.TypeIdsInInsertOrder[i]);

				for(int j = insertSorted; j < insertOps.Count; ++j)
				{
					ResourceDescription rd = insertOps[j];

					if((DMSType)ModelCodeHelper.GetTypeFromGID(rd.Id) == type)
					{
						ResourceDescription temp = insertOps[insertSorted];
						insertOps[insertSorted] = rd;
						insertOps[j] = temp;
						++insertSorted;
					}
				}
			}

			//deleted in reverse
			int deleteSorted = 0;

			for(int i = 0; i < Delta.ResourceDescs.TypeIdsInInsertOrder.Count; ++i)
			{
				DMSType type = ModelCodeHelper.GetTypeFromModelCode(Delta.ResourceDescs.TypeIdsInInsertOrder[i]);

				for(int j = deleteOps.Count - 1 - deleteSorted; j >= 0; --j)
				{
					ResourceDescription rd = deleteOps[j];

					if((DMSType)ModelCodeHelper.GetTypeFromGID(rd.Id) == type)
					{
						ResourceDescription temp = deleteOps[deleteOps.Count - 1 - deleteSorted];
						deleteOps[deleteOps.Count - 1 - deleteSorted] = rd;
						deleteOps[j] = temp;
						++deleteSorted;
					}
				}
			}

			//... handle RDs with invalid types (insertSorted < insertOps.Count || deleteSorted < deleteOps.Count)
		}
	}
}
