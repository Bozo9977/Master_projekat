using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.SCADA
{
	[ServiceContract]
	public interface ISCADAServiceContract
	{
		[OperationContract]
		bool ApplyUpdate(List<long> inserted, List<long> updated, List<long> deleted);

		[OperationContract]
		List<Tuple<long, float>> ReadAnalog(List<long> gids);

		[OperationContract]
		List<Tuple<long, int>> ReadDiscrete(List<long> gids);

		[OperationContract]
		void CommandAnalog(List<long> gids, List<float> values);

		[OperationContract]
		void CommandDiscrete(List<long> gids, List<int> values);
	}
}
