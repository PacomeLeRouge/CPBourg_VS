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

        public JobRepository()
        {
            _jobs = CreateSampleJobs();
            CurrentJob = _jobs.FirstOrDefault();
        }

        public IReadOnlyList<JobRecord> Jobs => _jobs;
        public JobRecord CurrentJob { get; private set; }

        public event EventHandler JobsChanged;
        public event EventHandler CurrentJobChanged;

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
