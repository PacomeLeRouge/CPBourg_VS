using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Reusable "Change [setting]" picker dialog - see ChangeValueDialog.xaml
    /// for the visual structure and the reasoning behind it.
    /// </summary>
    public partial class ChangeValueDialog : UserControl
    {
        /// <summary>Raised when OK is clicked with a selection made. The
        /// string is the selected option's <see cref="ChangeOptionInfo.Value"/>.</summary>
        public event EventHandler<string> Confirmed;

        private string _pendingSelection;

        public ChangeValueDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Configures and shows the dialog. <paramref name="options"/> should
        /// have exactly one item with <see cref="ChangeOptionInfo.IsSelected"/>
        /// set true, matching the setting's current value.
        /// </summary>
        public void Open(string title, string currentValueDisplay, List<ChangeOptionInfo> options)
        {
            DialogTitleText.Text = title;
            CurrentValueText.Text = LocalizationManager.Translate("Current: ") + currentValueDisplay;
            _pendingSelection = null;

            foreach (var option in options)
            {
                if (option.IsSelected)
                {
                    _pendingSelection = option.Value;
                    break;
                }
            }

            OptionsItemsControl.ItemsSource = options;
            Visibility = Visibility.Visible;
            LocalizationManager.Apply(this);
        }

        private void OnOptionChecked(object sender, RoutedEventArgs e)
        {
            var option = (sender as FrameworkElement)?.DataContext as ChangeOptionInfo;
            if (option != null)
            {
                _pendingSelection = option.Value;
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            if (_pendingSelection != null)
            {
                Confirmed?.Invoke(this, _pendingSelection);
            }
        }

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
