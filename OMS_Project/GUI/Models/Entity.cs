using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Models
{
    public class Entity
    {
        public string ID { get; set; }
        public string Type { get; set; }
        public bool Visited { get; set; }
        public string Up { get; set; }
        public string Right { get; set; }
        public string Down { get; set; }
        public string Left { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public Entity()
        {

        }

        public Entity(string id, string type, string up, string right, string down, string left, int row, int column)
        {
            Visited = false;
            ID = id;
            Type = type;
            Up = up;
            Right = right;
            Down = down;
            Left = left;
            Row = row;
            Column = column;
        }

        ~Entity()
        {

        }
    }
}
