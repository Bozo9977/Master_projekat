using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DB_Model
{
    public class AnalogPointItemDB: PointItemDB
    {
        public float CurrentEguValue { get; set; }
        public int CurrentRawValue { get; set; }

    }
}
