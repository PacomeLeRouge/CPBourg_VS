# CPBourg NextGen Operator Interface — Prototype

High-fidelity splash screen for the modernized CPBourg operator interface, built
in **C# / WPF** on **.NET Framework 4.7.2** (Windows 10 IoT Enterprise compatible).

This is the first screen of the NextGen GUI prototype described in the PRD
(`PRD_OpenHub_GUI_CPbourg_v1_1`). It renders the five-phase boot sequence and
hands off to a placeholder operator dashboard.

---

## Build & run

Requires Visual Studio 2019+ (or `dotnet` SDK) on Windows with the
.NET Framework 4.7.2 developer pack.

```
# from the project folder
dotnet build
dotnet run
```

Or open the folder in Visual Studio and press F5.

> **Targeting note.** The current CPBourg GUI targets .NET Framework 4.5.2.
> This prototype targets 4.7.2, which is an in-place, backward-compatible
> update — a machine with 4.7.2 runs 4.5.2-targeted apps. If your industrial
> image is locked to 4.5.2 and cannot be updated, change `<TargetFramework>`
> in the `.csproj` to `net452` and convert to a classic (non-SDK) project file.
> All WPF APIs used here have existed since 4.5, and the C# is written to
> language version 7.3.

---

## Architecture

The design keeps the **UI**, the **boot logic**, and the **WFM backend** in
separate, testable pieces — the separation the PRD asks for (NFR-02).

```
App.xaml.cs
  │  shows the splash, then runs the sequence, then opens the dashboard
  ▼
Startup/StartupSequencer      ← drives the 5 phases, reports IProgress<StartupProgress>
  │                             (no UI code — testable headless)
  ├─► Views/SplashWindow        ← "dumb" view: renders whatever progress it's handed
  │
  └─► Startup/IWfmConnectionProbe ← the ONLY seam to the backend
          ├─ SimulatedWfmConnectionProbe   (used now)
          └─ (real TCP / CPBObjectComGUI client — drops in later)
```

### Where the WFM connects

The "Connecting to WFM" phase calls `IWfmConnectionProbe` and nothing else.
To wire in the real backend (FR-01), implement that interface with a client
that opens the two TCP streams (base port 5150 + return stream) and exchanges
serialized `CPBObjectComGUI` objects, then swap it in one line in `App.xaml.cs`:

```csharp
var wfmProbe = new WfmConnectionProbe("127.0.0.1", 5150); // instead of the simulated one
```

No change to the splash or the sequencer is needed.

### Re-skinning

Every colour, brush and font lives in `Theme/BrandTheme.xaml`. The accent
colour (`#E2521A`), the grey secondary mark colour, and the two-square logo
mark are **placeholders** — replace them with the values from the official
CPBourg website graphic charter once marketing confirms them (UX-04). No
colour is hard-coded anywhere else.

### v2 — light mode

The splash was redesigned to a light-mode look at **1024x768 (4:3)**, matching
an approved reference mock: a light screen background, a two-square logo mark
(grey square offset behind an orange square), bold black company name, and a
simple dark progress bar on a light track with a status line above it. The
step-dot indicator and percentage readout from the first pass were dropped to
keep the screen as clean as the reference.

### v3 — scales to the real screen

The window now runs maximized and everything is authored on a fixed 1024x768
design canvas wrapped in a `Viewbox`. WPF scales the whole splash - logo,
text, progress bar - as one unit to fill whatever monitor it actually runs
on, so it looks correct on a 1080p or 4K industrial display instead of
appearing small in the corner of a much bigger screen. `Stretch="Uniform"`
keeps the 4:3 proportions and letterboxes if the real screen isn't 4:3; once
CPBourg confirms the exact industrial PC resolution, either switch to
`Stretch="Fill"` (if its aspect ratio matches 4:3) or change the 1024x768
canvas size to match it directly.

### v4 — Home dashboard

`MainWindow` is now the application shell rather than a placeholder. It has
three parts:

| Part | File | Purpose |
|------|------|---------|
| Header bar | `Views/MainWindow.xaml` (top row) | Hamburger, logo, page title, language selector (UI only, FR-10), live clock |
| Dashboard content | `Views/DashboardView.xaml(.cs)` | Counter & Productivity, Machines, action bar (Purge/Start/Pause/Stop), Current Jobs, Active Alerts |
| Global menu overlay | `Views/GlobalMenuView.xaml(.cs)` | Slide-out nav, grouped General / Configuration / Advanced / Help, opened by the hamburger button |

New supporting types:

- `Models/MachineStatus.cs`, `Models/MachineTileInfo.cs`, `Models/JobSummary.cs` -
  plain data types with **no WPF dependency**, so they stay reusable if a
  different UI layer is ever chosen (NFR-02).
- `Views/MachineStatusToBrushConverter.cs` - maps `MachineStatus` to the
  theme's status brushes. Kept in `Views`, not `Models`, so the model layer
  has no `System.Windows.Media` reference.

**Dashboard machine/job data is still sample data.** The Counter and
Productivity values now use functional local state: completed sets starts at
zero, completed/preset values support +/- and keypad entry, and preset zero is
unlimited. Replace these local values with WFM-reported counters once the
connection exists (FR-01, FR-02).

**Dashboard controls are functional prototype controls.** Counter changes,
Start/Pause/Stop/Purge, and their visible status feedback work locally; job
and error buttons navigate to their implemented screens. The machine-command
handlers remain deliberately isolated in `DashboardView.xaml.cs` so their
local behavior can be replaced with real WFM commands for FR-03.

**Global menu navigation is also a stub.** Only Home (this dashboard) exists
today; other items just update the header's page title as a placeholder.
Wire `GlobalMenuView.ItemSelected` in `MainWindow.xaml.cs` to real navigation
once those screens exist.

**Colour note:** the reference mock uses blue for the "New Job" button while
the logo and splash use orange. As of v6 (below), "New Job" now uses a
dedicated blue `JobsAccentBrush` to match the reference, while everything
else (Pause, the logo, the splash) keeps the orange brand accent - so there
are intentionally two accent colours in this build, not one.

**Icons** use the `Segoe MDL2 Assets` font, which ships with Windows 10
(including IoT) - no extra font file to deploy. If any glyph renders as a
box on your machine, the codepoint can be swapped in the XAML; see the
`IconFontFamily` resource in `Theme/BrandTheme.xaml`.

### v5 — fluid layout for tablet, closer to the reference

Two problems reported after the first dashboard pass, both fixed:

1. **Empty space on the sides in windowed/fullscreen mode.** `MainWindow`
   used to wrap everything in a `Viewbox` locked to a fixed 1024x768 (4:3)
   canvas. On any wider screen - which is virtually every tablet and
   monitor - `Stretch="Uniform"` preserved that 4:3 shape and padded the
   rest; since the padding colour matched the background, it read as "empty
   space" rather than an obvious letterbox bar. **Fixed** by removing the
   Viewbox and fixed canvas entirely. The header and `DashboardView` were
   already built with proportional (star-sized) columns, so they now
   genuinely stretch to fill whatever screen this runs on.

2. **Tablet-appropriate sizing.** The production interface runs on a tablet
   mounted on the machine, not a desktop monitor. Interactive elements
   across the header, dashboard, and global menu were resized to 44px+
   minimum touch targets (hamburger button, preset +/- buttons, card
   buttons, nav items), consistent with the reference's own "Touch Friendly:
   large controls" note.

Also tightened toward the reference mock:

- Introduced a shared `CardStyle` (rounded 12px corners + a soft drop
  shadow) in `Theme/BrandTheme.xaml`, applied to every dashboard card. This
  replaces the flatter 8px-corner, no-shadow cards from the first pass and
  reads much closer to the mock's elevated-card look.
- Fixed a few icon glyphs that rendered as blank/hollow boxes (Set Target,
  View Errors, and the job-card icon all used the same ambiguous codepoint;
  swapped to a clearer "list" glyph, `\uE7C3`).
- Start / Pause / Stop in the action bar now sit inside a coloured circle
  outline, matching the reference's circular badge treatment, instead of a
  bare icon next to the label.
- Language selector is now borderless/flat, closer to the reference's plain
  "EN ▾" text rather than a boxed dropdown control.

### v6 — layout and colour refinements

Four specific fixes from a design review against the reference mock:

1. **Counter & Productivity: preset controls moved closer to the number.**
   The `-`/`+`/Reset-to-zero cluster used to sit in a `Grid` column that
   absorbed all leftover card width, pinning it to the far right edge. It's
   now a single left-aligned `StackPanel` (number, then a fixed 48px gap,
   then the preset cluster), so it sits close to "Completed Sets" instead of
   floating off to the side.
2. **Completed Sets / Preset / Output-per-hour each got their own box.**
   These three mini-stats used to be plain icon+text rows with no visual
   container. Each is now wrapped in its own light, rounded `Border`,
   matching the reference's boxed treatment.
3. **Current Jobs now uses blue instead of orange.** Added a
   `JobsAccentBrush` token (`Theme/BrandTheme.xaml`) and pointed the "New
   Job" button at it, so job actions are visually distinct from the
   orange-accented machine-control actions (Pause) elsewhere on the
   dashboard.
4. **Global menu section headers (General/Configuration/Advanced/Help)** are
   now bold and coloured with `JobsAccentBrush` (blue), each followed by a
   solid 2px rule line for a clear section break. Nav item labels are
   `SemiBold` and use `TextPrimaryBrush` instead of the muted secondary
   colour, for a more pronounced, legible look on a tablet.

### v7 — merged stat box, static language label, real logo support, windowed-mode fix

1. **Completed Sets / Preset / Output-per-hour merged into one box.** These
   were three separate boxes in v6; they're now a single bordered box with
   thin vertical divider lines between the three sections, matching the
   latest feedback.
2. **Language indicator is now static, not a dropdown.** Switching language
   will happen on a dedicated Settings screen (still FR-10, not yet built),
   not from this header - so the `ComboBox` was replaced with a plain
   non-interactive "EN" label. A vertical divider was added between it and
   the clock/date, matching the divider already used elsewhere in the
   header.
3. **Real CPBourg logo support.** The two-square mark was always a
   placeholder. `Views/LogoLoader.cs` now checks for a real logo file at
   `Assets/cpbourg-logo.png` (relative to the built .exe) on startup, on
   both the splash screen and the app header, and swaps it in automatically
   if present - falling back to the placeholder mark cleanly if not. See
   `Assets/README.md` for exactly where to get the official file and how to
   add it.
   **Why this needed a manual step:** the actual logo file couldn't be
   downloaded directly into this project - the sandbox this was built in
   only has network access to a small allowlist of developer package
   registries (npm, PyPI, GitHub, etc.), not general websites like
   cpbourg.com. An official source was found
   (`https://www.cpbourg.com/files/Library/CPBOURG-logo.pdf`) and is
   documented in `Assets/README.md`, but retrieving and converting it is a
   step you'll need to do locally.
4. **Fixed buttons getting cut off in windowed mode.** Added
   `MinWidth="1180" MinHeight="760"` to `MainWindow`. Previously the window
   could be resized smaller than the header/action bar's natural minimum
   size, and since `Grid` columns don't wrap or scroll, content just got
   clipped by the window bounds. The window now can't shrink below a size
   that fits everything.

### v8 — Settings / Preferences screen, and a new sizing baseline

**Sizing baseline updated.** `DashboardView.xaml` and `GlobalMenuView.xaml`
were hand-edited outside this chat to noticeably enlarge touch targets and
type: buttons went from ~44px to ~52-56px tall, icon circles from ~34-44px
to ~46-52px, icon glyphs from ~14-18px to ~20-30px, and repeated text
patterns were extracted into named `Style` resources (e.g.
`GlobalMenuItemTextStyle`) rather than repeating inline properties on every
`TextBlock`. This is now the baseline for all new screens, including
Settings below - **note this for future iterations**, since earlier v1-v7
sizes in this document are now smaller than the current, larger standard.

**New: `Views/SettingsView.xaml(.cs)`** - the Settings / Operator Preferences
screen (FR-09), reachable from the global menu's "Settings / Preferences"
item (previously a title-only stub; now real navigation - see
`MainWindow.xaml.cs` `OnGlobalMenuItemSelected`). Two sections, "Language
and Region" and "Display", each a card of rows (icon, label, current value,
Change/Calibrate button), matching the reference layout.

**Demonstrates all three reference states as one interactive screen**,
rather than three static mockups:
- **Default** - no banner.
- **Unsaved changes** - clicking any row's Change/Calibrate button shows an
  amber banner ("Unsaved changes / Please apply changes before exiting").
- **Preferences saved** - clicking Apply shows a green banner ("Preferences
  saved"). Clicking Cancel, or the banner's own close (x), returns to
  Default.

No setting is actually changed or persisted yet - this only demonstrates the
three visual states. Real preference storage and the "what happens when you
click Change" flow (the layout the person plans to share next) are follow-up
work.

**New model:** `Models/SettingsItemInfo.cs` - plain data for one row, with
an `IsLetterIcon` flag for the one row whose "icon" is literal text ("Aa"
for UI Scale) rather than a Segoe MDL2 Assets glyph. This matters: icon
fonts remap ordinary letters to unrelated icon shapes, so text-as-icon must
render in the normal UI font (`BrandFontFamily`), never the icon font - the
XAML handles this with a `DataTrigger` that switches `FontFamily` based on
the flag.

**Icon note:** several row icons are new codepoints not yet visually
confirmed on a real Windows build (Language/Globe, Date & Time/Calendar,
Keyboard Layout, Mouse Cursor) - flagged in a comment at the top of
`SettingsView.xaml`. If any renders as a blank box, that's the first place
to look; the codepoint just needs swapping for a confirmed-working one from
elsewhere in the project (see the icon list in that comment).

**New theme tokens:** `WarningBrush` / `WarningBgBrush` (amber, for the
Unsaved Changes banner) in `Theme/BrandTheme.xaml`.

### v9 — fixed unreachable rows in windowed mode

Real bug: the preference sections had no scroll fallback. In windowed mode
(or on any screen shorter than the content), the bottom rows - Mouse
Cursor, UI Scale, Screen Calibration - simply ran off the bottom of the
window with no way to reach them except maximizing. It got worse the
moment a banner appeared, since the banner adds height above the sections
and pushes everything else down further with nowhere for the extra height
to go.

**Fixed** by wrapping the two preference-section cards in a `ScrollViewer`
(`VerticalScrollBarVisibility="Auto"`). The title row, Cancel/Apply
buttons, and status banners stay fixed at the top; only the Language and
Region / Display sections scroll. Every row is reachable regardless of
window size or banner state now.

### v10 — Change-value picker dialogs

**New: `Views/ChangeValueDialog.xaml(.cs)`** - one reusable modal picker
used by four of the seven Settings rows: Language, Units, Keyboard Layout,
Mouse Cursor (the four with a picker mockup so far). Centered card with a
title, "Current: X" subtitle, a scrollable radio-button list, and
Cancel/OK - built once and reused rather than as four separate dialogs,
matching the "extract repeated patterns" convention already used for
`CardStyle` and the global menu's text styles.

- Clicking "Change" on one of those four rows opens the dialog pre-selected
  to the current value; clicking OK updates the row and shows the amber
  Unsaved Changes banner; Cancel (or clicking the scrim) closes without
  changing anything.
- Date & Time, UI Scale, and Screen Calibration don't have a picker mockup
  yet, so their "Change"/"Calibrate" buttons remain simple dirty-marking
  stubs, same as before.
- **New: `Views/OptionRadioButtonStyle`** (in `Theme/BrandTheme.xaml`) - a
  custom `RadioButton` template (outlined circle, fills with a solid dot
  when checked) matching the reference's radio list, since the default WPF
  radio button doesn't look like that.
- **Language flags use real Unicode flag emoji** (e.g. 🇬🇧 🇫🇷 🇳🇱), rendered
  through `Segoe UI Emoji`, deliberately **not** a Segoe MDL2 Assets icon
  glyph - flags aren't in that icon font, and guessing a codepoint there
  risked another blank-box icon (as happened earlier in this project). Flag
  emoji are standard Unicode with a built-in Windows color font, so this is
  the safer choice; each flag's UTF-16 surrogate-pair sequence was checked
  by hand against the regional-indicator formula.
- **New model:** `Models/ChangeOptionInfo.cs` - one radio option (value,
  label, pre-selected flag, optional flag emoji).
- **New converter:** `Views/NullToCollapsedConverter.cs` - hides the flag
  `TextBlock` for options that don't have one, without needing a
  WPF-typed property on the plain model (same reasoning as
  `MachineStatusToBrushConverter`).

Current values (`_language`, `_units`, etc.) are tracked in private fields
in `SettingsView.xaml.cs` and genuinely update when a dialog is confirmed -
but nothing is persisted to disk or applied to the system yet.

### v11 — header language sync, live Date & Time

1. **Header language indicator now syncs with the Settings language
   picker.** Changing Language in Settings raises a new
   `SettingsView.LanguageChanged` event carrying a two-letter abbreviation
   (e.g. "FR"), which `MainWindow.xaml.cs` uses to update the header's
   language label (`LanguageIndicatorText`, now named so it can be updated
   - it was an unnamed static `TextBlock` before). This **only** updates
   that header label, not the rest of the UI's language - full translation
   is still future work under FR-10, as before.
2. **Date & Time row now shows the real current time**, matching the
   header clock, instead of a fixed sample value. `SettingsView` now runs
   its own one-second `DispatcherTimer` (same pattern as the header clock
   in `MainWindow.xaml.cs`) so the row keeps ticking while the Settings
   screen is open, rather than showing a snapshot from when the screen
   loaded.

### v12 — header only updates on Apply, and Cancel actually discards changes

The previous version updated the header language label the instant a
language was picked in the dialog - before Apply was even clicked. Fixed by
introducing a proper **pending vs applied** distinction for all four
picker-backed settings (Language, Units, Keyboard Layout, Mouse Cursor):

- **Pending** value: what you just picked. Shown in the settings row
  immediately, so you can see your selection, and marks the form dirty
  (amber banner) - but doesn't take effect anywhere else yet.
- **Applied** value: what's actually in effect. Only updated when **Apply**
  is clicked, which is also the only time `LanguageChanged` fires and the
  header updates.
- **Cancel now actually does something**: it discards pending changes back
  to the last applied values and refreshes the rows, instead of only
  hiding the banner while leaving unapplied selections sitting in the rows
  indefinitely. This fix applies uniformly to all four picker-backed
  settings, not just Language - the same "Cancel does nothing" gap existed
  for Units/Keyboard Layout/Mouse Cursor too, just less visible since
  nothing else read their values yet.

### v13 — Jobs / File Menu screen

**New: `Views/JobsView.xaml(.cs)`** - the Jobs / File Menu screen (FR-08 job
save/load, AC-06), reachable from the global menu's "Job / File Menu" item
(previously a title-only stub; now real navigation, alongside Home and
Settings - see `MainWindow.xaml.cs` `OnGlobalMenuItemSelected`).

Two columns, matching the reference:

- **Left: Saved Jobs list + Status.** A `ListBox` (not the plain
  `ItemsControl` used elsewhere) since this screen genuinely needs
  selection state - clicking a job updates the Summary panel on the right.
  Selection is shown with a left accent bar + light-blue background via a
  custom `ItemContainerStyle`, replacing the default WPF selection chrome
  entirely. The search box does a real live filter on job name (no server
  round-trip needed for 5 sample rows); the filter (funnel) button is a
  stub. Status is a green banner, same pattern as `SettingsView`'s Saved
  banner.
- **Right: Summary + six action tiles.** Summary shows the selected job's
  Name / Pages / Format / Comment / Barcode ID / Last modified. "Open Job"
  is the filled blue primary action (`JobsAccentBrush`); the other five
  (Save as New, Remove Job, View Log, Scan Barcode ID, Add Comment) are
  outline tiles with a blue icon - all six are stubs, same
  "Last action: ..." feedback pattern as `DashboardView`.

**New model:** `Models/JobRecord.cs` - plain data for one saved job.

**Icon note:** six icons here are new codepoints not yet visually confirmed
on a real Windows build - flagged in a comment at the top of
`JobsView.xaml`: search (magnifying glass), filter (funnel), save-as-new,
remove/trash, scan barcode, add comment. This screen has more unverified
icons than usual since it needed several concepts (search, filter, trash,
barcode, comment) that hadn't come up in earlier screens - worth a careful
look on the first real Windows test.

Sample data (5 jobs) is hard-coded in `JobsView.xaml.cs`. Nothing is
actually saved, loaded, removed, or scanned yet - replace with real
WFM/job-storage data once that's wired in, keeping the same
`JobRecord`/`ListBox` binding structure.

### v14 — job action dialogs (Add Comment, Open Job, Save As New, Remove)

Four of the Jobs screen's six action tiles now open real, functional
dialogs instead of showing a stub feedback line - matching the reference
mock's job-action screens. View Log and Scan Barcode ID don't have a
dialog mockup yet and remain simple stubs.

- **`Views/AddCommentDialog.xaml(.cs)`** - edits the selected job's comment
  (max 4 lines). Saving genuinely updates `JobRecord.Comment` in place
  (now a mutable property - was read-only before, since nothing needed to
  change it) and refreshes the Summary panel, then shows a shared success
  confirmation.
- **`Views/OpenJobDialog.xaml(.cs)`** - shows the selected job's name and
  format, plus a "Load saved RUN adjustments" checkbox. No success
  confirmation for this one, matching the reference (it just closes with a
  stub feedback line - opening a job would presumably navigate to the
  machine tab in a real build, once that exists).
- **`Views/SaveAsNewJobDialog.xaml(.cs)`** - the most involved of the four:
  one dialog handles **two states** rather than being two separate dialogs,
  so the typed job name survives switching between them:
  - **Input state**: normal name-entry form, pre-filled with
    "`[job] - Variant 1`".
  - **Conflict state** ("Job Name Already Exists!"): shown automatically if
    Save Job is clicked with a name that matches an existing job. "New
    Name" returns to the input state without losing what was typed; "Save
    Job" here means confirm overwrite.
  - The "Current Setup" (Format / Machine Line) shown in this dialog and in
    Remove Job is a **hard-coded stand-in for the live machine's current
    configuration** (not the archived job's own stored format) - this
    matches the reference mock, which shows identical values regardless of
    which job is involved. It's a placeholder until the WFM connection
    provides real live machine state (FR-01, FR-02).
- **`Views/RemoveJobDialog.xaml(.cs)`** - destructive confirmation with a
  red "cannot be reverted" banner and a red Remove Job button (reusing
  `StatusErrorBrush`, plus a new `StatusErrorBgBrush` token added to the
  theme instead of an inline hex colour - keeping with the "no hard-coded
  colours" convention).
- **`Views/ConfirmationDialog.xaml(.cs)`** - one shared success dialog
  (green checkmark circle, title, message, OK), reused for all three "...
  Saved!" / "... Removed!" screens rather than three near-identical
  dialogs - same reasoning as `ChangeValueDialog` on the Settings screen.

**A data-binding detail worth knowing:** the job list uses a plain
`List<JobRecord>`, which doesn't automatically notify the UI when items are
added or removed (unlike `ObservableCollection<T>`). `RefreshJobsList()` in
`JobsView.xaml.cs` works around this by clearing `ItemsSource` to `null`
and reassigning the same list reference, which forces the `ListBox` to
re-render. This is intentional, not an oversight - flagged here in case
future edits assume the list is auto-observable.

### v16 — Errors & Information screen

**New: `Views/ErrorsView.xaml(.cs)`** - the Errors & Information screen
(FR-06 error/warning management, AC-05), reachable from the global menu's
"Error & Information" item (previously a stub; now real navigation
alongside Home, Settings, and Jobs). Sample data reuses the PRD's own
"cover open" example almost verbatim (BPM Module 3/4/6), which was a nice
sign this screen maps cleanly onto the actual requirement.

- **Four summary tiles** (Critical / Warning / Info / Resolved) with live
  counts computed from the message list.
- **Active Messages list** - a `ListBox` (needs click/select behaviour,
  same reasoning as `JobsView`'s job list) showing Severity, Source,
  Module/Job, Time, and Details per row, with a count badge next to the
  section title.
- **Empty state** - swaps to a centred "No active messages" panel with a
  muted icon when the list is cleared, matching the reference's empty-inbox
  mock.
- **Row click -> `Views/ErrorDetailDialog.xaml(.cs)`** - a detail overlay
  with Severity/Source/Related Module/Related Job on the left and a full
  Description on the right, divided by a vertical rule. "Clear" here
  removes just that one message; the Active Messages card's own "Clear"
  button clears all of them.

**New models:** `Models/ErrorSeverity.cs` (Critical/Warning/Info/Resolved
enum) and `Models/ErrorRecord.cs` (one message's data). **New converter:**
`Views/ErrorSeverityToBrushConverter.cs`, mapping severity to the theme's
existing red/amber/blue/green tokens - no new colours were needed, since
this screen's four severities line up exactly with brushes already used
elsewhere (Stop button, Settings' Unsaved banner, Idle machine tiles,
Preferences Saved banner).

**Header change (affects every screen):** added a notification bell icon
to `MainWindow`'s shared header, between the page title and the language
indicator, matching both reference mockups (which show it consistently,
not just on this screen). Clicking it navigates to Errors & Information,
same destination as the global menu item - both now go through one shared
`NavigateTo(string)` method in `MainWindow.xaml.cs` rather than duplicating
the screen-switching logic. The bell's icon codepoint is new/unverified
(flagged in `ErrorsView.xaml`'s icon note); it doesn't yet show a badge
count for unread errors - straightforward to add later by having
`ErrorsView` raise an event when its message count changes, same pattern as
`SettingsView.LanguageChanged`.

Sample data (5 messages) is hard-coded in `ErrorsView.xaml.cs`. Nothing
here is connected to the WFM yet - replace with real WFM-reported
errors/warnings once that connection exists (FR-01, FR-02; PRD 3.3 WFM ->
GUI message flow).

### v17 — dark navy header, bell/language divider, Clear button moved outside the card

Three small fixes:

1. **Added a divider between the bell icon and the language indicator** in
   the header - there was a gap but no visible rule line there, unlike the
   dividers elsewhere in the header (after the logo, after the language
   indicator).
2. **Header background is now dark navy** (`HeaderBackgroundBrush`, now
   `#14213D` instead of white), fitting a more distinct corporate look for
   C.P. Bourg. Since the header now has a dark background while the rest of
   the app stays light-mode, four new tokens were added specifically for
   header content: `HeaderTextPrimaryBrush` (white), `HeaderTextSecondaryBrush`
   /`HeaderTextMutedBrush` (light greys), and `HeaderDividerBrush` (white at
   ~24% opacity, so the divider lines read correctly against navy instead of
   disappearing). Every header element (hamburger, logo text, page title,
   bell, language, clock/date, all divider lines) was switched from the
   general-purpose `TextPrimaryBrush`/`TextMutedBrush`/`CardBorderBrush`
   tokens to these header-specific ones. The header's bottom border was
   removed - the navy-vs-light contrast against the content area below now
   provides the visual separation on its own.
3. **The Errors screen's "Clear" button now sits outside the white Active
   Messages card**, on the page background, rather than inside it. The
   card's inner layout lost the row that used to hold the button; a new
   row was added below the card in `ErrorsView.xaml`'s root grid instead.

### v18 — Dashboard connected to Errors and Jobs

Two Dashboard elements that were previously stubs now do real things:

1. **Active Alerts card reflects the real state of the Errors &
   Information screen**, instead of always showing a static "No active
   alerts". `ErrorsView` now exposes `CriticalCount`/`WarningCount`/
   `InfoCount`/`TotalCount` properties and a `MessagesChanged` event,
   raised whenever the message list changes (load, Clear all, or clearing
   one message from the detail overlay). `DashboardView.UpdateAlertsSummary(...)`
   uses these to show either:
   - the original green "No active alerts / All systems are operating
     normally." when the total is zero, or
   - the real count ("3 active alerts") with a breakdown ("2 critical ·
     1 info") and a colour matching the highest severity present (critical
     > warning > info), when it isn't.

   `MainWindow.xaml.cs` wires `ErrorsScreen.MessagesChanged` to call this,
   and also calls it once at startup - `ErrorsScreen` loads its sample data
   in its own constructor, before `MainWindow` has a chance to subscribe,
   so without that initial call the Dashboard would show "No active
   alerts" until something changed, even though there are 5 sample
   messages from the start. "View Errors" also now genuinely navigates to
   the Errors & Information screen rather than showing a stub message.
2. **New Job / Load Job navigate to the Jobs / File Menu screen.**
   `DashboardView` raises a `NavigateToJobsRequested` event for both
   buttons, which `MainWindow.xaml.cs` wires to the same shared
   `NavigateTo(...)` method used by the global menu and the header bell.
   Both buttons currently land on the same Jobs screen (they don't yet
   auto-open a specific dialog there, e.g. jumping straight into "Save As
   New Job") - happy to wire that in too if you'd like New Job to open
   that dialog automatically on arrival.

This is the first place in the app where two screens' state is genuinely
linked rather than each screen being an island of its own sample data -
worth knowing as a pattern for connecting future screens: expose counts/
events from the "source of truth" screen, and have `MainWindow` (which
already owns every screen instance) wire them together, rather than having
screens reference each other directly.

### v15 — fixed cut-off warning text in Remove Job / Save As New Job

Real WPF layout bug: the warning banners in `RemoveJobDialog.xaml` and
`SaveAsNewJobDialog.xaml` (its "Job Name Already Exists!" conflict state)
put the icon and message text inside a horizontal `StackPanel`. A
`StackPanel` gives its children **infinite available width** in the
direction it stacks, so `TextWrapping="Wrap"` on the message `TextBlock`
had no effect - the text just ran past the card's edge instead of
wrapping, getting visually cut off.

**Fixed** in both places by replacing the horizontal `StackPanel` with a
`Grid` (icon in an `Auto` column, message in a `*` column). The message
`TextBlock` now has an actual bounded width to wrap within, so the full
warning text displays correctly. Checked the rest of the project for the
same pattern (horizontal `StackPanel` + a wrapped `TextBlock`) and found no
other instances - this was specific to these two warning banners.

### v19 — Machine Line Configuration screen

**New: `Views/MachineLineConfigurationView.xaml(.cs)`** - the Machine Line
Configuration screen (FR-05 line topology), reachable from the global menu's
"Machine Line Configuration" item (previously a title-only stub; now real
navigation, alongside Home, Settings, Jobs, and Errors - see
`MainWindow.xaml.cs` `NavigateTo`).

Demonstrates all three reference states as one interactive screen, same
convention as `SettingsView`/`JobsView`/`ErrorsView`, driven by how many
machines are on the line rather than by separate mockup screens:

- **Empty** (0 machines) - a dashed LINE CANVAS placeholder ("No machines
  configured") plus a "GET STARTED" tips panel (3 numbered steps + a tip
  banner) on the right.
- **Single** (1 machine) - an editable identity/network detail panel
  (Machine Name, Module Type, Mode, Connection Status, IP Address,
  Firmware) and Add Upstream / Add Downstream actions.
- **Overview** (2+ machines) - selectable cards on the LINE CANVAS
  (INPUT -> cards -> OUTPUT, connected by arrows), Add Machine / Remove /
  Move Left / Move Right actions, and a per-machine settings panel (Auto
  Start, Jam Detection, Output Counting toggles; Speed Limit and Reject Bin
  dropdowns) for whichever card is selected.

Clicking a card selects it (blue border) and refreshes the detail panel;
Apply Layout / Save Configuration show a shared green confirmation banner,
same pattern as `SettingsView`'s Saved banner. Nothing here is persisted or
sent to the WFM yet (FR-01, FR-02) - the sample line (Feeder, Booklet Maker,
Stacker) is hard-coded in the constructor, same "stub" convention as the
rest of this prototype.

**One deliberate deviation from the reference mock:** the mock's
single-machine action bar only has Add Upstream / Add Downstream, with no
way to remove the last machine. Since the Empty state's own tip says "you
can always add, remove, or rearrange machines later," a small trash-icon
Remove button was added to that bar - without it, Empty would be permanently
unreachable once any machine exists (the app always starts with 3 sample
machines).

**New model:** `Models/MachineLineItemInfo.cs` - plain data for one
machine/module, with `IsSelected`/`ShowConnector` view-state flags set on
every refresh (same non-observable `List<T>`-rebind pattern as
`JobsView.RefreshJobsList()`).

**New theme token:** `ToggleSwitchStyle` (in `Theme/BrandTheme.xaml`) - a
`CheckBox` re-templated as a pill switch, for Auto Start / Jam Detection /
Output Counting.

**Icon note:** most glyphs reuse Segoe MDL2 Assets codepoints already
confirmed elsewhere in this app. Two are new/unverified for this screen:
full-screen (zoom controls) and search/scan ("Scan for Devices") - flagged
in a comment at the top of `MachineLineConfigurationView.xaml`. Where the
reference mock uses a plain connector arrow, this screen deliberately uses
literal Unicode characters (→, ▶) instead of guessing another icon
codepoint, same reasoning as the flag emoji in `ChangeValueDialog`.

### v20 — Home dashboard: full-screen vs windowed scaling fixes

The dashboard is authored fluid (star-sized rows/columns, no fixed canvas -
see v5). That fills the width correctly, but several cards laid their content
out top-aligned, so the extra vertical height a full-screen window has over a
compact windowed / 1024x768 one showed up as empty white space. The machine
tiles had the opposite problem - a fixed tile height that got clipped when the
card was short. All four are Home-screen fixes in `DashboardView.xaml(.cs)`:

1. **Counter & Productivity: footer stat strip now reaches the bottom.** The
   spacer rows below the completed-sets block are now star-height with a
   `MinHeight` floor. At the compact windowed / tablet size they collapse to
   the same fixed gaps as before; any extra height (full screen) is absorbed
   between the sections so the grey stat strip sits at the bottom of the card
   instead of leaving empty white space under it. The header-to-number gap
   stays fixed so those two always read as one group.

2. **Current Jobs: no more empty gap under the buttons.** The card root became
   a three-row `Grid` (title / job summary / buttons) instead of a top-aligned
   `StackPanel`. The job-summary box takes the leftover height and the New job
   / Load job buttons stay pinned to the bottom.

3. **Active Alerts: no more empty gap under the button.** Same idea - a
   two-row `Grid` with the alert block centred in the remaining space, so full
   screen no longer leaves a large white gap beneath "View errors".

4. **Machines: tiles no longer clip in windowed mode; only STFO is online.**
   The tile row is wrapped in a `Viewbox` (`Stretch="Uniform"`,
   `StretchDirection="DownOnly"`) so it scales down to fit whenever the card is
   short - windowed mode and the 1024x768 tablet, where the fixed tile height
   was previously "3/4 cut off". When there is room (full screen) the tiles
   stay at their natural size, top-anchored as before. Separately, per operator
   feedback, every module except **STFO** is now shown Offline (greyed out) in
   the sample data (`DashboardView.xaml.cs` `LoadSampleMachines`) - STFO is the
   single active/controllable module in this prototype.

### v21 — windowed by default, Machine Line Configuration alignment, line-driven dashboard tiles

1. **App opens windowed, not maximized.** `MainWindow` now starts with
   `WindowState="Normal"` at a fixed default size (1280x820), centred on
   screen (`WindowStartupLocation="CenterScreen"`). The operator can still
   maximize it; `MinWidth`/`MinHeight` are unchanged.

2. **Machine Line Configuration: the four cards now line up as a clean 2x2.**
   The screen used two independent per-column grids, so the cards were
   staggered - Line Actions was shorter than Machine Line, and Configuration
   Status sat both higher and shorter than Selected Module, leaving an awkward
   gap. They now share one 3-column x 3-row grid, so the top row (Machine Line
   / Line Actions) shares a single height and the bottom row (Selected Module /
   Configuration Status) shares another - tops and bottoms aligned across both
   columns. Supporting tweaks: the Line Actions buttons live in star-height
   rows so they spread to fill the taller card instead of leaving empty space;
   `LineActionButtonStyle` gained `Padding`/`VerticalContentAlignment` so the
   action icons no longer hug the left border; and Configuration Status centres
   its content in the shared bottom-row height.

3. **Default line is just the Booklet Maker (STFO), and the dashboard follows
   the line.** `MachineLineConfigurationView` now starts with a single Booklet
   Maker module instead of Feeder + Booklet Maker + Stacker. It exposes
   `LineModuleTypes` and a `LineChanged` event; `MainWindow` wires that to the
   new `DashboardView.SetOnlineModules(...)`, which lights up a Home "Machines"
   tile (Running) only while its module type is on the line and greys it out
   (Offline) otherwise. So out of the box only STFO is online on the dashboard,
   and adding a Feeder / Stacker / Trimmer on the configuration screen brings
   the matching tile (BSF / BSE / TR) online - removing it greys it back out.
   The dashboard's machine tiles are no longer a hard-coded list.

### v22 — functional mockup controls (no WFM needed) + carousel "+" tiles add modules

1. **Dashboard counter / line controls now actually do something.** They used
   to be stubs that printed "(stub - not yet connected to the WFM)". With no
   WFM in this build they now act on local mockup state: preset +/- adjust the
   preset target (shown in both the preset field and the mini stat strip),
   Reset to zero clears the completed-sets counter, Set target sets a target
   just above the current count, and Start / Pause / Stop recolour the Current
   Job status pill (Running / Paused / Stopped). Purge and the others report a
   plain confirmation on the status line instead of a WFM stub message.
   (Startup already succeeds without a real WFM - the "Connecting to WFM"
   splash phase is simulated and never blocks - so nothing is gated on it.)

2. **The dashed "+" hint tiles in Machine Line Configuration add modules.**
   The tiles bookending the carousel strip were decorative; clicking either
   one now opens the Add Module wizard, same as the Line Actions "Add Module"
   button. Refactored the shared open path into `OpenAddModuleWizard()`.

### v23 — STFO individual machine configuration screen

**New: `Views/StfoConfigurationView.xaml(.cs)`** - the Individual Machine
Configuration wizard for the STFO (booklet maker), reached by tapping the STFO
tile on the Home dashboard (`DashboardView.NavigateToStfoRequested` ->
`MainWindow.NavigateToStfo`). Matches the approved high-fidelity mock of the
first ("Menu") step:

- A five-step tab bar - **1. Menu / 2. Stitching / 3. Folding / 4. Trimming /
  5. Conveyor** - with the active step filled navy; tapping a tab jumps to it.
- A **Status / Machine / Job** strip (coloured dots + document icon).
- A **Live Preview** card with a vector illustration of the STFO line (feeder
  unit + booklet maker + output tray), drawn from existing theme tokens so it
  re-skins with the app.
- A footer: **Back / Reset / Save / Next**. Back on the first step (and Finish
  on the last) returns to the dashboard; Next / the tabs walk the steps and the
  shell header title follows (`STFO - Stitching`, etc.). Save / Reset show a
  brief inline confirmation. All local - no WFM connection needed.

The per-step settings forms (Stitching etc.) are future work, so steps 2-5
currently share the Menu overview and differ by the highlighted tab, the title,
the Next label, and a step caption. Only the STFO tile navigates (it's the only
module screen that exists); the other dashboard tiles are inert for now.

Wiring: `MainWindow` hosts `StfoScreen` alongside the other content screens and
hides it from every `NavigateTo(...)` branch; `DashboardView` raises
`NavigateToStfoRequested` from a tile tap (`OnMachineTileClick`, guarded to the
STFO short code).

### v24 — STFO Stitching step (interactive form + live preview)

Built out step 2 (**Stitching**) of the STFO wizard
(`Views/StfoConfigurationView.xaml(.cs)`), matching the approved
high-fidelity mock. The step tab bar and Status/Machine/Job strip stay shared;
the content area now swaps between the machine-line overview (Menu + the
not-yet-built steps 3-5) and the Stitching two-column layout.

- **Stitching Parameters form (right):** Paper Width / Length fields, a
  five-option **Stitch Mode** selector (Saddle / Top / Right Corner / Left
  Corner / None, each a small drawn icon; the selected one is highlighted in
  the app blue), and Spacing / Horizontal Offset / Vertical Offset fields.
- **Live Preview (left) is dynamic** and redraws on every change
  (`RedrawStitchPreview`): the sheet is drawn to the current **W x L aspect**
  with **width and length dimension lines** (the "sizing on the pages" that was
  requested), and the **stitch marks change with the selected mode** - a dashed
  spine + a pair of saddle stitches (their gap = the Spacing value), a top pair,
  a single corner stitch, or "No stitching" - shifted by the horizontal /
  vertical offsets. A summary table underneath echoes the chosen values.
- Footer Back now reads **Back: {previous step}** past the first step; Save /
  Reset show a brief inline confirmation. All local - no WFM.

Steps 3-5 still share the Menu overview until their forms are designed. The
shell header title continues to track the step (`STFO - Stitching`).

### v25 — STFO Folding step (interactive form + dynamic fold preview)

Built out step 3 (**Folding**) of the STFO wizard
(`Views/StfoConfigurationView.xaml(.cs)`), matching the approved
high-fidelity mock. The content area now swaps between the machine-line
overview (Menu + steps 4-5), the Stitching layout, and the Folding layout.

- **Folding Settings form (right):** a **Fold Function** Enabled / Disabled
  toggle, a **Fold Position** stepper (`-` / value / `+`) paired with a
  Backward-to-Forward slider, and a **Pressure Mode** Auto / Default / Manual
  selector with a Less-to-More slider (enabled only in Manual). Selected
  choices fill blue.
- **Live Preview (left) is dynamic** (`RedrawFoldPreview`): an open booklet
  with a **dashed fold line that shifts to whichever side is selected** -
  positive/Forward moves it right of centre, negative/Backward left - the
  **"Fold direction" arrow flips** with the sign, and the offset arrow beneath
  shows the **distance from centre**. Disabling folding removes the fold
  indicators. A summary table echoes Folding / Fold Position / Pressure Mode.
- New reusable styles: `ChoiceButtonStyle`, `StepperButtonStyle`,
  `LineSliderStyle`.

Steps 4-5 (Trimming, Conveyor) still share the Menu overview until their
designs arrive. Footer Back reads **Back: Stitching**; header title tracks the
step (`STFO - Folding`).

### v26 — STFO Trimming step (interactive form + dynamic length preview)

Built out step 4 (**Trimming**) of the STFO wizard
(`Views/StfoConfigurationView.xaml(.cs)`), matching the approved
high-fidelity mock. Only steps 1-3 previously had bespoke content; the
content area now also swaps in the Trimming layout for step 4.

- **Trimming Settings form (right):** a **Trim Function** Enabled / Disabled
  toggle, a **Final Booklet Length** field with `-` / `+` steppers, a **Clamp
  Conveyor Height** Auto / Maximum / Minimum tile selector (drawn icons,
  selected one outlined blue), and a **Chip Blower** On / Off toggle.
- **Live Preview (left) is dynamic** (`RedrawTrimPreview`): the booklet is
  drawn front-on with its **width scaled to the Final Booklet Length** and a
  dimension line that updates live; when trimming is on, the trimmed fore-edge
  strip is shown. A side view of the clamp conveyor sits below, its height
  marker reflecting the selected clamp mode. A summary table echoes the
  values. Disabling trimming greys out the length / clamp controls.

Reuses the `ChoiceButtonStyle`, `StepperButtonStyle`, `NumberBoxStyle` and the
stitch-mode tile style. Step 5 (Conveyor) still shares the Menu overview until
its design arrives. Footer Back reads **Back: Folding**; header title tracks
the step (`STFO - Trimming`).

### v27 — STFO Conveyor step (interactive form + dynamic spacing preview); wizard complete

Built out step 5 (**Conveyor**) of the STFO wizard
(`Views/StfoConfigurationView.xaml(.cs)`), matching the approved
high-fidelity mock. All five steps now have bespoke content, so the Menu
overview only shows on step 1.

- **Conveyor Settings form (right):** **Booklet Spacing** (value box + `-`/`+`
  steppers + a 1-30 slider) with its "1 = small space ... 30 = continuous
  advance" caption, **Booklet Offset** (value box + steppers + slider), and a
  **Conveyor Full Detection** Enabled / Disabled toggle.
- **Live Preview (left) is dynamic** (`RedrawConveyorPreview`): the top
  conveyor shows booklets standing on the belt with the **gap between them
  scaled to the Booklet Spacing value** (dimensioned with the number); the
  bottom conveyor nudges **every Nth booklet forward** to show the Booklet
  Offset grouping. A summary table echoes the values.
- The last-step footer CTA now reads **Confirm** (was "Finish") and returns to
  the dashboard.

The STFO individual-machine configuration wizard (Menu / Stitching / Folding /
Trimming / Conveyor) is now complete end-to-end. Remaining polish for a later
pass: header title wording ("Machine Line - ..." vs "STFO - ..."), unifying the
navy/blue selection accent, and the mock's check badges on selected toggles.

### v28 - Technician Interface and responsive window/fullscreen layout

Added the Technician Interface screen, reachable from **Advanced > Technician
Interface** in the global menu. It contains the seven machine options from the
approved high-fidelity prototype, the status/speed panels, technician action
tiles, and Back / Reset / Save / Confirm footer actions.

- Technician choices are persisted per Windows user in
  `%LOCALAPPDATA%\CPBourg\NextGenGui\technician-settings.xml`. **Save** writes
  without leaving the screen; **Confirm** saves and returns Home; **Back**
  discards unsaved edits; **Reset** restores defaults as pending changes.
- **Technical Access** opens a masked, touch-friendly numeric keypad matching
  the Add Module technician-code step. The prototype accepts a non-empty code
  and does not retain it; production credential validation remains a backend
  integration point.
- The complete 1280 x 748 Technician design surface now sits in a uniformly
  scaling WPF `Viewbox`. It scales down to the default window's actual client
  area (including the space lost to the Windows title bar) and scales up in
  fullscreen while preserving proportions. The former vertical
  `ScrollViewer` was removed, so the footer is always visible without a mouse
  wheel or scrollbar.

### v29 - Functional completed-sets and production-preset counters

The Home dashboard's **Counter and Productivity** card now starts with
**Completed Sets = 0** instead of the old 5,234 sample value. Completed sets
and the production preset are both fully interactive in the local prototype:

- Each value has touch-sized **- / value / +** controls. Minus and plus adjust
  by exactly one, never go below zero, and are capped at 999,999,999.
- Tapping either value opens the reusable `NumericInputDialog`, a touchscreen
  keypad for direct whole-number entry. The existing **Set target** button
  opens the preset keypad as an additional shortcut.
- Completed Sets and Preset are visually separated by `/`. Counter edits are
  pending until the blue **Confirm** button is selected; Confirm occupies the
  former top-row Reset position and disables again after applying the values.
- A preset of **0 means unlimited production** and is displayed as the
  infinity symbol (`∞`) in both the main controls and the mini summary strip.
- **Reset to zero** and **Set target** now sit together beside Output/h. Reset
  stages a completed count of zero; after confirmation, the mini summary and
  Current Job completed count update together so the dashboard does not show
  conflicting applied values.

These counters remain local UI state until the WFM counter/command service is
connected; the handlers are isolated in `DashboardView.xaml.cs` for that
future integration.

### v30 - STFO Stitching windowed scrolling and decimal numpad

The STFO **Stitching Parameters** card no longer clips its lower settings in
the default window. The right-hand form now has its own vertical scrollbar
and touch-panning support, while the Live Preview, step tabs, and Back / Reset /
Save / Next footer remain fixed. Fullscreen keeps the same layout and only
shows the scrollbar when the parameter content exceeds the available height.

The five stitching number fields are now touch inputs: **Paper Width**, **Paper
Length**, **Spacing Between Stitches**, **Horizontal Offset**, and **Vertical
Offset** open `DecimalInputDialog` when pressed. The keypad supports fractional
millimeters, clear/backspace, and a `+/-` toggle for the two directional offset
fields. Confirmed values continue through the existing `TextChanged` path, so
the summary and live stitch preview update immediately without duplicating the
parameter logic.

### v31 - STFO Folding, Trimming, and Conveyor touch inputs

The remaining STFO setup steps now use the same touch-friendly number-entry
pattern as Stitching:

- **Folding:** pressing the Fold Position value opens the decimal keypad. It
  accepts signed millimeter values and updates the existing slider, summary,
  and live fold preview when confirmed.
- **Trimming:** pressing Final Booklet Length opens the decimal keypad, with
  the existing 50–350 mm limit preserved. The Trimming Settings card now has
  its own vertical scrollbar and touch panning so Chip Blower remains
  accessible in the default window.
- **Conveyor:** pressing Booklet Spacing or Booklet Offset opens the reusable
  whole-number keypad. Both inputs validate the same 1–30 range as their
  sliders. The Conveyor Settings card now scrolls independently so Conveyor
  Full Detection remains accessible without switching to fullscreen.

The preview panels, step navigation, and footer actions remain fixed while
only the taller settings cards scroll. In fullscreen, scrollbars appear only
when their content exceeds the available height.

### v32 - STFO transactional Reset and Save behavior

Reset and Save now operate on the active STFO configuration step (Stitching,
Folding, Trimming, or Conveyor):

- **Reset** immediately loads that step's defined factory defaults into the
  controls, summary, and live preview. The reset remains pending until Save.
- **Save** commits every value on the active step, including selectors,
  toggles, sliders, and numpad-entered values.
- Moving to another STFO step with tabs, Back, Next, or Confirm discards any
  unsaved edits and restores that step's last saved snapshot. Returning to the
  STFO dashboard and reopening the wizard follows the same rule.
- A reset followed by navigation is therefore discarded; a reset followed by
  Save makes the defaults the new saved configuration.

The snapshots are maintained locally for the current application session;
machine/WFM persistence remains a future integration point.

### v33 - Shared jobs, current-job loading, and format-aware STFO settings

Jobs are now connected across the application through one in-memory
`JobRepository` instead of separate placeholder data on each screen.

- On a fresh launch, the first (most recently listed) saved job is loaded
  automatically. The Dashboard's **Current Jobs** card shows its name, format,
  physical dimensions, page count, production preset, and completed sets.
- Confirming **Open Job** changes the repository's current job and returns to
  Home. Dashboard and every STFO step update immediately to the same job.
- Every sample and newly saved job owns a complete `StfoJobSettings` record.
  Paper dimensions match the job's book format; stitch/fold/trim/conveyor
  choices are deterministic prototype values derived from format and pages.
  STFO Save writes edited settings back only to the loaded job, so switching
  jobs restores each job's own setup.
- **Save As New Job** now captures Job Name, Pages, Preset Book Format, Width,
  and Length. Pages use the whole-number numpad; dimensions use the decimal
  numpad. Choosing A3, A4, A5, Letter, or 5 x 7 in fills its standard
  dimensions; Custom accepts any positive dimensions.
- Format display is dimension-driven. Dimensions matching a catalog preset
  use that preset name, while any mismatch is saved and displayed as
  **Custom** with its actual millimetre dimensions retained.

All job data intentionally starts fresh with the sample list on each program
launch. `JobRepository` is the replacement boundary for future WFM or durable
job storage.

### v34 - Functional production controls and unique machine modules

The Dashboard's Purge / Start / Pause / Stop controls now drive a local timed
production state machine instead of status-only feedback:

- **Start** begins or resumes production and adds one completed set per second.
  A positive confirmed preset stops automatically and shows Completed exactly
  at the target; preset 0 remains unlimited production.
- **Pause** freezes the timer and retains progress. Selecting Start resumes
  from the same completed count.
- **Stop** terminates the run while retaining its completed count. A stopped or
  completed run cannot restart until Purge is selected.
- **Purge** stops production and resets completed sets, preset, pending counter
  changes, and process state. The cleared preset displays as unlimited (`∞`).
- Start requires a loaded job and any pending counter edits must be confirmed
  first. Button enabled states follow the current Ready / Running / Paused /
  Stopped / Completed state.

Machine Line Configuration received two related changes:

- Line Action buttons no longer use the default grey fill and dark outer
  border. Their chrome is transparent, with thin blue horizontal dividers
  between Add, Remove, Replace, and Review.
- Add Module lists only module types not already on the line. The insertion
  handler also rejects duplicate requests defensively, Add disables when all
  four types are present, and Replace chooses only an unused module type.

### v35 - Help manuals link and prototype attribution

The stacked menu's final informational items now open dedicated screens:

- **Help / Manual** shows the official C.P. Bourg My C.P. Bourg support and
  download URL (`https://www.cpbourg.com/be/en/my-cpbourg`) as a clickable
  link. Selecting it opens the user's default browser; the URL remains visible
  so it can still be copied or entered manually on a restricted kiosk system.
- **About / Version** shows the prototype attribution: "This is a mockup
  prototype made by OpenHub for C.P. Bourg for the summer of 2026."

Both views use the shared responsive card styling and fit in the default
window as well as fullscreen mode.

### v36 - Barcode lookup, job-log export, date/time, and font size

The two remaining Jobs actions and two Settings placeholders are now
functional:

- **Scan Barcode ID** opens a focused scanner-input dialog. A standard USB
  keyboard-wedge barcode scanner can type the barcode and submit it with
  Enter; manual typing and the Find Job button work as a fallback. A matching
  barcode selects and scrolls to its saved job, while an unknown barcode keeps
  the dialog open with a retry message.
- **View Log** displays timestamped events for the selected job, including job
  creation, loading, comment changes, and barcode matches. Export Log opens the
  standard Windows save dialog and writes a UTF-8 CSV file to any chosen path,
  including a mounted USB drive.
- **Date and Time** opens a date picker with 24-hour hour/minute selectors. The
  value remains pending until Apply, Cancel restores the previous value, and
  Apply updates both the Settings row and shell header clock. This prototype
  uses an application clock offset and does not require permission to modify
  the Windows system clock.
- **Font Size** replaces the former UI Scale placeholder with Small, Medium,
  and Large choices. Apply changes authored text sizes throughout the shell and
  screens while leaving touch-target dimensions unchanged; Cancel restores the
  last applied choice.

Barcode mappings, job events, and operator preferences remain in memory for
the current application session, matching the prototype's existing fresh-start
job repository. Durable WFM-backed storage is still the integration boundary.

### v37 - Applied metric and imperial measurement systems

The Settings **Units** choice now participates fully in the existing
pending / Apply / Cancel workflow. Applying Metric displays millimetres;
applying Imperial displays inches. The preference immediately refreshes every
physical measurement surface:

- current-job dimensions on the Dashboard and Job List;
- new-job dimension labels, values, preset/custom classification, and decimal
  keypad input;
- exported job-log dimension labels and values;
- STFO paper dimensions, stitch spacing and offsets, fold position, final trim
  length, summaries, live-preview dimension annotations, unit suffixes, and
  decimal keypads;
- Technician Interface and Machine Line speed values (`mm/s` or `in/s`).

Machine values remain canonical millimetres internally and are converted only
at the display/input boundary. Repeated Metric -> Imperial -> Metric switching
therefore does not accumulate rounding error. Imperial values use three decimal
places for setup precision, and physical +/- steppers move 0.1 inch instead of
1 millimetre. Unitless settings such as page counts, completed sets, pressure,
and conveyor counting/advance indices are intentionally unchanged.

The selected unit is session-local like the other operator preferences; future
durable preference storage can persist the same `MeasurementUnit` value.

### v38 - Dashboard counter and alert alignment

The Home dashboard now uses shared grid tracks for related controls instead of
positioning each row independently. **Reset to zero** occupies the same column
and width as the Completed Sets editor, while **Set target** does the same for
the Preset editor. Their alignment therefore remains stable when the window is
resized or maximized.

The former grey Completed sets / Preset / Output per-hour footer has been
removed because it repeated the information directly above it. The editable
Completed Sets and Preset values, Confirm, Reset to zero, Set target, and
Output per hour now form a strict two-row control area. The first row contains
Completed Sets, Preset, and Output per hour; the second row contains their
corresponding Reset to zero, Set target, and Confirm actions. The Counter card
now receives 6/11 of the left-column height instead of 2/3, giving the Machines
card the remaining 5/11 and vertically centring its module tiles in the larger
area.

Home-page action buttons now use the same neutral system button treatment and
52-pixel touch height. New job no longer has a one-off blue background, and
its label uses the same regular weight as Load job. Confirm also no longer
changes to a lighter visual style when there are no pending
edits. Pending counter state is tracked separately so production still cannot
start until changed values are confirmed.

The Active Alerts summary is a single icon / message / action grid. **View
errors** stays vertically centred with the alert summary rather than wrapping
under the message or drifting within the extra full-screen space. A live
severity strip now shows separate Critical, Warning, and Information counts
from the same Errors-screen data source. When alerts exist, the former
count-summary subtitle is hidden so those counts are displayed only once; the
all-clear explanatory subtitle remains when the total is zero. The headline
block is vertically centred against the alert icon in both states.

The revised dashboard was render-checked at the default 1280 x 820 window size
and at a 2048 x 1192 widescreen size.

### Languages (FR-10)

The stage labels in `Startup/StartupStage.cs` are the strings that will move to
a French/English resource file when language switching is implemented. They are
inline for the prototype and marked with a comment.

---

## Files

| File | Purpose |
|------|---------|
| `App.xaml` / `App.xaml.cs` | Application entry; splash → sequence → dashboard handoff |
| `Theme/BrandTheme.xaml` | Single source of brand tokens (re-skin here) |
| `Views/SplashWindow.xaml(.cs)` | The high-fidelity splash view |
| `Views/MainWindow.xaml(.cs)` | App shell: header bar + hosts DashboardView + GlobalMenuView |
| `Views/DashboardView.xaml(.cs)` | Home dashboard content (Counter, Machines, Jobs, Alerts, action bar) |
| `Views/SettingsView.xaml(.cs)` | Settings / Operator Preferences screen (Default / Unsaved / Saved states) |
| `Views/ChangeValueDialog.xaml(.cs)` | Reusable "Change [setting]" picker dialog |
| `Views/JobsView.xaml(.cs)` | Jobs / File Menu screen (Saved Jobs list, Summary, actions) |
| `Views/AddCommentDialog.xaml(.cs)` | Edit a job's comment |
| `Views/OpenJobDialog.xaml(.cs)` | Open a job, optionally with saved RUN adjustments |
| `Views/SaveAsNewJobDialog.xaml(.cs)` | Save current setup as a new job (input + name-conflict states) |
| `Views/RemoveJobDialog.xaml(.cs)` | Destructive job removal confirmation |
| `Views/BarcodeScanDialog.xaml(.cs)` | Keyboard-wedge barcode scanner capture and validation overlay |
| `Views/JobLogDialog.xaml(.cs)` | Selected-job log preview and CSV export to a chosen path or USB drive |
| `Views/ConfirmationDialog.xaml(.cs)` | Shared "...Saved!" / "...Removed!" success dialog |
| `Views/ErrorsView.xaml(.cs)` | Errors & Information screen (summary tiles, Active Messages, empty state) |
| `Views/ErrorDetailDialog.xaml(.cs)` | Error/warning detail overlay |
| `Views/ErrorSeverityToBrushConverter.cs` | Maps `ErrorSeverity` to theme brushes |
| `Views/MachineLineConfigurationView.xaml(.cs)` | Machine Line Configuration screen (Empty / Single / Overview states) |
| `Views/TechnicianInterfaceView.xaml(.cs)` | Responsive Technician Interface screen and saved control state |
| `Views/TechnicalAccessDialog.xaml(.cs)` | Masked numeric technician-code keypad overlay |
| `Views/NumericInputDialog.xaml(.cs)` | Reusable touchscreen keypad for non-negative whole-number entry |
| `Views/DecimalInputDialog.xaml(.cs)` | Decimal touchscreen keypad with optional negative-value entry |
| `Views/DateTimeSettingsDialog.xaml(.cs)` | Operator-interface date and 24-hour time editor |
| `Views/FontSizeManager.cs` | Applies Small / Medium / Large text sizing without resizing touch targets |
| `Views/GlobalMenuView.xaml(.cs)` | Slide-out global navigation overlay |
| `Views/HelpView.xaml(.cs)` | Help screen with the clickable official manuals/support link |
| `Views/AboutVersionView.xaml(.cs)` | About / Version screen with the prototype attribution |
| `Views/MachineStatusToBrushConverter.cs` | Maps `MachineStatus` to theme brushes |
| `Views/NullToCollapsedConverter.cs` | Hides an element when a bound string is null/empty |
| `Views/LogoLoader.cs` | Loads a real logo file if present, falls back to the placeholder mark |
| `Assets/README.md` | Where to get the official CPBourg logo and how to add it |
| `Models/MachineStatus.cs` | Running / Idle / Offline enum |
| `Models/MachineTileInfo.cs` | Data for one machine tile |
| `Models/JobSummary.cs` | Data for the current-job card |
| `Models/SettingsItemInfo.cs` | Data for one Settings row |
| `Models/ChangeOptionInfo.cs` | Data for one radio option in a Change-value dialog |
| `Models/MeasurementUnit.cs` / `Models/MeasurementFormatter.cs` | Canonical mm/in preference, conversion, and formatting helpers |
| `Models/JobRecord.cs` | Data for one saved job |
| `Models/JobLogEntry.cs` | Timestamped activity entry exported by the selected-job log |
| `Models/JobRepository.cs` | Shared ordered job list and current-job state used by Jobs, Dashboard, and STFO |
| `Models/BookFormatPreset.cs` / `Models/BookFormatCatalog.cs` | Standard format dimensions and Custom classification |
| `Models/StfoJobSettings.cs` | Complete per-job Stitching, Folding, Trimming, and Conveyor settings |
| `Models/SaveJobRequest.cs` | Validated name/pages/dimensions submitted by Save As New Job |
| `Models/ErrorSeverity.cs` | Critical / Warning / Info / Resolved enum |
| `Models/ErrorRecord.cs` | Data for one error/warning message |
| `Models/MachineLineItemInfo.cs` | Data for one machine/module on the LINE CANVAS |
| `Models/TechnicianSettings.cs` | Saved Technician Interface option values and defaults |
| `Models/TechnicianSettingsStore.cs` | User-local XML persistence for technician settings |
| `Startup/StartupStage.cs` | The five boot phases + display text |
| `Startup/StartupProgress.cs` | Immutable progress snapshot |
| `Startup/StartupSequencer.cs` | Runs the sequence, UI-agnostic |
| `Startup/IWfmConnectionProbe.cs` | Backend seam (WFM connection abstraction) |
| `Startup/SimulatedWfmConnectionProbe.cs` | Prototype stand-in for the WFM |

---

## Try the error path

In `App.xaml.cs`, set `SimulateFailure = true` on the probe to see the
"WFM unavailable" state (FR-06): the bar and step indicator turn red and the
status line explains what to do.

```csharp
var wfmProbe = new SimulatedWfmConnectionProbe { SimulateFailure = true };
```
