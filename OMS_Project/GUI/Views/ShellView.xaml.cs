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
            InitializeGrid();
            DrawNetwork(Data.Shapes);
        }

        private void InitializeGrid()
        {
            for (int i = 0; i < 10; i++)
            {
                ColumnDefinition tempColumn = new ColumnDefinition();
                DrawingGrid.ColumnDefinitions.Add(tempColumn);
            }

            for (int i = 0; i < 30; i++)
            {
                RowDefinition tempRow = new RowDefinition();
                DrawingGrid.RowDefinitions.Add(tempRow);
            }
        }

        private void DrawNetwork(List<ShapeInfo> shapesToDraw)
        {
            foreach (var f in shapesToDraw)
            {
                Grid.SetRow(f.MyShape, f.Row);
                Grid.SetColumn(f.MyShape, f.Column);
                DrawingGrid.Children.Add(f.MyShape);
            }
        }
    }
}
