using System.Collections.Generic;
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
    /// focused module -> technician code), which inserts locally on
    /// Confirm - nothing here is persisted or sent to the WFM yet (FR-01,
    /// FR-02). Remove/Replace/Review Changes only show the stub feedback
    /// line, same convention as unbuilt flows elsewhere (e.g. JobsView's
    /// View Log).
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

        private const string SampleSpeed = "973 mm/sec";

        private readonly List<MachineLineItemInfo> _machines = new List<MachineLineItemInfo>();
        private int _focusedIndex = -1;

        public MachineLineConfigurationView()
        {
            InitializeComponent();

            LoadSampleLine();
            RefreshAll();
        }

        private void LoadSampleLine()
        {
            _machines.Add(CreateMachine("Feeder"));
            _machines.Add(CreateMachine("Booklet Maker"));
            _machines.Add(CreateMachine("Stacker"));
            _focusedIndex = 1;
        }

        private MachineLineItemInfo CreateMachine(string moduleType)
        {
            var entry = Catalog.First(c => c.ModuleType == moduleType);
            int sequence = _machines.Count(m => m.ModuleType == moduleType) + 1;
            string modelCode = $"{entry.Code}-2000-{sequence:00}";
            return new MachineLineItemInfo(entry.ModuleType, modelCode, SampleSpeed);
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
                Text = machine.ModuleType,
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

            var grid = new Grid { Width = 68, Height = 118, Margin = new Thickness(6, 0, 6, 0) };
            grid.Children.Add(rect);
            grid.Children.Add(iconCircle);
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
            SelectedModuleLineText.Text = $"{focused.ModelCode} {focused.ModuleType}";
            PositionValueText.Text = $"{_focusedIndex + 1} of {_machines.Count}";
            StatusValueText.Text = focused.IsRegistered ? "Registered" : "Not Registered";
            SpeedValueText.Text = focused.Speed;
        }

        private void RefreshConfigurationStatus()
        {
            bool hasMachines = _machines.Count > 0;
            ConfigStatusText.Text = hasMachines ? "All machines online." : "No status.";
            ConfigStatusIconBg.Background = (Brush)FindResource(hasMachines ? "StatusRunningBgBrush" : "StatusOfflineBgBrush");
            ConfigStatusIconText.Foreground = (Brush)FindResource(hasMachines ? "StatusRunningBrush" : "StatusOfflineBrush");
        }

        private void RefreshLineActionsEnabled()
        {
            bool hasMachines = _machines.Count > 0;
            AddModuleButton.IsEnabled = hasMachines;
            RemoveModuleButton.IsEnabled = hasMachines;
            ReplaceModuleButton.IsEnabled = hasMachines;
            ReviewChangesButton.IsEnabled = hasMachines;
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

        private void OnAddModuleClick(object sender, RoutedEventArgs e)
        {
            string anchorModuleType = _machines.Count > 0 ? _machines[_focusedIndex].ModuleType : null;
            AddModuleWizardDialogControl.Open(Catalog.Select(c => c.ModuleType), anchorModuleType);
        }

        private void OnAddModuleConfirmed(object sender, AddModuleRequestInfo request)
        {
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
            RefreshAll();

            LastActionText.Text = $"Last action: Added {request.ModuleType} ({request.PositionSummary}).";
        }

        private void OnRemoveModuleClick(object sender, RoutedEventArgs e)
        {
            if (_machines.Count == 0)
            {
                return;
            }

            _machines.RemoveAt(_focusedIndex);
            _focusedIndex = _machines.Count == 0 ? -1 : System.Math.Min(_focusedIndex, _machines.Count - 1);
            RefreshAll();
        }

        private void OnReplaceModuleClick(object sender, RoutedEventArgs e)
        {
            if (_machines.Count == 0)
            {
                return;
            }

            string currentType = _machines[_focusedIndex].ModuleType;
            int currentCatalogIndex = System.Array.FindIndex(Catalog, c => c.ModuleType == currentType);
            var nextEntry = Catalog[(currentCatalogIndex + 1) % Catalog.Length];
            _machines[_focusedIndex] = CreateMachine(nextEntry.ModuleType);
            RefreshAll();
        }

        private void OnReviewChangesClick(object sender, RoutedEventArgs e)
        {
            LastActionText.Text = "Last action: Review changes (stub)";
        }
    }
}
