namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Severity of one entry on the Errors &amp; Information screen (FR-06,
    /// AC-05). Colours are resolved in the Views layer (see
    /// ErrorSeverityToBrushConverter.cs) so this stays WPF-free (NFR-02).
    /// </summary>
    public enum ErrorSeverity
    {
        Critical,
        Warning,
        Info,
        Resolved
    }
}
