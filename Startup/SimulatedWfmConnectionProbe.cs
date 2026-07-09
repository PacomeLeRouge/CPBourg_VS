using System.Threading;
using System.Threading.Tasks;

namespace CPBourg.NextGenGui.Startup
{
    /// <summary>
    /// Prototype stand-in for the WFM connection.
    ///
    /// Replace with a real probe that connects to the CPBourg line simulator
    /// (and later the WFM) over TCP on port 5150 and performs a minimal
    /// CPBObjectComGUI handshake. Because it implements <see cref="IWfmConnectionProbe"/>,
    /// swapping it in requires no change to the splash or the sequencer.
    ///
    /// To exercise the error path (FR-06), set <see cref="SimulateFailure"/> to true.
    /// </summary>
    public sealed class SimulatedWfmConnectionProbe : IWfmConnectionProbe
    {
        public bool SimulateFailure { get; set; }

        public async Task<WfmConnectionResult> ConnectAsync(CancellationToken cancellationToken)
        {
            // Pretend a handshake takes a moment; honour cancellation.
            await Task.Delay(650, cancellationToken).ConfigureAwait(false);

            if (SimulateFailure)
            {
                return WfmConnectionResult.Failed("WFM unavailable - check the simulator is running");
            }

            return WfmConnectionResult.Ok();
        }
    }
}
