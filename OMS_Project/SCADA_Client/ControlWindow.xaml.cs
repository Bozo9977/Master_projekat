using SCADA_Client.ViewModel.PointViewModels;
using System.Windows;

namespace SCADA_Client
{
	/// <summary>
	/// Interaction logic for ControlWindow.xaml
	/// </summary>
	internal partial class ControlWindow : Window
	{
		public ControlWindow()
		{
			InitializeComponent();
		}

		public ControlWindow(BasePointItem dataContext) : this()
		{
			this.DataContext = dataContext;
			Title = string.Format("Control Window - {0}", dataContext.Name);
		}
    }
}