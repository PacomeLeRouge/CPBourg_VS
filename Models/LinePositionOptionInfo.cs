namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// One selectable row in the Add Module wizard's "Select Position" step
    /// (<see cref="Views.AddModuleWizardDialog"/>) - "Before X" / "After X"
    /// relative to whichever module is currently focused on the Machine
    /// Line. Plain data, no WPF dependency - same reasoning as
    /// ChangeOptionInfo.cs.
    /// </summary>
    public sealed class LinePositionOptionInfo
    {
        public LinePositionOptionInfo(bool isBeforeAnchor, string displayLabel)
        {
            IsBeforeAnchor = isBeforeAnchor;
            DisplayLabel = displayLabel;
        }

        /// <summary>True for "Before {anchor}", false for "After {anchor}".</summary>
        public bool IsBeforeAnchor { get; }

        public string DisplayLabel { get; }
    }
}
