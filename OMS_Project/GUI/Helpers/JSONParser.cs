using Messages.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Helpers
{
    public class JSONParser : IDisposable
    {
        private string analogPath;
        private string discretePath;

        public JSONParser()
        {
            analogPath = "../../analogPoints.json";
            discretePath = "../../discretePoints.json";
        }

        ~JSONParser()
        {

        }

        /*public Dictionary<string, Entity> Import(string path)
        {
            string readText = File.ReadAllText(path);

            if (String.IsNullOrWhiteSpace(readText))
                return new Dictionary<string, Entity>();

            Dictionary<string, Entity> temp = JsonConvert.DeserializeObject<Dictionary<string, Entity>>(readText);

            if (temp == null)
                return new Dictionary<string, Entity>();

            return temp;
        }*/

        public void AddAnalogPoint(AnalogUpdated point)
        {
            if (!File.Exists(analogPath))
                File.WriteAllText(analogPath, JsonConvert.SerializeObject(new List<AnalogUpdated> { }, Formatting.Indented));
            

            List<AnalogUpdated> list = JsonConvert.DeserializeObject<List<AnalogUpdated>>(File.ReadAllText(analogPath));
            list.Add(point);

            var toWrite =  JsonConvert.SerializeObject(list);

            if (!File.Exists(analogPath))
                File.WriteAllText(analogPath, toWrite);
        }

        public void AddDiscretePoint(DiscreteUpdated point)
        {
            if (!File.Exists(discretePath))
                File.WriteAllText(discretePath, JsonConvert.SerializeObject(new List<DiscreteUpdated> { }, Formatting.Indented));


            List<DiscreteUpdated> list = JsonConvert.DeserializeObject<List<DiscreteUpdated>>(File.ReadAllText(discretePath));
            list.Add(point);

            var toWrite = JsonConvert.SerializeObject(list, Formatting.Indented);

            if (!File.Exists(discretePath))
                File.WriteAllText(discretePath, toWrite);
        }


        public void Dispose()
        {

        }
    }
}
