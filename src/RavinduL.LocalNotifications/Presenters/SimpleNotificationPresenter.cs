namespace RavinduL.LocalNotifications.Presenters
{
	using System;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Controls;
	using Windows.UI.Xaml.Input;
	using Windows.UI.Xaml.Media;
	using Windows.UI.Xaml.Media.Animation;

	public sealed class SimpleNotificationPresenter : LocalNotificationPresenter
	{
		#region Dependency properties
		public string Glyph
		{
			get { return (string)GetValue(GlyphProperty); }
			set { SetValue(GlyphProperty, value); }
		}

		public static readonly DependencyProperty GlyphProperty =
			DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(SimpleNotificationPresenter), new PropertyMetadata(""));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(nameof(Text), typeof(string), typeof(SimpleNotificationPresenter), new PropertyMetadata(""));

		public Action Action { get; set; }
		#endregion

		private DoubleAnimation ShowAnimation;
		private DoubleAnimation HideAnimation;
		private DoubleAnimation RestoreAnimation;

		private Storyboard ShowStoryboard;
		private Storyboard HideStoryboard;
		private Storyboard RestoreStoryboard;

		private Grid LayoutRoot;
		private Grid Target;
		private Button HideButton;
		private Grid TitleBarBackgroundGrid;

		private TranslateTransform translation
		{
			get { return LayoutRoot.RenderTransform as TranslateTransform; }
			set { LayoutRoot.RenderTransform = value; }
		}

		protected override void OnStateChanged(LocalNotificationState newState, LocalNotificationState previousState)
		{
			if (LayoutRoot != null)
			{
				LayoutRoot.ManipulationMode = (newState == LocalNotificationState.Shown ? ManipulationModes.TranslateY : ManipulationModes.None);
			}
		}
		
		public SimpleNotificationPresenter(TimeSpan duration) : base(duration)
		{
			DefaultStyleKey = typeof(SimpleNotificationPresenter);

			Duration = duration;
		}

		protected override void OnLoaded(object sender, RoutedEventArgs e)
		{
			ShowAnimation.From = HideAnimation.To = -LayoutRoot.ActualHeight;
		}

		protected override void OnApplyTemplate()
		{
			LayoutRoot = (Grid)GetTemplateChild(nameof(LayoutRoot));
			Target = (Grid)GetTemplateChild(nameof(Target));
			HideButton = (Button)GetTemplateChild(nameof(HideButton));
			TitleBarBackgroundGrid = (Grid)GetTemplateChild(nameof(TitleBarBackgroundGrid));

			ShowStoryboard = (Storyboard)GetTemplateChild(nameof(ShowStoryboard));
			HideStoryboard = (Storyboard)GetTemplateChild(nameof(HideStoryboard));
			RestoreStoryboard = (Storyboard)GetTemplateChild(nameof(RestoreStoryboard));

			ShowAnimation = (DoubleAnimation)GetTemplateChild(nameof(ShowAnimation));
			HideAnimation = (DoubleAnimation)GetTemplateChild(nameof(HideAnimation));
			RestoreAnimation = (DoubleAnimation)GetTemplateChild(nameof(RestoreAnimation));

			ShowStoryboard.Completed += (sender, e) => State = LocalNotificationState.Shown;
			HideStoryboard.Completed += (sender, e) => State = LocalNotificationState.Hidden;
			RestoreStoryboard.Completed += (sender, e) => State = LocalNotificationState.Shown;

			LayoutRoot.ManipulationDelta += LayoutRoot_ManipulationDelta;
			LayoutRoot.ManipulationCompleted += LayoutRoot_ManipulationCompleted;

			Target.Tapped += Target_Tapped;

			HideButton.Click += HideButton_Click;
		}

		private void HideButton_Click(object sender, RoutedEventArgs e)
		{
			HideButton.Click -= HideButton_Click;

			Hide();
		}

		private void Target_Tapped(object sender, TappedRoutedEventArgs e)
		{
			Target.Tapped -= Target_Tapped;

			Action?.Invoke();
			Hide();
		}

		private void LayoutRoot_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			if (e.Cumulative.Translation.Y < -40)
			{
				Hide();
			}
			else
			{
				Restore();
			}
		}

		private void LayoutRoot_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			translation.Y += e.Delta.Translation.Y;

			if (translation.Y > 20)
			{
				translation.Y = 20;
			}
		}

		protected override void OnShowing()
		{
			ExecuteAfterLoading(ShowStoryboard.Begin);
		}

		protected override void OnHiding()
		{
			HideAnimation.From = translation.Y;
			HideStoryboard.Begin();
		}

		protected override void OnRestoring()
		{
			RestoreAnimation.From = translation.Y;
			RestoreStoryboard.Begin();
		}
	}
}
