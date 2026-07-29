using System.Globalization;
using System.Windows.Input;
using System.Windows.Markup;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Requests the Windows input language that corresponds to the selected
    /// physical keyboard family. Windows uses the closest installed layout.
    /// </summary>
    public static class KeyboardLayoutManager
    {
        public static void Apply(string layout, System.Windows.FrameworkElement root)
        {
            string cultureName;
            switch (layout)
            {
                case "QWERTZ":
                    cultureName = "de-DE";
                    break;
                case "QWERTY":
                    cultureName = "en-GB";
                    break;
                default:
                    cultureName = "fr-FR";
                    break;
            }

            try
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                InputLanguageManager.Current.CurrentInputLanguage = culture;
                if (root != null)
                {
                    root.Language = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
                }
            }
            catch (System.ArgumentException)
            {
                // The saved preference remains valid; locked-down Windows
                // images may not have every optional keyboard pack installed.
            }
        }
    }
}
