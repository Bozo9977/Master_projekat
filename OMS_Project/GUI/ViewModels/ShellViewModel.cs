using Caliburn.Micro;
using GUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.ViewModels
{
    public class ShellViewModel : Screen
    {
        public BindableCollection<Entity> Children = new BindableCollection<Entity>();

        #region Binding Test
        /*private int _number1 = 0;

        public int Number1
        {
            get { return _number1; }
            set 
            { 
                _number1 = value;
                NotifyOfPropertyChange(() => Number1);
                NotifyOfPropertyChange(() => Result);
            }
        }

        private int _number2 = 0;

        public int Number2
        {
            get { return _number2; }
            set 
            { 
                _number2 = value;
                NotifyOfPropertyChange(() => Number2);
                NotifyOfPropertyChange(() => Result);
            }
        }


        public string Result
        {
            get { return (Number1 + Number2).ToString(); }
        }*/
        #endregion

        public ShellViewModel()
        {
            //ProcessGraph();
        }

        private void ProcessGraph(Dictionary<string, Entity> D)
        {
            int srcColumn = 50;
            int srcRow = 0;

            DFS(D, GetFirstEntity(D), srcColumn, srcRow);
        }

        private void DFS(Dictionary<string, Entity> D, Entity u, int column, int row)
        {
            u.Visited = true;
            u.Column = column;
            u.Row = row;
            Children.Add(u);

            if (D[u.Up].Visited == false)
                DFS(D, D[u.Up], u.Column, u.Row--);
            else if (D[u.Left].Visited == false)
                DFS(D, D[u.Left], u.Column--, u.Row);
            else if (D[u.Right].Visited == false)
                DFS(D, D[u.Right], u.Column++, u.Row);
            else if (D[u.Down].Visited == false)
                DFS(D, D[u.Down], u.Column, u.Row++);
        }

        private Entity GetFirstEntity(Dictionary<string, Entity> D)
        {
            foreach(var entity in D)
            {
                if (entity.Value.Up == "SRC")
                    return entity.Value;
            }

            throw new Exception("Missing SRC connection!");
        }
    }
}
