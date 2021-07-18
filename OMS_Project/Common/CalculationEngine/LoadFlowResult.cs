using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.CalculationEngine
{
	public enum LoadFlowResultType { None, IR, II, UR, UI, SR, SI }

	[DataContract]
	public struct LoadFlowResultItem
	{
		[DataMember]
		public double Value { get; private set; }

		[DataMember]
		public LoadFlowResultType Type { get; private set; }

		public LoadFlowResultItem(double value, LoadFlowResultType type)
		{
			Value = value;
			Type = type;
		}
	}

	[DataContract]
	public class LoadFlowResult
	{
		[DataMember]
		List<LoadFlowResultItem> items;

		public LoadFlowResult()
		{
			items = new List<LoadFlowResultItem>();
		}

		public bool Add(LoadFlowResultItem item)
		{
			for(int i = 0; i < items.Count; ++i)
			{
				if(items[i].Type == item.Type)
					return false;
			}

			items.Add(item);
			return true;
		}

		public bool Remove(LoadFlowResultType type)
		{
			for(int i = 0; i < items.Count; ++i)
			{
				if(items[i].Type == type)
				{
					items.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		public double Get(LoadFlowResultType type)
		{
			for(int i = 0; i < items.Count; ++i)
			{
				LoadFlowResultItem item = items[i];

				if(item.Type == type)
					return item.Value;
			}

			return double.NaN;
		}
	}
}
