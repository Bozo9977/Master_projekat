using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Common.GDA
{
	[DataContract]
	public class ResourceDescription
	{
		[DataMember]
		private long id;
		[DataMember]
		private Dictionary<ModelCode, Property> properties;

		public ResourceDescription(long id)
		{
			this.id = id;
			properties = new Dictionary<ModelCode, Property>();
		}

		public ResourceDescription(ResourceDescription toCopy)
		{
			id = toCopy.id;
			properties = new Dictionary<ModelCode, Property>();

			foreach(KeyValuePair<ModelCode, Property> kvp in toCopy.properties)
			{
				AddProperty(kvp.Value.Clone());
			}
		}

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		public IReadOnlyDictionary<ModelCode, Property> Properties
		{
			get { return properties; }
		}

		public bool AddProperty(Property property)
		{
			if(properties.ContainsKey(property.Id))
				return false;

			properties.Add(property.Id, property.Clone());
			return true;
		}

		public void SetProperty(Property property)
		{
			properties[property.Id] = property.Clone();
		}

		public bool UpdateProperty(Property property)
		{
			if(!properties.ContainsKey(property.Id))
				return false;

			properties[property.Id] = property.Clone();
			return true;
		}

		public bool ContainsProperty(ModelCode propertyID)
		{
			return properties.ContainsKey(propertyID);
		}

		public Property GetProperty(ModelCode propertyID)
		{
			Property p;
			properties.TryGetValue(propertyID, out p);
			return p;
		}

		public bool RemoveProperty(ModelCode propertyId)
		{
			return properties.Remove(propertyId);
		}
	}
}
