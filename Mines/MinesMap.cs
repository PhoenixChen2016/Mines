using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Mines
{
	public class MinesMap : INotifyPropertyChanged
	{
		private readonly int m_AreaSize = 10;
		private Area[][] m_Areas;
		private TimeSpan m_GameTime;
		private bool m_IsGameStarting;

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

		public TimeSpan GameTime
		{
			get => m_GameTime;
			set
			{
				m_GameTime = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameTime)));
			}
		}

		public MinesMap()
		{
			GenerateMap();
			RandomPutBomb(10);
			ScanNearBombs();
		}

		public void ResetGame()
		{
			foreach (var area in Areas.SelectMany(a => a))
			{
				area.AreaClicked -= OnAreaClicked;
				area.AreaTagged -= OnAreaTagged;
			}

			GenerateMap();
			RandomPutBomb(10);
		}

		private Area CreateArea(int x, int y)
		{
			var area = new Area
			{
				X = x,
				Y = y
			};

			area.AreaClicked += OnAreaClicked;
			area.AreaTagged += OnAreaTagged;

			return area;
		}

		private void DestroyArea(Area area)
		{
			area.AreaClicked -= OnAreaClicked;
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

					if (targetArea.Status == AreaStatus.SteppedOn || targetArea.Status == AreaStatus.Flag)
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

		private void FireAllBombs()
		{
			foreach (var area in Areas.SelectMany(array => array))
			{
				if (area.HasBomb && !area.IsTagged)
				{
					area.Status = AreaStatus.Boom;
					area.IsSteppedOn = true;
				}

				if (area.IsTagged && !area.HasBomb)
				{
					area.IsTagged = false;
					area.IsSteppedOn = true;
					area.Status = AreaStatus.SteppedOn;
				}
			}
		}

		private void GenerateMap()
		{
			Areas = Enumerable.Range(0, m_AreaSize)
				.Select(y => Enumerable.Range(0, m_AreaSize)
					.Select(x => CreateArea(x, y))
					.ToArray())
				.ToArray();
		}

		private void OnAreaClicked(object sender, AreaArgs args)
		{
			if (!m_IsGameStarting)
			{
				StartTimer();
				m_IsGameStarting = true;
			}

			var targetArea = Areas[args.Y][args.X];

			if (targetArea.IsSteppedOn || targetArea.IsTagged)
				return;

			targetArea.IsSteppedOn = true;

			if (targetArea.HasBomb)
			{
				targetArea.Status = AreaStatus.Boom;
				FireAllBombs();
				return;
			}
			else
				targetArea.Status = AreaStatus.SteppedOn;

			var waitProcessingQueue = new Queue<Area>();
			waitProcessingQueue.Enqueue(targetArea);

			while (waitProcessingQueue.Count > 0)
			{
				var processingArea = waitProcessingQueue.Dequeue();
				FindSafeAreas(processingArea, waitProcessingQueue);
			}
		}

		private void OnAreaTagged(object sender, AreaArgs args)
		{
			var targetArea = Areas[args.Y][args.X];

			if (targetArea.IsSteppedOn)
				return;

			targetArea.IsTagged = !targetArea.IsTagged;
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
			foreach (var area in Areas.SelectMany(a => a))
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

		private void StartTimer()
		{
			var startTime = DateTime.Now;
			Observable.Interval(TimeSpan.FromMilliseconds(100))
				.Subscribe(n =>
				{
					var currentTime = DateTime.Now;

					GameTime = currentTime - startTime;
				});
		}
	}
}
