namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// One selectable row in the Add Module wizard's "Available Modules" step
    /// (<see cref="Views.AddModuleWizardDialog"/>). Plain data, no WPF
    /// dependency - same reasoning as ChangeOptionInfo.cs.
    /// </summary>
    public sealed class ModuleTypeOptionInfo
    {
        public ModuleTypeOptionInfo(string moduleType, string displayName,
            bool isSelected = false)
        {
            ModuleType = moduleType;
            DisplayName = displayName;
            IsSelected = isSelected;
        }

        /// <summary>Language-neutral value used by line configuration logic.</summary>
        public string ModuleType { get; }

        /// <summary>Localized module name displayed to the operator.</summary>
        public string DisplayName { get; }

        /// <summary>Whether this row is pre-highlighted when the step opens
        /// (the wizard defaults to the first catalog entry, matching the
        /// reference mock's "Feeder" already selected).</summary>
        public bool IsSelected { get; }
    }
}
