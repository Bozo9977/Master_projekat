using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.GDA
{
	[DataContract]
	public class Association
	{
		[DataMember]
		bool inverse;
		[DataMember]
		ModelCode propertyId;
		[DataMember]
		ModelCode type;

		public Association(ModelCode property, ModelCode type, bool inverse)
		{
			this.inverse = inverse;
			this.propertyId = property;
			this.type = type;
		}

		public bool Inverse
		{
			get { return inverse; }
			set { inverse = value; }
		}

		public ModelCode PropertyId
		{
			get { return propertyId; }
			set { propertyId = value; }
		}

		public ModelCode Type
		{
			get { return type; }
			set { type = value; }
		}
	}
}
