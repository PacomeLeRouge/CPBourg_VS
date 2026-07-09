namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Coarse machine/module status shown on a dashboard tile.
    /// This mirrors the Idle / Ready / Running / Error / JAM states the WFM
    /// tracks (PRD 3.2) but is simplified to what the Home dashboard tile
    /// needs to display at a glance. The detailed state machine belongs on
    /// the per-module tab (e.g. the STFO tab), not here.
    /// </summary>
    public enum MachineStatus
    {
        Running,
        Idle,
        Offline
    }
}
