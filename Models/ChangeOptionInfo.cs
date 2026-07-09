namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// One selectable option inside a <see cref="Views.ChangeValueDialog"/>
    /// (e.g. one language, one keyboard layout). Plain data, no WPF
    /// dependency - see MachineTileInfo.cs for why that separation matters.
    /// </summary>
    public sealed class ChangeOptionInfo
    {
        public ChangeOptionInfo(string value, string displayLabel, bool isSelected = false, string flagEmoji = null)
        {
            Value = value;
            DisplayLabel = displayLabel;
            IsSelected = isSelected;
            FlagEmoji = flagEmoji;
        }

        /// <summary>The value shown back on the settings row once confirmed (e.g. "Fran\u00e7ais").</summary>
        public string Value { get; }

        /// <summary>Label shown next to the radio button - usually the same as <see cref="Value"/>.</summary>
        public string DisplayLabel { get; }

        /// <summary>
        /// Whether this option is pre-selected when the dialog opens. Mutable
        /// (not just constructor-set) only in the sense that each dialog
        /// invocation builds a fresh list with the current value marked -
        /// this is configuration for one dialog showing, not persisted state.
        /// </summary>
        public bool IsSelected { get; }

        /// <summary>Optional flag emoji for language options (e.g. "\ud83c\uddec\ud83c\udde7"); null for non-language pickers.</summary>
        public string FlagEmoji { get; }
    }
}
