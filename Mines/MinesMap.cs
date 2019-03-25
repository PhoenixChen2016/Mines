using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;

namespace Mines
{
	public class MinesMap : INotifyPropertyChanged
	{
		private MinesLevel m_SelectedLevel;
		private Area[][] m_Areas;
		private string m_GameOverMessage;
		private MinesStatus m_GameStatus = MinesStatus.None;
		private TimeSpan m_GameTime;
		private bool m_IsGameOver;
		private bool m_IsGameStarting;
		private IDisposable m_TimerProcess;

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

		public string GameOverMessage
		{
			get => m_GameOverMessage;
			set
			{
				m_GameOverMessage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameOverMessage)));
			}
		}

		public MinesStatus GameStatus
		{
			get => m_GameStatus;
			set
			{
				m_GameStatus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameStatus)));
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

		public MinesLevel[] Scenes => new[] {
			new MinesLevel
			{
				AreaSize = 10,
				BombCount = 10,
				LevelName = "Level 1"
			},
			new MinesLevel
			{
				AreaSize = 15,
				BombCount = 30,
				LevelName = "Level 2"
			},
			new MinesLevel
			{
				AreaSize = 20,
				BombCount = 50,
				LevelName = "Level 3"
			},
			new MinesLevel
			{
				AreaSize = 22,
				BombCount = 100,
				LevelName = "Level 4"
			},
			new MinesLevel
			{
				AreaSize = 22,
				BombCount = 150,
				LevelName = "Level 5"
			}
		};

		public MinesLevel SelectedLevel
		{
			get => m_SelectedLevel;
			set
			{
				m_SelectedLevel = value;
				CreateNewSence();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedLevel)));
			}
		}

		public MinesMap()
		{
			m_SelectedLevel = Scenes.First();
			CreateNewSence();
		}

		private void CreateNewSence()
		{
			GenerateMap();
			RandomPutBomb();
			ScanNearBombs();

			ResetTimer();
			m_IsGameOver = false;
			m_IsGameStarting = false;
			GameStatus = MinesStatus.None;
		}

		public void ResetGame()
		{
			foreach (var area in Areas.SelectMany(a => a))
			{
				area.AreaClicked -= OnAreaClicked;
				area.AreaTagged -= OnAreaTagged;
			}

			CreateNewSence();
		}

		private void CheckWinTheGame()
		{
			var TaggedAreas = Areas.SelectMany(a => a).Where(area => area.IsTagged).ToArray();

			if (TaggedAreas.Length == SelectedLevel.BombCount && !TaggedAreas.Any(area => !area.HasBomb))
			{
				StopTimer();
				m_IsGameOver = true;
				GameStatus = MinesStatus.PlayerWin;
				GameOverMessage = "完成掃雷";
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
			area.AreaTagged += OnAreaTagged;

			return area;
		}

		private void DestroyArea(Area area)
		{
			area.AreaClicked -= OnAreaClicked;
		}

		private void FindAllBombs()
		{
			foreach (var area in Areas.SelectMany(array => array))
			{
				if (area.HasBomb && !area.IsTagged && area.Status != AreaStatus.Boom)
				{
					area.Status = AreaStatus.Bomb;
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
			if (bottomY > SelectedLevel.AreaSize - 1)
				bottomY = SelectedLevel.AreaSize - 1;

			var rightX = centerArea.X + 1;
			if (rightX > SelectedLevel.AreaSize - 1)
				rightX = SelectedLevel.AreaSize - 1;

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

		private void GenerateMap()
		{
			Areas = Enumerable.Range(0, SelectedLevel.AreaSize)
				.Select(y => Enumerable.Range(0, SelectedLevel.AreaSize)
					.Select(x => CreateArea(x, y))
					.ToArray())
				.ToArray();
		}

		private void OnAreaClicked(object sender, AreaArgs args)
		{
			if (m_IsGameOver)
				return;

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
				m_IsGameOver = true;
				GameStatus = MinesStatus.PlayerLose;
				GameOverMessage = "失敗了";
				FindAllBombs();
				StopTimer();

				return;
			}
			else
				targetArea.Status = AreaStatus.SteppedOn;

			if (targetArea.NearBombCount > 0)
				return;

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
			if (m_IsGameOver)
				return;

			var targetArea = Areas[args.Y][args.X];

			if (targetArea.IsSteppedOn)
				return;

			targetArea.IsTagged = !targetArea.IsTagged;

			CheckWinTheGame();
		}

		private void RandomPutBomb()
		{
			var random = RandomNumberGenerator.Create();

			char RandomNumber()
			{
				var data = new byte[2];

				random.GetBytes(data);

				return BitConverter.ToChar(data, 0);
			}

			var randomTopAreaByCount = (from row in m_Areas
										from area in row
										let o = RandomNumber()
										let t = (Index: o, Area: area)
										orderby t.Index
										select t.Area).Take(SelectedLevel.BombCount);

			foreach (var area in randomTopAreaByCount)
				area.HasBomb = true;
		}

		private void ResetTimer()
		{
			m_TimerProcess?.Dispose();

			m_TimerProcess = null;
			GameTime = new TimeSpan();
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
				if (bottomY > SelectedLevel.AreaSize - 1)
					bottomY = SelectedLevel.AreaSize - 1;

				var rightX = area.X + 1;
				if (rightX > SelectedLevel.AreaSize - 1)
					rightX = SelectedLevel.AreaSize - 1;

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
			m_TimerProcess = Observable.Interval(TimeSpan.FromMilliseconds(100))
				.Subscribe(n =>
				{
					var currentTime = DateTime.Now;

					GameTime = currentTime - startTime;
				});
		}

		private void StopTimer()
		{
			m_TimerProcess?.Dispose();
			m_TimerProcess = null;
		}
	}
}
