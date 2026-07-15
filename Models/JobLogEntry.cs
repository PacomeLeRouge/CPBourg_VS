using System;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>One timestamped event retained in a job's prototype log.</summary>
    public sealed class JobLogEntry
    {
        public JobLogEntry(DateTime timestamp, string action, string details)
        {
            Timestamp = timestamp;
            Action = action ?? string.Empty;
            Details = details ?? string.Empty;
        }

        public DateTime Timestamp { get; }
        public string Action { get; }
        public string Details { get; }
        public string TimestampLabel => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
