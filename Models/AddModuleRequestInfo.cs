namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Result of a completed <see cref="Views.AddModuleWizardDialog"/> run -
    /// carried by its Confirmed event back to
    /// MachineLineConfigurationView.xaml.cs, which does the actual insert.
    /// </summary>
    public sealed class AddModuleRequestInfo
    {
        public AddModuleRequestInfo(string moduleType, bool? placeBeforeAnchor,
            string anchorModuleType)
        {
            ModuleType = moduleType;
            PlaceBeforeAnchor = placeBeforeAnchor;
            AnchorModuleType = anchorModuleType;
        }

        public string ModuleType { get; }

        /// <summary>True = insert before the focused module, false = insert
        /// after it, null = the line was empty so there was nothing to
        /// position against - just add it.</summary>
        public bool? PlaceBeforeAnchor { get; }

        /// <summary>Language-neutral anchor used to rebuild localized position
        /// feedback after a language change; null for an initially empty line.</summary>
        public string AnchorModuleType { get; }
    }
}
