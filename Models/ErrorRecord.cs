namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Data for one row in the Errors &amp; Information screen's Active
    /// Messages list, and the corresponding detail overlay (FR-06, AC-05 -
    /// cover open, JAM, unavailable states, and useful operator messages).
    /// Plain data, no WPF dependency - see MachineTileInfo.cs for why that
    /// separation matters (NFR-02).
    /// </summary>
    public sealed class ErrorRecord
    {
        public ErrorRecord(ErrorSeverity severity, string source, string moduleOrJob, string time,
            string details, string description, string relatedModule, string relatedJob)
        {
            Severity = severity;
            Source = source;
            ModuleOrJob = moduleOrJob;
            Time = time;
            Details = details;
            Description = description;
            RelatedModule = relatedModule;
            RelatedJob = relatedJob;
        }

        public ErrorSeverity Severity { get; }

        /// <summary>Where the message originated (e.g. "Machine").</summary>
        public string Source { get; }

        /// <summary>The module or job code shown in its own column (e.g. "BPM").</summary>
        public string ModuleOrJob { get; }

        public string Time { get; }

        /// <summary>Short one-line summary shown in the list row (e.g. "Cover open on Module 3").</summary>
        public string Details { get; }

        /// <summary>Longer explanation shown in the detail overlay.</summary>
        public string Description { get; }

        public string RelatedModule { get; }
        public string RelatedJob { get; }

        public string SeverityLabel => Severity.ToString();
    }
}
