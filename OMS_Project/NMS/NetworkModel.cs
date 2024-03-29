﻿using Common.GDA;
using Common.DataModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace NMS
{
	class NetworkModel
	{
		readonly Dictionary<DMSType, Container> containers;
		readonly ReaderWriterLockSlim rwLock;
		readonly INMSDatabase db;
		List<IdentifiedObject> inserted;
		List<IdentifiedObject> updatedOld;
		List<IdentifiedObject> updatedNew;
		List<IdentifiedObject> deleted;
		Dictionary<DMSType, int> counters;
		Dictionary<DMSType, int> oldCounters;

		public NetworkModel()
		{
			DMSType[] types = ModelResourcesDesc.TypeIdsInInsertOrder;
			containers = new Dictionary<DMSType, Container>(types.Length);
			counters = new Dictionary<DMSType, int>(types.Length);

			for(int i = 0; i < types.Length; ++i)
			{
				DMSType t = types[i];
				containers.Add(t, new Container());
				counters.Add(t, 0);
			}
			
			rwLock = new ReaderWriterLockSlim();
		}

		public NetworkModel(NetworkModel nm)
		{
			nm.rwLock.EnterReadLock();

			try
			{
				containers = new Dictionary<DMSType, Container>(nm.containers.Count);

				foreach(KeyValuePair<DMSType, Container> container in nm.containers)
					containers.Add(container.Key, new Container(container.Value));

				counters = new Dictionary<DMSType, int>(nm.counters);

				db = nm.db;
				rwLock = new ReaderWriterLockSlim();
			}
			finally
			{
				nm.rwLock.ExitReadLock();
			}
		}

		public NetworkModel(INMSDatabase db)
		{
			DMSType[] types = ModelResourcesDesc.TypeIdsInInsertOrder;
			containers = new Dictionary<DMSType, Container>(types.Length);
			counters = new Dictionary<DMSType, int>(types.Length);

			for(int i = 0; i < types.Length; ++i)
			{
				DMSType t = types[i];
				containers.Add(t, new Container(db.GetList(t)));
				counters.Add(t, 0);
			}

			Dictionary<ModelCode, long> refs = new Dictionary<ModelCode, long>();

			foreach(KeyValuePair<DMSType, Container> container in containers)
			{
				foreach(KeyValuePair<long, IdentifiedObject> io in container.Value)
				{
					refs.Clear();
					io.Value.GetSourceReferences(refs);

					foreach(KeyValuePair<ModelCode, long> r in refs)
					{
						IdentifiedObject target;

						if(!TryGetEntity(r.Value, out target))
							continue;

						target.AddTargetReference(r.Key, io.Value.GID);
					}
				}
			}

			foreach(KeyValuePair<DMSType, int> pair in db.GetCounters())
			{
				counters[pair.Key] = pair.Value;
			}

			this.db = db;
			rwLock = new ReaderWriterLockSlim();
		}

		public Tuple<Dictionary<long, long>, List<long>, List<long>> ApplyUpdate(Delta delta)
		{
			rwLock.EnterWriteLock();

			try
			{
				Dictionary<DMSType, int> counters = new Dictionary<DMSType, int>(this.counters);
				Dictionary<long, long> ids = delta.ResolveIds(x => counters.ContainsKey(x) ? (counters[x] = counters[x] + 1) : -1);

				if(ids == null)
					return null;

				delta.SortOperations();

				HashSet<long> toValidate = new HashSet<long>();
				List<IdentifiedObject> inserted = new List<IdentifiedObject>();
				List<IdentifiedObject> updatedOld = new List<IdentifiedObject>();
				List<IdentifiedObject> updatedNew = new List<IdentifiedObject>();
				List<IdentifiedObject> deleted = new List<IdentifiedObject>();
				Func<long, IdentifiedObject> entityGetter = x => { IdentifiedObject y; TryGetEntity(x, out y); return y; };

				foreach(ResourceDescription rd in delta.InsertOperations)
				{
					IdentifiedObject io = InsertEntity(rd);

					if(io == null)
						return null;

					toValidate.Add(io.GID);
					io.GetEntitiesToValidate(entityGetter, toValidate);
					inserted.Add(io);
				}

				foreach(ResourceDescription rd in delta.UpdateOperations)
				{
					Tuple<IdentifiedObject, IdentifiedObject> io = UpdateEntity(rd);

					if(io == null)
						return null;

					toValidate.Add(io.Item1.GID);
					io.Item1.GetEntitiesToValidate(entityGetter, toValidate);
					io.Item2.GetEntitiesToValidate(entityGetter, toValidate);
					updatedOld.Add(io.Item1);
					updatedNew.Add(io.Item2);
				}

				foreach(ResourceDescription rd in delta.DeleteOperations)
				{
					IdentifiedObject io = DeleteEntity(rd);

					if(io == null)
						return null;

					toValidate.Add(io.GID);
					io.GetEntitiesToValidate(entityGetter, toValidate);
					deleted.Add(io);
				}

				foreach(long gid in toValidate)
				{
					IdentifiedObject io;

					if(!TryGetEntity(gid, out io))
						continue;

					if(!io.Validate(entityGetter))
						return null;
				}

				if(db != null)
				{
					this.inserted = inserted;
					this.updatedOld = updatedOld;
					this.updatedNew = updatedNew;
					this.deleted = deleted;
					this.oldCounters = this.counters;
				}

				this.counters = counters;

				return new Tuple<Dictionary<long, long>, List<long>, List<long>>(ids, updatedOld.Select(x => x.GID).ToList(), deleted.Select(x => x.GID).ToList());
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public bool PersistUpdate()
		{
			rwLock.EnterReadLock();

			try
			{
				return db != null && inserted != null && db.PersistDelta(inserted, updatedNew, deleted, counters);
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public bool CommitUpdate()
		{
			inserted = null;
			updatedNew = null;
			updatedOld = null;
			deleted = null;
			oldCounters = null;

			return true;
		}

		public bool RollbackUpdate()
		{
			rwLock.EnterReadLock();

			try
			{
				bool ok = db != null && inserted != null && db.RollbackDelta(inserted, updatedOld, deleted, oldCounters);

				inserted = null;
				updatedNew = null;
				updatedOld = null;
				deleted = null;
				oldCounters = null;

				return ok;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		bool TryGetEntity(long gid, out IdentifiedObject io)
		{
			Container container;
			io = null;
			return containers.TryGetValue(ModelCodeHelper.GetTypeFromGID(gid), out container) && container.Get(gid, out io);
		}

		bool TryGetEntity(long gid, out IdentifiedObject io, out Container container)
		{
			io = null;
			return containers.TryGetValue(ModelCodeHelper.GetTypeFromGID(gid), out container) && container.Get(gid, out io);
		}

		IdentifiedObject InsertEntity(ResourceDescription rd)
		{
			if(rd == null)
				return null;

			DMSType type = ModelCodeHelper.GetTypeFromGID(rd.Id);
			Container container;

			if(!containers.TryGetValue(type, out container) || container.Contains(rd.Id))
				return null;

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
					Container targetContainer;

					if(!TryGetEntity(targetGID, out target, out targetContainer))
						return null;

					target = target.Clone();
					target.AddTargetReference(prop.Id, io.GID);
					targetContainer.Set(target);
				}
			}

			containers[type].Add(io);
			return io;
		}

		Tuple<IdentifiedObject, IdentifiedObject> UpdateEntity(ResourceDescription rd)
		{
			if(rd == null)
				return null;

			IdentifiedObject oldIO;
			Container container;

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
						Container oldTargetContainer;

						if(TryGetEntity(oldTargetGID, out oldTarget, out oldTargetContainer))
						{
							oldTarget = oldTarget.Clone();
							oldTarget.RemoveTargetReference(prop.Id, io.GID);
							oldTargetContainer.Set(oldTarget);
						}
					}

					long targetGID = ((ReferenceProperty)prop).Value;

					if(targetGID != 0)
					{
						IdentifiedObject target;
						Container targetContainer;

						if(TryGetEntity(targetGID, out target, out targetContainer))
						{
							target = target.Clone();
							target.AddTargetReference(prop.Id, io.GID);
							targetContainer.Set(target);
						}
					}
				}

				if(!io.SetProperty(prop))
					return null;
			}

			container.Set(io);
			return new Tuple<IdentifiedObject, IdentifiedObject>(oldIO, io);
		}

		IdentifiedObject DeleteEntity(ResourceDescription rd)
		{
			if(rd == null)
				return null;

			IdentifiedObject io;
			Container container;

			if(!TryGetEntity(rd.Id, out io, out container))
				return null;

			if(io.IsReferenced())
				return null;

			Dictionary<ModelCode, long> targetGIDs = new Dictionary<ModelCode, long>();
			io.GetSourceReferences(targetGIDs);

			foreach(KeyValuePair<ModelCode, long> pair in targetGIDs)
			{
				IdentifiedObject target;
				Container targetContainer;

				if(pair.Value != 0 && TryGetEntity(pair.Value, out target, out targetContainer))
				{
					target = target.Clone();
					target.RemoveTargetReference(pair.Key, io.GID);
					targetContainer.Set(target);
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

		public ResourceIterator GetMultipleValues(List<long> resourceIds, Dictionary<DMSType, List<ModelCode>> typeToProps)
		{
			return new ResourceIterator(resourceIds, typeToProps);
		}

		public ResourceIterator GetExtentValues(DMSType entityType, List<ModelCode> propIds)
		{
			rwLock.EnterReadLock();

			try
			{
				Container container;

				if(!containers.TryGetValue(entityType, out container))
					return null;

				return new ResourceIterator(container.GetKeys(), new Dictionary<DMSType, List<ModelCode>>(1) { { entityType, propIds } });
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
