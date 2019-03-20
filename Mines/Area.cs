using System.ComponentModel;

namespace Mines
{
	public class Area : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasBomb { get; set; }

		public int X { get; set; }

		public int Y { get; set; }
	}
}
