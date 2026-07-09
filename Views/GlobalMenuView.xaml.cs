using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Slide-out global navigation panel. Raises <see cref="CloseRequested"/>
    /// when the user dismisses it (X button or clicking the scrim) and
    /// <see cref="ItemSelected"/> when a nav item is clicked - the shell
    /// (MainWindow) owns actually navigating anywhere.
    /// </summary>
    public partial class GlobalMenuView : UserControl
    {
        public event EventHandler CloseRequested;
        public event EventHandler<string> ItemSelected;

        public GlobalMenuView()
        {
            InitializeComponent();
        }

        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnNavItemClick(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement)?.Tag as string ?? string.Empty;
            ItemSelected?.Invoke(this, tag);
            // Close after navigating - matches the reference behaviour where
            // picking an item dismisses the overlay.
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
