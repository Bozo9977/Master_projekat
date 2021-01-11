using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GUI.Models
{
    public class ShapeInfo
    {
        public Shape MyShape { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public ShapeInfo()
        {
            Row = 0;
            Column = 0;
        }
    }
}
