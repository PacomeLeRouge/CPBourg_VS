using System;
using System.Collections.Generic;
using System.Linq;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Shared standard-format catalog used by job creation and STFO settings.
    /// A dimension must match within 0.05 mm; otherwise it is classified as
    /// Custom even when the operator originally selected a preset.
    /// </summary>
    public static class BookFormatCatalog
    {
        private const double MatchToleranceMm = 0.05;

        private static readonly List<BookFormatPreset> PresetList = new List<BookFormatPreset>
        {
            new BookFormatPreset("A3", 297, 420),
            new BookFormatPreset("A4", 210, 297),
            new BookFormatPreset("A5", 148, 210),
            new BookFormatPreset("Letter", 215.9, 279.4),
            new BookFormatPreset("5 x 7 in", 127, 177.8),
        };

        public static IReadOnlyList<BookFormatPreset> Presets => PresetList;

        public static BookFormatPreset Find(string name)
        {
            return PresetList.FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static BookFormatPreset Match(double widthMm, double lengthMm)
        {
            return PresetList.FirstOrDefault(p =>
                Math.Abs(p.WidthMm - widthMm) <= MatchToleranceMm &&
                Math.Abs(p.LengthMm - lengthMm) <= MatchToleranceMm);
        }

        public static string ResolveName(double widthMm, double lengthMm)
        {
            var preset = Match(widthMm, lengthMm);
            return preset == null ? "Custom" : preset.Name;
        }
    }
}
