using System;
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
			GenerateMap(10);
			RandomPutBomb(10);
		}

		private void GenerateMap(int size)
		{
			Areas = Enumerable.Range(0, size)
				.Select(y => Enumerable.Range(0, size)
					.Select(x => new Area
					{
						X = x,
						Y = y
					})
					.ToArray())
				.ToArray();
		}

		private void RandomPutBomb(int count)
		{
			var random = new Random();

			var randomTopAreaByCount = (from row in m_Areas
										from area in row
										let o = random.Next(0, 1000)
										let t = (Index: o, Area: area)
										orderby t.Index
										select t.Area).Take(count);

			foreach (var area in randomTopAreaByCount)
				area.HasBomb = true;
		}
	}
}
