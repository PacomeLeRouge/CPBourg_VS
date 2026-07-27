using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Add Module wizard - see AddModuleWizardDialog.xaml for the panel
    /// structure and the reasoning behind it.
    /// </summary>
    public partial class AddModuleWizardDialog : UserControl
    {
        /// <summary>Raised when the operator confirms the pending module
        /// selection. Technician authorization happens later, once for the
        /// complete line configuration.</summary>
        public event EventHandler<AddModuleRequestInfo> Confirmed;

        private string _anchorModuleType;
        private string _pendingModuleType;
        private bool? _pendingPlaceBeforeAnchor;

        public AddModuleWizardDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Configures and shows the dialog. <paramref name="anchorModuleType"/>
        /// is the currently focused module on the line (Before/After it are
        /// the two position choices) or null if the line is empty, in which
        /// case the Position step is skipped entirely.
        /// </summary>
        public void Open(IEnumerable<string> moduleTypeCatalog, string anchorModuleType)
        {
            _anchorModuleType = anchorModuleType;
            _pendingModuleType = null;
            _pendingPlaceBeforeAnchor = null;

            var options = moduleTypeCatalog
                .Select((moduleType, index) => new ModuleTypeOptionInfo(moduleType, isSelected: index == 0))
                .ToList();
            _pendingModuleType = options.FirstOrDefault()?.ModuleType;
            ModuleOptionsItemsControl.ItemsSource = options;

            ShowModuleStep();
            Visibility = Visibility.Visible;
        }

        // ================= Step switching =================

        private void ShowModuleStep()
        {
            ModuleStepPanel.Visibility = Visibility.Visible;
            PositionStepPanel.Visibility = Visibility.Collapsed;
            ConfirmStepPanel.Visibility = Visibility.Collapsed;

            ModuleContinueButton.Visibility = Visibility.Visible;
            PositionContinueButton.Visibility = Visibility.Collapsed;
            ConfirmButton.Visibility = Visibility.Collapsed;
        }

        private void ShowPositionStep()
        {
            ModuleStepPanel.Visibility = Visibility.Collapsed;
            PositionStepPanel.Visibility = Visibility.Visible;
            ConfirmStepPanel.Visibility = Visibility.Collapsed;

            ModuleContinueButton.Visibility = Visibility.Collapsed;
            PositionContinueButton.Visibility = Visibility.Visible;
            ConfirmButton.Visibility = Visibility.Collapsed;

            PositionContinueButton.IsEnabled = false;
            _pendingPlaceBeforeAnchor = null;

            PositionStepModuleText.Text = _pendingModuleType;
            PositionOptionsItemsControl.ItemsSource = new List<LinePositionOptionInfo>
            {
                new LinePositionOptionInfo(true, $"Before {_anchorModuleType}"),
                new LinePositionOptionInfo(false, $"After {_anchorModuleType}"),
            };
        }

        private void ShowConfirmStep()
        {
            ModuleStepPanel.Visibility = Visibility.Collapsed;
            PositionStepPanel.Visibility = Visibility.Collapsed;
            ConfirmStepPanel.Visibility = Visibility.Visible;

            ModuleContinueButton.Visibility = Visibility.Collapsed;
            PositionContinueButton.Visibility = Visibility.Collapsed;
            ConfirmButton.Visibility = Visibility.Visible;

            ConfirmModuleText.Text = _pendingModuleType;
            ConfirmPositionText.Text = BuildPositionSummary();
        }

        private string BuildPositionSummary()
        {
            if (_pendingPlaceBeforeAnchor == null)
            {
                return "Start of line";
            }

            return (_pendingPlaceBeforeAnchor.Value ? "Before " : "After ") + _anchorModuleType;
        }

        // ================= Step 1: Module Selection =================

        private void OnModuleOptionChecked(object sender, RoutedEventArgs e)
        {
            var option = (sender as FrameworkElement)?.DataContext as ModuleTypeOptionInfo;
            if (option != null)
            {
                _pendingModuleType = option.ModuleType;
            }
        }

        private void OnModuleContinueClick(object sender, RoutedEventArgs e)
        {
            if (_pendingModuleType == null)
            {
                return;
            }

            if (_anchorModuleType == null)
            {
                ShowConfirmStep();
            }
            else
            {
                ShowPositionStep();
            }
        }

        // ================= Step 2: Position Selection =================

        private void OnPositionOptionChecked(object sender, RoutedEventArgs e)
        {
            var option = (sender as FrameworkElement)?.DataContext as LinePositionOptionInfo;
            if (option != null)
            {
                _pendingPlaceBeforeAnchor = option.IsBeforeAnchor;
                PositionContinueButton.IsEnabled = true;
            }
        }

        private void OnPositionContinueClick(object sender, RoutedEventArgs e)
        {
            if (_pendingPlaceBeforeAnchor == null)
            {
                return;
            }

            ShowConfirmStep();
        }

        // ================= Step 3: Review pending module =================

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Confirmed?.Invoke(this, new AddModuleRequestInfo(_pendingModuleType, _pendingPlaceBeforeAnchor, BuildPositionSummary()));
        }

        // ================= Cancel =================

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
