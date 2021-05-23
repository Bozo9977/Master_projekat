using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.CalculationEngine
{
	[ServiceContract]
	public interface ICalculationEngineServiceContract
	{
		[OperationContract]
		bool ApplyUpdate(List<long> inserted, List<long> updated, List<long> deleted);

		[OperationContract]
		List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> GetLineEnergization();
	}
}
