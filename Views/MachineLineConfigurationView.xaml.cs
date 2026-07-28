using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Machine Line Configuration screen. See MachineLineConfigurationView.xaml
    /// for the reference states this demonstrates (Empty Line / Single-Module /
    /// Configured) and why they're one interactive screen rather than three
    /// static mockups - same convention as SettingsView/JobsView/ErrorsView.
    ///
    /// The Machine Line carousel strip is focused on one module at a time
    /// (<see cref="_focusedIndex"/> into <see cref="_machines"/>); the
    /// Selected Module and Configuration Status panels, and which Line
    /// Actions are enabled, all follow that focus. Add Module opens
    /// <see cref="AddModuleWizardDialog"/> (module type -> Before/After the
    /// focused module -> review), which inserts a pending local edit on
    /// Confirm. Module types already on the line are filtered out and a
    /// second defensive check rejects duplicates at insertion time. Add,
    /// Remove, and Replace remain pending until Review &amp; Confirm requests
    /// one technician PIN for the complete line. Nothing is sent to the WFM
    /// yet (FR-01, FR-02).
    /// </summary>
    public partial class MachineLineConfigurationView : UserControl
    {
        private static readonly (string ModuleType, string Code)[] Catalog =
        {
            ("Feeder", "FD"),
            ("Booklet Maker", "BM"),
            ("Stacker", "SK"),
            ("Trimmer", "TR"),
        };

        private const double SampleSpeedMillimetersPerSecond = 973;

        private readonly List<MachineLineItemInfo> _machines = new List<MachineLineItemInfo>();
        private int _focusedIndex = -1;
        private bool _hasPendingChanges;
        private MeasurementUnit _measurementUnit = MeasurementUnit.Millimeters;
        private Func<string> _lastActionRenderer;

        /// <summary>
        /// Raised after the complete pending line has been authorized with a
        /// technician PIN. MainWindow uses this to publish the confirmed
        /// modules to the Home dashboard.
        /// </summary>
        public event EventHandler LineChanged;

        /// <summary>Module types currently on the line, in order.</summary>
        public IEnumerable<string> LineModuleTypes => _machines.Select(m => m.ModuleType);

        public MachineLineConfigurationView()
        {
            InitializeComponent();

            LoadSampleLine();
            RefreshAll();
        }

        public void SetMeasurementUnit(MeasurementUnit unit)
        {
            _measurementUnit = unit;
            foreach (var machine in _machines)
            {
                machine.Speed = FormatSampleSpeed();
            }
            RefreshSelectedModule();
        }

        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            RefreshAll();
            AddModuleWizardDialogControl.ApplyLanguage();
            ConfigurationPinDialog.ApplyLanguage();
            RenderLastAction();
        }

        private string FormatSampleSpeed()
        {
            return MeasurementFormatter.FormatValue(
                       SampleSpeedMillimetersPerSecond, _measurementUnit, "0", "0.0") + " " +
                   MeasurementFormatter.SpeedUnitSymbol(_measurementUnit);
        }

        private void LoadSampleLine()
        {
            // Start with just the Booklet Maker (the STFO) on the line; the
            // operator adds Feeder / Stacker / Trimmer as needed. The Home
            // dashboard reflects this - only modules actually on the line show
            // as online there (MainWindow wires LineChanged ->
            // DashboardView.SetOnlineModules).
            _machines.Add(CreateMachine("Booklet Maker"));
            _focusedIndex = 0;
        }

        private MachineLineItemInfo CreateMachine(string moduleType)
        {
            var entry = Catalog.First(c => c.ModuleType == moduleType);
            int sequence = _machines.Count(m => m.ModuleType == moduleType) + 1;
            string modelCode = $"{entry.Code}-2000-{sequence:00}";
            return new MachineLineItemInfo(entry.ModuleType, modelCode, FormatSampleSpeed());
        }

        // ================= Refresh =================

        private void RefreshAll()
        {
            RefreshCarousel();
            RefreshSelectedModule();
            RefreshConfigurationStatus();
            RefreshLineActionsEnabled();
        }

        private void RefreshCarousel()
        {
            bool isEmpty = _machines.Count == 0;
            EmptyCarouselPanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            PopulatedCarouselPanel.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;

            MachineStripPanel.Children.Clear();
            if (!isEmpty)
            {
                MachineStripPanel.Children.Add(BuildAddHintTile());
                for (int i = 0; i < _machines.Count; i++)
                {
                    MachineStripPanel.Children.Add(BuildMachineTile(_machines[i], i));
                }
                MachineStripPanel.Children.Add(BuildAddHintTile());
            }

            PaginationDotsPanel.Children.Clear();
            for (int i = 0; i < _machines.Count; i++)
            {
                PaginationDotsPanel.Children.Add(new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Margin = new Thickness(3, 0, 3, 0),
                    Fill = i == _focusedIndex
                        ? (Brush)FindResource("JobsAccentBrush")
                        : (Brush)FindResource("OutlineButtonBorderBrush"),
                });
            }
        }

        // Small selectable tile for one machine in the strip - clicking a tile focuses it,
        // same as paging there with the chevrons.
        private Border BuildMachineTile(MachineLineItemInfo machine, int index)
        {
            bool isFocused = index == _focusedIndex;

            var icon = new TextBlock
            {
                Text = "",
                FontFamily = (FontFamily)FindResource("IconFontFamily"),
                FontSize = 20,
                Foreground = (Brush)FindResource(isFocused ? "StatusIdleBrush" : "TextMutedBrush"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var iconBox = new Border
            {
                Width = 52,
                Height = 46,
                CornerRadius = new CornerRadius(10),
                Background = (Brush)FindResource(isFocused ? "StatusIdleBgBrush" : "ScreenBackgroundBrush"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = icon,
            };

            var nameText = new TextBlock
            {
                Text = T(machine.ModuleType),
                FontSize = 12,
                FontWeight = isFocused ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = (Brush)FindResource(isFocused ? "TextPrimaryBrush" : "TextSecondaryBrush"),
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };

            var codeText = new TextBlock
            {
                Text = machine.ModelCode,
                FontSize = 10,
                Foreground = (Brush)FindResource("TextMutedBrush"),
                Margin = new Thickness(0, 2, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var content = new StackPanel { Margin = new Thickness(10) };
            content.Children.Add(iconBox);
            content.Children.Add(nameText);
            content.Children.Add(codeText);

            var tile = new Border
            {
                Width = 100,
                CornerRadius = new CornerRadius(14),
                Background = (Brush)FindResource("CardBackgroundBrush"),
                BorderBrush = (Brush)FindResource(isFocused ? "JobsAccentBrush" : "CardBorderBrush"),
                BorderThickness = new Thickness(isFocused ? 2 : 1),
                Margin = new Thickness(6, 0, 6, 0),
                Cursor = Cursors.Hand,
                Child = content,
            };

            tile.MouseLeftButtonUp += (s, e) =>
            {
                _focusedIndex = index;
                RefreshCarousel();
                RefreshSelectedModule();
            };

            return tile;
        }

        // Dashed placeholder bookending the strip on each side - visual "you can add a
        // module here" hint, mirroring EmptyCarouselPanel's dashed style.
        private Grid BuildAddHintTile()
        {
            var rect = new Rectangle
            {
                Stroke = (Brush)FindResource("OutlineButtonBorderBrush"),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 5, 4 },
                RadiusX = 12,
                RadiusY = 12,
            };

            var icon = new TextBlock
            {
                Text = "",
                FontFamily = (FontFamily)FindResource("IconFontFamily"),
                FontSize = 16,
                Foreground = (Brush)FindResource("TextMutedBrush"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var iconCircle = new Border
            {
                Width = 34,
                Height = 34,
                CornerRadius = new CornerRadius(17),
                BorderThickness = new Thickness(1.5),
                BorderBrush = (Brush)FindResource("OutlineButtonBorderBrush"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = icon,
            };

            var grid = new Grid
            {
                Width = 68,
                Height = 118,
                Margin = new Thickness(6, 0, 6, 0),
                Background = Brushes.Transparent,   // make the whole tile hit-testable, not just the shapes
                Cursor = Cursors.Hand,
                ToolTip = T("Add a module"),
            };
            grid.Children.Add(rect);
            grid.Children.Add(iconCircle);

            // The "+" hint tiles bookending the strip are also a way to add a
            // module, matching the two Add Module buttons.
            grid.MouseLeftButtonUp += (s, e) => OpenAddModuleWizard();

            return grid;
        }

        private void RefreshSelectedModule()
        {
            bool isEmpty = _machines.Count == 0;
            EmptySelectedModulePanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            PopulatedSelectedModulePanel.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;

            if (isEmpty)
            {
                return;
            }

            var focused = _machines[_focusedIndex];
            SelectedModuleLineText.Text = focused.ModelCode + " " + T(focused.ModuleType);
            PositionValueText.Text = TF("{0} of {1}", _focusedIndex + 1, _machines.Count);
            StatusValueText.Text = T(focused.IsRegistered ? "Registered" : "Not Registered");
            SpeedValueText.Text = focused.Speed;
        }

        private void RefreshConfigurationStatus()
        {
            bool hasMachines = _machines.Count > 0;
            if (_hasPendingChanges)
            {
                ConfigStatusText.Text = T("Unsaved changes — review and confirm.");
                ConfigStatusIconBg.Background = (Brush)FindResource("WarningBgBrush");
                ConfigStatusIconText.Foreground = (Brush)FindResource("WarningBrush");
                return;
            }

            ConfigStatusText.Text = T(hasMachines
                ? "Configuration confirmed."
                : "No modules configured.");
            ConfigStatusIconBg.Background = (Brush)FindResource(hasMachines ? "StatusRunningBgBrush" : "StatusOfflineBgBrush");
            ConfigStatusIconText.Foreground = (Brush)FindResource(hasMachines ? "StatusRunningBrush" : "StatusOfflineBrush");
        }

        private void RefreshLineActionsEnabled()
        {
            bool hasMachines = _machines.Count > 0;
            bool hasAvailableModules = Catalog.Any(c =>
                _machines.All(m => m.ModuleType != c.ModuleType));
            AddModuleButton.IsEnabled = hasAvailableModules;
            RemoveModuleButton.IsEnabled = hasMachines;
            ReplaceModuleButton.IsEnabled = hasMachines;
            ReviewChangesButton.IsEnabled = _hasPendingChanges;
        }

        // ================= Carousel navigation =================

        private void OnPrevClick(object sender, RoutedEventArgs e)
        {
            if (_machines.Count == 0)
            {
                return;
            }

            _focusedIndex = (_focusedIndex - 1 + _machines.Count) % _machines.Count;
            RefreshCarousel();
            RefreshSelectedModule();
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            if (_machines.Count == 0)
            {
                return;
            }

            _focusedIndex = (_focusedIndex + 1) % _machines.Count;
            RefreshCarousel();
            RefreshSelectedModule();
        }

        // ================= Line Actions =================

        private void OnAddModuleClick(object sender, RoutedEventArgs e) => OpenAddModuleWizard();

        // Shared by the Line Actions "Add Module" button, the empty-state
        // "Add Module" button, and the dashed "+" hint tiles that bookend the
        // carousel strip - all of them open the Add Module wizard.
        private void OpenAddModuleWizard()
        {
            var availableModuleTypes = Catalog
                .Select(c => c.ModuleType)
                .Where(moduleType => _machines.All(m => m.ModuleType != moduleType))
                .ToList();
            if (availableModuleTypes.Count == 0)
            {
                SetLastAction(() => T("All available module types are already on the line."));
                return;
            }

            string anchorModuleType = _machines.Count > 0 ? _machines[_focusedIndex].ModuleType : null;
            AddModuleWizardDialogControl.Open(availableModuleTypes, anchorModuleType);
        }

        private void OnAddModuleConfirmed(object sender, AddModuleRequestInfo request)
        {
            if (_machines.Any(m => m.ModuleType == request.ModuleType))
            {
                string duplicateType = request.ModuleType;
                SetLastAction(() => TF(
                    "{0} is already on the line and cannot be added again.",
                    T(duplicateType)));
                return;
            }

            var machine = CreateMachine(request.ModuleType);

            int insertIndex;
            if (request.PlaceBeforeAnchor == null || _machines.Count == 0)
            {
                insertIndex = _machines.Count;
            }
            else
            {
                insertIndex = request.PlaceBeforeAnchor.Value ? _focusedIndex : _focusedIndex + 1;
            }

            _machines.Insert(insertIndex, machine);
            _focusedIndex = insertIndex;
            _hasPendingChanges = true;
            RefreshAll();

            string addedType = request.ModuleType;
            bool? placeBefore = request.PlaceBeforeAnchor;
            string anchorType = request.AnchorModuleType;
            SetLastAction(() => TF(
                "Pending: Added {0} ({1}). Select Review & Confirm when finished.",
                T(addedType), FormatPositionSummary(placeBefore, anchorType)));
        }

        private void OnRemoveModuleClick(object sender, RoutedEventArgs e)
        {
            if (_machines.Count == 0)
            {
                return;
            }

            _machines.RemoveAt(_focusedIndex);
            _focusedIndex = _machines.Count == 0 ? -1 : System.Math.Min(_focusedIndex, _machines.Count - 1);
            _hasPendingChanges = true;
            RefreshAll();
            SetLastAction(() => T(
                "Pending: Module removed. Select Review & Confirm when finished."));
        }

        private void OnReplaceModuleClick(object sender, RoutedEventArgs e)
        {
            if (_machines.Count == 0)
            {
                return;
            }

            string currentType = _machines[_focusedIndex].ModuleType;
            int currentCatalogIndex = System.Array.FindIndex(Catalog, c => c.ModuleType == currentType);
            var moduleTypesAtOtherPositions = new HashSet<string>(
                _machines.Where((machine, index) => index != _focusedIndex)
                         .Select(machine => machine.ModuleType));
            var nextEntry = Enumerable.Range(1, Catalog.Length - 1)
                .Select(offset => Catalog[(currentCatalogIndex + offset) % Catalog.Length])
                .FirstOrDefault(entry => !moduleTypesAtOtherPositions.Contains(entry.ModuleType));
            if (string.IsNullOrEmpty(nextEntry.ModuleType))
            {
                SetLastAction(() => T("No unused module type is available for replacement."));
                return;
            }

            _machines[_focusedIndex] = CreateMachine(nextEntry.ModuleType);
            _hasPendingChanges = true;
            RefreshAll();
            string replacementType = nextEntry.ModuleType;
            SetLastAction(() => TF(
                "Pending: Replaced {0} with {1}. Select Review & Confirm when finished.",
                T(currentType), T(replacementType)));
        }

        private void OnReviewChangesClick(object sender, RoutedEventArgs e)
        {
            if (!_hasPendingChanges)
            {
                return;
            }

            string moduleSummary = _machines.Count == 0
                ? T("an empty machine line")
                : TF(_machines.Count == 1 ? "{0} module" : "{0} modules", _machines.Count);
            ConfigurationPinDialog.Open(
                T("Confirm Machine Line"),
                TF("Review complete. Enter the technician PIN once to apply {0} and publish the configuration.",
                    moduleSummary),
                T("Confirm"));
        }

        private void OnConfigurationPinConfirmed(object sender, string submittedCode)
        {
            _hasPendingChanges = false;
            RefreshConfigurationStatus();
            RefreshLineActionsEnabled();
            SetLastAction(() => T("Machine line configuration confirmed and applied."));
            LineChanged?.Invoke(this, EventArgs.Empty);
        }

        private static string FormatPositionSummary(bool? placeBeforeAnchor,
            string anchorModuleType)
        {
            if (placeBeforeAnchor == null)
            {
                return T("Start of line");
            }

            return TF(placeBeforeAnchor.Value ? "Before {0}" : "After {0}",
                T(anchorModuleType));
        }

        private void SetLastAction(Func<string> renderer)
        {
            _lastActionRenderer = renderer;
            RenderLastAction();
        }

        private void RenderLastAction()
        {
            LastActionText.Text = _lastActionRenderer == null
                ? string.Empty
                : _lastActionRenderer();
        }

        private static string T(string source)
        {
            return LocalizationManager.Translate(source);
        }

        private static string TF(string source, params object[] values)
        {
            return string.Format(CultureInfo.CurrentCulture, T(source), values);
        }
    }
}
