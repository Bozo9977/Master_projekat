using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.Data
{
    public interface ISCADAModelPointItem
    {
        long Gid { get; set; }
        short Address { get; set; }
        string Name { get; set; }
        PointType RegisterType { get; set; }
        AlarmType Alarm { get; set; }
        bool Initialized { get; }
    }
}
