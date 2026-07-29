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
    /// updates the Summary panel; its action tiles open functional dialogs
    /// for loading, saving, removing, commenting, barcode lookup, and job-log
    /// export.
    ///
    /// JobRepository is also consumed by the dashboard and STFO, so opening a
    /// job updates all three screens from the same JobRecord instance.
    /// </summary>
    public partial class JobsView : UserControl
    {
        private const string CurrentSetupMachineLine = "BSF + BSE";

        private List<JobRecord> _allJobs = new List<JobRecord>();
        private JobRepository _repository;
        private MeasurementUnit _measurementUnit = MeasurementUnit.Millimeters;
        private string _lastActionSource;
        private object[] _lastActionArguments;

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

        public void SetMeasurementUnit(MeasurementUnit unit)
        {
            _measurementUnit = unit;
            SaveAsNewJobDialogControl.SetMeasurementUnit(unit);
            JobLogDialogControl.SetMeasurementUnit(unit);
            OnJobSelectionChanged(this, null);
        }

        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            SaveAsNewJobDialogControl.ApplyLanguage();
            JobLogDialogControl.ApplyLanguage();
            OnJobSelectionChanged(this, null);
            RenderLastAction();
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
            StatusMessageText.Text = T("No job selected.");
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
            SummaryFormatText.Text = job.Format + " (" +
                MeasurementFormatter.FormatDimensions(job.WidthMm, job.LengthMm, _measurementUnit) + ")";
            SummaryCommentText.Text = T(job.Comment);
            SummaryBarcodeText.Text = job.BarcodeId;
            SummaryLastModifiedText.Text = job.LastModified;

            StatusMessageText.Text = T("Selected job is ready to open.");
            LocalizationManager.Apply(JobsListBox);
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
            SetLastAction("Last action: {0} (stub - not yet connected to job storage)",
                T(actionName));
        }

        private void OnFilterClick(object sender, RoutedEventArgs e) => ShowStub("Filter");
        private void OnViewLogClick(object sender, RoutedEventArgs e)
        {
            var job = SelectedJob;
            if (job == null) return;
            JobLogDialogControl.Open(job);
        }

        private void OnScanBarcodeClick(object sender, RoutedEventArgs e)
        {
            BarcodeScanDialogControl.Open();
        }

        private void OnBarcodeScanRequested(object sender, string barcodeId)
        {
            var job = _allJobs.FirstOrDefault(candidate =>
                string.Equals(candidate.BarcodeId, barcodeId, StringComparison.OrdinalIgnoreCase));
            if (job == null)
            {
                BarcodeScanDialogControl.ShowError(
                    TF("No saved job uses barcode ID “{0}”. Check the book and scan again.", barcodeId));
                return;
            }

            BarcodeScanDialogControl.Close();
            SearchBox.Text = string.Empty;
            JobsListBox.SelectedItem = job;
            JobsListBox.ScrollIntoView(job);
            job.AddLog("Barcode scanned", "Barcode " + barcodeId + " matched this saved job.");
            StatusMessageText.Text = TF("Barcode matched “{0}”. It is ready to open.", job.Name);
            SetLastAction("Barcode {0} matched job “{1}”.", barcodeId, job.Name);
        }

        private void OnJobLogExported(object sender, string path)
        {
            SetLastAction("Job log exported to {0}", path);
        }

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
            SetLastAction(loadRunAdjustments
                    ? "Loaded job “{0}” with saved RUN adjustments."
                    : "Loaded job “{0}”.",
                job.Name);
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
            job.AddLog("Comment updated", job.Comment);
            SummaryCommentText.Text = job.Comment;

            ConfirmationDialogControl.Open(T("Comment Saved!"),
                TF("The comment for “{0}” has been successfully saved.", job.Name));
        }

        // ===================== Save As New Job =====================

        private void OnSaveAsNewClick(object sender, RoutedEventArgs e)
        {
            var job = SelectedJob;
            string suggestedName = job != null ? job.Name + " - " + T("Variant") + " 1" : T("New Job");
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

            ConfirmationDialogControl.Open(T("New Job Saved!"),
                TF("The new job “{0}” was saved as {1} with {2} pages.",
                    request.Name, request.Format, request.Pages));
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

            ConfirmationDialogControl.Open(T("Job Removed!"),
                TF("The job “{0}” has been successfully removed.", jobName));
        }

        private void SetLastAction(string source, params object[] arguments)
        {
            _lastActionSource = source;
            _lastActionArguments = arguments;
            RenderLastAction();
        }

        private void RenderLastAction()
        {
            LastActionText.Text = string.IsNullOrEmpty(_lastActionSource)
                ? string.Empty
                : TF(_lastActionSource, _lastActionArguments ?? new object[0]);
        }

        private static string T(string source)
        {
            return LocalizationManager.Translate(source);
        }

        private static string TF(string source, params object[] values)
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture,
                T(source), values);
        }
    }
}
