using System;
using System.Windows;
using System.Windows.Threading;

namespace CPBourg.NextGenGui.Views
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _clockTimer;

        public MainWindow()
        {
            InitializeComponent();

            LogoLoader.Apply(LogoImage, LogoPlaceholder);

            GlobalMenu.CloseRequested += (s, e) => GlobalMenu.Visibility = Visibility.Collapsed;
            GlobalMenu.ItemSelected += OnGlobalMenuItemSelected;

            SettingsScreen.LanguageChanged += (s, abbreviation) => LanguageIndicatorText.Text = abbreviation;

            Dashboard.NavigateToJobsRequested += (s, e) => NavigateTo("Job / File Menu");
            Dashboard.NavigateToErrorsRequested += (s, e) => NavigateTo("Error & Information");

            // Keep the Dashboard's Active Alerts card in sync with the real
            // Errors & Information state - both on every change, and once
            // now for the initial counts (ErrorsScreen already loaded its
            // sample data before this handler was attached).
            ErrorsScreen.MessagesChanged += (s, e) => UpdateDashboardAlertsSummary();
            UpdateDashboardAlertsSummary();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => UpdateClock();
            _clockTimer.Start();
            UpdateClock();
        }

        private void OnMenuToggleClick(object sender, RoutedEventArgs e)
        {
            GlobalMenu.Visibility = GlobalMenu.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void OnBellClick(object sender, RoutedEventArgs e)
        {
            NavigateTo("Error & Information");
        }

        private void OnGlobalMenuItemSelected(object sender, string itemName)
        {
            NavigateTo(itemName);
        }

        /// <summary>
        /// Only Home (Dashboard), Settings / Preferences, Job / File Menu,
        /// and Error &amp; Information exist as real screens today. Other
        /// items are placeholders until their screens are built - they
        /// update the header title only, and leave whichever real screen is
        /// currently showing untouched (so, for example, clicking
        /// "Help / Manual" while on Settings will show the "Help / Manual"
        /// title over the Settings screen - an acceptable prototype
        /// limitation, not a real navigation).
        ///
        /// Shared by both the global menu (<see cref="OnGlobalMenuItemSelected"/>)
        /// and the header bell (<see cref="OnBellClick"/>), since they can
        /// both lead to the same screens.
        /// </summary>
        private void NavigateTo(string itemName)
        {
            if (itemName == "Home")
            {
                Dashboard.Visibility = Visibility.Visible;
                SettingsScreen.Visibility = Visibility.Collapsed;
                JobsScreen.Visibility = Visibility.Collapsed;
                ErrorsScreen.Visibility = Visibility.Collapsed;
                PageTitleText.Text = "Home";
            }
            else if (itemName == "Settings / Preferences")
            {
                Dashboard.Visibility = Visibility.Collapsed;
                SettingsScreen.Visibility = Visibility.Visible;
                JobsScreen.Visibility = Visibility.Collapsed;
                ErrorsScreen.Visibility = Visibility.Collapsed;
                PageTitleText.Text = "Settings";
            }
            else if (itemName == "Job / File Menu")
            {
                Dashboard.Visibility = Visibility.Collapsed;
                SettingsScreen.Visibility = Visibility.Collapsed;
                JobsScreen.Visibility = Visibility.Visible;
                ErrorsScreen.Visibility = Visibility.Collapsed;
                PageTitleText.Text = "Jobs / File Menu";
            }
            else if (itemName == "Error & Information")
            {
                Dashboard.Visibility = Visibility.Collapsed;
                SettingsScreen.Visibility = Visibility.Collapsed;
                JobsScreen.Visibility = Visibility.Collapsed;
                ErrorsScreen.Visibility = Visibility.Visible;
                PageTitleText.Text = "Errors";
            }
            else
            {
                PageTitleText.Text = itemName;
            }
        }

        private void UpdateDashboardAlertsSummary()
        {
            Dashboard.UpdateAlertsSummary(
                ErrorsScreen.CriticalCount, ErrorsScreen.WarningCount,
                ErrorsScreen.InfoCount, ErrorsScreen.TotalCount);
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            ClockText.Text = now.ToString("HH:mm");
            DateText.Text = now.ToString("yyyy-MM-dd");
        }
    }
}
