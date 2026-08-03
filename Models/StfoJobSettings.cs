using System;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>Complete STFO setup stored independently on every job.</summary>
    public sealed class StfoJobSettings
    {
        public double PaperWidth { get; set; }
        public double PaperLength { get; set; }
        public double StitchSpacing { get; set; }
        public double HorizontalOffset { get; set; }
        public double VerticalOffset { get; set; }
        public string StitchMode { get; set; }

        public bool FoldEnabled { get; set; }
        public double FoldPosition { get; set; }
        public string PressureMode { get; set; }
        public double PressureLevel { get; set; }

        public bool TrimEnabled { get; set; }
        public double FinalBookletLength { get; set; }
        public double ClampPressure { get; set; }
        public bool ChipBlower { get; set; }

        public int BookletSpacing { get; set; }
        public int BookletOffset { get; set; }
        public bool FullDetection { get; set; }

        /// <summary>Creates an independent shallow copy of the scalar settings.</summary>
        public StfoJobSettings Clone()
        {
            return (StfoJobSettings)MemberwiseClone();
        }

        /// <summary>
        /// Creates plausible prototype values from physical format and page
        /// count. The deterministic variation makes jobs genuinely distinct
        /// while keeping dimensional controls accurate to their format.
        /// </summary>
        public static StfoJobSettings CreateForFormat(double widthMm, double lengthMm,
            int pages, int variationSeed)
        {
            int safePages = Math.Max(1, pages);
            bool multiPageBook = safePages > 4;
            double finalLength = Math.Max(50, Math.Min(350, widthMm - 5));

            return new StfoJobSettings
            {
                PaperWidth = widthMm,
                PaperLength = lengthMm,
                StitchSpacing = Math.Min(30, 8 + safePages % 7),
                HorizontalOffset = (variationSeed % 3 - 1) * 0.5,
                VerticalOffset = ((variationSeed + 1) % 3 - 1) * 0.5,
                StitchMode = multiPageBook ? "Saddle" : "None",

                FoldEnabled = multiPageBook,
                FoldPosition = multiPageBook ? variationSeed % 3 - 1 : 0,
                PressureMode = safePages >= 80 ? "Auto" : "Manual",
                PressureLevel = Math.Min(0.9, 0.3 + safePages / 250.0),

                TrimEnabled = true,
                FinalBookletLength = finalLength,
                ClampPressure = Math.Min(90, Math.Max(20, 30 + safePages * 0.4)),
                ChipBlower = true,

                BookletSpacing = Math.Max(1, Math.Min(30, 6 + safePages % 10)),
                BookletOffset = Math.Max(1, Math.Min(30, 8 + variationSeed % 8)),
                FullDetection = true,
            };
        }
    }
}
