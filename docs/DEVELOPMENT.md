# Development guide

## Prerequisites

- Windows 10 or later
- Visual Studio 2019 or later with **.NET desktop development**, or a compatible
  `dotnet` SDK
- .NET Framework 4.7.2 Developer Pack / reference assemblies
- Git

The project is an SDK-style WPF project targeting `net472` with C# 7.3. Keep
new language features compatible with that version unless the target framework
and deployment image are intentionally upgraded together.

## Build and run

From the repository root:

```powershell
dotnet restore CPBourg.NextGenGui.csproj
dotnet build CPBourg.NextGenGui.csproj --configuration Debug
dotnet run --project CPBourg.NextGenGui.csproj
```

In Visual Studio, open `CPBourg.NextGenGui.csproj`, select Debug and Any CPU or
x64 as appropriate for the workstation, and press F5.

The normal startup includes a short simulated connection sequence. A successful
probe opens `MainWindow`; a failed probe deliberately leaves the splash visible.

## Project layout

```text
App.xaml(.cs)                  Application resources and startup composition
CPBourg.NextGenGui.csproj      Target framework, assembly metadata, asset copy
Theme/                         Shared WPF resources and styles
Assets/                        Runtime image assets copied to output
Models/                        Plain data, repositories, stores, measurement rules
Startup/                       UI-independent startup sequence and WFM probe seam
Views/                         Screens, dialogs, converters, localization, UI helpers
HighFidelity/, LowFidelity/    Design references only
docs/                          Maintainer documentation
```

See [Architecture](ARCHITECTURE.md) for ownership and event flows.

## Change workflow

1. Create a focused branch from the latest approved `master`.
2. Record the current behavior and reproduce the issue before editing.
3. Change the smallest owning component. Avoid duplicating shared state in
   sibling views.
4. Update localization for every new operator-facing source string.
5. Preserve canonical millimeter values for measurements.
6. Update the relevant file in `docs/` when an invariant or integration seam
   changes.
7. Build Debug and Release.
8. Run the relevant sections of `CLIENT_TEST_CHECKLIST.md` in both windowed and
   maximized modes.
9. Confirm `KNOWN_LIMITATIONS.md` and release notes still describe the build.

## WPF conventions used here

- XAML owns layout, resource references, control names, and event hookups.
- Code-behind owns screen-local interaction and view state.
- Plain model classes must not reference WPF.
- Main screens request navigation through events; `MainWindow` owns routing.
- Embedded dialog user controls raise typed events on confirmation.
- Use resources from `Theme/BrandTheme.xaml`; avoid hard-coded brand colors.
- Use internal scrolling for long task panels while keeping persistent footer
  actions visible.
- Preserve touch targets when changing font size or responsive layout.

## Adding a screen

1. Add `Views/<Name>View.xaml` and its code-behind.
2. Place one instance in the content area of `MainWindow.xaml`, initially
   collapsed.
3. Add it to `HideContentScreens` and `NavigateTo`.
4. Add the global menu item if it is globally accessible.
5. Raise navigation intent events from the view instead of referencing the
   shell directly.
6. Apply language, font, and units through the existing shell paths when the
   screen contains affected content.
7. Add windowed, maximized, and translated cases to the test checklist.

## Adding or changing a dialog

Dialogs in this prototype are overlay `UserControl` instances rather than new
Windows. The parent embeds the dialog in XAML, calls `Open`, and subscribes to a
typed confirmation event. On close, clear sensitive or stale input and collapse
the overlay. Reapply localization when opening because the language may have
changed since construction.

## Persistence

Do not write next to the executable. Installed or extracted application folders
may be read-only. Per-user configuration belongs under:

```text
%LOCALAPPDATA%\CPBourg\NextGenGui
```

Stores must validate values, fall back safely when reading malformed data, and
return a useful error without partially applying pending settings when saving
fails. If a schema changes, add explicit migration or version handling; do not
silently reinterpret an existing token.

## Versioning

The visible build version currently appears in both `CPBourg.NextGenGui.csproj`
and `App.xaml.cs`. For a release:

1. update `Version`, `AssemblyVersion`, `FileVersion`, and
   `InformationalVersion` in the project;
2. update the splash version/build constants;
3. create or update release notes and known limitations;
4. update the operator guide and client checklist;
5. build the exact release configuration and test the packaged output.

Centralizing version metadata is recommended before production.

## Publishing the portable prototype

A framework-dependent Windows x64 package can be produced with:

```powershell
dotnet publish CPBourg.NextGenGui.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained false
```

Distribute the complete publish directory, including configuration files and
the `Assets` folder. The target PC requires .NET Framework 4.7.2 or later.
This prototype is not digitally signed and has no installer or automatic update
mechanism.

## Debugging tips

- Startup failure: set a breakpoint in `StartupSequencer.RunAsync` and inspect
  the `WfmConnectionResult`.
- Stale job title: verify `JobRepository.CurrentJobChanged` reaches
  `MainWindow.SyncCurrentJob` and `StfoConfigurationView.LoadJob`.
- Start disabled: check current job, total active alerts, pending counter edits,
  and production state in that order.
- Text not translated: verify the English source key exists and dynamically
  generated text is rebuilt by the view's `ApplyLanguage` method.
- Wrong dimensions after unit change: check that the model remains millimeters
  and conversion happens only through `MeasurementFormatter`.
- Settings revert: inspect the two XML files under LocalAppData and the error
  banner produced by the relevant store.

## Documentation maintenance

The root README is the project and release entry point. Durable implementation
knowledge belongs in `docs/`. XML comments should explain public contracts,
ownership, state transitions, units, persistence, or security boundaries. Do
not use comments as a substitute for extracting testable services when code
becomes too complex.
