using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DB_Model
{
    public class AnalogPointItemDB: PointItemDB
    {
        public float MaxValue { get; set; }
        public float MinValue { get; set; }
        public float NormalValue { get; set; }

    }
}
