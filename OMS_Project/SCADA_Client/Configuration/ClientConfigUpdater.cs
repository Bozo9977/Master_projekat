using SCADA_Common;
using SCADA_Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Client.Configuration
{
    internal class ClientConfigUpdater
    {
        public string FilePath { get; set; }
        public ClientConfigUpdater(string filepath)
        {
            FilePath = filepath;
        }

        public void UpdateConfigFile(Dictionary<long, ISCADAModelPointItem> points)
        {
            string minValue = String.Empty;
            string maxValue = String.Empty;
            string nominalValue = String.Empty;
            string type = String.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("STA 1");
            sb.AppendLine("TCP 502");
            sb.AppendLine();
            
            foreach(var point in points.Values)
            {
                switch(point.RegisterType)
                {
                    case PointType.ANALOG_INPUT:
                        sb.Append("IN_REG ");
                        minValue = ((IAnalogSCADAModelPointItem)point).MinRawValue.ToString();
                        maxValue = ((IAnalogSCADAModelPointItem)point).MaxRawValue.ToString();
                        nominalValue = ((IAnalogSCADAModelPointItem)point).NormalValue.ToString();
                        type = "AI";
                        break;
                    case PointType.ANALOG_OUTPUT:
                        sb.Append("HR_INT ");
                        minValue = ((IAnalogSCADAModelPointItem)point).MinRawValue.ToString();
                        maxValue = ((IAnalogSCADAModelPointItem)point).MaxRawValue.ToString();
                        nominalValue = ((IAnalogSCADAModelPointItem)point).NormalValue.ToString();
                        type = "AO";
                        break;
                    case PointType.DIGITAL_INPUT:
                        sb.Append("DI_REG ");
                        minValue = ((IDiscreteSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IDiscreteSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IDiscreteSCADAModelPointItem)point).NormalValue.ToString();
                        type = "DI";
                        break;
                    case PointType.DIGITAL_OUTPUT:
                        sb.Append("DO_REG ");
                        minValue = ((IDiscreteSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IDiscreteSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IDiscreteSCADAModelPointItem)point).NormalValue.ToString();
                        type = "DO";
                        break;
                }

                sb.Append("1 ");
                sb.Append(point.Address.ToString() + " ");
                sb.Append("0 ");
                sb.Append(minValue + " ");
                sb.Append(maxValue + " ");
                sb.Append(nominalValue + " ");
                sb.Append(type + " ");
                sb.Append($"@{point.Name} ");
                sb.Append("2"); // Acquisition period
            }
        }
    }
}
