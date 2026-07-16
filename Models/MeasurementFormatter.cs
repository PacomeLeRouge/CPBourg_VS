using System.Globalization;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>Shared millimetre/inch conversion and consistent display formatting.</summary>
    public static class MeasurementFormatter
    {
        public const double MillimetersPerInch = 25.4;

        public static string UnitSymbol(MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches ? "in" : "mm";
        }

        public static string SpeedUnitSymbol(MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches ? "in/s" : "mm/s";
        }

        public static double ToDisplay(double millimeters, MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches
                ? millimeters / MillimetersPerInch
                : millimeters;
        }

        public static double ToMillimeters(double displayValue, MeasurementUnit unit)
        {
            return unit == MeasurementUnit.Inches
                ? displayValue * MillimetersPerInch
                : displayValue;
        }

        public static string FormatValue(double millimeters, MeasurementUnit unit,
            string metricFormat = "0.0##", string inchFormat = "0.000")
        {
            string format = unit == MeasurementUnit.Inches ? inchFormat : metricFormat;
            return ToDisplay(millimeters, unit).ToString(format, CultureInfo.InvariantCulture);
        }

        public static string FormatLength(double millimeters, MeasurementUnit unit,
            string metricFormat = "0.0##", string inchFormat = "0.000")
        {
            return FormatValue(millimeters, unit, metricFormat, inchFormat) + " " + UnitSymbol(unit);
        }

        public static string FormatDimensions(double widthMm, double lengthMm, MeasurementUnit unit)
        {
            return FormatValue(widthMm, unit, "0.0#", "0.000") + " x " +
                   FormatValue(lengthMm, unit, "0.0#", "0.000") + " " + UnitSymbol(unit);
        }
    }
}
