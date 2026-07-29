namespace CPBourg.NextGenGui.Models
{
    /// <summary>Durable operator choices from Settings / Preferences.</summary>
    public sealed class OperatorPreferences
    {
        public string Language { get; set; }
        public string Units { get; set; }
        public string KeyboardLayout { get; set; }
        public string MouseCursor { get; set; }
        public long DateTimeOffsetTicks { get; set; }
        public string FontSize { get; set; }
        public bool ScreenCalibrated { get; set; }
        public double CalibrationErrorPixels { get; set; }

        public static OperatorPreferences CreateDefaults()
        {
            return new OperatorPreferences
            {
                Language = "English",
                Units = "Millimeters",
                KeyboardLayout = "AZERTY",
                MouseCursor = "Disabled",
                DateTimeOffsetTicks = 0,
                FontSize = "Medium",
                ScreenCalibrated = false,
                CalibrationErrorPixels = 0,
            };
        }

        public OperatorPreferences Clone()
        {
            return new OperatorPreferences
            {
                Language = Language,
                Units = Units,
                KeyboardLayout = KeyboardLayout,
                MouseCursor = MouseCursor,
                DateTimeOffsetTicks = DateTimeOffsetTicks,
                FontSize = FontSize,
                ScreenCalibrated = ScreenCalibrated,
                CalibrationErrorPixels = CalibrationErrorPixels,
            };
        }
    }
}
