using System.Threading;
using System.Threading.Tasks;

namespace CPBourg.NextGenGui.Startup
{
    /// <summary>
    /// The single seam between this front-end prototype and the WFM backend.
    ///
    /// The splash's "Connecting to WFM" phase calls this - and ONLY this - to
    /// reach the backend. The real implementation will open the two TCP streams
    /// (base port 5150 + return stream) and exchange serialized CPBObjectComGUI
    /// business objects. Keeping that behind an interface satisfies:
    ///   - FR-01  connect to the existing WFM without modifying it
    ///   - NFR-02 clean separation of the UI layer from the WFM comms layer
    ///   - NFR-03 the UI can be tested against a simulated probe with no machine
    /// Do not let TCP or serialization details leak outside implementations of
    /// this interface.
    /// </summary>
    public interface IWfmConnectionProbe
    {
        Task<WfmConnectionResult> ConnectAsync(CancellationToken cancellationToken);
    }

    /// <summary>Outcome of a WFM connection attempt.</summary>
    public sealed class WfmConnectionResult
    {
        private WfmConnectionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; }
        public string Message { get; }

        public static WfmConnectionResult Ok() =>
            new WfmConnectionResult(true, "Connected to WFM");

        public static WfmConnectionResult Failed(string message) =>
            new WfmConnectionResult(false, message);
    }
}
