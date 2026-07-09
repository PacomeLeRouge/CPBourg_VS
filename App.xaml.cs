using System;
using System.Windows;
using CPBourg.NextGenGui.Startup;
using CPBourg.NextGenGui.Views;

namespace CPBourg.NextGenGui
{
    public partial class App : Application
    {
        private const string Version = "1.29.0";
        private const string Build = "2026-06";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            RunStartupAsync();
        }

        private async void RunStartupAsync()
        {
            var splash = new SplashWindow();
            splash.SetBuildInfo(Version, Build);
            splash.Show();

            // The UI updates itself from progress reports; IProgress marshals
            // the callback back onto the UI thread.
            var progress = new Progress<StartupProgress>(splash.Apply);

            // Swap SimulatedWfmConnectionProbe for the real TCP/CPBObjectComGUI
            // client when the simulator connection is implemented (FR-01).
            var wfmProbe = new SimulatedWfmConnectionProbe();
            var sequencer = new StartupSequencer(wfmProbe);

            bool ready;
            try
            {
                ready = await sequencer.RunAsync(progress);
            }
            catch (Exception ex)
            {
                splash.Apply(new StartupProgress(
                    StartupStage.ConnectingToWfm, 0, isError: true,
                    message: "Startup failed: " + ex.Message));
                return; // leave the splash showing the error for the demo
            }

            if (!ready)
            {
                // A phase failed (e.g. WFM unreachable). The splash is already
                // showing the error. A production build would offer Retry / Quit.
                return;
            }

            // Hand off to the operator dashboard, then dismiss the splash so the
            // application is never left window-less (which would exit it).
            var main = new MainWindow();
            MainWindow = main;
            main.Show();
            splash.Close();
        }
    }
}
