namespace CPBourg.NextGenGui.Startup
{
    /// <summary>
    /// An immutable snapshot of boot progress, reported by the
    /// <see cref="StartupSequencer"/> and consumed by the splash view.
    /// The view never reaches into the sequencer - it only receives these.
    /// </summary>
    public sealed class StartupProgress
    {
        public StartupProgress(StartupStage stage, int percent, bool isError = false, string message = null)
        {
            Stage = stage;
            Percent = percent;
            IsError = isError;
            Message = message ?? StartupStageInfo.Describe(stage);
        }

        /// <summary>The stage currently in progress (or the one that failed).</summary>
        public StartupStage Stage { get; }

        /// <summary>Overall completion, 0-100.</summary>
        public int Percent { get; }

        /// <summary>True when a stage failed (e.g. the WFM could not be reached).</summary>
        public bool IsError { get; }

        /// <summary>Operator-facing status line.</summary>
        public string Message { get; }
    }
}
