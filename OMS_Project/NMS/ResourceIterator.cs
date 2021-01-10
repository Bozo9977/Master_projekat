using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS
{
	class ResourceIterator
	{
		List<long> gids;
		Dictionary<DMSType, List<ModelCode>> properties;
		int position;

		public ResourceIterator(List<long> GIDs, Dictionary<DMSType, List<ModelCode>> properties)
		{
			gids = GIDs;
			this.properties = properties;
		}

		public int ResourcesLeft()
		{
			return gids.Count - position;
		}

		public int ResourcesTotal()
		{
			return gids.Count;
		}

		public List<ResourceDescription> Next(int n, NetworkModel model)
		{
			if(n < 0)
				return null;

			int left = ResourcesLeft();
			int count = n < left ? n : left;
			List<long> resultGIDs = gids.GetRange(position, count);
			position += count;
			List<ResourceDescription> result = new List<ResourceDescription>(count);

			foreach(long gid in resultGIDs)
				result.Add(model.GetValues(gid, properties[ModelCodeHelper.GetTypeFromGID(gid)]));

			return result;
		}

		public void Rewind()
		{
			position = 0;
		}
	}
}
