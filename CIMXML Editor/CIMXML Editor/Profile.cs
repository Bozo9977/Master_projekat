using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMXML_Editor
{
	public enum AttributeType { Bool, Enum, Int32, Float, String, Reference }

	public class Attribute
	{
		public readonly string Name;
		public readonly AttributeType Type;
		public readonly Class TargetType;
		public readonly string DefaultValue;

		public Attribute(string name, AttributeType type, Class targetType, string defaultValue = "")
		{
			Name = name;
			Type = type;
			TargetType = targetType;
			DefaultValue = defaultValue;
		}
	}

	public class Class
	{
		public readonly string Name;
		public readonly Dictionary<string, Attribute> Attributes;
		public readonly Class Base;

		public Class(string name, Dictionary<string, Attribute> attributes, Class baseClass)
		{
			Name = name;
			Attributes = attributes;
			Base = baseClass;
		}

		public Dictionary<string, Attribute> AllAttributes
		{
			get
			{
				if(Base == null)
					return new Dictionary<string, Attribute>(Attributes);

				Dictionary<string, Attribute> attr = Base.AllAttributes;
				
				foreach(KeyValuePair<string, Attribute> kvp in Attributes)
					attr.Add(kvp.Key, kvp.Value);

				return attr;
			}
		}

		public bool IsSubtypeOf(Class type)
		{
			Class t = this;

			do
			{
				if(t == type)
					return true;
			}
			while((t = t.Base) != null);

			return false;
		}
	}

	public class Profile
	{
		public readonly Dictionary<string, Class> Classes;
		public readonly List<Class> ConcreteClasses;

		public Profile()
		{
			Classes = new Dictionary<string, Class>();
			ConcreteClasses = new List<Class>(15);
			Dictionary<string, Attribute> attrs;
			Class c;
			Attribute a;

			attrs = new Dictionary<string, Attribute>();
			c = new Class("SignalDirection", attrs, null);
			a = new Attribute("Read", AttributeType.String, null);
			attrs.Add(a.Name, a);
			a = new Attribute("Write", AttributeType.String, null);
			attrs.Add(a.Name, a);
			a = new Attribute("ReadWrite", AttributeType.String, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("MeasurementType", attrs, null);
			a = new Attribute("Other", AttributeType.String, null);
			attrs.Add(a.Name, a);
			a = new Attribute("SwitchState", AttributeType.String, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("ConsumerClass", attrs, null);
			a = new Attribute("Residential", AttributeType.String, null);
			attrs.Add(a.Name, a);
			a = new Attribute("Industrial", AttributeType.String, null);
			attrs.Add(a.Name, a);
			a = new Attribute("Administrative", AttributeType.String, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("IdentifiedObject", attrs, null);
			a = new Attribute("mRID", AttributeType.String, null);
			attrs.Add(a.Name, a);
			a = new Attribute("name", AttributeType.String, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("ConnectivityNode", attrs, Classes["IdentifiedObject"]);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("BaseVoltage", attrs, Classes["IdentifiedObject"]);
			a = new Attribute("nominalVoltage", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("PowerSystemResource", attrs, Classes["IdentifiedObject"]);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Equipment", attrs, Classes["PowerSystemResource"]);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("ConductingEquipment", attrs, Classes["Equipment"]);
			a = new Attribute("BaseVoltage", AttributeType.Reference, Classes["BaseVoltage"]);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("EnergyConsumer", attrs, Classes["ConductingEquipment"]);
			a = new Attribute("pFixed", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			a = new Attribute("qFixed", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			a = new Attribute("consumerClass", AttributeType.Enum, Classes["ConsumerClass"]);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Conductor", attrs, Classes["ConductingEquipment"]);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("ACLineSegment", attrs, Classes["Conductor"]);
			a = new Attribute("ratedCurrent", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Switch", attrs, Classes["ConductingEquipment"]);
			a = new Attribute("normalOpen", AttributeType.Bool, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Disconnector", attrs, Classes["Switch"]);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("ProtectedSwitch", attrs, Classes["Switch"]);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Breaker", attrs, Classes["ProtectedSwitch"]);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Recloser", attrs, Classes["ProtectedSwitch"]);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("DistributionGenerator", attrs, Classes["ConductingEquipment"]);
			a = new Attribute("ratedCosPhi", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			a = new Attribute("ratedPower", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			a = new Attribute("ratedVoltage", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("PowerTransformer", attrs, Classes["Equipment"]);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("TransformerWinding", attrs, Classes["ConductingEquipment"]);
			a = new Attribute("PowerTransformer", AttributeType.Reference, Classes["PowerTransformer"]);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("TapChanger", attrs, Classes["PowerSystemResource"]);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("RatioTapChanger", attrs, Classes["TapChanger"]);
			a = new Attribute("nominalStep", AttributeType.Int32, null);
			attrs.Add(a.Name, a);
			a = new Attribute("stepCount", AttributeType.Int32, null);
			attrs.Add(a.Name, a);
			a = new Attribute("voltageStep", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			a = new Attribute("TransformerWinding", AttributeType.Reference, Classes["TransformerWinding"]);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("EnergySource", attrs, Classes["ConductingEquipment"]);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Terminal", attrs, Classes["IdentifiedObject"]);
			a = new Attribute("ConnectivityNode", AttributeType.Reference, Classes["ConnectivityNode"]);
			attrs.Add(a.Name, a);
			a = new Attribute("ConductingEquipment", AttributeType.Reference, Classes["ConductingEquipment"]);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Measurement", attrs, Classes["IdentifiedObject"]);
			a = new Attribute("baseAddress", AttributeType.Int32, null);
			attrs.Add(a.Name, a);
			a = new Attribute("direction", AttributeType.Enum, Classes["SignalDirection"]);
			attrs.Add(a.Name, a);
			a = new Attribute("measurementType", AttributeType.Enum, Classes["MeasurementType"]);
			attrs.Add(a.Name, a);
			a = new Attribute("PowerSystemResource", AttributeType.Reference, Classes["PowerSystemResource"]);
			attrs.Add(a.Name, a);
			a = new Attribute("Terminal", AttributeType.Reference, Classes["Terminal"]);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Analog", attrs, Classes["Measurement"]);
			a = new Attribute("maxValue", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			a = new Attribute("minValue", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			a = new Attribute("normalValue", AttributeType.Float, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);

			attrs = new Dictionary<string, Attribute>();
			c = new Class("Discrete", attrs, Classes["Measurement"]);
			a = new Attribute("maxValue", AttributeType.Int32, null);
			attrs.Add(a.Name, a);
			a = new Attribute("minValue", AttributeType.Int32, null);
			attrs.Add(a.Name, a);
			a = new Attribute("normalValue", AttributeType.Int32, null);
			attrs.Add(a.Name, a);
			Classes.Add(c.Name, c);
			ConcreteClasses.Add(c);
		}
	}
}
