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

namespace CIMXML_Editor
{
	/// <summary>
	/// Interaction logic for SelectWindow.xaml
	/// </summary>
	public partial class SelectWindow : Window
	{
		public List<bool> Result { get; private set; }

		public SelectWindow(IEnumerable<Tuple<bool, string>> items)
		{
			InitializeComponent();

			foreach(Tuple<bool, string> i in items)
				spItems.Children.Add(new CheckBox() { IsChecked = i.Item1, Content = i.Item2 });
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Result = new List<bool>(spItems.Children.Count);

			foreach(object uie in spItems.Children)
				Result.Add(((CheckBox)uie).IsChecked == true);

			Close();
		}
	}
}
