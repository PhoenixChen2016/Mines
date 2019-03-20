using System;
using System.Globalization;
using System.Windows.Data;

namespace Mines
{
	internal class AreaStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Area area)
			{
				if (area.HasBomb)
					return "Assets/Boom.jpg";
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
