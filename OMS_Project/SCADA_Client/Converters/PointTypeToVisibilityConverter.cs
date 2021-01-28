using SCADA_Common;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCADA_Client.Converters
{
    //DO_REG RW
    //DI_REG R
    //IN_REG R
    //HR_INT RW

    public class PointTypeToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value is PointType)
			{
				PointType pt = (PointType)value;
				if (pt == PointType.DIGITAL_OUTPUT || pt == PointType.ANALOG_OUTPUT)
				{
					return Visibility.Visible;
				}
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}