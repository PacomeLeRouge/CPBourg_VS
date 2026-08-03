using System;
using System.Collections.Generic;
using System.Linq;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Single in-memory source for the Jobs screen, dashboard, and STFO.
    /// Startup loads the first (most recent) sample job as the current job.
    /// </summary>
    public sealed class JobRepository
    {
        private readonly List<JobRecord> _jobs;

        /// <summary>Creates the seeded prototype list and selects its first job.</summary>
        public JobRepository()
        {
            _jobs = CreateSampleJobs();
            CurrentJob = _jobs.FirstOrDefault();
        }

        /// <summary>Current in-memory jobs in newest-first display order.</summary>
        public IReadOnlyList<JobRecord> Jobs => _jobs;

        /// <summary>The job shared by Home and STFO, or null when the list is empty.</summary>
        public JobRecord CurrentJob { get; private set; }

        /// <summary>Raised after the collection is inserted into or removed from.</summary>
        public event EventHandler JobsChanged;

        /// <summary>Raised after <see cref="CurrentJob"/> changes identity.</summary>
        public event EventHandler CurrentJobChanged;

        /// <summary>
        /// Makes an existing repository record current and appends a load log.
        /// Unknown or null records are ignored.
        /// </summary>
        public void Load(JobRecord job)
        {
            if (job == null || !_jobs.Contains(job))
            {
                return;
            }

            CurrentJob = job;
            job.AddLog("Job loaded", "Loaded as the current production job.");
            CurrentJobChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Creates a newest-first job with deterministic STFO settings. Returns
        /// null when the request is null or a duplicate name is not authorized
        /// for overwrite.
        /// </summary>
        public JobRecord SaveNew(SaveJobRequest request, bool overwrite)
        {
            if (request == null)
            {
                return null;
            }

            var existing = _jobs.FirstOrDefault(j =>
                string.Equals(j.Name, request.Name, StringComparison.OrdinalIgnoreCase));
            bool replacingCurrent = ReferenceEquals(CurrentJob, existing);
            if (existing != null)
            {
                if (!overwrite)
                {
                    return null;
                }

                _jobs.Remove(existing);
            }

            var now = DateTime.Now;
            var settings = StfoJobSettings.CreateForFormat(
                request.WidthMm, request.LengthMm, request.Pages, _jobs.Count + 1);
            var job = new JobRecord(request.Name, request.Pages, request.Format,
                request.WidthMm, request.LengthMm,
                now.ToString("yyyy-MM-dd"), now.ToString("HH:mm"),
                "-", "BC-" + now.ToString("yyyyMMddHHmmss"),
                now.ToString("yyyy-MM-dd HH:mm"), settings);

            _jobs.Insert(0, job);
            if (replacingCurrent)
            {
                CurrentJob = job;
            }
            JobsChanged?.Invoke(this, EventArgs.Empty);
            if (replacingCurrent)
            {
                CurrentJobChanged?.Invoke(this, EventArgs.Empty);
            }
            return job;
        }

        /// <summary>
        /// Removes an existing record. Removing the current job selects the new
        /// first item and raises both collection and current-job notifications.
        /// </summary>
        public void Remove(JobRecord job)
        {
            if (job == null || !_jobs.Remove(job))
            {
                return;
            }

            bool removedCurrent = ReferenceEquals(CurrentJob, job);
            if (removedCurrent)
            {
                CurrentJob = _jobs.FirstOrDefault();
            }

            JobsChanged?.Invoke(this, EventArgs.Empty);
            if (removedCurrent)
            {
                CurrentJobChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private static List<JobRecord> CreateSampleJobs()
        {
            return new List<JobRecord>
            {
                CreateSample("Spring Catalog 2026", 48, "A4", "2026-06-21", "14:37",
                    "Ready for reprint", "BC-10458-22", 1),
                CreateSample("Booklet Batch A", 32, "Letter", "2026-06-20", "09:12",
                    "-", "BC-10457-11", 2),
                CreateSample("Training Manual Rev 3", 120, "A4", "2026-06-19", "16:05",
                    "-", "BC-10455-08", 3),
                CreateSample("Promo Cards 5x7", 2, "5 x 7 in", "2026-06-18", "11:28",
                    "-", "BC-10450-02", 4),
                CreateSample("Service Guide 2026", 64, "A5", "2026-06-17", "08:44",
                    "-", "BC-10448-19", 5),
            };
        }

        private static JobRecord CreateSample(string name, int pages, string format,
            string date, string time, string comment, string barcode, int variationSeed)
        {
            var preset = BookFormatCatalog.Find(format);
            double width = preset?.WidthMm ?? 210;
            double length = preset?.LengthMm ?? 297;
            return new JobRecord(name, pages, format, width, length, date, time,
                comment, barcode, date + " " + time,
                StfoJobSettings.CreateForFormat(width, length, pages, variationSeed));
        }
    }
}
