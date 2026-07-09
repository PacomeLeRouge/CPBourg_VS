namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Data for one machine/module on the Machine Line Configuration screen's
    /// Machine Line carousel. Plain data, no WPF dependency - same reasoning
    /// as MachineTileInfo.cs (NFR-02).
    /// </summary>
    public sealed class MachineLineItemInfo
    {
        public MachineLineItemInfo(string moduleType, string modelCode, string speed)
        {
            ModuleType = moduleType;
            ModelCode = modelCode;
            Speed = speed;
            IsRegistered = true;
        }

        public string ModuleType { get; set; }

        /// <summary>e.g. "BM-2000-01" - shown under the carousel image and in Selected Module.</summary>
        public string ModelCode { get; set; }

        public string Speed { get; set; }
        public bool IsRegistered { get; set; }
    }
}
