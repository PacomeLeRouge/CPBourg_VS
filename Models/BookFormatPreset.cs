namespace CPBourg.NextGenGui.Models
{
    /// <summary>A standard sheet/book format expressed in millimetres.</summary>
    public sealed class BookFormatPreset
    {
        public BookFormatPreset(string name, double widthMm, double lengthMm)
        {
            Name = name;
            WidthMm = widthMm;
            LengthMm = lengthMm;
        }

        public string Name { get; }
        public double WidthMm { get; }
        public double LengthMm { get; }

        public string DimensionLabel => WidthMm.ToString("0.0#") + " x " +
                                        LengthMm.ToString("0.0#") + " mm";

        public override string ToString()
        {
            return Name + " (" + DimensionLabel + ")";
        }
    }
}
