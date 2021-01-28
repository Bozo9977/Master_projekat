using SCADA_Client.Exceptions;
using SCADA_Client.ViewModel;
using SCADA_Client.ViewModel.PointViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace SCADA_Client
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			try
			{
				InitializeComponent();
				this.Closed += Window_Closed;
				DataContext = new MainViewModel();
				
			}
			catch (ConfigurationException confEx)
			{
				MessageBox.Show($"{confEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Unexpected error occured!{Environment.NewLine}Stack trace:{Environment.NewLine}{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Close();
			}
		}

		private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ControlWindow cw = new ControlWindow(dgPoints.SelectedItem as BasePointItem);
			cw.Owner = this;
			cw.ShowDialog();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			(DataContext as IDisposable).Dispose();
		}
	}
}