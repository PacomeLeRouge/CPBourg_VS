using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Settings / Operator Preferences screen.
    ///
    /// Demonstrates the three reference states as an actual interactive
    /// stub rather than three static screens:
    ///   - Default:          both banners hidden
    ///   - Unsaved changes:  any row's Change/Calibrate button was clicked,
    ///                       or a Change-value dialog was confirmed
    ///   - Preferences saved: Apply was clicked after a change
    ///
    /// Language, Units, Keyboard Layout, Mouse Cursor, and Font Size use the
    /// reusable option picker. Date &amp; Time uses a date/time editor and
    /// Screen Calibration uses a four-point touch workflow.
    ///
    /// PENDING vs APPLIED: each picker-backed setting has a
    /// "pending" value (shown in the row as soon as you pick it in the
    /// dialog, so you can see what you're about to apply) and an "applied"
    /// value (what actually takes effect elsewhere in the app, including
    /// language, measurement units, the display clock, and font size). Apply
    /// commits pending -> applied;
    /// Cancel discards pending changes back to the last applied value. This
    /// is what makes Cancel actually mean something, and what keeps the
    /// header from updating before Apply is clicked.
    ///
    /// Preferences are saved under the current user's LocalAppData folder and
    /// reapplied when the application starts.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        /// <summary>
        /// Raised when the APPLIED language changes (i.e. after clicking
        /// Apply, not just after picking a language in the dialog), carrying
        /// the new two-letter abbreviation (e.g. "FR"). MainWindow listens
        /// to this to keep the header's language indicator in sync.
        /// </summary>
        public event EventHandler<string> LanguageChanged;
        public event EventHandler<string> UiLanguageChanged;
        public event EventHandler<MeasurementUnit> UnitsChanged;
        public event EventHandler<TimeSpan> DateTimeOffsetChanged;
        public event EventHandler<string> FontSizeChanged;
        public event EventHandler<string> KeyboardLayoutChanged;
        public event EventHandler<bool> MouseCursorChanged;

        // Applied = currently in effect. Pending = selected but not yet
        // applied; this is what the rows display, so you can see your
        // selection immediately, even before clicking Apply.
        private string _appliedLanguage;
        private string _pendingLanguage;

        private string _appliedUnits;
        private string _pendingUnits;

        private string _appliedKeyboardLayout;
        private string _pendingKeyboardLayout;

        private string _appliedMouseCursor;
        private string _pendingMouseCursor;

        private TimeSpan _appliedDateTimeOffset = TimeSpan.Zero;
        private TimeSpan _pendingDateTimeOffset = TimeSpan.Zero;

        private string _appliedFontSize;
        private string _pendingFontSize;

        private bool _appliedScreenCalibrated;
        private bool _pendingScreenCalibrated;
        private double _appliedCalibrationErrorPixels;
        private double _pendingCalibrationErrorPixels;

        // Which row's dialog is currently open, so OnChangeDialogConfirmed
        // knows which field to update.
        private string _pendingSettingTag;

        // Keeps the Date & Time row showing the real current time, same as
        // the header clock, rather than a fixed sample value.
        private readonly DispatcherTimer _clockTimer;
        private readonly OperatorPreferencesStore _preferencesStore;

        public SettingsView()
            : this(new OperatorPreferencesStore())
        {
        }

        internal SettingsView(OperatorPreferencesStore preferencesStore)
        {
            InitializeComponent();

            _preferencesStore = preferencesStore ?? new OperatorPreferencesStore();
            ApplyPreferencesToState(_preferencesStore.Load());
            LocalizationManager.SetLanguage(_appliedLanguage);

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => RefreshRows();
            _clockTimer.Start();

            RefreshRows();
            Dispatcher.BeginInvoke(new Action(() => LocalizationManager.Apply(this)));
        }

        private void RefreshRows()
        {
            string currentDateTime = (DateTime.Now + _pendingDateTimeOffset).ToString("yyyy-MM-dd HH:mm");

            LanguageRegionItems.ItemsSource = new List<SettingsItemInfo>
            {
                new SettingsItemInfo("\uE774", T("Language"), _pendingLanguage, T("Change"), "Language"),
                new SettingsItemInfo("\uE9D9", T("Units"), T(_pendingUnits), T("Change"), "Units"),
                new SettingsItemInfo("\uE787", T("Date and Time"), currentDateTime, T("Change"), "DateTime"),
            };

            DisplayItems.ItemsSource = new List<SettingsItemInfo>
            {
                new SettingsItemInfo("\uE765", T("Keyboard Layout"), _pendingKeyboardLayout, T("Change"), "KeyboardLayout"),
                new SettingsItemInfo("\uE962", T("Mouse Cursor"), T(_pendingMouseCursor), T("Change"), "MouseCursor"),
                new SettingsItemInfo("Aa", T("Font Size"), T(_pendingFontSize), T("Change"), "FontSize", isLetterIcon: true),
                new SettingsItemInfo("\uE946", T("Screen Calibration"), CalibrationDisplay(),
                    T("Calibrate"), "ScreenCalibration"),
            };

            if (_appliedFontSize != "Medium")
            {
                Dispatcher.BeginInvoke(new Action(() => FontSizeManager.Apply(this, _appliedFontSize)));
            }
        }

        private void OnRowActionClick(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement)?.Tag as string ?? string.Empty;

            switch (tag)
            {
                case "Language":
                    OpenLanguageDialog();
                    break;
                case "Units":
                    OpenUnitsDialog();
                    break;
                case "KeyboardLayout":
                    OpenKeyboardLayoutDialog();
                    break;
                case "MouseCursor":
                    OpenMouseCursorDialog();
                    break;
                case "DateTime":
                    DateTimeDialog.Open(DateTime.Now + _pendingDateTimeOffset);
                    break;
                case "FontSize":
                    OpenFontSizeDialog();
                    break;
                case "ScreenCalibration":
                    ScreenCalibrationDialog.Open();
                    break;
            }
        }

        private void OpenLanguageDialog()
        {
            _pendingSettingTag = "Language";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("English", "English", _pendingLanguage == "English", "\ud83c\uddec\ud83c\udde7"),
                new ChangeOptionInfo("Fran\u00e7ais", "Fran\u00e7ais", _pendingLanguage == "Fran\u00e7ais", "\ud83c\uddeb\ud83c\uddf7"),
                new ChangeOptionInfo("Nederlands", "Nederlands", _pendingLanguage == "Nederlands", "\ud83c\uddf3\ud83c\uddf1"),
                new ChangeOptionInfo("Deutsch", "Deutsch", _pendingLanguage == "Deutsch", "\ud83c\udde9\ud83c\uddea"),
                new ChangeOptionInfo("Espa\u00f1ol", "Espa\u00f1ol", _pendingLanguage == "Espa\u00f1ol", "\ud83c\uddea\ud83c\uddf8"),
                new ChangeOptionInfo("Italiano", "Italiano", _pendingLanguage == "Italiano", "\ud83c\uddee\ud83c\uddf9"),
            };
            ChangeDialog.Open(T("Change Language"), _pendingLanguage, options);
        }

        private void OpenUnitsDialog()
        {
            _pendingSettingTag = "Units";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("Millimeters", T("Metric (millimeters)"), _pendingUnits == "Millimeters"),
                new ChangeOptionInfo("Inches", T("Imperial (inches)"), _pendingUnits == "Inches"),
            };
            ChangeDialog.Open(T("Change Units"), T(_pendingUnits), options);
        }

        private void OpenKeyboardLayoutDialog()
        {
            _pendingSettingTag = "KeyboardLayout";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("AZERTY", "AZERTY", _pendingKeyboardLayout == "AZERTY"),
                new ChangeOptionInfo("QWERTY", "QWERTY", _pendingKeyboardLayout == "QWERTY"),
                new ChangeOptionInfo("QWERTZ", "QWERTZ", _pendingKeyboardLayout == "QWERTZ"),
            };
            ChangeDialog.Open(T("Change Keyboard Layout"), _pendingKeyboardLayout, options);
        }

        private void OpenMouseCursorDialog()
        {
            _pendingSettingTag = "MouseCursor";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("Disabled", T("Disabled"), _pendingMouseCursor == "Disabled"),
                new ChangeOptionInfo("Enabled", T("Enabled"), _pendingMouseCursor == "Enabled"),
            };
            ChangeDialog.Open(T("Change Mouse Cursor"), T(_pendingMouseCursor), options);
        }

        private void OpenFontSizeDialog()
        {
            _pendingSettingTag = "FontSize";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("Small", T("Small"), _pendingFontSize == "Small"),
                new ChangeOptionInfo("Medium", T("Medium (recommended)"), _pendingFontSize == "Medium"),
                new ChangeOptionInfo("Large", T("Large"), _pendingFontSize == "Large"),
            };
            ChangeDialog.Open(T("Change Font Size"), T(_pendingFontSize), options);
        }

        private void OnDateTimeConfirmed(object sender, DateTime selectedDateTime)
        {
            _pendingDateTimeOffset = selectedDateTime - DateTime.Now;
            RefreshRows();
            ShowBanner(unsaved: true, saved: false);
        }

        private void OnChangeDialogConfirmed(object sender, string selectedValue)
        {
            // Only updates the PENDING value. Nothing takes effect (and the
            // header does not change) until Apply is clicked - see OnApplyClick.
            switch (_pendingSettingTag)
            {
                case "Language": _pendingLanguage = selectedValue; break;
                case "Units": _pendingUnits = selectedValue; break;
                case "KeyboardLayout": _pendingKeyboardLayout = selectedValue; break;
                case "MouseCursor": _pendingMouseCursor = selectedValue; break;
                case "FontSize": _pendingFontSize = selectedValue; break;
            }

            RefreshRows();
            ShowBanner(unsaved: true, saved: false);
        }

        private void OnScreenCalibrationConfirmed(object sender, ScreenCalibrationResult result)
        {
            _pendingScreenCalibrated = true;
            _pendingCalibrationErrorPixels = result.AverageErrorPixels;
            RefreshRows();
            ShowBanner(unsaved: true, saved: false);
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            bool languageChanged = _pendingLanguage != _appliedLanguage;
            bool unitsChanged = _pendingUnits != _appliedUnits;
            bool dateTimeChanged = _pendingDateTimeOffset != _appliedDateTimeOffset;
            bool fontSizeChanged = _pendingFontSize != _appliedFontSize;
            bool keyboardChanged = _pendingKeyboardLayout != _appliedKeyboardLayout;
            bool cursorChanged = _pendingMouseCursor != _appliedMouseCursor;

            string errorMessage;
            OperatorPreferences pendingPreferences = CreatePendingPreferences();
            if (!_preferencesStore.TrySave(pendingPreferences, out errorMessage))
            {
                UnsavedDetailText.Text = errorMessage;
                ShowBanner(unsaved: true, saved: false);
                return;
            }

            // Commit pending -> applied only after durable storage succeeds.
            ApplyPreferencesToState(pendingPreferences);

            if (languageChanged)
            {
                LocalizationManager.SetLanguage(_appliedLanguage);
                UiLanguageChanged?.Invoke(this, _appliedLanguage);
                LanguageChanged?.Invoke(this, LocalizationManager.GetAbbreviation(_appliedLanguage));
                RefreshRows();
                LocalizationManager.Apply(this);
            }
            if (unitsChanged)
            {
                UnitsChanged?.Invoke(this, _appliedUnits == "Inches"
                    ? MeasurementUnit.Inches
                    : MeasurementUnit.Millimeters);
            }
            if (dateTimeChanged)
            {
                DateTimeOffsetChanged?.Invoke(this, _appliedDateTimeOffset);
            }
            if (fontSizeChanged)
            {
                FontSizeChanged?.Invoke(this, _appliedFontSize);
            }
            if (keyboardChanged)
            {
                KeyboardLayoutChanged?.Invoke(this, _appliedKeyboardLayout);
            }
            if (cursorChanged)
            {
                MouseCursorChanged?.Invoke(this, _appliedMouseCursor == "Enabled");
            }

            UnsavedDetailText.Text = T("Please apply changes before exiting.");
            ShowBanner(unsaved: false, saved: true);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            // Discard pending changes - revert back to the last applied values.
            _pendingLanguage = _appliedLanguage;
            _pendingUnits = _appliedUnits;
            _pendingKeyboardLayout = _appliedKeyboardLayout;
            _pendingMouseCursor = _appliedMouseCursor;
            _pendingDateTimeOffset = _appliedDateTimeOffset;
            _pendingFontSize = _appliedFontSize;
            _pendingScreenCalibrated = _appliedScreenCalibrated;
            _pendingCalibrationErrorPixels = _appliedCalibrationErrorPixels;

            RefreshRows();
            ShowBanner(unsaved: false, saved: false);
        }

        private void OnBannerCloseClick(object sender, RoutedEventArgs e)
        {
            ShowBanner(unsaved: false, saved: false);
        }

        private void ShowBanner(bool unsaved, bool saved)
        {
            UnsavedBanner.Visibility = unsaved ? Visibility.Visible : Visibility.Collapsed;
            SavedBanner.Visibility = saved ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Applies preferences loaded before MainWindow subscribed to
        /// this view's events.</summary>
        public void ApplyStoredPreferences()
        {
            LocalizationManager.SetLanguage(_appliedLanguage);
            UiLanguageChanged?.Invoke(this, _appliedLanguage);
            LanguageChanged?.Invoke(this, LocalizationManager.GetAbbreviation(_appliedLanguage));
            UnitsChanged?.Invoke(this, _appliedUnits == "Inches"
                ? MeasurementUnit.Inches : MeasurementUnit.Millimeters);
            DateTimeOffsetChanged?.Invoke(this, _appliedDateTimeOffset);
            FontSizeChanged?.Invoke(this, _appliedFontSize);
            KeyboardLayoutChanged?.Invoke(this, _appliedKeyboardLayout);
            MouseCursorChanged?.Invoke(this, _appliedMouseCursor == "Enabled");
        }

        private void ApplyPreferencesToState(OperatorPreferences preferences)
        {
            _appliedLanguage = preferences.Language;
            _pendingLanguage = preferences.Language;
            _appliedUnits = preferences.Units;
            _pendingUnits = preferences.Units;
            _appliedKeyboardLayout = preferences.KeyboardLayout;
            _pendingKeyboardLayout = preferences.KeyboardLayout;
            _appliedMouseCursor = preferences.MouseCursor;
            _pendingMouseCursor = preferences.MouseCursor;
            _appliedDateTimeOffset = SafeDateTimeOffset(preferences.DateTimeOffsetTicks);
            _pendingDateTimeOffset = _appliedDateTimeOffset;
            _appliedFontSize = preferences.FontSize;
            _pendingFontSize = preferences.FontSize;
            _appliedScreenCalibrated = preferences.ScreenCalibrated;
            _pendingScreenCalibrated = preferences.ScreenCalibrated;
            double calibrationError = preferences.CalibrationErrorPixels;
            if (double.IsNaN(calibrationError) || double.IsInfinity(calibrationError) ||
                calibrationError < 0)
            {
                calibrationError = 0;
            }
            _appliedCalibrationErrorPixels = calibrationError;
            _pendingCalibrationErrorPixels = calibrationError;
        }

        private OperatorPreferences CreatePendingPreferences()
        {
            return new OperatorPreferences
            {
                Language = _pendingLanguage,
                Units = _pendingUnits,
                KeyboardLayout = _pendingKeyboardLayout,
                MouseCursor = _pendingMouseCursor,
                DateTimeOffsetTicks = _pendingDateTimeOffset.Ticks,
                FontSize = _pendingFontSize,
                ScreenCalibrated = _pendingScreenCalibrated,
                CalibrationErrorPixels = _pendingCalibrationErrorPixels,
            };
        }

        private string CalibrationDisplay()
        {
            if (!_pendingScreenCalibrated)
            {
                return T("Not calibrated");
            }

            return T("Calibrated") + " (" +
                   _pendingCalibrationErrorPixels.ToString("0.0") + " px)";
        }

        private static string T(string source) => LocalizationManager.Translate(source);

        private static TimeSpan SafeDateTimeOffset(long ticks)
        {
            try
            {
                TimeSpan offset = TimeSpan.FromTicks(ticks);
                DateTime.Now.Add(offset);
                return offset;
            }
            catch (ArgumentOutOfRangeException)
            {
                return TimeSpan.Zero;
            }
        }
    }
}
