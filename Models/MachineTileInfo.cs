namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Data for one tile in the dashboard's "Machines" card
    /// (e.g. BSF, STFO, BSE, TR - the modules of the current train, PRD 3.1).
    /// The actual list must come from the WFM/train configuration once wired
    /// up; this prototype uses a hard-coded sample list (see DashboardView).
    /// </summary>
    public sealed class MachineTileInfo
    {
        public MachineTileInfo(string shortCode, MachineStatus status,
            string statusLabel)
        {
            ShortCode = shortCode;
            Status = status;
            StatusLabel = statusLabel;
        }

        /// <summary>Short module code as used in the current GUI (e.g. "BSF").</summary>
        public string ShortCode { get; }

        public MachineStatus Status { get; }

        /// <summary>Localized display label supplied by the Dashboard while
        /// <see cref="Status"/> remains a language-neutral enum.</summary>
        public string StatusLabel { get; }
    }
}
