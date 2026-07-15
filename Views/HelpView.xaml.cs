using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Single-link Help screen for the official manuals area.</summary>
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void OnManualsRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Handled = true;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true,
                });
            }
            catch (Exception)
            {
                // The target may block launching a browser in kiosk mode. The
                // visible URL remains available for an operator to copy.
            }
        }
    }
}
