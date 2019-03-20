using System;
using System.Globalization;
using System.Windows.Data;

namespace Mines
{
	internal class AreaStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is AreaStatus status)
			{
				switch (status)
				{
					case AreaStatus.Bomb:
						return "Assets/Bomb.jpg";
					case AreaStatus.Boom:
						return "Assets/Boom.jpg";
					case AreaStatus.Flag:
						return "Assets/Flag.png";
					case AreaStatus.SteppedOn:
						return "Assets/SteppedOn.jpg";
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
