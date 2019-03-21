using System;
using System.ComponentModel;

namespace Mines
{
	public class Area : INotifyPropertyChanged
	{
		private bool m_HasBomb = false;

		private AreaStatus m_Status = AreaStatus.None;

		private bool m_IsSteppedOn = false;

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<AreaClickedArgs> AreaClicked;

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

		public AreaStatus Status
		{
			get
			{
				return m_Status;
			}
			set
			{
				m_Status = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
			}
		}

		public int X { get; set; }

		public int Y { get; set; }

		public int NearBombCount { get; set; }

		public bool ShowBombCount
		{
			get
			{
				return !HasBomb && NearBombCount > 0;
			}
		}

		public bool IsSteppedOn
		{
			get => m_IsSteppedOn;
			set
			{
				m_IsSteppedOn = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSteppedOn)));
			}
		}

		public void ClickArea()
		{
			if (IsSteppedOn)
				return;

			IsSteppedOn = true;

			if (HasBomb)
				Status = AreaStatus.Boom;
			else
				Status = AreaStatus.SteppedOn;

			AreaClicked?.Invoke(
				this,
				new AreaClickedArgs
				{
					X = X,
					Y = Y
				});
		}

		public void SetSteppedOn()
		{
			Status = AreaStatus.SteppedOn;
			IsSteppedOn = true;
		}

		public void TagArea()
		{
			if (IsSteppedOn)
				return;

			Status = AreaStatus.Flag;
		}
	}
}
