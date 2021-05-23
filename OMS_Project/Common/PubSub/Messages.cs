using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.PubSub
{
	[DataContract]
	public class NetworkModelChanged : PubSubMessage
	{
		public override ETopic Topic
		{
			get { return ETopic.NetworkModelChanged; }
		}
	}

	[DataContract]
	public class MeasurementValuesChanged : PubSubMessage
	{
		public override ETopic Topic
		{
			get { return ETopic.MeasurementValuesChanged; }
		}

		[DataMember]
		public List<Tuple<long, float>> AnalogInputs { get; set; }

		[DataMember]
		public List<Tuple<long, float>> AnalogOutputs { get; set; }

		[DataMember]
		public List<Tuple<long, int>> DiscreteInputs { get; set; }

		[DataMember]
		public List<Tuple<long, int>> DiscreteOutputs { get; set; }
	}

	[DataContract]
	public class TopologyChanged : PubSubMessage
	{
		public override ETopic Topic
		{
			get { return ETopic.TopologyChanged; }
		}
	}
}
