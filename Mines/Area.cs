using System.ComponentModel;

namespace Mines
{
	public class Area : INotifyPropertyChanged
	{
		private bool m_HasBomb = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasBomb
		{
			get
			{
				return m_HasBomb;
			}
			set
			{
				m_HasBomb = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasBomb)));
			}
		}

		public int X { get; set; }

		public int Y { get; set; }
	}
}
