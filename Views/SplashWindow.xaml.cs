using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CPBourg.NextGenGui.Startup;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// The splash view. It is deliberately "dumb": it renders whatever
    /// <see cref="StartupProgress"/> it is handed and knows nothing about how
    /// the boot sequence works. All timing and backend logic lives in the
    /// Startup namespace.
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            LogoLoader.Apply(LogoImage, LogoPlaceholder);
        }

        /// <summary>Set the version / build labels shown in the footer.</summary>
        public void SetBuildInfo(string version, string build)
        {
            VersionText.Text = "Version: " + version;
            BuildText.Text = "Build: " + build;
        }

        /// <summary>
        /// Apply a progress snapshot. Marshals to the UI thread so it is safe
        /// to call from an <see cref="IProgress{T}"/> continuation.
        /// </summary>
        public void Apply(StartupProgress progress)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => Apply(progress)));
                return;
            }

            StatusText.Text = progress.Message;

            if (progress.IsError)
            {
                var error = (Brush)FindResource("StatusErrorBrush");
                StatusText.Foreground = error;
                ProgressFill.Background = error;
            }
            else
            {
                StatusText.Foreground = (Brush)FindResource("TextSecondaryBrush");
                ProgressFill.Background = (Brush)FindResource("ProgressFillBrush");
            }

            AnimateFillTo(progress.Percent);
        }

        private void AnimateFillTo(int percent)
        {
            // Width = fraction of the track's actual width.
            var track = (FrameworkElement)ProgressFill.Parent;
            double target = track.ActualWidth * (percent / 100.0);

            var animation = new DoubleAnimation
            {
                To = target,
                Duration = TimeSpan.FromMilliseconds(280),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ProgressFill.BeginAnimation(WidthProperty, animation);
        }
    }
}
