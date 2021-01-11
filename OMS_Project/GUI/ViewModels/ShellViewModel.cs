using Caliburn.Micro;
using GUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

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
            Dictionary<string, Entity> D = new Dictionary<string, Entity>();
            
            using(JSONParser jp = new JSONParser())
            {
                D = jp.Import("../../mock_network.json");
            }
            
            // testing src entity
            Entity src = new Entity("000", "source", "", "", "001", "", 0, 50);
            src.Visited = true;
            D.Add("SRC", src);

            ProcessGraph(D);
            InitializeShapeInfo();
        }

        private void ProcessGraph(Dictionary<string, Entity> D)
        {
            // custom initial value
            int srcColumn = 5;
            int srcRow = 0;

            try
            {
                DFS(D, GetFirstEntity(D), srcColumn, srcRow);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void DFS(Dictionary<string, Entity> D, Entity u, int column, int row)
        {
            u.Visited = true;
            u.Column = column;
            u.Row = row;
            Children.Add(u);

            if (D.ContainsKey(u.Up) && D[u.Up].Visited == false)
                DFS(D, D[u.Up], u.Column, u.Row - 1);
            if (D.ContainsKey(u.Left) && D[u.Left].Visited == false)
                DFS(D, D[u.Left], u.Column - 1, u.Row);
            if (D.ContainsKey(u.Right) && D[u.Right].Visited == false)
                DFS(D, D[u.Right], u.Column + 1, u.Row);
            if (D.ContainsKey(u.Down) && D[u.Down].Visited == false)
                DFS(D, D[u.Down], u.Column, u.Row + 1);
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

        private Shape CreateShape(int width, int height, Brush stroke, Brush fill)
        {
            Ellipse e = new Ellipse();
            e.Width = width;
            e.Height = height;
            e.Stroke = stroke;
            e.Fill = fill;

            return e;
        }

        private void InitializeShapeInfo()
        {
            foreach(var entity in Children)
            {
                ShapeInfo s = new ShapeInfo();

                if(entity.Type == "terminal")
                    s.MyShape = CreateShape(10, 10, Brushes.Black, Brushes.Black);
                else if (entity.Type == "connectivityNode")
                    s.MyShape = CreateShape(20, 20, Brushes.Black, Brushes.White);
                else if (entity.Type == "breaker")
                    s.MyShape = CreateShape(20, 20, Brushes.Black, Brushes.GreenYellow);
                else if (entity.Type == "disconnector")
                    s.MyShape = CreateShape(20, 20, Brushes.Black, Brushes.Cyan);
                else if (entity.Type == "transformer")
                    s.MyShape = CreateShape(20, 20, Brushes.Black, Brushes.CornflowerBlue);
                else if (entity.Type == "ACLineSegment")
                    s.MyShape = CreateShape(20, 20, Brushes.Black, Brushes.Gray);

                s.Row = entity.Row;
                s.Column = entity.Column;
                Data.Shapes.Add(s);
            }
        }
    }
}
