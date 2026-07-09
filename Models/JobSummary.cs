namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Summary of a job as shown on the dashboard's "Current Jobs" card.
    /// This is prototype sample data (FR-08 job save/load lives on the
    /// dedicated Jobs screen, not implemented here yet).
    /// </summary>
    public sealed class JobSummary
    {
        public JobSummary(string name, string statusLabel, int quantity, int completed)
        {
            Name = name;
            StatusLabel = statusLabel;
            Quantity = quantity;
            Completed = completed;
        }

        public string Name { get; }
        public string StatusLabel { get; }
        public int Quantity { get; }
        public int Completed { get; }
    }
}
