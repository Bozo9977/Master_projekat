﻿using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.CalculationEngine
{
	public enum EEnergization { Unknown, NotEnergized, Energized }
	public enum ESwitchState { Unknown, Closed, Open }

	[ServiceContract]
	public interface ICalculationEngineServiceContract
	{
		[OperationContract]
		bool ApplyUpdate(List<long> inserted, List<long> updated, List<long> deleted);

		[OperationContract]
		List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> GetLineEnergization();

		[OperationContract]
		List<KeyValuePair<long, LoadFlowResult>> GetLoadFlowResults();

		[OperationContract(IsOneWay = true)]
		void UpdateMeasurements(List<long> gids);

		[OperationContract]
		bool MarkSwitchState(long gid, bool open);

		[OperationContract]
		bool UnmarkSwitchState(long gid);

		[OperationContract]
		List<KeyValuePair<long, bool>> GetMarkedSwitches();
	}
}
