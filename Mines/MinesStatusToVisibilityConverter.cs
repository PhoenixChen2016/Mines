using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mines
{
	public class MinesStatusToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is MinesStatus status)
			{
				switch (status)
				{
					case MinesStatus.None:
						return Visibility.Hidden;
					default:
						return Visibility.Visible;
				}
			}

			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
