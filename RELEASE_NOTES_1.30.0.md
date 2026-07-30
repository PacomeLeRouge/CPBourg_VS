# CPBourg NextGen Operator Interface 1.30.0

Release candidate date: 2026-07-30

Platform: Windows x64

Runtime: .NET Framework 4.7.2

## Release purpose

This build is an interactive prototype for client evaluation. It consolidates
the dashboard, job, STFO/BBM, machine-line, settings, errors, and technician
work completed during the summer 2026 prototype cycle.

## Highlights

- Responsive Home dashboard for windowed and fullscreen operation.
- Counter and productivity editing, target handling, and simulated production
  Start/Pause/Stop/Purge behavior.
- Job list, current-job linking, preset/custom book formats, barcode capture,
  and CSV job-log export.
- Job-specific STFO Stitching, Folding, Trimming, and Conveyor configuration
  with touchscreen numeric keypads, scrolling, Save, and Reset behavior.
- Clearer BBM paper-path, folding-bypass, booklet-length, trim-strip, and
  clamp-pressure information.
- Machine-line add/remove/replace workflow with duplicate prevention and a
  technician PIN required only for final confirmation.
- Persisted operator preferences and technician settings.
- Metric and imperial measurements.
- English, French, Dutch, German, Spanish, and Italian user-interface text
  across Home, Jobs, STFO, Machine Line Configuration, Errors, Settings, and
  Technician Interface screens.

## Installation and launch

1. Copy or extract the complete release directory. Do not copy only the EXE;
   the configuration file and `Assets` directory belong with it.
2. Confirm that .NET Framework 4.7.2 or later is installed.
3. Run `CPBourg.NextGenGui.exe`.
4. If Windows marks a downloaded ZIP as blocked, open its Properties and
   select **Unblock** before extracting it.

The release is portable and does not install files into Program Files. Operator
preferences and technician settings are stored under:

`%LOCALAPPDATA%\CPBourg\NextGenGui`

## Client feedback

Please include the following with every issue:

- Application version (`1.30.0`)
- Windows version
- Display resolution and scaling percentage
- Windowed or fullscreen mode
- Selected language, units, and font size
- Loaded job and machine state
- Exact steps to reproduce
- Screenshot or short recording
- Whether the issue remains after restarting the application

Complete the accompanying `CLIENT_TEST_CHECKLIST.md` before acceptance.
