namespace CPBourg.NextGenGui.Models
{
    /// <summary>Validated values submitted by Save As New Job.</summary>
    public sealed class SaveJobRequest
    {
        public SaveJobRequest(string name, int pages, double widthMm, double lengthMm)
        {
            Name = name;
            Pages = pages;
            WidthMm = widthMm;
            LengthMm = lengthMm;
            Format = BookFormatCatalog.ResolveName(widthMm, lengthMm);
        }

        public string Name { get; }
        public int Pages { get; }
        public double WidthMm { get; }
        public double LengthMm { get; }
        public string Format { get; }
    }
}
