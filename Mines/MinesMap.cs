using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Mines
{
	public class MinesMap : INotifyPropertyChanged
	{
		private Area[][] m_Areas;
		private readonly int m_AreaSize = 10;

		public event PropertyChangedEventHandler PropertyChanged;

		public Area[][] Areas
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
			GenerateMap();
			RandomPutBomb(10);
			ScanNearBombs();
		}

		private void GenerateMap()
		{
			Areas = Enumerable.Range(0, m_AreaSize)
				.Select(y => Enumerable.Range(0, m_AreaSize)
					.Select(x => CreateArea(x, y))
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

		private void ScanNearBombs()
		{
			foreach (var area in from row in m_Areas
								 from a in row
								 select a)
			{
				var topY = area.Y - 1;
				if (topY < 0)
					topY = 0;

				var leftX = area.X - 1;
				if (leftX < 0)
					leftX = 0;

				var bottomY = area.Y + 1;
				if (bottomY > m_AreaSize - 1)
					bottomY = m_AreaSize - 1;

				var rightX = area.X + 1;
				if (rightX > m_AreaSize - 1)
					rightX = m_AreaSize - 1;

				int bombCount = 0;
				for (var scanX = leftX; scanX <= rightX; scanX++)
					for (var scanY = topY; scanY <= bottomY; scanY++)
					{
						if (scanX == area.X && scanY == area.Y)
							continue;

						if (m_Areas[scanY][scanX].HasBomb)
							bombCount++;
					}

				area.NearBombCount = bombCount;
			}
		}

		private Area CreateArea(int x, int y)
		{
			var area = new Area
			{
				X = x,
				Y = y
			};

			area.AreaClicked += OnAreaClicked;

			return area;
		}

		private void DestroyArea(Area area)
		{
			area.AreaClicked -= OnAreaClicked;
		}

		private void OnAreaClicked(object sender, AreaClickedArgs args)
		{
			var waitProcessingQueue = new Queue<Area>();
			waitProcessingQueue.Enqueue(Areas[args.Y][args.X]);
			//var n = 1000;
			while (waitProcessingQueue.Count > 0)
			{
				var processingArea = waitProcessingQueue.Dequeue();
				FindSafeAreas(processingArea, waitProcessingQueue);
			}
		}

		private void FindSafeAreas(Area centerArea, Queue<Area> waitProcessingQueue)
		{
			if (centerArea.Status == AreaStatus.Boom)
				return;

			var topY = centerArea.Y - 1;
			if (topY < 0)
				topY = 0;

			var leftX = centerArea.X - 1;
			if (leftX < 0)
				leftX = 0;

			var bottomY = centerArea.Y + 1;
			if (bottomY > m_AreaSize - 1)
				bottomY = m_AreaSize - 1;

			var rightX = centerArea.X + 1;
			if (rightX > m_AreaSize - 1)
				rightX = m_AreaSize - 1;

			for (var scanX = leftX; scanX <= rightX; scanX++)
				for (var scanY = topY; scanY <= bottomY; scanY++)
				{
					if (scanX == centerArea.X && scanY == centerArea.Y)
						continue;

					var targetArea = m_Areas[scanY][scanX];

					if (targetArea.Status == AreaStatus.SteppedOn)
						continue;

					if (targetArea.HasBomb)
						continue;

					if (targetArea.NearBombCount > 0)
					{
						targetArea.SetSteppedOn();
						continue;
					}

					targetArea.SetSteppedOn();
					waitProcessingQueue.Enqueue(targetArea);
				}
		}
	}
}
