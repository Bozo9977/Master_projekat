﻿using System;
using System.Collections.Generic;

namespace Common.GDA
{
	public enum DMSType : short
	{
		Analog = 0x0001,
		Discrete = 0x0002,
		ConnectivityNode = 0x0003,
		Terminal = 0x0004,
		BaseVoltage = 0x0005,
		PowerTransformer = 0x0006,
		TransformerWinding = 0x0007,
		RatioTapChanger = 0x0008,
		EnergySource = 0x0009,
		DistributionGenerator = 0x000A,
		EnergyConsumer = 0x000B,
		ACLineSegment = 0x000C,
		Breaker = 0x000D,
		Recloser = 0x000E,
		Disconnector = 0x000F,
		SwitchingSchedule = 0x0010,
		SwitchingStep = 0x0011,
	}

	[Flags]
	public enum ModelCode : long
	{
		IDENTIFIEDOBJECT = 0x1000000000000000,
		IDENTIFIEDOBJECT_GID = 0x1000000000000104,
		IDENTIFIEDOBJECT_MRID = 0x1000000000000207,
		IDENTIFIEDOBJECT_NAME = 0x1000000000000307,

		POWERSYSTEMRESOURCE = 0x1100000000000000,
		POWERSYSTEMRESOURCE_MEASUREMENTS = 0x1100000000000119,

		MEASUREMENT = 0x1200000000000000,
		MEASUREMENT_BASEADDRESS = 0x1200000000000103,
		MEASUREMENT_DIRECTION = 0x120000000000020A,
		MEASUREMENT_MEASUREMENTTYPE = 0x120000000000030A,
		MEASUREMENT_TERMINAL = 0x1200000000000409,
		MEASUREMENT_POWERSYSTEMRESOURCE = 0x1200000000000509,

		ANALOG = 0x1210000000010000,
		ANALOG_MAXVALUE = 0x1210000000010105,
		ANALOG_MINVALUE = 0x1210000000010205,
		ANALOG_NORMALVALUE = 0x1210000000010305,

		DISCRETE = 0x1220000000020000,
		DISCRETE_MAXVALUE = 0x1220000000020103,
		DISCRETE_MINVALUE = 0x1220000000020203,
		DISCRETE_NORMALVALUE = 0x1220000000020303,

		CONNECTIVITYNODE = 0x1300000000030000,
		CONNECTIVITYNODE_TERMINALS = 0x1300000000030119,

		TERMINAL = 0x1400000000040000,
		TERMINAL_CONNECTIVITYNODE = 0x1400000000040109,
		TERMINAL_MEASUREMENTS = 0x1400000000040219,
		TERMINAL_CONDUCTINGEQUIPMENT = 0x1400000000040309,

		BASEVOLTAGE = 0x1500000000050000,
		BASEVOLTAGE_NOMINALVOLTAGE = 0x1500000000050105,
		BASEVOLTAGE_CONDUCTINGEQUIPMENT = 0x1500000000050219,

		EQUIPMENT = 0x1110000000000000,

		CONDUCTINGEQUIPMENT = 0x1111000000000000,
		CONDUCTINGEQUIPMENT_BASEVOLTAGE = 0x1111000000000109,
		CONDUCTINGEQUIPMENT_TERMINALS = 0x1111000000000219,

		TAPCHANGER = 0x1120000000000000,

		RATIOTAPCHANGER = 0x1121000000080000,
		RATIOTAPCHANGER_NOMINALSTEP = 0x1121000000080103,
		RATIOTAPCHANGER_STEPCOUNT = 0x1121000000080203,
		RATIOTAPCHANGER_VOLTAGESTEP = 0x1121000000080305,
		RATIOTAPCHANGER_TRANSFORMERWINDING = 0x1121000000080409,

		POWERTRANSFORMER = 0x1112000000060000,
		POWERTRANSFORMER_TRANSFORMERWINDINGS = 0x1112000000060119,

		TRANSFORMERWINDING = 0x1111100000070000,
		TRANSFORMERWINDING_POWERTRANSFORMER = 0x1111100000070109,
		TRANSFORMERWINDING_RATIOTAPCHANGER = 0x1111100000070219,

		ENERGYSOURCE = 0x1111200000090000,

		DISTRIBUTIONGENERATOR = 0x11113000000A0000,
		DISTRIBUTIONGENERATOR_RATEDCOSPHI = 0x11113000000A0105,
		DISTRIBUTIONGENERATOR_RATEDPOWER = 0x11113000000A0205,
		DISTRIBUTIONGENERATOR_RATEDVOLTAGE = 0x11113000000A0305,

		ENERGYCONSUMER = 0x11114000000B0000,
		ENERGYCONSUMER_PFIXED = 0x11114000000B0105,
		ENERGYCONSUMER_QFIXED = 0x11114000000B0205,
		ENERGYCONSUMER_CONSUMERCLASS = 0x11114000000B030A,

		CONDUCTOR = 0x1111500000000000,
		CONDUCTOR_LENGTH = 0x1111500000000105,

		ACLINESEGMENT = 0x11115100000C0000,
		ACLINESEGMENT_RATEDCURRENT = 0x11115100000C0105,
		ACLINESEGMENT_PERLENGTHPHASERESISTANCE = 0x11115100000C0205,
		ACLINESEGMENT_PERLENGTHPHASEREACTANCE = 0x11115100000C0305,

		SWITCH = 0x1111600000000000,
		SWITCH_NORMALOPEN = 0x1111600000000101,
		SWITCH_SWITCHINGSTEPS = 0x1111600000000219,

		PROTECTEDSWITCH = 0x1111610000000000,

		BREAKER = 0x11116110000D0000,

		RECLOSER = 0x11116120000E0000,

		DISCONNECTOR = 0x11116200000F0000,

		DOCUMENT = 0x1600000000000000,

		SWITCHINGSCHEDULE = 0x1600000000100000,
		SWITCHINGSCHEDULE_SWITCHINGSTEPS = 0x1600000000100119,

		SWITCHINGSTEP = 0x1700000000110000,
		SWITCHINGSTEP_SWITCHINGSCHEDULE = 0x1700000000110109,
		SWITCHINGSTEP_SWITCH = 0x1700000000110209,
		SWITCHINGSTEP_OPEN = 0x1700000000110301,
		SWITCHINGSTEP_INDEX = 0x1700000000110403,
	}

	public enum MeasurementType { ActivePower, Other, ReactivePower, SwitchState, TapChangerPosition, VoltageI, VoltageR }
	public enum SignalDirection { Read, ReadWrite, Write }
	public enum ConsumerClass { Administrative, Industrial, Residential }

	public class ModelResourcesDesc
	{
		public static readonly DMSType[] TypeIdsInInsertOrder = { DMSType.ConnectivityNode, DMSType.BaseVoltage, DMSType.EnergyConsumer, DMSType.ACLineSegment, DMSType.Disconnector, DMSType.Breaker, DMSType.Recloser, DMSType.DistributionGenerator, DMSType.PowerTransformer, DMSType.TransformerWinding, DMSType.RatioTapChanger, DMSType.EnergySource, DMSType.Terminal, DMSType.Analog, DMSType.Discrete, DMSType.SwitchingSchedule, DMSType.SwitchingStep };

		public static Dictionary<DMSType, List<ModelCode>> GetTypeToPropertiesMap()
		{
			Dictionary<DMSType, List<ModelCode>> d = new Dictionary<DMSType, List<ModelCode>>(TypeIdsInInsertOrder.Length);
			Dictionary<DMSType, ModelCode> typeToMC = new Dictionary<DMSType, ModelCode>();
			List<ModelCode> abstractType = new List<ModelCode>();

			foreach(DMSType type in TypeIdsInInsertOrder)
				d.Add(type, new List<ModelCode>());

			foreach(ModelCode mc in Enum.GetValues(typeof(ModelCode)))
			{
				DMSType type = ModelCodeHelper.GetTypeFromModelCode(mc);

				if(ModelCodeHelper.IsProperty(mc))
				{
					(type == 0 ? abstractType : d[type]).Add(mc);
				}
				else if(type != 0)
				{
					typeToMC.Add(type, mc);
				}
			}

			foreach(ModelCode mc in abstractType)
			{
				foreach(KeyValuePair<DMSType, List<ModelCode>> type in d)
				{
					if(ModelCodeHelper.ModelCodeClassIsSubClassOf(typeToMC[type.Key], mc))
						type.Value.Add(mc);
				}
			}

			return d;
		}

		public static Dictionary<DMSType, ModelCode> GetTypeToModelCodeMap()
		{
			Dictionary<DMSType, ModelCode> d = new Dictionary<DMSType, ModelCode>(TypeIdsInInsertOrder.Length);

			foreach(ModelCode mc in Enum.GetValues(typeof(ModelCode)))
			{
				DMSType type = ModelCodeHelper.GetTypeFromModelCode(mc);

				if(ModelCodeHelper.IsClass(mc) && type != 0)
				{
					d.Add(type, mc);
				}
			}

			return d;
		}
	}
}
