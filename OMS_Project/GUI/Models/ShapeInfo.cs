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

        public double X { get; set; }

        public double Y { get; set; }

        public ShapeInfo()
        {
            X = 0;
            Y = 0;
        }
    }
}
