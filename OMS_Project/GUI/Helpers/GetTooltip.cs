using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Helpers
{
    public static class GetTooltip
    {
        public static string Analog(string id, string name, string baseAddress, string value)
        {
            string StringToReturn = "";
            StringToReturn += "ANALOG\n";
            StringToReturn += "ID: " + id + "\n";
            StringToReturn += "Name: " + name + "\n";
            StringToReturn += "Base Address: " + baseAddress + "\n";
            StringToReturn += "Value: " + value;

            return StringToReturn;
        }

        public static string Discrete(string id, string name, string baseAddress, string value)
        {
            string StringToReturn = "";
            StringToReturn += "DISCRETE\n";
            StringToReturn += "ID: " + id + "\n";
            StringToReturn += "Name: " + name + "\n";
            StringToReturn += "Base Address: " + baseAddress + "\n";
            StringToReturn += "Value: " + value;

            return StringToReturn;
        }
    }
}
