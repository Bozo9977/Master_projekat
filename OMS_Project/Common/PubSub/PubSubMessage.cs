using System.Runtime.Serialization;

namespace Common.PubSub
{
	public enum ETopic { NetworkModelChanged, MeasurementValuesChanged, TopologyChanged };

	[DataContract]
	[KnownType(typeof(NetworkModelChanged))]
	[KnownType(typeof(MeasurementValuesChanged))]
	[KnownType(typeof(TopologyChanged))]
	public abstract class PubSubMessage
	{
		public abstract ETopic Topic { get; }
	}
}
