using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Mines
{
	public class MinesMap : INotifyPropertyChanged
	{
		private IEnumerable<IEnumerable<Area>> m_Areas;

		public event PropertyChangedEventHandler PropertyChanged;

		public IEnumerable<IEnumerable<Area>> Areas
		{
			get
			{
				return m_Areas;
			}
			set
			{
				m_Areas = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Areas)));
			}
		}

		public MinesMap()
		{
			Areas = Enumerable.Range(0, 10)
				.Select(y => Enumerable.Range(0, 10)
					.Select(x => new Area
					{
						X = x,
						Y = y
					})
					.ToArray())
				.ToArray();
		}
	}
}
