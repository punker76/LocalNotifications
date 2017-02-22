namespace RavinduL.LocalNotifications
{
	using System;
	using System.Collections.Generic;
	using Windows.UI.Xaml.Controls;

	public class LocalNotificationManager
	{
		private Grid container;

		private LocalNotificationPresenter current;

		Queue<Tuple<LocalNotificationPresenter, LocalNotificationCollisionBehaviour>> queue;

		public LocalNotificationManager(Grid container)
		{
			this.container = container;
			queue = new Queue<Tuple<LocalNotificationPresenter, LocalNotificationCollisionBehaviour>>();
		}

		public void Show(LocalNotificationPresenter presenter, LocalNotificationCollisionBehaviour collisionBehaviour = LocalNotificationCollisionBehaviour.Wait)
		{
			if (current == null)
			{
				current = presenter;

				current.StateChanged += Current_StateChanged;
				current.LayoutUpdated += Current_LayoutUpdated;

				container.Children.Add(current);

				current.UpdateLayout();
			}
			else
			{
				queue.Enqueue(new Tuple<LocalNotificationPresenter, LocalNotificationCollisionBehaviour>(presenter, collisionBehaviour));

				if (collisionBehaviour == LocalNotificationCollisionBehaviour.Replace)
				{
					HideCurrent();
				}
			}
		}

		private void Current_LayoutUpdated(object sender, object e)
		{
			current.LayoutUpdated -= Current_LayoutUpdated;
			current.Show();
		}

		private void Current_StateChanged(object sender, LocalNotificationState state)
		{
			if (state == LocalNotificationState.Hidden)
			{
				current.StateChanged -= Current_StateChanged;

				container.Children.Remove(current);

				current = null;

				if (queue.Count > 0)
				{
					var t = queue.Dequeue();
					Show(t.Item1, t.Item2);
				}
			}
		}

		public void HideCurrent()
		{
			current?.Hide();
		}

		public void HideAll()
		{
			queue.Clear();
			HideCurrent();
		}
	}
}
