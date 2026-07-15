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
    /// shared in-memory job repository - see the reference mock's job-action
    /// screens. View Log and Scan Barcode ID don't have a dialog mockup yet
    /// and remain simple stub feedback, same pattern as before.
    ///
    /// JobRepository is also consumed by the dashboard and STFO, so opening a
    /// job updates all three screens from the same JobRecord instance.
    /// </summary>
    public partial class JobsView : UserControl
    {
        private const string CurrentSetupMachineLine = "BSF + BSE";

        private List<JobRecord> _allJobs = new List<JobRecord>();
        private JobRepository _repository;

        public event EventHandler<JobRecord> JobLoaded;

        public JobsView()
        {
            InitializeComponent();
        }

        public void InitializeRepository(JobRepository repository)
        {
            if (_repository != null)
            {
                _repository.JobsChanged -= OnRepositoryJobsChanged;
            }

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _repository.JobsChanged += OnRepositoryJobsChanged;
            ReloadRepositoryJobs(0);
        }

        private void OnRepositoryJobsChanged(object sender, EventArgs e)
        {
            ReloadRepositoryJobs(0);
        }

        private void ReloadRepositoryJobs(int selectIndex)
        {
            _allJobs = _repository?.Jobs.ToList() ?? new List<JobRecord>();
            RefreshJobsList(selectIndex);
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
            SummaryFormatText.Text = job.Format + " (" + job.DimensionsLabel + ")";
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
            if (job == null || _repository == null)
            {
                return;
            }

            _repository.Load(job);
            string suffix = loadRunAdjustments ? " with saved RUN adjustments" : string.Empty;
            LastActionText.Text = "Loaded job \"" + job.Name + "\"" + suffix + ".";
            JobLoaded?.Invoke(this, job);
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
            var basis = _repository?.CurrentJob ?? job;
            var a4 = BookFormatCatalog.Find("A4");
            SaveAsNewJobDialogControl.Open(
                suggestedName,
                basis?.Pages ?? 1,
                basis?.Format ?? "A4",
                basis?.WidthMm ?? a4.WidthMm,
                basis?.LengthMm ?? a4.LengthMm,
                CurrentSetupMachineLine);
        }

        private void OnSaveRequested(object sender, SaveJobRequest request)
        {
            bool exists = _allJobs.Any(j => string.Equals(j.Name, request.Name, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                SaveAsNewJobDialogControl.ShowConflict();
                return;
            }

            SaveAsNewJobDialogControl.Close();
            SaveNewJob(request, overwrite: false);
        }

        private void OnOverwriteConfirmed(object sender, SaveJobRequest request)
        {
            SaveNewJob(request, overwrite: true);
        }

        private void SaveNewJob(SaveJobRequest request, bool overwrite)
        {
            var newJob = _repository?.SaveNew(request, overwrite);
            if (newJob == null)
            {
                return;
            }

            ConfirmationDialogControl.Open("New Job Saved!",
                "The new job \u201c" + request.Name + "\u201d was saved as " +
                request.Format + " with " + request.Pages + " pages.");
        }

        // ===================== Remove Job =====================

        private void OnRemoveJobClick(object sender, RoutedEventArgs e)
        {
            var job = SelectedJob;
            if (job == null) return;
            RemoveJobDialogControl.Open(job.Name, job.Format, CurrentSetupMachineLine);
        }

        private void OnJobRemoved(object sender, string jobName)
        {
            var job = _allJobs.FirstOrDefault(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase));
            _repository?.Remove(job);

            ConfirmationDialogControl.Open("Job Removed!",
                "The job \u201c" + jobName + "\u201d has been successfully removed.");
        }
    }
}
