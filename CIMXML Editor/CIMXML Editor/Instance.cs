using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMXML_Editor
{
	public class Instance
	{
		public readonly Class Class;
		Dictionary<string, string> Fields;

		public Instance(Class c)
		{
			Class = c;
			Fields = new Dictionary<string, string>(Class.AllAttributes.Count);

			foreach(KeyValuePair<string, Attribute> kvp in Class.AllAttributes)
				Fields.Add(kvp.Key, "");
		}

		public Instance(Instance i)
		{
			Class = i.Class;
			Fields = new Dictionary<string, string>(i.Fields);
		}

		public string GetProperty(string name)
		{
			return Fields.ContainsKey(name) ? Fields[name] : null;
		}

		public Dictionary<string, string> GetAllProperties()
		{
			return new Dictionary<string, string>(Fields);
		}

		public bool SetProperty(string name, string value)
		{
			if(value == null || !Fields.ContainsKey(name))
				return false;

			Fields[name] = value;
			return true;
		}
	}
}
