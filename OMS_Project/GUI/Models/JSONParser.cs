using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Models
{
    public class JSONParser
    {

        public JSONParser()
        {

        }

        ~JSONParser()
        {

        }

        public Dictionary<string, Entity> Import(string path)
        {
            string readText = File.ReadAllText(path);

            if (String.IsNullOrWhiteSpace(readText))
                return new Dictionary<string, Entity>();

            Dictionary<string, Entity> temp = JsonConvert.DeserializeObject<Dictionary<string, Entity>>(readText);

            if (temp == null)
                return new Dictionary<string, Entity>();

            return temp;
        }

        public void Export(Dictionary<string, Entity> toExport, string path)
        {
            if (toExport.Count == 0 || toExport == null)
                return;

            string json = JsonConvert.SerializeObject(toExport, Formatting.Indented);

            if (String.IsNullOrWhiteSpace(path))
                return;

            if (!File.Exists(path))
                File.WriteAllText(path, json);
        }

    }
}
