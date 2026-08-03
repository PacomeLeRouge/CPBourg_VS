# Testing guide

## Current status

Version 1.30.0 does not contain an automated test project. The current release
gate is a successful build plus manual regression using
`CLIENT_TEST_CHECKLIST.md`. That is appropriate for a prototype demonstration,
but insufficient for production integration.

## Required checks for every change

1. Build Debug and Release with no errors.
2. Launch from a clean process and complete the splash sequence.
3. Exercise the changed workflow in windowed and maximized modes.
4. Test at 100% and the client-relevant Windows scaling percentage.
5. Test keyboard/mouse and touch-equivalent click paths.
6. Switch away from and back to the affected screen to expose stale state.
7. Repeat in every affected language and both measurement systems.
8. Restart when persistence or startup behavior changed.
9. Confirm known limitations remain accurate.

Use [Client test checklist](../CLIENT_TEST_CHECKLIST.md) for complete release
coverage and [Known limitations](../KNOWN_LIMITATIONS.md) for the safety boundary.

## High-risk regression matrix

| Area | Essential cases |
|---|---|
| Startup | success, simulated failure, visible progress, no hidden main window |
| Dashboard | Start interlocks, pause/resume, stop/restart, finite completion, unlimited preset, purge |
| Counters | plus/minus, keypad, pending confirmation, running lockout, legible readouts |
| Jobs | first job loaded, search, load, preset/custom creation, overwrite, removal, barcode match/miss |
| Logs | translated CSV headings, escaping, chosen units, writable/unwritable destination |
| STFO | per-job values, every keypad, Reset then leave, Save then leave, step scroll, unit change |
| Machine line | add position, duplicate prevention, remove, replace, cancel final code, confirm and Home sync |
| Settings | pending/applied/cancel, save failure, restart persistence, every language and unit |
| Errors | counts, detail, clear one, clear all, Home navigation, Start availability |
| Technician | load, Save, Confirm, Reset pending defaults, Back discard, code clearing |

## Suggested automated test structure

Create a separate test project rather than putting test-only dependencies in
the WPF executable:

```text
tests/
  CPBourg.NextGenGui.Tests/
    Models/
    Startup/
    Services/
    Localization/
```

Use a framework compatible with .NET Framework 4.7.2 and the approved build
environment. Keep most tests free of WPF; use STA/dispatcher-aware tests only
for behavior that cannot be extracted.

## First tests to add

### Pure model tests

- `MeasurementFormatter`: exact 25.4 conversion, formatting, speed symbols,
  round trips, zero, negative offsets, and boundary precision.
- `BookFormatCatalog`: each preset, 0.05 mm tolerance, custom dimensions, and
  case-insensitive lookup.
- `StfoJobSettings.CreateForFormat`: deterministic output and all clamps.
- `JobRepository`: first job current, load events, insert order, overwrite,
  current-job replacement, and current-job removal.
- settings stores: defaults, round trip, malformed XML, invalid tokens, missing
  directory, and write failure.
- `JobLogDialog.BuildCsv`: escaping, translated headings, comments, and unit
  formatting. This method is already internal and deterministic.

### Startup tests

- stage order and final progress;
- successful and failed probe results;
- cancellation during every delayed stage;
- null argument guards;
- a fake probe proving transport details do not leak into the sequencer.

### Extract-before-testing candidates

The following logic currently lives inside WPF code-behind. Extract it into
plain classes before adding broad automated coverage:

- production state transitions and button capabilities;
- counter working/confirmed transaction;
- STFO step snapshots and save/restore rules;
- machine-line working/confirmed transaction and duplicate validation;
- localization catalog completeness.

## State-machine test examples

For every state transition, assert both the resulting state and allowed
commands. Include rejected transitions, not just the happy path.

```text
Ready + Start + job + no errors + no pending counters -> Running
Ready + Start + active error                         -> Ready, rejected
Running + Pause                                     -> Paused
Paused + Start                                      -> Running
Paused + Stop                                       -> Stopped
Stopped + Purge                                     -> Ready, counters cleared
Running + finite preset reached                     -> Completed
```

## Localization verification

Add a catalog audit that gathers every English key used by code and asserts a
non-empty entry for all five translated dictionaries. Format strings must have
the same placeholder indexes as their source. A UI smoke test should traverse
each screen after repeated English/non-English switching and flag remaining
English source text where a translation is expected.

## Responsive visual verification

Record the tested matrix with every release:

- client windowed size;
- maximized/fullscreen size;
- Windows scaling/DPI;
- Small, Medium, and Large font setting;
- longest supported translation;
- dialogs, internal scroll regions, and persistent footer actions.

Prefer screenshot comparison for stable screens, but retain interaction tests
for scrolling, hit targets, focus, and disabled controls.

## Persistence isolation

Automated tests must never read or overwrite a developer's real LocalAppData
preferences. Both stores already expose internal path constructors; grant the
test assembly internal access or introduce an injected path/file abstraction,
then use a unique temporary directory for each test and remove it afterward.

## Release evidence

Before client delivery, retain:

- source commit and branch;
- build command and toolchain version;
- artifact hash;
- completed client checklist;
- tested resolution/DPI/language/unit matrix;
- known failures and accepted limitations;
- approval identifying the exact package tested.
