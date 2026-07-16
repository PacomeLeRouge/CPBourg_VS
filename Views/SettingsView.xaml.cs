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
    /// reusable option picker. Date &amp; Time uses a date/time editor. Screen
    /// Calibration remains the only prototype-only placeholder.
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
    /// Preferences are retained for the current application session. Wire
    /// durable preference storage in once the settings model is defined.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        /// <summary>
        /// Raised when the APPLIED language changes (i.e. after clicking
        /// Apply, not just after picking a language in the dialog), carrying
        /// the new two-letter abbreviation (e.g. "FR"). MainWindow listens
        /// to this to keep the header's language indicator in sync. This
        /// does NOT translate the rest of the UI yet - that is future work
        /// (FR-10).
        /// </summary>
        public event EventHandler<string> LanguageChanged;
        public event EventHandler<MeasurementUnit> UnitsChanged;
        public event EventHandler<TimeSpan> DateTimeOffsetChanged;
        public event EventHandler<string> FontSizeChanged;

        // Applied = currently in effect. Pending = selected but not yet
        // applied; this is what the rows display, so you can see your
        // selection immediately, even before clicking Apply.
        private string _appliedLanguage = "English";
        private string _pendingLanguage = "English";

        private string _appliedUnits = "Millimeters";
        private string _pendingUnits = "Millimeters";

        private string _appliedKeyboardLayout = "AZERTY";
        private string _pendingKeyboardLayout = "AZERTY";

        private string _appliedMouseCursor = "Disabled";
        private string _pendingMouseCursor = "Disabled";

        private TimeSpan _appliedDateTimeOffset = TimeSpan.Zero;
        private TimeSpan _pendingDateTimeOffset = TimeSpan.Zero;

        private string _appliedFontSize = "Medium";
        private string _pendingFontSize = "Medium";

        // Which row's dialog is currently open, so OnChangeDialogConfirmed
        // knows which field to update.
        private string _pendingSettingTag;

        // Keeps the Date & Time row showing the real current time, same as
        // the header clock, rather than a fixed sample value.
        private readonly DispatcherTimer _clockTimer;

        public SettingsView()
        {
            InitializeComponent();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => RefreshRows();
            _clockTimer.Start();

            RefreshRows();
        }

        private void RefreshRows()
        {
            string currentDateTime = (DateTime.Now + _pendingDateTimeOffset).ToString("yyyy-MM-dd HH:mm");

            LanguageRegionItems.ItemsSource = new List<SettingsItemInfo>
            {
                new SettingsItemInfo("\uE774", "Language", _pendingLanguage, "Change", "Language"),
                new SettingsItemInfo("\uE9D9", "Units", _pendingUnits, "Change", "Units"),
                new SettingsItemInfo("\uE787", "Date and Time", currentDateTime, "Change", "DateTime"),
            };

            DisplayItems.ItemsSource = new List<SettingsItemInfo>
            {
                new SettingsItemInfo("\uE765", "Keyboard Layout", _pendingKeyboardLayout, "Change", "KeyboardLayout"),
                new SettingsItemInfo("\uE962", "Mouse Cursor", _pendingMouseCursor, "Change", "MouseCursor"),
                new SettingsItemInfo("Aa", "Font Size", _pendingFontSize, "Change", "FontSize", isLetterIcon: true),
                new SettingsItemInfo("\uE946", "Screen Calibration", "Required if touch offset occurs", "Calibrate", "ScreenCalibration"),
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
                default:
                    // Screen Calibration remains a prototype placeholder.
                    ShowBanner(unsaved: true, saved: false);
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
                new ChangeOptionInfo("Italiana", "Italiana", _pendingLanguage == "Italiana", "\ud83c\uddee\ud83c\uddf9"),
            };
            ChangeDialog.Open("Change Language", _pendingLanguage, options);
        }

        private void OpenUnitsDialog()
        {
            _pendingSettingTag = "Units";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("Millimeters", "Metric (millimeters)", _pendingUnits == "Millimeters"),
                new ChangeOptionInfo("Inches", "Imperial (inches)", _pendingUnits == "Inches"),
            };
            ChangeDialog.Open("Change Units", _pendingUnits, options);
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
            ChangeDialog.Open("Change Keyboard Layout", _pendingKeyboardLayout, options);
        }

        private void OpenMouseCursorDialog()
        {
            _pendingSettingTag = "MouseCursor";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("Disabled", "Disabled", _pendingMouseCursor == "Disabled"),
                new ChangeOptionInfo("Enabled", "Enabled", _pendingMouseCursor == "Enabled"),
            };
            ChangeDialog.Open("Change Mouse Cursor", _pendingMouseCursor, options);
        }

        private void OpenFontSizeDialog()
        {
            _pendingSettingTag = "FontSize";
            var options = new List<ChangeOptionInfo>
            {
                new ChangeOptionInfo("Small", "Small", _pendingFontSize == "Small"),
                new ChangeOptionInfo("Medium", "Medium (recommended)", _pendingFontSize == "Medium"),
                new ChangeOptionInfo("Large", "Large", _pendingFontSize == "Large"),
            };
            ChangeDialog.Open("Change Font Size", _pendingFontSize, options);
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

        /// <summary>
        /// Two-letter abbreviation shown in the header for a given language
        /// display name. This only drives that header label - it does not
        /// translate anything else in the UI yet (FR-10 future work).
        /// </summary>
        private static string GetLanguageAbbreviation(string language)
        {
            switch (language)
            {
                case "English": return "EN";
                case "Fran\u00e7ais": return "FR";
                case "Nederlands": return "NL";
                case "Deutsch": return "DE";
                case "Espa\u00f1ol": return "ES";
                case "Italiana": return "IT";
                default: return "EN";
            }
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            bool languageChanged = _pendingLanguage != _appliedLanguage;
            bool unitsChanged = _pendingUnits != _appliedUnits;
            bool dateTimeChanged = _pendingDateTimeOffset != _appliedDateTimeOffset;
            bool fontSizeChanged = _pendingFontSize != _appliedFontSize;

            // Commit pending -> applied for all picker-backed settings.
            _appliedLanguage = _pendingLanguage;
            _appliedUnits = _pendingUnits;
            _appliedKeyboardLayout = _pendingKeyboardLayout;
            _appliedMouseCursor = _pendingMouseCursor;
            _appliedDateTimeOffset = _pendingDateTimeOffset;
            _appliedFontSize = _pendingFontSize;

            if (languageChanged)
            {
                LanguageChanged?.Invoke(this, GetLanguageAbbreviation(_appliedLanguage));
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
    }
}
