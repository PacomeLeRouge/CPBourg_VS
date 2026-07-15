namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Data for one row in the Jobs / File Menu screen's "Saved Jobs" list
    /// and the corresponding "Summary" panel (FR-08 job save/load). Plain
    /// data, no WPF dependency - see MachineTileInfo.cs for why that
    /// separation matters (NFR-02).
    ///
    /// <see cref="Comment"/> is intentionally mutable (unlike the other
    /// properties) since the Add Comment dialog updates it on an existing
    /// job in place, rather than replacing the whole record.
    /// </summary>
    public sealed class JobRecord
    {
        public JobRecord(string name, int pages, string format, double widthMm, double lengthMm,
            string savedDate, string savedTime, string comment, string barcodeId,
            string lastModified, StfoJobSettings stfoSettings)
        {
            Name = name;
            Pages = pages;
            Format = format;
            WidthMm = widthMm;
            LengthMm = lengthMm;
            SavedDate = savedDate;
            SavedTime = savedTime;
            Comment = comment;
            BarcodeId = barcodeId;
            LastModified = lastModified;
            StfoSettings = stfoSettings ??
                StfoJobSettings.CreateForFormat(widthMm, lengthMm, pages, 0);
        }

        public string Name { get; }
        public int Pages { get; }
        public string Format { get; }
        public double WidthMm { get; }
        public double LengthMm { get; }
        public StfoJobSettings StfoSettings { get; set; }

        /// <summary>Date shown at the top-right of the row (e.g. "2026-06-21").</summary>
        public string SavedDate { get; }

        /// <summary>Time shown below the date (e.g. "14:37").</summary>
        public string SavedTime { get; }

        public string Comment { get; set; }
        public string BarcodeId { get; }
        public string LastModified { get; }

        /// <summary>"48 pages" style summary shown in the row subtitle.</summary>
        public string PagesLabel => Pages + " pages";
        public string DimensionsLabel => WidthMm.ToString("0.0#") + " x " +
                                         LengthMm.ToString("0.0#") + " mm";
    }
}
