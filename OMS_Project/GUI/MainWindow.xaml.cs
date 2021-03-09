using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		NetworkModel networkModel;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void menuItemImport_Click(object sender, RoutedEventArgs e)
		{
			NetworkModelDownload download = new NetworkModelDownload();

			if(download.Download())
				networkModel = new NetworkModel(download);
		}

		private void menuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Exit();
			base.OnClosing(e);
		}

		void Exit()
		{
			
		}
	}
}
