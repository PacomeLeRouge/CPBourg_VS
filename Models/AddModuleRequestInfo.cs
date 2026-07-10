namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Result of a completed <see cref="Views.AddModuleWizardDialog"/> run -
    /// carried by its Confirmed event back to
    /// MachineLineConfigurationView.xaml.cs, which does the actual insert.
    /// </summary>
    public sealed class AddModuleRequestInfo
    {
        public AddModuleRequestInfo(string moduleType, bool? placeBeforeAnchor, string positionSummary)
        {
            ModuleType = moduleType;
            PlaceBeforeAnchor = placeBeforeAnchor;
            PositionSummary = positionSummary;
        }

        public string ModuleType { get; }

        /// <summary>True = insert before the focused module, false = insert
        /// after it, null = the line was empty so there was nothing to
        /// position against - just add it.</summary>
        public bool? PlaceBeforeAnchor { get; }

        /// <summary>Human-readable position, for the demo feedback line (e.g.
        /// "before Booklet Maker (BM-2000-01)" or "start of line").</summary>
        public string PositionSummary { get; }
    }
}
