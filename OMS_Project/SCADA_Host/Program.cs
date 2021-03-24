using NServiceBus;
using NServiceBus.Logging;
using SCADA_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Host
{
    class Program
    {
        static void Main(string[] args)
        {
            string message = "Starting SCADA Service...";
            Console.WriteLine("\n{0}\n", message);
            SCADAService scadaService = new SCADAService();

            scadaService.Start();

            message = "Press <Enter> to stop the service.";
            Console.WriteLine(message);
            Console.ReadLine();
            scadaService.CloseSCADA();
        }
  
    }
}
