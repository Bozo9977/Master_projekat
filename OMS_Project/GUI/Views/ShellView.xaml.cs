using GUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
            DrawNetwork(Data.Shapes);
        }


        private void DrawNetwork(List<ShapeInfo> shapesToDraw)
        {
            foreach (var f in shapesToDraw)
            {

                Canvas.SetTop(f.MyShape, f.Y);
                Canvas.SetLeft(f.MyShape, f.X);

                DrawingCanvas.Children.Add(f.MyShape);
            }
        }
    }
}
