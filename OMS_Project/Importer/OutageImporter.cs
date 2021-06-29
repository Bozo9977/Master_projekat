using CIM.Model;
using Common.GDA;
using FTN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importer
{
	class OutageImporter
	{
		Dictionary<DMSType, int> counters;
		Dictionary<string, long> idMapping;

		public OutageImporter()
		{
			Array types = Enum.GetValues(typeof(DMSType));
			counters = new Dictionary<DMSType, int>(types.Length);

			foreach(DMSType t in types)
				counters.Add(t, 0);

			idMapping = new Dictionary<string, long>();
		}

		long GetGID(string id)
		{
			long gid;
			idMapping.TryGetValue(id, out gid);
			return gid;
		}

		public Delta CreateNMSDelta(ConcreteModel concreteModel)
		{
			if(concreteModel == null || concreteModel.ModelMap == null)
				return null;

			Delta delta = new Delta();

			foreach(DMSType type in ModelResourcesDesc.TypeIdsInInsertOrder)
			{
				SortedDictionary<string, object> cimObjects = concreteModel.GetAllObjectsOfType("FTN." + ModelCodeHelper.DMSTypeToName(type));

				if(cimObjects == null)
					continue;

				foreach(KeyValuePair<string, object> cimObjectPair in cimObjects)
				{
					IDClass idc = cimObjectPair.Value as IDClass;
					long gid = ModelCodeHelper.CreateGID(0, type, --counters[type]);
					ResourceDescription rd = new ResourceDescription(gid);
					idMapping[idc.ID] = gid;
					PopulateProperties(idc, rd);
					delta.InsertOperations.Add(rd);
				}
			}

			return delta;
		}

		public void PopulateProperties(IDClass idc, ResourceDescription rd)
		{
			if(idc == null || rd == null)
				return;

			switch(ModelCodeHelper.GetTypeFromGID(rd.Id))
			{
				case DMSType.ACLineSegment:
					PopulateACLineSegmentProperties((ACLineSegment)idc, rd);
					break;

				case DMSType.Analog:
					PopulateAnalogProperties((Analog)idc, rd);
					break;

				case DMSType.BaseVoltage:
					PopulateBaseVoltageProperties((BaseVoltage)idc, rd);
					break;

				case DMSType.Breaker:
					PopulateBreakerProperties((Breaker)idc, rd);
					break;

				case DMSType.ConnectivityNode:
					PopulateConnectivityNodeProperties((ConnectivityNode)idc, rd);
					break;

				case DMSType.Disconnector:
					PopulateDisconnectorProperties((Disconnector)idc, rd);
					break;

				case DMSType.Discrete:
					PopulateDiscreteProperties((Discrete)idc, rd);
					break;

				case DMSType.DistributionGenerator:
					PopulateDistributionGeneratorProperties((DistributionGenerator)idc, rd);
					break;

				case DMSType.EnergyConsumer:
					PopulateEnergyConsumerProperties((EnergyConsumer)idc, rd);
					break;

				case DMSType.EnergySource:
					PopulateEnergySourceProperties((EnergySource)idc, rd);
					break;

				case DMSType.PowerTransformer:
					PopulatePowerTransformerProperties((PowerTransformer)idc, rd);
					break;

				case DMSType.RatioTapChanger:
					PopulateRatioTapChangerProperties((RatioTapChanger)idc, rd);
					break;

				case DMSType.Recloser:
					PopulateRecloserProperties((Recloser)idc, rd);
					break;

				case DMSType.Terminal:
					PopulateTerminalProperties((Terminal)idc, rd);
					break;

				case DMSType.TransformerWinding:
					PopulateTransformerWindingProperties((TransformerWinding)idc, rd);
					break;
			}
		}

		void PopulateIdentifiedObjectProperties(IdentifiedObject x, ResourceDescription rd)
		{
			if(x.MRIDHasValue)
				rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_MRID, x.MRID));

			if(x.NameHasValue)
				rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_NAME, x.Name));
		}

		void PopulateMeasurementProperties(Measurement x, ResourceDescription rd)
		{
			PopulateIdentifiedObjectProperties(x, rd);

			if(x.BaseAddressHasValue)
				rd.AddProperty(new Int32Property(ModelCode.MEASUREMENT_BASEADDRESS, x.BaseAddress));

			if(x.DirectionHasValue)
				rd.AddProperty(new EnumProperty(ModelCode.MEASUREMENT_DIRECTION, (short)x.Direction));

			if(x.MeasurementTypeHasValue)
				rd.AddProperty(new EnumProperty(ModelCode.MEASUREMENT_MEASUREMENTTYPE, (short)x.MeasurementType));

			if(x.PowerSystemResourceHasValue)
				rd.AddProperty(new ReferenceProperty(ModelCode.MEASUREMENT_POWERSYSTEMRESOURCE, GetGID(x.PowerSystemResource.ID)));

			if(x.TerminalHasValue)
				rd.AddProperty(new ReferenceProperty(ModelCode.MEASUREMENT_TERMINAL, GetGID(x.Terminal.ID)));
		}

		void PopulateAnalogProperties(Analog x, ResourceDescription rd)
		{
			PopulateMeasurementProperties(x, rd);

			if(x.MaxValueHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ANALOG_MAXVALUE, x.MaxValue));

			if(x.MinValueHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ANALOG_MINVALUE, x.MinValue));

			if(x.NormalValueHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ANALOG_NORMALVALUE, x.NormalValue));
		}

		void PopulateDiscreteProperties(Discrete x, ResourceDescription rd)
		{
			PopulateMeasurementProperties(x, rd);

			if(x.MaxValueHasValue)
				rd.AddProperty(new Int32Property(ModelCode.DISCRETE_MAXVALUE, x.MaxValue));

			if(x.MinValueHasValue)
				rd.AddProperty(new Int32Property(ModelCode.DISCRETE_MINVALUE, x.MinValue));

			if(x.NormalValueHasValue)
				rd.AddProperty(new Int32Property(ModelCode.DISCRETE_NORMALVALUE, x.NormalValue));
		}

		void PopulateBaseVoltageProperties(BaseVoltage x, ResourceDescription rd)
		{
			PopulateIdentifiedObjectProperties(x, rd);

			if(x.NominalVoltageHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE, x.NominalVoltage));
		}

		void PopulateConnectivityNodeProperties(ConnectivityNode x, ResourceDescription rd)
		{
			PopulateIdentifiedObjectProperties(x, rd);
		}

		void PopulateTerminalProperties(Terminal x, ResourceDescription rd)
		{
			PopulateIdentifiedObjectProperties(x, rd);

			if(x.ConductingEquipmentHasValue)
				rd.AddProperty(new ReferenceProperty(ModelCode.TERMINAL_CONDUCTINGEQUIPMENT, GetGID(x.ConductingEquipment.ID)));

			if(x.ConnectivityNodeHasValue)
				rd.AddProperty(new ReferenceProperty(ModelCode.TERMINAL_CONNECTIVITYNODE, GetGID(x.ConnectivityNode.ID)));
		}

		void PopulatePowerSystemResourceProperties(PowerSystemResource x, ResourceDescription rd)
		{
			PopulateIdentifiedObjectProperties(x, rd);
		}

		void PopulateEquipmentProperties(Equipment x, ResourceDescription rd)
		{
			PopulatePowerSystemResourceProperties(x, rd);
		}

		void PopulateConductingEquipmentProperties(ConductingEquipment x, ResourceDescription rd)
		{
			PopulateEquipmentProperties(x, rd);

			if(x.BaseVoltageHasValue)
				rd.AddProperty(new ReferenceProperty(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE, GetGID(x.BaseVoltage.ID)));
		}

		void PopulateTapChangerProperties(TapChanger x, ResourceDescription rd)
		{
			PopulatePowerSystemResourceProperties(x, rd);
		}

		void PopulateTapChangerProperties(ConductingEquipment x, ResourceDescription rd)
		{
			PopulatePowerSystemResourceProperties(x, rd);
		}

		void PopulateRatioTapChangerProperties(RatioTapChanger x, ResourceDescription rd)
		{
			PopulateTapChangerProperties(x, rd);

			if(x.NominalStepHasValue)
				rd.AddProperty(new Int32Property(ModelCode.RATIOTAPCHANGER_NOMINALSTEP, x.NominalStep));

			if(x.StepCountHasValue)
				rd.AddProperty(new Int32Property(ModelCode.RATIOTAPCHANGER_STEPCOUNT, x.StepCount));

			if(x.VoltageStepHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.RATIOTAPCHANGER_VOLTAGESTEP, x.VoltageStep));

			if(x.TransformerWindingHasValue)
				rd.AddProperty(new ReferenceProperty(ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING, GetGID(x.TransformerWinding.ID)));
		}

		void PopulatePowerTransformerProperties(PowerTransformer x, ResourceDescription rd)
		{
			PopulateEquipmentProperties(x, rd);
		}

		void PopulateTransformerWindingProperties(TransformerWinding x, ResourceDescription rd)
		{
			PopulateConductingEquipmentProperties(x, rd);

			if(x.PowerTransformerHasValue)
				rd.AddProperty(new ReferenceProperty(ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER, GetGID(x.PowerTransformer.ID)));
		}

		void PopulateEnergySourceProperties(EnergySource x, ResourceDescription rd)
		{
			PopulateConductingEquipmentProperties(x, rd);
		}

		void PopulateDistributionGeneratorProperties(DistributionGenerator x, ResourceDescription rd)
		{
			PopulateConductingEquipmentProperties(x, rd);

			if(x.RatedCosPhiHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.DISTRIBUTIONGENERATOR_RATEDCOSPHI, x.RatedCosPhi));

			if(x.RatedPowerHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.DISTRIBUTIONGENERATOR_RATEDPOWER, x.RatedPower));

			if(x.RatedVoltageHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.DISTRIBUTIONGENERATOR_RATEDVOLTAGE, x.RatedVoltage));
		}

		void PopulateEnergyConsumerProperties(EnergyConsumer x, ResourceDescription rd)
		{
			PopulateConductingEquipmentProperties(x, rd);

			if(x.PfixedHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ENERGYCONSUMER_PFIXED, x.Pfixed));

			if(x.QfixedHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ENERGYCONSUMER_QFIXED, x.Qfixed));

			if(x.ConsumerClassHasValue)
				rd.AddProperty(new EnumProperty(ModelCode.ENERGYCONSUMER_CONSUMERCLASS, (short)x.ConsumerClass));
		}

		void PopulateConductorProperties(Conductor x, ResourceDescription rd)
		{
			PopulateConductingEquipmentProperties(x, rd);

			if(x.LengthHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.CONDUCTOR_LENGTH, x.Length));
		}

		void PopulateACLineSegmentProperties(ACLineSegment x, ResourceDescription rd)
		{
			PopulateConductorProperties(x, rd);

			if(x.RatedCurrentHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ACLINESEGMENT_RATEDCURRENT, x.RatedCurrent));

			if(x.PerLengthPhaseResistanceHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ACLINESEGMENT_PERLENGTHPHASERESISTANCE, x.PerLengthPhaseResistance));

			if(x.PerLengthPhaseReactanceHasValue)
				rd.AddProperty(new FloatProperty(ModelCode.ACLINESEGMENT_PERLENGTHPHASEREACTANCE, x.PerLengthPhaseReactance));
		}

		void PopulateSwitchProperties(Switch x, ResourceDescription rd)
		{
			PopulateConductingEquipmentProperties(x, rd);

			if(x.NormalOpenHasValue)
				rd.AddProperty(new BoolProperty(ModelCode.SWITCH_NORMALOPEN, x.NormalOpen));
		}

		void PopulateProtectedSwitchProperties(ProtectedSwitch x, ResourceDescription rd)
		{
			PopulateSwitchProperties(x, rd);
		}

		void PopulateBreakerProperties(Breaker x, ResourceDescription rd)
		{
			PopulateProtectedSwitchProperties(x, rd);
		}

		void PopulateRecloserProperties(Recloser x, ResourceDescription rd)
		{
			PopulateProtectedSwitchProperties(x, rd);
		}

		void PopulateDisconnectorProperties(Disconnector x, ResourceDescription rd)
		{
			PopulateSwitchProperties(x, rd);
		}
	}
}
