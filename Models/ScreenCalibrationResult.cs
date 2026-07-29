namespace CPBourg.NextGenGui.Models
{
    public sealed class ScreenCalibrationResult
    {
        public ScreenCalibrationResult(double averageErrorPixels)
        {
            AverageErrorPixels = averageErrorPixels;
        }

        public double AverageErrorPixels { get; }
    }
}
