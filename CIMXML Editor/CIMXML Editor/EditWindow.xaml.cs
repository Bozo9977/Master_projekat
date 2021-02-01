using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CIMXML_Editor
{
	/// <summary>
	/// Interaction logic for EditWindow.xaml
	/// </summary>
	public partial class EditWindow : Window
	{
		public Dictionary<string, string> Result { get; private set; }

		public EditWindow(IEnumerable<Input> inputs)
		{
			InitializeComponent();

			foreach(Input input in inputs)
			{
				UniformGrid ug = new UniformGrid() { Columns = 2, Rows = 1, VerticalAlignment = VerticalAlignment.Top };
				ug.Children.Add(new TextBlock() { Text = input.Name, VerticalAlignment = VerticalAlignment.Center });
				UIElement element;

				if(input.OfferedValues != null)
				{
					ComboBox cb = new ComboBox() { IsEditable = true, Text = input.Value };

					foreach(string s in input.OfferedValues)
						cb.Items.Add(s);

					element = cb;
				}
				else
				{
					element = new TextBox() { Text = input.Value };
				}

				ug.Children.Add(element);
				spFields.Children.Add(ug);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Result = new Dictionary<string, string>();

			foreach(object row in spFields.Children)
			{
				UniformGrid ug = (UniformGrid)row;
				string value = ug.Children[1] as TextBox == null ? ((ComboBox)ug.Children[1]).Text : ((TextBox)ug.Children[1]).Text;
				Result.Add(((TextBlock)ug.Children[0]).Text, value);
			}

			Close();
		}
	}
}
