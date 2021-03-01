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

        public void AddAnalogPoint(AnalogPoint p)
        {

            if (!File.Exists(analogPath))
                File.WriteAllText(analogPath, JsonConvert.SerializeObject(new Dictionary<int, AnalogPoint> { }, Formatting.Indented));


            Dictionary<int, AnalogPoint> dict = JsonConvert.DeserializeObject<Dictionary<int, AnalogPoint>>(File.ReadAllText(analogPath));

            if (!dict.ContainsKey(p.Address))
                dict.Add(p.Address, p);
            else
                dict[p.Address] = p;

            var toWrite =  JsonConvert.SerializeObject(dict, Formatting.Indented);

            if (File.Exists(analogPath))
                File.WriteAllText(analogPath, toWrite);
        }

        public void AddDiscretePoint(DiscretePoint p)
        {

            if (!File.Exists(discretePath))
                File.WriteAllText(discretePath, JsonConvert.SerializeObject(new Dictionary<int, DiscretePoint> { }, Formatting.Indented));

            Dictionary<int, DiscretePoint> dict = JsonConvert.DeserializeObject<Dictionary<int, DiscretePoint>>(File.ReadAllText(discretePath));

            if (!dict.ContainsKey(p.Address))
                dict.Add(p.Address, p);
            else
                dict[p.Address] = p;

            var toWrite = JsonConvert.SerializeObject(dict, Formatting.Indented);

            if (File.Exists(discretePath))
                File.WriteAllText(discretePath, toWrite);
        }

        public void Reset()
        {
            if (File.Exists(analogPath))
                File.Delete(analogPath);

            if (File.Exists(discretePath))
                File.Delete(discretePath);
        }


        public void Dispose()
        {

        }
    }
}
