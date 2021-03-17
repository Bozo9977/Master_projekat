using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DB_Model
{
    public class DiscretePointItemDB: PointItemDB
    {
        public short MaxValue { get; set; }
        public short MinValue { get; set; }
        public short CurrentValue { get; set; }

    }
}
