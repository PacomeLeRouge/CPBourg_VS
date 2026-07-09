using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Jobs / File Menu screen (FR-08, AC-06). Selecting a job in the list
    /// updates the Summary panel; the six action tiles open real dialogs
    /// (Add Comment, Open Job, Save As New Job, Remove Job) that mutate the
    /// in-memory sample job list - see the reference mock's job-action
    /// screens. View Log and Scan Barcode ID don't have a dialog mockup yet
    /// and remain simple stub feedback, same pattern as before.
    ///
    /// "Current Setup" (Format / Machine Line) shown in the Save As New Job
    /// and Remove Job dialogs represents the live machine's current running
    /// configuration, not the archived job's own stored format - this
    /// matches the reference mock, which shows the same fixed values in
    /// both dialogs regardless of which job is selected. It's a hard-coded
    /// stand-in until the WFM connection provides real live machine state
    /// (FR-01, FR-02).
    ///
    /// Sample data is hard-coded here. Replace with real WFM/job-storage
    /// data once that's wired in, keeping the same JobRecord/ListBox
    /// binding structure.
    /// </summary>
    public partial class JobsView : UserControl
    {
        // Stand-in for the live machine's current configuration - see the
        // class-level comment above for why this isn't per-job data.
        private const string CurrentSetupFormat = "A4 Booklet";
        private const string CurrentSetupMachineLine = "BSF + BSE";

        private List<JobRecord> _allJobs;

        public JobsView()
        {
            InitializeComponent();
            LoadSampleJobs();
        }

        private void LoadSampleJobs()
        {
            _allJobs = new List<JobRecord>
            {
                new JobRecord("Spring Catalog 2026", 48, "A4 Booklet", "2026-06-21", "14:37",
                    "Ready for reprint", "BC-10458-22", "2026-06-21 14:37"),
                new JobRecord("Booklet Batch A", 32, "Letter Booklet", "2026-06-20", "09:12",
                    "-", "BC-10457-11", "2026-06-20 09:12"),
                new JobRecord("Training Manual Rev 3", 120, "A4", "2026-06-19", "16:05",
                    "-", "BC-10455-08", "2026-06-19 16:05"),
                new JobRecord("Promo Cards 5x7", 2, "5x7", "2026-06-18", "11:28",
                    "-", "BC-10450-02", "2026-06-18 11:28"),
                new JobRecord("Service Guide 2026", 64, "A5 Booklet", "2026-06-17", "08:44",
                    "-", "BC-10448-19", "2026-06-17 08:44"),
            };

            RefreshJobsList(selectIndex: 0);
        }

        /// <summary>Rebinds the list and selects the given index (or the last
        /// item if the index is now out of range, e.g. after a removal).</summary>
        private void RefreshJobsList(int selectIndex)
        {
            JobsListBox.ItemsSource = null;
            JobsListBox.ItemsSource = _allJobs;

            if (_allJobs.Count == 0)
            {
                ClearSummary();
                return;
            }

            int index = Math.Min(selectIndex, _allJobs.Count - 1);
            JobsListBox.SelectedIndex = index;
        }

        private JobRecord SelectedJob => JobsListBox.SelectedItem as JobRecord;

        private void ClearSummary()
        {
            SummaryNameText.Text = "-";
            SummaryPagesText.Text = "-";
            SummaryFormatText.Text = "-";
            SummaryCommentText.Text = "-";
            SummaryBarcodeText.Text = "-";
            SummaryLastModifiedText.Text = "-";
            StatusMessageText.Text = "No job selected.";
        }

        private void OnJobSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var job = SelectedJob;
            if (job == null)
            {
                ClearSummary();
                return;
            }

            SummaryNameText.Text = job.Name;
            SummaryPagesText.Text = job.Pages.ToString();
            SummaryFormatText.Text = job.Format;
            SummaryCommentText.Text = job.Comment;
            SummaryBarcodeText.Text = job.BarcodeId;
            SummaryLastModifiedText.Text = job.LastModified;

            StatusMessageText.Text = "Selected job is ready to open.";
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            string query = SearchBox.Text?.Trim() ?? string.Empty;

            JobsListBox.ItemsSource = string.IsNullOrEmpty(query)
                ? _allJobs
                : _allJobs.Where(j => j.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (JobsListBox.Items.Count > 0)
            {
                JobsListBox.SelectedIndex = 0;
            }
            else
            {
                ClearSummary();
            }
        }

        private void ShowStub(string actionName)
        {
            LastActionText.Text = "Last action: " + actionName +
                " (stub - not yet connected to job storage)";
        }

        private void OnFilterClick(object sender, RoutedEventArgs e) => ShowStub("Filter");
        private void OnViewLogClick(object sender, RoutedEventArgs e) => ShowStub("View log");
        private void OnScanBarcodeClick(object sender, RoutedEventArgs e) => ShowStub("Scan barcode ID");

        // ===================== Open Job =====================

        private void OnOpenJobClick(object sender, RoutedEventArgs e)
        {
            var job = SelectedJob;
            if (job == null) return;
            OpenJobDialogControl.Open(job.Name, job.Format);
        }

        private void OnJobOpened(object sender, bool loadRunAdjustments)
        {
            var job = SelectedJob;
            string suffix = loadRunAdjustments ? " with saved RUN adjustments" : string.Empty;
            LastActionText.Text = "Last action: Open job \"" + (job?.Name ?? "-") + "\"" + suffix +
                " (stub - not yet connected to the WFM)";
        }

        // ===================== Add Comment =====================

        private void OnAddCommentClick(object sender, RoutedEventArgs e)
        {
            var job = SelectedJob;
            if (job == null) return;
            AddCommentDialogControl.Open(job.Name, job.Comment);
        }

        private void OnCommentSaved(object sender, string newComment)
        {
            var job = SelectedJob;
            if (job == null) return;

            job.Comment = string.IsNullOrWhiteSpace(newComment) ? "-" : newComment.Trim();
            SummaryCommentText.Text = job.Comment;

            ConfirmationDialogControl.Open("Comment Saved!",
                "The comment for \u201c" + job.Name + "\u201d has been successfully saved.");
        }

        // ===================== Save As New Job =====================

        private void OnSaveAsNewClick(object sender, RoutedEventArgs e)
        {
            var job = SelectedJob;
            string suggestedName = job != null ? job.Name + " - Variant 1" : "New Job";
            SaveAsNewJobDialogControl.Open(suggestedName, CurrentSetupFormat, CurrentSetupMachineLine);
        }

        private void OnSaveRequested(object sender, string jobName)
        {
            bool exists = _allJobs.Any(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                // Re-open the same dialog in its conflict state rather than
                // closing - SaveAsNewJobDialog already collapsed itself when
                // it raised this event, so show it again.
                SaveAsNewJobDialogControl.Open(jobName, CurrentSetupFormat, CurrentSetupMachineLine);
                SaveAsNewJobDialogControl.ShowConflict();
                return;
            }

            SaveNewJob(jobName);
        }

        private void OnOverwriteConfirmed(object sender, string jobName)
        {
            var existing = _allJobs.FirstOrDefault(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _allJobs.Remove(existing);
            }

            SaveNewJob(jobName);
        }

        private void SaveNewJob(string jobName)
        {
            var now = DateTime.Now;
            var newJob = new JobRecord(jobName, 0, CurrentSetupFormat,
                now.ToString("yyyy-MM-dd"), now.ToString("HH:mm"),
                "-", "BC-" + now.ToString("yyyyMMddHHmm"), now.ToString("yyyy-MM-dd HH:mm"));

            _allJobs.Insert(0, newJob);
            RefreshJobsList(selectIndex: 0);

            ConfirmationDialogControl.Open("New Job Saved!",
                "The new job \u201c" + jobName + "\u201d has been successfully saved.");
        }

        // ===================== Remove Job =====================

        private void OnRemoveJobClick(object sender, RoutedEventArgs e)
        {
            var job = SelectedJob;
            if (job == null) return;
            RemoveJobDialogControl.Open(job.Name, CurrentSetupFormat, CurrentSetupMachineLine);
        }

        private void OnJobRemoved(object sender, string jobName)
        {
            var job = _allJobs.FirstOrDefault(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase));
            if (job != null)
            {
                _allJobs.Remove(job);
            }

            RefreshJobsList(selectIndex: 0);

            ConfirmationDialogControl.Open("Job Removed!",
                "The job \u201c" + jobName + "\u201d has been successfully removed.");
        }
    }
}
