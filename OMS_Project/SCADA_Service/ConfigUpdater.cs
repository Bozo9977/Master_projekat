using SCADA_Common;
using SCADA_Common.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service
{
    internal class ConfigUpdater
    {
        private string serverFilePath = String.Empty;
        private string clientFilePath = String.Empty;

        public ConfigUpdater()
        {
            serverFilePath = "../../../MdbSim/RtuCfg.txt";
            clientFilePath = "../../../SCADA_Client/RtuCfg.txt";
        }

        public void UpdateServerConfigFile(Dictionary<long, ISCADAModelPointItem> points)
        {
            string minValue = String.Empty;
            string maxValue = String.Empty;
            string nominalValue = String.Empty;
            string type = String.Empty;

            StringBuilder sbServer = new StringBuilder();

            sbServer.AppendLine("STA 1");
            sbServer.AppendLine("TCP 502");
            sbServer.AppendLine();

            foreach (var point in points.Values)
            {
                switch (point.RegisterType)
                {
                    case PointType.ANALOG_INPUT:
                        sbServer.Append("IN_REG ");
                        minValue = ((IAnalogSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IAnalogSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IAnalogSCADAModelPointItem)point).NormalValue.ToString();
                        type = "AI";
                        break;
                    case PointType.ANALOG_OUTPUT:
                        sbServer.Append("HR_INT ");
                        minValue = ((IAnalogSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IAnalogSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IAnalogSCADAModelPointItem)point).NormalValue.ToString();
                        type = "AO";
                        break;
                    case PointType.DIGITAL_INPUT:
                        sbServer.Append("DI_REG ");
                        minValue = ((IDiscreteSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IDiscreteSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IDiscreteSCADAModelPointItem)point).NormalValue.ToString();
                        type = "DI";
                        break;
                    case PointType.DIGITAL_OUTPUT:
                        sbServer.Append("DO_REG ");
                        minValue = ((IDiscreteSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IDiscreteSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IDiscreteSCADAModelPointItem)point).NormalValue.ToString();
                        type = "DO";
                        break;
                }

                sbServer.Append("1 ");
                sbServer.Append(point.Address.ToString() + " ");
                sbServer.Append("0 ");
                sbServer.Append(minValue + " ");
                sbServer.Append(maxValue + " ");
                sbServer.Append(nominalValue + " ");
                sbServer.Append(type + " ");
                sbServer.AppendLine($"@{point.Name}"); // Descriptor
            }

            File.WriteAllText(serverFilePath, sbServer.ToString());
        }

        public void UpdateClientConfigFile(Dictionary<long, ISCADAModelPointItem> points)
        {
            string minValue = String.Empty;
            string maxValue = String.Empty;
            string nominalValue = String.Empty;
            string type = String.Empty;

            StringBuilder sbClient = new StringBuilder();

            sbClient.AppendLine("STA 1");
            sbClient.AppendLine("TCP 502");
            sbClient.AppendLine();

            foreach (var point in points.Values)
            {
                switch (point.RegisterType)
                {
                    case PointType.ANALOG_INPUT:
                        sbClient.Append("IN_REG ");
                        minValue = ((IAnalogSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IAnalogSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IAnalogSCADAModelPointItem)point).NormalValue.ToString();
                        type = "AI";
                        break;
                    case PointType.ANALOG_OUTPUT:
                        sbClient.Append("HR_INT ");
                        minValue = ((IAnalogSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IAnalogSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IAnalogSCADAModelPointItem)point).NormalValue.ToString();
                        type = "AO";
                        break;
                    case PointType.DIGITAL_INPUT:
                        sbClient.Append("DI_REG ");
                        minValue = ((IDiscreteSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IDiscreteSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IDiscreteSCADAModelPointItem)point).NormalValue.ToString();
                        type = "DI";
                        break;
                    case PointType.DIGITAL_OUTPUT:
                        sbClient.Append("DO_REG ");
                        minValue = ((IDiscreteSCADAModelPointItem)point).MinValue.ToString();
                        maxValue = ((IDiscreteSCADAModelPointItem)point).MaxValue.ToString();
                        nominalValue = ((IDiscreteSCADAModelPointItem)point).NormalValue.ToString();
                        type = "DO";
                        break;
                }

                sbClient.Append("1 ");
                sbClient.Append(point.Address.ToString() + " ");
                sbClient.Append("0 ");
                sbClient.Append(minValue + " ");
                sbClient.Append(maxValue + " ");
                sbClient.Append(nominalValue + " ");
                sbClient.Append(type + " ");
                sbClient.Append($"@{point.Name} "); // Descriptor
                sbClient.AppendLine("2"); // Acquisition period
            }

            File.WriteAllText(clientFilePath, sbClient.ToString());
        }
    }
}
