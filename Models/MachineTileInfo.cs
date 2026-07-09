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
        public MachineTileInfo(string shortCode, MachineStatus status)
        {
            ShortCode = shortCode;
            Status = status;
        }

        /// <summary>Short module code as used in the current GUI (e.g. "BSF").</summary>
        public string ShortCode { get; }

        public MachineStatus Status { get; }

        public string StatusLabel
        {
            get
            {
                switch (Status)
                {
                    case MachineStatus.Running: return "Running";
                    case MachineStatus.Idle: return "Idle";
                    case MachineStatus.Offline: return "Offline";
                    default: return Status.ToString();
                }
            }
        }
    }
}
