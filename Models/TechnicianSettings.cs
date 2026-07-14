namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Saved choices from the Technician Interface. The values intentionally
    /// mirror the labels used by the machine UI so they can later be mapped to
    /// WFM parameters without changing the screen's state model.
    /// </summary>
    public sealed class TechnicianSettings
    {
        public string CalibrationSetOption { get; set; }
        public bool SaveAdjustmentsEnabled { get; set; }
        public string StitchHeadForm { get; set; }
        public bool DisableStitchHead1 { get; set; }
        public bool DisableStitchHead2 { get; set; }
        public bool StitchSingleSheetEnabled { get; set; }
        public string PurgeOption { get; set; }
        public bool SheetInCompilerRestartAllowed { get; set; }

        public static TechnicianSettings CreateDefaults()
        {
            return new TechnicianSettings
            {
                CalibrationSetOption = "Always Reject",
                SaveAdjustmentsEnabled = false,
                StitchHeadForm = "Normal Stitch",
                DisableStitchHead1 = false,
                DisableStitchHead2 = false,
                StitchSingleSheetEnabled = true,
                PurgeOption = "Ask Operator",
                SheetInCompilerRestartAllowed = true,
            };
        }

        public TechnicianSettings Clone()
        {
            return new TechnicianSettings
            {
                CalibrationSetOption = CalibrationSetOption,
                SaveAdjustmentsEnabled = SaveAdjustmentsEnabled,
                StitchHeadForm = StitchHeadForm,
                DisableStitchHead1 = DisableStitchHead1,
                DisableStitchHead2 = DisableStitchHead2,
                StitchSingleSheetEnabled = StitchSingleSheetEnabled,
                PurgeOption = PurgeOption,
                SheetInCompilerRestartAllowed = SheetInCompilerRestartAllowed,
            };
        }
    }
}
