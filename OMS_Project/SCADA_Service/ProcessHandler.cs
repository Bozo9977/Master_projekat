using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service
{
    public static class ProcessHandler
    {
        public static List<Process> ActiveProcesses = new List<Process>();

        public static void KillProcesses()
        {
            foreach(var proc in ActiveProcesses)
            {
                proc.Kill();
            }
        }
    }
}
