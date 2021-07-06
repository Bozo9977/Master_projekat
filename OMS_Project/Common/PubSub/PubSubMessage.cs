using System.Runtime.Serialization;

namespace Common.PubSub
{
	public enum ETopic { NetworkModelChanged, MeasurementValuesChanged, TopologyChanged, LoadFlowChanged, MarkedSwitchesChanged };

	[DataContract]
	[KnownType(typeof(NetworkModelChanged))]
	[KnownType(typeof(MeasurementValuesChanged))]
	[KnownType(typeof(TopologyChanged))]
	[KnownType(typeof(LoadFlowChanged))]
	[KnownType(typeof(MarkedSwitchesChanged))]
	public abstract class PubSubMessage
	{
		public abstract ETopic Topic { get; }
	}
}
