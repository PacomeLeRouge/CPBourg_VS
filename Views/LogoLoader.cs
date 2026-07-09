using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Loads the real CPBourg logo from disk if it has been provided, and
    /// swaps it in for the placeholder two-square mark used elsewhere in
    /// the app. Falls back to the placeholder cleanly if the file is
    /// missing, so the app never breaks for not having a real logo yet.
    ///
    /// Expected location (relative to the built executable):
    ///     Assets/cpbourg-logo.png
    ///
    /// See README "Using the real CPBourg logo" for where to get the
    /// official file and how to add it.
    /// </summary>
    public static class LogoLoader
    {
        private const string RelativePath = "Assets/cpbourg-logo.png";

        public static void Apply(Image logoImage, UIElement placeholder)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RelativePath);

            if (!File.Exists(path))
            {
                // No real logo provided yet - keep showing the placeholder mark.
                return;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();

                logoImage.Source = bitmap;
                logoImage.Visibility = Visibility.Visible;
                placeholder.Visibility = Visibility.Collapsed;
            }
            catch
            {
                // Malformed/unreadable image file - fail safe to the placeholder
                // rather than crashing the app over a branding asset.
                logoImage.Visibility = Visibility.Collapsed;
                placeholder.Visibility = Visibility.Visible;
            }
        }
    }
}
