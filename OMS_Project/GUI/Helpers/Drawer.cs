using Common.GDA;
using GUI.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUI.Helpers
{
    public static class Drawer
    {
        public static List<Shape> Draw(DrawingModel dM)
        {
            List<Shape> Shapes = new List<Shape>();

            foreach (var k in dM.CurrentModel.Keys)
            {
                switch (k)
                {
                    case DMSType.Analog:
                        foreach (var analog in dM.CurrentModel[DMSType.Analog])
                        {
                            string ID = analog.Key.ToString();
                            string Name = analog.Value.Name;
                            string BaseAddress = ((Analog)analog.Value).BaseAddress.ToString();
                            string Value = ((Analog)analog.Value).NormalValue.ToString();
                            Shapes.Add(DrawAnalog(ID, Name, BaseAddress, Value, (Analog)analog.Value));
                        }
                        break;
                    case DMSType.Discrete:
                        foreach (var discrete in dM.CurrentModel[DMSType.Discrete])
                        {
                            string ID = discrete.Key.ToString();
                            string Name = discrete.Value.Name;
                            string BaseAddress = ((Discrete)discrete.Value).BaseAddress.ToString();
                            string Value = ((Discrete)discrete.Value).NormalValue.ToString();
                            Shapes.Add(DrawDiscrete(ID, Name, BaseAddress, Value, (Discrete)discrete.Value));
                        }
                        break;
                }
            }

            return Shapes;
        }

        private static Shape DrawAnalog(string id, string name, string baseAddress, string value, Analog a)
        {
            Rectangle analog = new Rectangle();
            analog.Fill = Brushes.PaleVioletRed;
            analog.Height = 50;
            analog.Width = 50;
            analog.ToolTip = GetTooltip.Analog(id, name, baseAddress, value);
            analog.MouseLeftButtonDown += (sender, args) => Analog_MouseLeftButtonDown(sender, args, a);

            return analog;
        }

        private static void Analog_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e, Analog a)
        {
            PropertyWindow window = new PropertyWindow(DMSType.Analog, a);
            window.Show();
        }

        private static Shape DrawDiscrete(string id, string name, string baseAddress, string value, Discrete d)
        {
            Rectangle discrete = new Rectangle();
            discrete.Fill = Brushes.SkyBlue;
            discrete.Height = 50;
            discrete.Width = 50;
            discrete.ToolTip = GetTooltip.Discrete(id, name, baseAddress, value);
            discrete.MouseLeftButtonDown += (sender, args) => Discrete_MouseLeftButtonDown(sender, args, d);

            return discrete;
        }

        private static void Discrete_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e, Discrete d)
        {
            PropertyWindow window = new PropertyWindow(DMSType.Discrete, d);
            window.Show();
        }
    }
}

