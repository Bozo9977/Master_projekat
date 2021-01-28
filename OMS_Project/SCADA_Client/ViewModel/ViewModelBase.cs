using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Client.ViewModel
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		internal void OnPropertyChanged(string prop)
		{
			if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
