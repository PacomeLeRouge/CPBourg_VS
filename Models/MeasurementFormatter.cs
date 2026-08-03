using System.Globalization;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>Shared millimetre/inch conversion and consistent display formatting.</summary>
    public static class MeasurementFormatter
    {
        /// <summary>Exact conversion constant used at every UI boundary.</summary>
        public const double MillimetersPerInch = 25.4;

        /// <summary>Returns the display suffix for a length.</summary>
        public static string UnitSymbol(MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches ? "in" : "mm";
        }

        /// <summary>Returns the display suffix for a speed.</summary>
        public static string SpeedUnitSymbol(MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches ? "in/s" : "mm/s";
        }

        /// <summary>Converts a canonical millimeter value for presentation.</summary>
        public static double ToDisplay(double millimeters, MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches
                ? millimeters / MillimetersPerInch
                : millimeters;
        }

        /// <summary>Converts operator input back to canonical millimeters.</summary>
        public static double ToMillimeters(double displayValue, MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches
                ? displayValue * MillimetersPerInch
                : displayValue;
        }

        /// <summary>Formats a canonical value without appending a unit symbol.</summary>
        public static string FormatValue(double millimeters, MeasurementUnit unit,
            string metricFormat = "0.0##", string inchFormat = "0.000")
        {
            string format = unit == MeasurementUnit.Inches ? inchFormat : metricFormat;
            return ToDisplay(millimeters, unit).ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>Formats a canonical length and appends its unit symbol.</summary>
        public static string FormatLength(double millimeters, MeasurementUnit unit,
            string metricFormat = "0.0##", string inchFormat = "0.000")
        {
            return FormatValue(millimeters, unit, metricFormat, inchFormat) + " " + UnitSymbol(unit);
        }

        /// <summary>Formats canonical width and length as one dimension label.</summary>
        public static string FormatDimensions(double widthMm, double lengthMm, MeasurementUnit unit)
        {
            return FormatValue(widthMm, unit, "0.0#", "0.000") + " x " +
                   FormatValue(lengthMm, unit, "0.0#", "0.000") + " " + UnitSymbol(unit);
        }
    }
}
