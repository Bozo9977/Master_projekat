using System.Runtime.Serialization;

namespace Common.PubSub
{
	public enum ETopic { NetworkModelChanged, MeasurementValuesChanged };

	[DataContract]
	[KnownType(typeof(NetworkModelChanged))]
	[KnownType(typeof(MeasurementValuesChanged))]
	public abstract class PubSubMessage
	{
		public abstract ETopic Topic { get; }
	}
}
