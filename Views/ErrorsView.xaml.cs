using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Errors &amp; Information screen (FR-06, AC-05). Selecting a row opens
    /// <see cref="ErrorDetailDialog"/>; clearing (either the whole list or
    /// one message from the detail overlay) removes it from the in-memory
    /// sample list and refreshes the counts/empty state.
    ///
    /// Sample data is hard-coded here. Replace with real WFM-reported
    /// errors/warnings once that connection exists (FR-01, FR-02) - the
    /// list would then be populated/updated from WFM -> GUI messages
    /// instead (PRD 3.3).
    /// </summary>
    public partial class ErrorsView : UserControl
    {
        /// <summary>Raised whenever the message list changes (load, clear
        /// all, or clear one from the detail overlay). MainWindow.xaml.cs
        /// wires this to DashboardView.UpdateAlertsSummary so the Active
        /// Alerts card reflects the real current state.</summary>
        public event EventHandler MessagesChanged;

        /// <summary>Raised by the bottom-left Home action. MainWindow handles
        /// the request through its shared navigation method.</summary>
        public event EventHandler NavigateHomeRequested;

        /// <summary>Number of active critical sample records.</summary>
        public int CriticalCount { get; private set; }

        /// <summary>Number of active warning sample records.</summary>
        public int WarningCount { get; private set; }

        /// <summary>Number of active informational sample records.</summary>
        public int InfoCount { get; private set; }

        /// <summary>Total records used by the dashboard Start interlock.</summary>
        public int TotalCount { get; private set; }

        private List<ErrorRecord> _allMessages;

        private sealed class ErrorDisplayItem
        {
            public ErrorDisplayItem(ErrorRecord record)
            {
                Record = record;
                SeverityLabel = LocalizationManager.Translate(record.SeverityLabel);
                Source = LocalizationManager.Translate(record.Source);
                ModuleOrJob = record.ModuleOrJob;
                Time = record.Time;
                Details = LocalizationManager.Translate(record.Details);
            }

            public ErrorRecord Record { get; }
            public string SeverityLabel { get; }
            public string Source { get; }
            public string ModuleOrJob { get; }
            public string Time { get; }
            public string Details { get; }
        }

        /// <summary>Creates and displays the seeded in-memory error list.</summary>
        public ErrorsView()
        {
            InitializeComponent();
            LoadSampleMessages();
        }

        /// <summary>Rebuilds translated display records and the detail overlay.</summary>
        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            RefreshMessages();
            ErrorDetailDialogControl.ApplyLanguage();
        }

        private void LoadSampleMessages()
        {
            _allMessages = new List<ErrorRecord>
            {
                new ErrorRecord(ErrorSeverity.Critical, "Machine", "BPM", "14:31",
                    "Cover open on Module 3",
                    "The access cover on BPM Module 3 is open. Verify all covers are fully closed.",
                    "BPM Module 3", "None"),
                new ErrorRecord(ErrorSeverity.Critical, "Machine", "BPM", "14:31",
                    "Cover open on Module 4",
                    "The access cover on BPM Module 4 is open. Verify all covers are fully closed.",
                    "BPM Module 4", "None"),
                new ErrorRecord(ErrorSeverity.Critical, "Machine", "BPM", "14:31",
                    "Cover open on Module 6",
                    "The access cover on BPM Module 6 is open. Verify all covers are fully closed.",
                    "BPM Module 6", "None"),
                new ErrorRecord(ErrorSeverity.Info, "Machine", "BPM", "14:31",
                    "Machine needs setup",
                    "The machine requires initial setup before it can run a job.",
                    "BPM", "None"),
                new ErrorRecord(ErrorSeverity.Info, "Machine", "BPM", "14:31",
                    "Machine needs setup",
                    "The machine requires initial setup before it can run a job.",
                    "BPM", "None"),
            };

            RefreshMessages();
        }

        private void RefreshMessages()
        {
            MessagesListBox.ItemsSource = null;
            MessagesListBox.ItemsSource = _allMessages.Select(record =>
                new ErrorDisplayItem(record)).ToList();

            CriticalCount = _allMessages.Count(m => m.Severity == ErrorSeverity.Critical);
            WarningCount = _allMessages.Count(m => m.Severity == ErrorSeverity.Warning);
            InfoCount = _allMessages.Count(m => m.Severity == ErrorSeverity.Info);
            TotalCount = _allMessages.Count;

            CriticalCountText.Text = CriticalCount.ToString();
            WarningCountText.Text = WarningCount.ToString();
            InfoCountText.Text = InfoCount.ToString();
            ResolvedCountText.Text = _allMessages.Count(m => m.Severity == ErrorSeverity.Resolved).ToString();
            ActiveCountBadgeText.Text = TotalCount.ToString();

            bool hasMessages = TotalCount > 0;
            MessagesPopulatedPanel.Visibility = hasMessages ? Visibility.Visible : Visibility.Collapsed;
            MessagesEmptyPanel.Visibility = hasMessages ? Visibility.Collapsed : Visibility.Visible;

            MessagesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var displayItem = MessagesListBox.SelectedItem as ErrorDisplayItem;
            if (displayItem == null)
            {
                return;
            }

            ErrorDetailDialogControl.Open(displayItem.Record);

            // Clear the ListBox's own selection highlight once the detail
            // overlay is showing, so re-clicking the same row still raises
            // SelectionChanged next time.
            MessagesListBox.SelectedItem = null;
        }

        private void OnDetailCleared(object sender, ErrorRecord record)
        {
            _allMessages.Remove(record);
            RefreshMessages();
        }

        private void OnClearAllClick(object sender, RoutedEventArgs e)
        {
            _allMessages.Clear();
            RefreshMessages();
        }

        private void OnHomeClick(object sender, RoutedEventArgs e)
        {
            NavigateHomeRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
