using SCADA_Common;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SCADA_Client.Converters
{
    public class ConnectionStateToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value is ConnectionState)
			{
				return (ConnectionState)value == ConnectionState.CONNECTED ? Brushes.Green : Brushes.Red;
			}
			return Brushes.Transparent;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}