namespace RavinduL.LocalNotifications
{
	using System;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Controls;
	
	public abstract class LocalNotificationPresenter : Control
	{
		protected DispatcherTimer timer;

		public TimeSpan Duration { get; set; }

		public void Show()
		{
			State = LocalNotificationState.Showing;

			OnShowing();

			timer = new DispatcherTimer
			{
				Interval = Duration,
			};

			timer.Tick += Timer_Tick;

			timer.Start();
		}

		private void Timer_Tick(object sender, object e)
		{
			timer.Tick -= Timer_Tick;

			Hide();
		}

		public void Hide()
		{
			State = LocalNotificationState.Hiding;

			timer.Stop();

			OnHiding();
		}

		public void Restore()
		{
			State = LocalNotificationState.Restoring;

			OnRestoring();
		}

		protected abstract void OnShowing();

		protected abstract void OnHiding();

		protected abstract void OnRestoring();

		public LocalNotificationPresenter(TimeSpan duration)
		{
			Duration = duration;

			Loaded += (sender, e) =>
			{
				loaded = true;

				OnLoaded(sender, e);
			};
		}

		private bool loaded;

		private RoutedEventHandler delayedExecute;

		protected void ExecuteAfterLoading(Action action)
		{
			if (loaded)
			{
				action();
			}
			else
			{
				delayedExecute = (sender, e) =>
				{
					Loaded -= delayedExecute;

					action();
				};

				Loaded += delayedExecute;
			}
		}
		
		protected virtual void OnLoaded(object sender, RoutedEventArgs e)
		{
		}
		
		public event EventHandler<LocalNotificationState> StateChanged;

		private LocalNotificationState _State;

		public LocalNotificationState State
		{
			get { return _State; }
			protected set
			{
				if (_State != value)
				{
					var p = _State;
					_State = value;

					OnStateChanged(value, p);

					StateChanged?.Invoke(this, value);
				}
			}
		}

		protected virtual void OnStateChanged(LocalNotificationState newState, LocalNotificationState previousState)
		{
		}
	}
}
