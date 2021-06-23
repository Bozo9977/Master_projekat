using System.Runtime.Serialization;

namespace Common.PubSub
{
	public enum ETopic { NetworkModelChanged, MeasurementValuesChanged, TopologyChanged, LoadFlowChanged };

	[DataContract]
	[KnownType(typeof(NetworkModelChanged))]
	[KnownType(typeof(MeasurementValuesChanged))]
	[KnownType(typeof(TopologyChanged))]
	[KnownType(typeof(LoadFlowChanged))]
	public abstract class PubSubMessage
	{
		public abstract ETopic Topic { get; }
	}
}
