using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DB_Model
{
    public class DiscretePointItemDB: PointItemDB
    {
        public ushort CurrentValue { get; set; }

    }
}
