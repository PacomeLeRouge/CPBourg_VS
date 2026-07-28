using System;
using System.Windows;
using System.Windows.Threading;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _clockTimer;
        private readonly JobRepository _jobRepository;
        private TimeSpan _operatorClockOffset = TimeSpan.Zero;
        private string _fontSizeSetting = "Medium";

        public MainWindow()
        {
            InitializeComponent();

            _jobRepository = new JobRepository();
            JobsScreen.InitializeRepository(_jobRepository);
            _jobRepository.CurrentJobChanged += (s, e) => SyncCurrentJob();
            JobsScreen.JobLoaded += (s, job) => NavigateTo("Home");
            SyncCurrentJob();

            LogoLoader.Apply(LogoImage, LogoPlaceholder);

            GlobalMenu.CloseRequested += (s, e) => GlobalMenu.Visibility = Visibility.Collapsed;
            GlobalMenu.ItemSelected += OnGlobalMenuItemSelected;

            SettingsScreen.LanguageChanged += (s, abbreviation) => LanguageIndicatorText.Text = abbreviation;
            SettingsScreen.UiLanguageChanged += (s, language) =>
            {
                LocalizationManager.SetLanguage(language);
                LocalizationManager.Apply(this);
                GlobalMenu.ApplyLanguage();
            };
            SettingsScreen.UnitsChanged += (s, unit) => ApplyMeasurementUnit(unit);
            SettingsScreen.DateTimeOffsetChanged += (s, offset) =>
            {
                _operatorClockOffset = offset;
                UpdateClock();
            };
            SettingsScreen.FontSizeChanged += (s, setting) =>
            {
                _fontSizeSetting = setting;
                FontSizeManager.Apply(this, _fontSizeSetting);
            };
            SettingsScreen.KeyboardLayoutChanged += (s, layout) =>
                KeyboardLayoutManager.Apply(layout, this);
            SettingsScreen.MouseCursorChanged += (s, enabled) =>
                Cursor = enabled ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.None;

            Dashboard.NavigateToJobsRequested += (s, e) => NavigateTo("Job / File Menu");
            Dashboard.NavigateToErrorsRequested += (s, e) => NavigateTo("Error & Information");
            Dashboard.NavigateToStfoRequested += (s, e) => NavigateToStfo();
            ErrorsScreen.NavigateHomeRequested += (s, e) => NavigateTo("Home");

            // The STFO wizard drives the shell header title as its step
            // changes, and asks to return to the dashboard on Back-from-first /
            // Finish.
            StfoScreen.TitleChanged += (s, title) =>
                LocalizationManager.SetLocalizedText(PageTitleText, title);
            StfoScreen.CloseRequested += (s, e) => NavigateTo("Home");

            TechnicianScreen.CloseRequested += (s, e) => NavigateTo("Home");

            // Keep the Dashboard's Active Alerts card in sync with the real
            // Errors & Information state - both on every change, and once
            // now for the initial counts (ErrorsScreen already loaded its
            // sample data before this handler was attached).
            ErrorsScreen.MessagesChanged += (s, e) => UpdateDashboardAlertsSummary();
            UpdateDashboardAlertsSummary();

            // Keep the Dashboard's Machines tiles in sync with the machine
            // line: a module shows online there only while it's on the line.
            // Sync once now for the default line, then on every change.
            MachineLineConfigScreen.LineChanged += (s, e) => UpdateDashboardMachines();
            UpdateDashboardMachines();

            // Settings loads durable preferences during construction; emit
            // them now that the shell has subscribed to every preference.
            SettingsScreen.ApplyStoredPreferences();
            LocalizationManager.Apply(this);

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
            if (GlobalMenu.Visibility == Visibility.Visible)
            {
                GlobalMenu.ApplyLanguage();
            }
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
        /// Shared by both the global menu (<see cref="OnGlobalMenuItemSelected"/>)
        /// and the header bell (<see cref="OnBellClick"/>), since they can
        /// both lead to the same screens.
        /// </summary>
        private void NavigateTo(string itemName)
        {
            if (itemName == "Home")
            {
                ShowContentScreen(Dashboard, "Home");
            }
            else if (itemName == "Settings / Preferences")
            {
                ShowContentScreen(SettingsScreen, "Settings");
            }
            else if (itemName == "Job / File Menu")
            {
                ShowContentScreen(JobsScreen, "Jobs / File Menu");
            }
            else if (itemName == "Error & Information")
            {
                ShowContentScreen(ErrorsScreen, "Errors");
            }
            else if (itemName == "Machine Line Configuration")
            {
                ShowContentScreen(MachineLineConfigScreen, "Machine Line Configuration");
            }
            else if (itemName == "Technician Interface")
            {
                ShowContentScreen(TechnicianScreen, "Technician Interface");
            }
            else if (itemName == "Help / Manual")
            {
                ShowContentScreen(HelpScreen, "Help / Manual");
            }
            else if (itemName == "About / Version")
            {
                ShowContentScreen(AboutVersionScreen, "About / Version");
            }
            else
            {
                LocalizationManager.SetLocalizedText(PageTitleText, itemName);
            }
        }

        /// <summary>
        /// Opens the STFO individual-machine configuration wizard - reached by
        /// tapping the STFO tile on the Home dashboard, not the global menu.
        /// Entry always starts on the first (Menu) step; the page title then
        /// tracks the wizard step via <see cref="StfoConfigurationView.TitleChanged"/>.
        /// </summary>
        private void NavigateToStfo()
        {
            HideContentScreens();
            StfoScreen.Visibility = Visibility.Visible;
            StfoScreen.ResetToStart();
        }

        private void ShowContentScreen(UIElement screen, string title)
        {
            HideContentScreens();
            screen.Visibility = Visibility.Visible;
            LocalizationManager.SetLocalizedText(PageTitleText, title);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FontSizeManager.Apply(screen, _fontSizeSetting);
                LocalizationManager.Apply(screen);
            }));
        }

        private void HideContentScreens()
        {
            Dashboard.Visibility = Visibility.Collapsed;
            SettingsScreen.Visibility = Visibility.Collapsed;
            JobsScreen.Visibility = Visibility.Collapsed;
            ErrorsScreen.Visibility = Visibility.Collapsed;
            MachineLineConfigScreen.Visibility = Visibility.Collapsed;
            StfoScreen.Visibility = Visibility.Collapsed;
            TechnicianScreen.Visibility = Visibility.Collapsed;
            HelpScreen.Visibility = Visibility.Collapsed;
            AboutVersionScreen.Visibility = Visibility.Collapsed;
        }

        private void UpdateDashboardAlertsSummary()
        {
            Dashboard.UpdateAlertsSummary(
                ErrorsScreen.CriticalCount, ErrorsScreen.WarningCount,
                ErrorsScreen.InfoCount, ErrorsScreen.TotalCount);
        }

        private void UpdateDashboardMachines()
        {
            Dashboard.SetOnlineModules(MachineLineConfigScreen.LineModuleTypes);
        }

        private void SyncCurrentJob()
        {
            Dashboard.SetCurrentJob(_jobRepository.CurrentJob);
            StfoScreen.LoadJob(_jobRepository.CurrentJob);
        }

        private void ApplyMeasurementUnit(MeasurementUnit unit)
        {
            Dashboard.SetMeasurementUnit(unit);
            JobsScreen.SetMeasurementUnit(unit);
            StfoScreen.SetMeasurementUnit(unit);
            TechnicianScreen.SetMeasurementUnit(unit);
            MachineLineConfigScreen.SetMeasurementUnit(unit);
        }

        private void UpdateClock()
        {
            var now = DateTime.Now + _operatorClockOffset;
            ClockText.Text = now.ToString("HH:mm");
            DateText.Text = now.ToString("yyyy-MM-dd");
        }
    }
}
