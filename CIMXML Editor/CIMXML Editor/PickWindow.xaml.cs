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
	/// Interaction logic for PickWindow.xaml
	/// </summary>
	public partial class PickWindow : Window
	{
		public int Result { get; private set; }

		public PickWindow(IEnumerable<string> items)
		{
			InitializeComponent();
			Result = -1;

			foreach(string i in items)
				spItems.Children.Add(new RadioButton() { Content = i });
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			for(int i = 0; i < spItems.Children.Count; ++i)
			{
				if(((RadioButton)spItems.Children[i]).IsChecked == true)
				{
					Result = i;
					break;
				}
			}

			Close();
		}
	}
}
