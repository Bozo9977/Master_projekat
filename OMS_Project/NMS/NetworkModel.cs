using Common.GDA;
using NMS.DataModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NMS
{
	class NetworkModel
	{
		Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers;
		ReaderWriterLockSlim rwLock;

		public NetworkModel()
		{
			Array types = Enum.GetValues(typeof(DMSType));
			containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(types.Length);

			foreach(DMSType t in types)
				containers.Add(t, new Dictionary<long, IdentifiedObject>(0));

			rwLock = new ReaderWriterLockSlim();
		}

		public NetworkModel(NetworkModel nm)
		{
			containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(nm.containers);

			foreach(DMSType k in nm.containers.Keys)
				containers[k] = new Dictionary<long, IdentifiedObject>(nm.containers[k]);

			rwLock = new ReaderWriterLockSlim();
		}

		public Dictionary<long, long> ApplyUpdate(Delta delta)
		{
			rwLock.EnterWriteLock();

			try
			{
				Dictionary<DMSType, int> counters = new Dictionary<DMSType, int>();

				foreach(KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> c in containers)
					counters.Add(c.Key, c.Value.Count);

				Dictionary<long, long> ids = delta.ResolveIds(x => counters.ContainsKey(x) ? counters[x]++ : -1);

				if(ids == null)
					return null;

				delta.SortOperations();

				HashSet<long> toValidate = new HashSet<long>();

				foreach(ResourceDescription rd in delta.InsertOperations)
				{
					IdentifiedObject io = InsertEntity(rd);

					if(io == null)
						return null;

					toValidate.Add(io.GID);
					io.GetEntitiesToValidate(toValidate);
				}

				foreach(ResourceDescription rd in delta.UpdateOperations)
				{
					Tuple<IdentifiedObject, IdentifiedObject> io = UpdateEntity(rd);

					if(io == null)
						return null;

					toValidate.Add(io.Item1.GID);
					io.Item1.GetEntitiesToValidate(toValidate);
					io.Item2.GetEntitiesToValidate(toValidate);
				}

				foreach(ResourceDescription rd in delta.DeleteOperations)
				{
					IdentifiedObject io = DeleteEntity(rd);

					if(io == null)
						return null;

					toValidate.Add(io.GID);
					io.GetEntitiesToValidate(toValidate);
				}

				foreach(long gid in toValidate)
				{
					IdentifiedObject io;

					if(!TryGetEntity(gid, out io))
						continue;

					if(!io.Validate())
						return null;
				}

				return ids;
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		bool TryGetEntity(long gid, out IdentifiedObject io)
		{
			Dictionary<long, IdentifiedObject> container;
			io = null;
			return containers.TryGetValue(ModelCodeHelper.GetTypeFromGID(gid), out container) && container.TryGetValue(gid, out io);
		}

		bool TryGetEntity(long gid, out IdentifiedObject io, out Dictionary<long, IdentifiedObject> container)
		{
			io = null;
			return containers.TryGetValue(ModelCodeHelper.GetTypeFromGID(gid), out container) && container.TryGetValue(gid, out io);
		}

		IdentifiedObject InsertEntity(ResourceDescription rd)
		{
			if(rd == null)
				return null;

			DMSType type = ModelCodeHelper.GetTypeFromGID(rd.Id);
			Dictionary<long, IdentifiedObject> container;

			if(!containers.TryGetValue(type, out container) || container.ContainsKey(rd.Id))
				return null;

			rd.SetProperty(new Int64Property(ModelCode.IDENTIFIEDOBJECT_GID, rd.Id));
			IdentifiedObject io = IdentifiedObject.Create(rd);

			if(io == null)
				return null;

			foreach(Property prop in rd.Properties.Values)
			{
				if(prop.Type == PropertyType.Reference)
				{
					long targetGID = ((ReferenceProperty)prop).Value;

					if(targetGID == 0)
						continue;

					IdentifiedObject target;
					Dictionary<long, IdentifiedObject> targetContainer;

					if(!TryGetEntity(targetGID, out target, out targetContainer))
						return null;

					target = target.Clone();
					target.AddTargetReference(prop.Id, io.GID);
					targetContainer[targetGID] = target;
				}
			}

			containers[type].Add(io.GID, io);
			return io;
		}

		Tuple<IdentifiedObject, IdentifiedObject> UpdateEntity(ResourceDescription rd)
		{
			if(rd == null)
				return null;

			IdentifiedObject oldIO;
			Dictionary<long, IdentifiedObject> container;

			if(!TryGetEntity(rd.Id, out oldIO, out container))
				return null;

			IdentifiedObject io = oldIO.Clone();

			foreach(Property prop in rd.Properties.Values)
			{
				if(prop.Type == PropertyType.Reference)
				{
					long oldTargetGID = ((ReferenceProperty)io.GetProperty(prop.Id)).Value;

					if(oldTargetGID != 0)
					{
						IdentifiedObject oldTarget;
						Dictionary<long, IdentifiedObject> oldTargetContainer;

						if(TryGetEntity(oldTargetGID, out oldTarget, out oldTargetContainer))
						{
							oldTarget = oldTarget.Clone();
							oldTarget.RemoveTargetReference(prop.Id, io.GID);
							oldTargetContainer[oldTargetGID] = oldTarget;
						}
					}

					long targetGID = ((ReferenceProperty)prop).Value;

					if(targetGID != 0)
					{
						IdentifiedObject target;
						Dictionary<long, IdentifiedObject> targetContainer;

						if(TryGetEntity(targetGID, out target, out targetContainer))
						{
							target = target.Clone();
							target.AddTargetReference(prop.Id, io.GID);
							targetContainer[targetGID] = target;
						}
					}
				}

				if(!io.SetProperty(prop))
					return null;
			}

			container[io.GID] = io;
			return new Tuple<IdentifiedObject, IdentifiedObject>(oldIO, io);
		}

		IdentifiedObject DeleteEntity(ResourceDescription rd)
		{
			if(rd == null)
				return null;

			IdentifiedObject io;
			Dictionary<long, IdentifiedObject> container;

			if(!TryGetEntity(rd.Id, out io, out container))
				return null;

			if(io.IsReferenced())
				return null;

			Dictionary<ModelCode, long> targetGIDs = new Dictionary<ModelCode, long>();
			io.GetSourceReferences(targetGIDs);

			foreach(KeyValuePair<ModelCode, long> pair in targetGIDs)
			{
				IdentifiedObject target;
				Dictionary<long, IdentifiedObject> targetContainer;

				if(pair.Value != 0 && TryGetEntity(pair.Value, out target, out targetContainer))
				{
					target = target.Clone();
					target.RemoveTargetReference(pair.Key, pair.Value);
					targetContainer[pair.Value] = target;
				}
			}

			container.Remove(io.GID);
			return io;
		}

		public ResourceDescription GetValues(long GID, List<ModelCode> propIds)
		{
			rwLock.EnterReadLock();

			try
			{
				IdentifiedObject io;

				if(!TryGetEntity(GID, out io))
					return null;

				ResourceDescription rd = new ResourceDescription(GID);

				foreach(ModelCode p in propIds)
				{
					Property property = io.GetProperty(p);

					if(property == null)
						return null;

					rd.AddProperty(property);
				}

				return rd;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public ResourceIterator GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			rwLock.EnterReadLock();

			try
			{
				Dictionary<long, IdentifiedObject> container;
				DMSType type = ModelCodeHelper.GetTypeFromModelCode(entityType);

				if(!containers.TryGetValue(type, out container))
					return null;

				return new ResourceIterator(new List<long>(container.Keys), new Dictionary<DMSType, List<ModelCode>>(1) { { type, propIds } });
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public ResourceIterator GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			rwLock.EnterReadLock();

			try
			{
				IdentifiedObject io;

				if(!TryGetEntity(source, out io))
					return null;

				Property property = io.GetProperty(association.PropertyId);

				if(property == null)
					return null;

				List<long> relatedGIDs = new List<long>();
				Dictionary<DMSType, List<ModelCode>> properties = new Dictionary<DMSType, List<ModelCode>>();
				DMSType associationType = ModelCodeHelper.GetTypeFromModelCode(association.Type);

				switch(property.Type)
				{
					case PropertyType.Reference:
					{
						long target = ((ReferenceProperty)property).Value;
						DMSType targetType = ModelCodeHelper.GetTypeFromGID(target);

						if(target == 0 || (association.Type != 0 && associationType != targetType))
							break;

						relatedGIDs.Add(target);
						properties[targetType] = propIds;
					}
					break;

					case PropertyType.ReferenceVector:
					{
						List<long> targets = ((ReferencesProperty)property).Value;

						foreach(long target in targets)
						{
							DMSType targetType = ModelCodeHelper.GetTypeFromGID(target);

							if(association.Type != 0 && associationType != targetType)
								continue;

							relatedGIDs.Add(target);
							properties[targetType] = propIds;
						}
					}
					break;

					default:
						return null;
				}

				return new ResourceIterator(relatedGIDs, properties);
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}
	}
}
