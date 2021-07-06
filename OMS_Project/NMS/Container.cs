using Common.GDA;
using Common.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS
{
	class Container : IEnumerable<KeyValuePair<long, IdentifiedObject>>
	{
		Dictionary<long, IdentifiedObject> entities;

		public Container()
		{
			entities = new Dictionary<long, IdentifiedObject>();
		}

		public Container(Container c)
		{
			entities = new Dictionary<long, IdentifiedObject>(c.entities);
		}

		public Container(IEnumerable<IdentifiedObject> l)
		{
			entities = new Dictionary<long, IdentifiedObject>(l.Count());

			foreach(IdentifiedObject io in l)
				Add(io);
		}

		public IEnumerator<KeyValuePair<long, IdentifiedObject>> GetEnumerator()
		{
			return entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return entities.GetEnumerator();
		}

		public bool Get(long gid, out IdentifiedObject io)
		{
			return entities.TryGetValue(gid, out io);
		}

		public bool Contains(long gid)
		{
			return entities.ContainsKey(gid);
		}

		public void Set(IdentifiedObject io)
		{
			entities[io.GID] = io;
		}

		public void Add(IdentifiedObject io)
		{
			entities.Add(io.GID, io);
		}

		public bool Remove(long gid)
		{
			return entities.Remove(gid);
		}

		public List<long> GetKeys()
		{
			return new List<long>(entities.Keys);
		}
	}
}
