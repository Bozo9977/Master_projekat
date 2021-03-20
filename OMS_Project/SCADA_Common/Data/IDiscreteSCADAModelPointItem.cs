using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.Data
{
    public interface IDiscreteSCADAModelPointItem
    {
        ushort MinValue { get; set; }
        ushort MaxValue { get; set; }
        ushort NormalValue { get; set; }
    }
}
