# Known limitations — version 1.30.0

This release is a user-interface and workflow prototype. It is not production
machine-control software.

- WFM connectivity is simulated. The application does not connect to a real
  CPBourg controller, PLC, safety circuit, or production line.
- Start, Pause, Stop, Purge, counters, speeds, machine states, and alerts are
  local simulations. They must not be used to operate or validate machinery.
- Jobs and job activity are seeded/in-memory prototype data and begin from the
  prototype's initial state when the application starts.
- Error records and machine availability are sample/simulated data.
- Barcode capture expects a scanner configured as a keyboard-wedge input
  device. Scanner drivers and serial/network protocols are outside this build.
- Job logs are local CSV exports. Automatic synchronization, retention,
  audit signing, and centralized log storage are not implemented.
- Date/time settings adjust the interface display only; they do not change the
  Windows system clock.
- Screen calibration is a prototype interaction and does not calibrate the
  Windows touch driver.
- The technical-access PIN demonstrates workflow protection and is not a
  production authentication or authorization system.
- The release is portable rather than installed and is not digitally signed.
  Windows SmartScreen behavior can therefore vary between client computers.
- The interface has been designed for the tested Windows desktop environment,
  but final industrial display resolution, DPI, touch hardware, and Windows
  image validation remain client acceptance activities.

Before production use, the simulated seams must be replaced with approved WFM
and machine integrations, security must be reviewed, and the complete system
must pass CPBourg safety, reliability, and acceptance testing.
