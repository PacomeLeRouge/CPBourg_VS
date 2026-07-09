using System;

namespace CPBourg.NextGenGui.Startup
{
    /// <summary>
    /// The ordered boot phases shown on the splash screen.
    /// Keep this list and its order aligned with the splash step indicator.
    /// </summary>
    public enum StartupStage
    {
        InitializingSystem = 0,
        LoadingUiResources = 1,
        ReadingMachineConfiguration = 2,
        ConnectingToWfm = 3,
        OpeningDashboard = 4
    }

    public static class StartupStageInfo
    {
        /// <summary>Total number of phases (used for progress maths).</summary>
        public const int Count = 5;

        /// <summary>
        /// Short operator-facing label for a stage.
        ///
        /// FR-10 (languages): these strings are the ones that must come from a
        /// localisation resource (French / English) rather than being hard-coded.
        /// For the prototype they are inline; swap for resource lookups when the
        /// language-switching mechanism is wired in.
        /// </summary>
        public static string Describe(StartupStage stage)
        {
            switch (stage)
            {
                case StartupStage.InitializingSystem:          return "Initializing system";
                case StartupStage.LoadingUiResources:          return "Loading UI resources";
                case StartupStage.ReadingMachineConfiguration: return "Reading machine configuration";
                case StartupStage.ConnectingToWfm:             return "Connecting to WFM";
                case StartupStage.OpeningDashboard:            return "Opening operator dashboard";
                default: throw new ArgumentOutOfRangeException(nameof(stage));
            }
        }

        /// <summary>Very short label for the 5-dot step indicator.</summary>
        public static string ShortLabel(StartupStage stage)
        {
            switch (stage)
            {
                case StartupStage.InitializingSystem:          return "Init";
                case StartupStage.LoadingUiResources:          return "UI";
                case StartupStage.ReadingMachineConfiguration: return "Config";
                case StartupStage.ConnectingToWfm:             return "WFM";
                case StartupStage.OpeningDashboard:            return "Dashboard";
                default: throw new ArgumentOutOfRangeException(nameof(stage));
            }
        }
    }
}
