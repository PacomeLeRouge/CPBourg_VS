using System;
using System.Threading;
using System.Threading.Tasks;

namespace CPBourg.NextGenGui.Startup
{
    /// <summary>
    /// Runs the boot sequence and reports progress. Contains no UI code: it
    /// talks to callers only through <see cref="IProgress{T}"/>, so the same
    /// sequence can drive the splash, an automated test, or a headless run.
    /// </summary>
    public sealed class StartupSequencer
    {
        private readonly IWfmConnectionProbe _wfmProbe;

        public StartupSequencer(IWfmConnectionProbe wfmProbe)
        {
            _wfmProbe = wfmProbe ?? throw new ArgumentNullException(nameof(wfmProbe));
        }

        /// <summary>
        /// Executes all phases in order. Returns true if the application may
        /// proceed to the operator dashboard, false if a phase failed.
        /// </summary>
        public async Task<bool> RunAsync(
            IProgress<StartupProgress> progress,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            // Phase 1 - Initialize
            await BeginStageAsync(progress, StartupStage.InitializingSystem, 8, 400, cancellationToken)
                .ConfigureAwait(false);

            // Phase 2 - Load UI resources
            await BeginStageAsync(progress, StartupStage.LoadingUiResources, 32, 550, cancellationToken)
                .ConfigureAwait(false);

            // Phase 3 - Read machine configuration
            await BeginStageAsync(progress, StartupStage.ReadingMachineConfiguration, 55, 550, cancellationToken)
                .ConfigureAwait(false);

            // Phase 4 - Connect to the WFM (the one backend touch-point)
            progress.Report(new StartupProgress(StartupStage.ConnectingToWfm, 70));
            WfmConnectionResult wfm = await _wfmProbe.ConnectAsync(cancellationToken).ConfigureAwait(false);
            if (!wfm.Success)
            {
                progress.Report(new StartupProgress(
                    StartupStage.ConnectingToWfm, 70, isError: true, message: wfm.Message));
                return false;
            }
            progress.Report(new StartupProgress(StartupStage.ConnectingToWfm, 85, message: wfm.Message));

            // Phase 5 - Open the operator dashboard
            await BeginStageAsync(progress, StartupStage.OpeningDashboard, 100, 400, cancellationToken)
                .ConfigureAwait(false);

            return true;
        }

        private static async Task BeginStageAsync(
            IProgress<StartupProgress> progress,
            StartupStage stage,
            int percentWhenDone,
            int simulatedWorkMs,
            CancellationToken cancellationToken)
        {
            progress.Report(new StartupProgress(stage, Math.Max(0, percentWhenDone - 6)));

            // Placeholder for the real work of each phase (loading config files,
            // building resources, etc.). Simulated here so the sequence is visible.
            await Task.Delay(simulatedWorkMs, cancellationToken).ConfigureAwait(false);

            progress.Report(new StartupProgress(stage, percentWhenDone));
        }
    }
}
