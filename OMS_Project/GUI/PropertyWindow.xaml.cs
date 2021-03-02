using Common.GDA;
using GUI.DataModel;
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

namespace GUI
{
    /// <summary>
    /// Interaction logic for PropertyWindow.xaml
    /// </summary>
    public partial class PropertyWindow : Window
    {
        public DMSType Type { get; set; }
        public IdentifiedObject IO { get; set; }
        public Grid myGrid;
        public PropertyWindow(DMSType type, IdentifiedObject io)
        {
            Type = type;
            IO = io;
            InitializeGrid();
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            myGrid = new Grid();
            myGrid.Width = 300;
            myGrid.Height = 480;
            myGrid.HorizontalAlignment = HorizontalAlignment.Left;
            myGrid.VerticalAlignment = VerticalAlignment.Top;

            switch (Type)
            {
                case DMSType.Analog:
                case DMSType.Discrete:
                    for (int i = 0; i < 10; i++)
                    {
                        RowDefinition rd = new RowDefinition();
                        rd.Height = new GridLength(40);
                        myGrid.RowDefinitions.Add(rd);
                    }
                    ColumnDefinition cd1 = new ColumnDefinition();
                    cd1.Width = new GridLength(140);
                    myGrid.ColumnDefinitions.Add(cd1);

                    ColumnDefinition cd2 = new ColumnDefinition();
                    cd2.Width = new GridLength(20);
                    myGrid.ColumnDefinitions.Add(cd2);

                    ColumnDefinition cd3 = new ColumnDefinition();
                    cd3.Width = new GridLength(140);
                    myGrid.ColumnDefinitions.Add(cd3);

                    break;
            }
            GenerateFields();
            this.Content = myGrid;
        }

        private void GenerateFields()
        {
            switch(Type)
            {
                case DMSType.Analog:
                    #region Analog Field Generation
                    Label l1 = new Label();
                    l1.Content = "ID";
                    l1.HorizontalContentAlignment = HorizontalAlignment.Left;
                    l1.VerticalContentAlignment = VerticalAlignment.Center;
                    l1.Margin = new Thickness(10, 0, 10, 0);
                    Label l2 = new Label();
                    l2.Content = "Name";
                    l2.HorizontalContentAlignment = HorizontalAlignment.Left;
                    l2.VerticalContentAlignment = VerticalAlignment.Center;
                    l2.Margin = new Thickness(10, 0, 10, 0);
                    Label l3 = new Label();
                    l3.Content = "Base Address";
                    l3.HorizontalContentAlignment = HorizontalAlignment.Left;
                    l3.VerticalContentAlignment = VerticalAlignment.Center;
                    l3.Margin = new Thickness(10, 0, 10, 0);
                    Label l4 = new Label();
                    l4.Content = " Current Value";
                    l4.HorizontalContentAlignment = HorizontalAlignment.Left;
                    l4.VerticalContentAlignment = VerticalAlignment.Center;
                    l4.Margin = new Thickness(10, 0, 10, 0);

                    TextBox tb1 = new TextBox();
                    tb1.Text = IO.GID.ToString();
                    tb1.IsReadOnly = true;
                    tb1.HorizontalContentAlignment = HorizontalAlignment.Center;
                    tb1.VerticalContentAlignment = VerticalAlignment.Center;
                    tb1.Margin = new Thickness(10, 0, 10, 0);
                    TextBox tb2 = new TextBox();
                    tb2.Text = IO.Name;
                    tb2.IsReadOnly = true;
                    tb2.HorizontalContentAlignment = HorizontalAlignment.Center;
                    tb2.VerticalContentAlignment = VerticalAlignment.Center;
                    tb2.Margin = new Thickness(10, 0, 10, 0);
                    TextBox tb3 = new TextBox();
                    tb3.Text = ((Analog)IO).BaseAddress.ToString();
                    tb3.IsReadOnly = true;
                    tb3.HorizontalContentAlignment = HorizontalAlignment.Center;
                    tb3.VerticalContentAlignment = VerticalAlignment.Center;
                    tb3.Margin = new Thickness(10, 0, 10, 0);
                    TextBox tb4 = new TextBox();
                    tb4.Text = ((Analog)IO).NormalValue.ToString();
                    tb4.IsReadOnly = true;
                    tb4.HorizontalContentAlignment = HorizontalAlignment.Center;
                    tb4.VerticalContentAlignment = VerticalAlignment.Center;
                    tb4.Margin = new Thickness(10, 0, 10, 0);

                    Button btn1 = new Button();
                    btn1.Content = "Save";
                    btn1.Padding = new Thickness(10);
                    btn1.Margin = new Thickness(10, 0, 10, 0);
                    btn1.Click += Btn1_Click;
                    Button btn2 = new Button();
                    btn2.Content = "Cancel";
                    btn2.Padding = new Thickness(10);
                    btn2.Margin = new Thickness(10, 0, 10, 0);
                    btn2.Click += Btn2_Click;

                    Grid.SetColumn(l1, 0);
                    Grid.SetColumn(l2, 0);
                    Grid.SetColumn(l3, 0);
                    Grid.SetColumn(l4, 0);
                    Grid.SetColumn(btn1, 0);
                    Grid.SetColumn(tb1, 2);
                    Grid.SetColumn(tb2, 2);
                    Grid.SetColumn(tb3, 2);
                    Grid.SetColumn(tb4, 2);
                    Grid.SetColumn(btn2, 2);

                    Grid.SetRow(l1, 0);
                    Grid.SetRow(tb1, 0);
                    Grid.SetRow(l2, 2);
                    Grid.SetRow(tb2, 2);
                    Grid.SetRow(l3, 4);
                    Grid.SetRow(tb3, 4);
                    Grid.SetRow(l4, 6);
                    Grid.SetRow(tb4, 6);
                    Grid.SetRow(btn1, 9);
                    Grid.SetRow(btn2, 9);

                    myGrid.Children.Add(l1);
                    myGrid.Children.Add(l2);
                    myGrid.Children.Add(l3);
                    myGrid.Children.Add(l4);
                    myGrid.Children.Add(tb1);
                    myGrid.Children.Add(tb2);
                    myGrid.Children.Add(tb3);
                    myGrid.Children.Add(tb4);
                    myGrid.Children.Add(btn1);
                    myGrid.Children.Add(btn2);
                    #endregion
                    break;
                case DMSType.Discrete:
                    #region Discrete Field Generation
                    Label dl1 = new Label();
                    dl1.Content = "ID";
                    dl1.HorizontalContentAlignment = HorizontalAlignment.Left;
                    dl1.VerticalContentAlignment = VerticalAlignment.Center;
                    dl1.Margin = new Thickness(10, 0, 10, 0);
                    Label dl2 = new Label();
                    dl2.Content = "Name";
                    dl2.HorizontalContentAlignment = HorizontalAlignment.Left;
                    dl2.VerticalContentAlignment = VerticalAlignment.Center;
                    dl2.Margin = new Thickness(10, 0, 10, 0);
                    Label dl3 = new Label();
                    dl3.Content = "Base Address";
                    dl3.HorizontalContentAlignment = HorizontalAlignment.Left;
                    dl3.VerticalContentAlignment = VerticalAlignment.Center;
                    dl3.Margin = new Thickness(10, 0, 10, 0);
                    Label dl4 = new Label();
                    dl4.Content = " Current Value";
                    dl4.HorizontalContentAlignment = HorizontalAlignment.Left;
                    dl4.VerticalContentAlignment = VerticalAlignment.Center;
                    dl4.Margin = new Thickness(10, 0, 10, 0);

                    TextBox dtb1 = new TextBox();
                    dtb1.Text = IO.GID.ToString();
                    dtb1.IsReadOnly = true;
                    dtb1.HorizontalContentAlignment = HorizontalAlignment.Center;
                    dtb1.VerticalContentAlignment = VerticalAlignment.Center;
                    dtb1.Margin = new Thickness(10, 0, 10, 0);
                    TextBox dtb2 = new TextBox();
                    dtb2.Text = IO.Name;
                    dtb2.IsReadOnly = true;
                    dtb2.HorizontalContentAlignment = HorizontalAlignment.Center;
                    dtb2.VerticalContentAlignment = VerticalAlignment.Center;
                    dtb2.Margin = new Thickness(10, 0, 10, 0);
                    TextBox dtb3 = new TextBox();
                    dtb3.Text = ((Discrete)IO).BaseAddress.ToString();
                    dtb3.IsReadOnly = true;
                    dtb3.HorizontalContentAlignment = HorizontalAlignment.Center;
                    dtb3.VerticalContentAlignment = VerticalAlignment.Center;
                    dtb3.Margin = new Thickness(10, 0, 10, 0);
                    TextBox dtb4 = new TextBox();
                    dtb4.Text = ((Discrete)IO).NormalValue.ToString();
                    dtb4.IsReadOnly = true;
                    dtb4.HorizontalContentAlignment = HorizontalAlignment.Center;
                    dtb4.VerticalContentAlignment = VerticalAlignment.Center;
                    dtb4.Margin = new Thickness(10, 0, 10, 0);

                    Button dbtn1 = new Button();
                    dbtn1.Content = "Save";
                    dbtn1.Padding = new Thickness(10);
                    dbtn1.Margin = new Thickness(10, 0, 10, 0);
                    dbtn1.Click += Btn1_Click;
                    Button dbtn2 = new Button();
                    dbtn2.Content = "Cancel";
                    dbtn2.Padding = new Thickness(10);
                    dbtn2.Margin = new Thickness(10, 0, 10, 0);
                    dbtn2.Click += Btn2_Click;

                    Grid.SetColumn(dl1, 0);
                    Grid.SetColumn(dl2, 0);
                    Grid.SetColumn(dl3, 0);
                    Grid.SetColumn(dl4, 0);
                    Grid.SetColumn(dbtn1, 0);
                    Grid.SetColumn(dtb1, 2);
                    Grid.SetColumn(dtb2, 2);
                    Grid.SetColumn(dtb3, 2);
                    Grid.SetColumn(dtb4, 2);
                    Grid.SetColumn(dbtn2, 2);

                    Grid.SetRow(dl1, 0);
                    Grid.SetRow(dtb1, 0);
                    Grid.SetRow(dl2, 2);
                    Grid.SetRow(dtb2, 2);
                    Grid.SetRow(dl3, 4);
                    Grid.SetRow(dtb3, 4);
                    Grid.SetRow(dl4, 6);
                    Grid.SetRow(dtb4, 6);
                    Grid.SetRow(dbtn1, 9);
                    Grid.SetRow(dbtn2, 9);

                    myGrid.Children.Add(dl1);
                    myGrid.Children.Add(dl2);
                    myGrid.Children.Add(dl3);
                    myGrid.Children.Add(dl4);
                    myGrid.Children.Add(dtb1);
                    myGrid.Children.Add(dtb2);
                    myGrid.Children.Add(dtb3);
                    myGrid.Children.Add(dtb4);
                    myGrid.Children.Add(dbtn1);
                    myGrid.Children.Add(dbtn2);
                    #endregion
                    break;
            }
            
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            // implement actual save feature
            MessageBox.Show("Saved!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
