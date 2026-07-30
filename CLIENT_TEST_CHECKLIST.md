# Client test checklist — version 1.30.0

Tester: ____________________

Date: ____________________

Windows version: ____________________

Resolution / scaling: ____________________

Input: Mouse / Touch / Barcode scanner

Mark each item **Pass**, **Fail**, or **N/A**, and add issue notes or screenshots.

## 1. Package and startup

| Result | Check |
|---|---|
|  | The ZIP extracts without missing-file warnings. |
|  | The application launches from the extracted directory. |
|  | Splash screen shows version 1.30.0 and build 2026-07. |
|  | Home opens with the top job from the job list loaded. |
|  | Restarting the application succeeds after changing preferences. |

## 2. Display and interaction

| Result | Check |
|---|---|
|  | Windowed mode exposes all controls without inaccessible clipped content. |
|  | Fullscreen/maximized mode remains aligned with no excessive empty areas. |
|  | Screens with long content provide usable scrolling. |
|  | Controls remain usable at 100%, 125%, and 150% Windows scaling where available. |
|  | Touch targets, focus, selected states, and disabled states are visually clear. |

## 3. Settings and localization

Repeat the primary navigation check in English, French, Dutch, German, Spanish,
and Italian.

| Result | Check |
|---|---|
|  | Apply changes the interface language across every main screen. |
|  | Home, Jobs, STFO, Machine Line, Errors, and Technician screens contain no unintended English text. |
|  | Metric displays millimeters and imperial displays inches with correct conversions. |
|  | Small, Medium, and Large font sizes remain readable without clipping. |
|  | Date/time, keyboard, cursor, and calibration workflows respond as expected. |
|  | Applied preferences remain after an application restart. |

## 4. Home and production simulation

| Result | Check |
|---|---|
|  | Completed sets and preset accept +, -, and numeric-keypad entry. |
|  | Preset 0 is displayed and treated as unlimited. |
|  | Confirm, Reset to Zero, and Set Target update the intended values. |
|  | Start is available only when no active error prevents running. |
|  | Stop and Pause are available while running; Purge is available when stopped. |
|  | Starting does not require a purge first. |
|  | Completed sets increment while running and stop at a finite preset. |
|  | Counter-edit buttons are muted and unavailable while running; number displays remain legible. |

## 5. Jobs

| Result | Check |
|---|---|
|  | The dashboard current job matches the latest loaded job. |
|  | A preset format can be saved with a page count. |
|  | Nonstandard dimensions are identified as Custom. |
|  | Loading a different job updates Home and every STFO step. |
|  | Barcode input accepts a keyboard-wedge scanner or typed test value. |
|  | View Log previews and exports a readable CSV to a chosen folder/USB drive. |

## 6. STFO / BBM

| Result | Check |
|---|---|
|  | Overview explains the workflow and shows paper-path direction. |
|  | Stitching diagrams and selections are understandable and correctly oriented. |
|  | Folding disabled shows the bypass path to the top tray. |
|  | Folding offers Auto and Manual without an unexplained Default mode. |
|  | Trimming shows finished booklet length, total booklet length, and trimmed strip length. |
|  | Clamp pressure provides the expected graduated selection. |
|  | Every numeric field opens a keypad and accepts whole/decimal values intuitively. |
|  | Save preserves the current step; Reset restores defaults. |
|  | Leaving a step without saving restores its previously saved configuration. |
|  | Windowed mode can reach all Stitching, Trimming, and Conveyor settings. |

## 7. Machine line and technician access

| Result | Check |
|---|---|
|  | Add, remove, and replace workflows show clear pending changes. |
|  | A module already present in the line cannot be added again. |
|  | No PIN is requested during intermediate configuration steps. |
|  | The technician PIN is requested when final changes are confirmed. |
|  | Canceling or entering an invalid PIN leaves the saved line unchanged. |
|  | Technician settings save and remain after restart. |

## 8. Errors and navigation

| Result | Check |
|---|---|
|  | Active-alert counts and severity breakdown match the visible entries. |
|  | View Errors opens the error list and details. |
|  | The Home navigation button returns from Errors to the dashboard. |
|  | Active critical errors prevent Start; clearing/resolving them restores availability. |

## Acceptance

Critical failures: ______

Noncritical failures: ______

Accepted for prototype evaluation: Yes / No

Client representative: ____________________

Signature/date: ____________________
