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
        public List<LineGeometry> Lines = new List<LineGeometry>();

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
            InitializeLineInfo();
            InitializeShapeInfo();
        }

        private void ProcessGraph(Dictionary<string, Entity> D)
        {
            // custom initial value
            int srcY = 360;
            int srcX = 25;

            try
            {
                DFS(D, GetFirstEntity(D), srcY, srcX);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void DFS(Dictionary<string, Entity> D, Entity u, double y, double x)
        {
            u.Visited = true;
            u.Y = y;
            u.X = x;
            Children.Add(u);

            if (D.ContainsKey(u.Up) && D[u.Up].Visited == false)
            {
                LineGeometry l = new LineGeometry
                {
                    StartPoint = new System.Windows.Point(u.Y, u.X),
                    EndPoint = new System.Windows.Point(u.Y, u.X - 25)
                };

                Lines.Add(l);

                DFS(D, D[u.Up], u.Y, u.X - 25);
            }
            if (D.ContainsKey(u.Left) && D[u.Left].Visited == false)
            {
                LineGeometry l = new LineGeometry
                {
                    StartPoint = new System.Windows.Point(u.Y, u.X),
                    EndPoint = new System.Windows.Point(u.Y - 25, u.X)
                };

                Lines.Add(l);

                DFS(D, D[u.Left], u.Y - 25, u.X);
            }
            if (D.ContainsKey(u.Right) && D[u.Right].Visited == false)
            {
                LineGeometry l = new LineGeometry
                {
                    StartPoint = new System.Windows.Point(u.Y, u.X),
                    EndPoint = new System.Windows.Point(u.Y + 25, u.X)
                };

                Lines.Add(l);

                DFS(D, D[u.Right], u.Y + 25, u.X);
            }
            if (D.ContainsKey(u.Down) && D[u.Down].Visited == false)
            {
                LineGeometry l = new LineGeometry
                {
                    StartPoint = new System.Windows.Point(u.Y, u.X),
                    EndPoint = new System.Windows.Point(u.Y, u.X + 25)
                };

                Lines.Add(l);

                DFS(D, D[u.Down], u.Y, u.X + 25);
            }
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

        private Shape CreateEntityShape(int width, int height, Brush stroke, Brush fill)
        {
            Ellipse e = new Ellipse();
            e.Width = width;
            e.Height = height;
            e.Stroke = stroke;
            e.Fill = fill;
            e.Margin = new System.Windows.Thickness(-e.Width / 2, -e.Height / 2, 0, 0);
            return e;
        }

        private Path CreateLineShape(double thickness, Brush fill, Brush stroke, LineGeometry line)
        {
            Path p = new Path();
            p.StrokeThickness = thickness;
            p.Stroke = stroke;
            p.Fill = fill;
            p.Visibility = System.Windows.Visibility.Visible;
            p.Data = line;

            return p;
        }

        private void InitializeLineInfo()
        {
            foreach(var k in Lines)
            {
                Data.Paths.Add(CreateLineShape(0.75, Brushes.Black, Brushes.Black, k));
            }
        }

        private void InitializeShapeInfo()
        {
            foreach(var entity in Children)
            {
                ShapeInfo s = new ShapeInfo();

                if(entity.Type == "terminal")
                    s.MyShape = CreateEntityShape(10, 10, Brushes.Black, Brushes.Black);
                else if (entity.Type == "connectivityNode")
                    s.MyShape = CreateEntityShape(20, 20, Brushes.Black, Brushes.White);
                else if (entity.Type == "breaker")
                    s.MyShape = CreateEntityShape(20, 20, Brushes.Black, Brushes.GreenYellow);
                else if (entity.Type == "disconnector")
                    s.MyShape = CreateEntityShape(20, 20, Brushes.Black, Brushes.Cyan);
                else if (entity.Type == "transformer")
                    s.MyShape = CreateEntityShape(20, 20, Brushes.Black, Brushes.CornflowerBlue);
                else if (entity.Type == "ACLineSegment")
                    s.MyShape = CreateEntityShape(20, 20, Brushes.Black, Brushes.Gray);

                s.X = entity.X;
                s.Y = entity.Y;
                Data.Shapes.Add(s);
            }
        }
    }
}

/*
 link.Stroke = Brushes.White;
                link.Fill = Brushes.White;
                link.StrokeThickness = 0.75;
                link.Visibility = Visibility.Visible;
                link.Data = gLink;
                link.ToolTip = StringFormatter.PrintLine(lajna);
                link.Opacity = 0.9;
 */