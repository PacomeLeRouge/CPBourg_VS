using System;
using System.Collections.Generic;
using System.Globalization;
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
        private List<string> _moduleTypeCatalog = new List<string>();

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

            _moduleTypeCatalog = moduleTypeCatalog?.ToList() ?? new List<string>();
            _pendingModuleType = _moduleTypeCatalog.FirstOrDefault();

            ShowModuleStep();
            Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            RebuildModuleOptions();

            if (PositionStepPanel.Visibility == Visibility.Visible)
            {
                RefreshPositionStep();
            }
            else if (ConfirmStepPanel.Visibility == Visibility.Visible)
            {
                RefreshConfirmStep();
            }
        }

        private void RebuildModuleOptions()
        {
            ModuleOptionsItemsControl.ItemsSource = _moduleTypeCatalog
                .Select(moduleType => new ModuleTypeOptionInfo(
                    moduleType,
                    T(moduleType),
                    isSelected: moduleType == _pendingModuleType))
                .ToList();
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

            RefreshPositionStep();
        }

        private void RefreshPositionStep()
        {
            PositionStepModuleText.Text = T(_pendingModuleType);
            PositionOptionsItemsControl.ItemsSource = new List<LinePositionOptionInfo>
            {
                new LinePositionOptionInfo(true, TF("Before {0}", T(_anchorModuleType))),
                new LinePositionOptionInfo(false, TF("After {0}", T(_anchorModuleType))),
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

            RefreshConfirmStep();
        }

        private void RefreshConfirmStep()
        {
            ConfirmModuleText.Text = T(_pendingModuleType);
            ConfirmPositionText.Text = BuildPositionSummary();
        }

        private string BuildPositionSummary()
        {
            if (_pendingPlaceBeforeAnchor == null)
            {
                return T("Start of line");
            }

            return TF(_pendingPlaceBeforeAnchor.Value ? "Before {0}" : "After {0}",
                T(_anchorModuleType));
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
            Confirmed?.Invoke(this, new AddModuleRequestInfo(
                _pendingModuleType, _pendingPlaceBeforeAnchor, _anchorModuleType));
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
