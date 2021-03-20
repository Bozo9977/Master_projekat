using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.Data
{
    public interface IAnalogSCADAModelPointItem
    {
        float MinValue { get; set; }
        float MaxValue { get; set; }
        float NormalValue { get; set; }
    }
}
