﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.GDA
{
	public enum PropertyType : short
	{
		Empty = 0,

		Bool		= 0x01,
		//Byte		= 0x02,
		Int32		= 0x03,
		//Int64		= 0x04,
		Float		= 0x05,
		//Double		= 0x06,
		String		= 0x07,
		//DateTime	= 0x08,
		Reference	= 0x09,
		Enum		= 0x0A,
		//Struct		= 0x0B,
		//TimeSpan	= 0x0C,

		//BoolVector			= 0x11,
		//ByteVector			= 0x12,
		//Int32Vector			= 0x13,
		//Int64Vector			= 0x14,
		//FloatVector			= 0x15,
		//DoubleVector		= 0x16,
		//StringVector		= 0x17,
		//DateTimeVector		= 0x18,
		ReferenceVector		= 0x19,
		//EnumVector			= 0x1A,
		//StructVector		= 0x1B,
		//TimeSpanVector		= 0x1C,
	}

	[DataContract]
	[KnownType(typeof(BoolProperty))]
	[KnownType(typeof(Int32Property))]
	[KnownType(typeof(FloatProperty))]
	[KnownType(typeof(StringProperty))]
	[KnownType(typeof(ReferenceProperty))]
	[KnownType(typeof(EnumProperty))]
	[KnownType(typeof(ReferencesProperty))]
	public abstract class Property
	{
		[DataMember]
		private ModelCode id;

		public Property(ModelCode id, PropertyType type)
		{
			this.id = id;

			if(Type != type)
				throw new Exception("Wrong ModelCode property type.");
		}

		public Property(Property toCopy)
		{
			id = toCopy.id;
		}

		public ModelCode Id
		{
			get { return id; }
		}

		public PropertyType Type
		{
			get
			{
				return (PropertyType)((long)id & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE);
			}
		}

		public abstract Property Clone();

		public virtual float AsFloat() { throw new Exception("Not a float."); }
		public virtual long AsLong() { throw new Exception("Not a long."); }
		public virtual short AsEnum() { throw new Exception("Not an enum."); }
		public virtual long AsReference() { throw new Exception("Not a reference."); }
		public virtual string AsString() { throw new Exception("Not a string."); }
		public virtual long[] AsLongs() { throw new Exception("Not a long vector."); }
		public virtual long[] AsReferences() { throw new Exception("Not a reference vector."); }
	}

	[DataContract]
	public class BoolProperty : Property
	{
		[DataMember]
		bool val;

		public BoolProperty(ModelCode id, bool value) : base(id, PropertyType.Bool)
		{
			Value = value;
		}

		public BoolProperty(BoolProperty p) : base(p)
		{
			Value = p.val;
		}

		public bool Value
		{
			get { return val; }
			set { val = value; }
		}

		public override Property Clone()
		{
			return new BoolProperty(this);
		}

		public override long AsLong()
		{
			return Value ? 1 : 0;
		}
	}

	[DataContract]
	public class Int32Property : Property
	{
		[DataMember]
		int val;

		public Int32Property(ModelCode id, int value) : base(id, PropertyType.Int32)
		{
			Value = value;
		}

		public Int32Property(Int32Property p) : base(p)
		{
			Value = p.val;
		}

		public int Value
		{
			get { return val; }
			set { val = value; }
		}

		public override Property Clone()
		{
			return new Int32Property(this);
		}

		public override long AsLong()
		{
			return Value;
		}
	}

	[DataContract]
	public class FloatProperty : Property
	{
		[DataMember]
		float val;

		public FloatProperty(ModelCode id, float value) : base(id, PropertyType.Float)
		{
			Value = value;
		}

		public FloatProperty(FloatProperty p) : base(p)
		{
			Value = p.val;
		}

		public float Value
		{
			get { return val; }
			set { val = value; }
		}

		public override Property Clone()
		{
			return new FloatProperty(this);
		}

		public override float AsFloat()
		{
			return Value;
		}
	}

	[DataContract]
	public class StringProperty : Property
	{
		[DataMember]
		string val;

		public StringProperty(ModelCode id, string value) : base(id, PropertyType.String)
		{
			Value = value;
		}

		public StringProperty(StringProperty p) : base(p)
		{
			Value = p.val;
		}

		public string Value
		{
			get { return val; }
			set { val = value; }
		}

		public override Property Clone()
		{
			return new StringProperty(this);
		}

		public override string AsString()
		{
			return Value;
		}
	}

	[DataContract]
	public class ReferenceProperty : Property
	{
		[DataMember]
		long val;

		public ReferenceProperty(ModelCode id, long value) : base(id, PropertyType.Reference)
		{
			Value = value;
		}

		public ReferenceProperty(ReferenceProperty p) : base(p)
		{
			Value = p.val;
		}

		public long Value
		{
			get { return val; }
			set { val = value; }
		}

		public override Property Clone()
		{
			return new ReferenceProperty(this);
		}

		public override long AsReference()
		{
			return Value;
		}
	}

	[DataContract]
	public class EnumProperty : Property
	{
		[DataMember]
		short val;

		public EnumProperty(ModelCode id, short value) : base(id, PropertyType.Enum)
		{
			Value = value;
		}

		public EnumProperty(EnumProperty p) : base(p)
		{
			Value = p.val;
		}

		public short Value
		{
			get { return val; }
			set { val = value; }
		}

		public override Property Clone()
		{
			return new EnumProperty(this);
		}

		public override short AsEnum()
		{
			return Value;
		}
	}

	[DataContract]
	public class ReferencesProperty : Property
	{
		[DataMember]
		long[] val;

		public ReferencesProperty(ModelCode id, long[] value) : base(id, PropertyType.ReferenceVector)
		{
			Value = value;
		}

		public ReferencesProperty(ReferencesProperty p) : base(p)
		{
			Value = p.val;
		}

		public long[] Value
		{
			get { return (long[])val.Clone(); }
			set { val = (long[])value.Clone(); }
		}

		public override Property Clone()
		{
			return new ReferencesProperty(this);
		}

		public override long[] AsLongs()
		{
			return Value;
		}

		public override long[] AsReferences()
		{
			return Value;
		}
	}
}
