namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Data for one row on the Settings / Operator Preferences screen
    /// (e.g. "Language: English [Change]"). Plain data, no WPF dependency -
    /// see MachineTileInfo.cs for why that separation matters (NFR-02).
    /// </summary>
    public sealed class SettingsItemInfo
    {
        public SettingsItemInfo(string iconGlyph, string label, string value, string buttonLabel, string tag,
            bool isLetterIcon = false)
        {
            IconGlyph = iconGlyph;
            Label = label;
            Value = value;
            ButtonLabel = buttonLabel;
            Tag = tag;
            IsLetterIcon = isLetterIcon;
        }

        /// <summary>Segoe MDL2 Assets glyph shown in the row's icon circle
        /// (or, if <see cref="IsLetterIcon"/> is true, literal text shown in
        /// the normal UI font instead - see Font Size's "Aa").</summary>
        public string IconGlyph { get; }

        public string Label { get; }
        public string Value { get; }

        /// <summary>Usually "Change", but "Calibrate" for the screen calibration row.</summary>
        public string ButtonLabel { get; }

        /// <summary>Identifies which row was acted on, for the click handler.</summary>
        public string Tag { get; }

        /// <summary>
        /// True for rows whose "icon" is actually literal text (e.g. "Aa" for
        /// Font Size) rather than a Segoe MDL2 Assets glyph. Icon fonts remap
        /// ordinary letters to unrelated icon shapes, so literal text must be
        /// rendered in the normal UI font, never the icon font.
        /// </summary>
        public bool IsLetterIcon { get; }
    }
}
