using System.Runtime.Serialization;

namespace Common.CalculationEngine
{
	[DataContract]
	public class SerializedNode
	{
		[DataMember]
		public long GID { get; set; }

		[DataMember]
		public int ParentIndex { get; set; }

		public SerializedNode(long gid, int parentIndex)
		{
			GID = gid;
			ParentIndex = parentIndex;
		}
	}
}
