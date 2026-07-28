using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Persists operator preferences under LocalAppData so installed builds do
    /// not need write access to their program directory.
    /// </summary>
    public sealed class OperatorPreferencesStore
    {
        private readonly string _filePath;

        public OperatorPreferencesStore()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CPBourg", "NextGenGui", "operator-preferences.xml"))
        {
        }

        internal OperatorPreferencesStore(string filePath)
        {
            _filePath = filePath;
        }

        public OperatorPreferences Load()
        {
            OperatorPreferences defaults = OperatorPreferences.CreateDefaults();
            if (!File.Exists(_filePath))
            {
                return defaults;
            }

            try
            {
                XElement root = XDocument.Load(_filePath).Root;
                if (root == null)
                {
                    return defaults;
                }

                return new OperatorPreferences
                {
                    Language = ReadChoice(root, "Language", defaults.Language,
                        "English", "Français", "Nederlands", "Deutsch", "Español", "Italiano", "Italiana"),
                    Units = ReadChoice(root, "Units", defaults.Units, "Millimeters", "Inches"),
                    KeyboardLayout = ReadChoice(root, "KeyboardLayout", defaults.KeyboardLayout,
                        "AZERTY", "QWERTY", "QWERTZ"),
                    MouseCursor = ReadChoice(root, "MouseCursor", defaults.MouseCursor,
                        "Disabled", "Enabled"),
                    DateTimeOffsetTicks = ReadLong(root, "DateTimeOffsetTicks",
                        defaults.DateTimeOffsetTicks),
                    FontSize = ReadChoice(root, "FontSize", defaults.FontSize,
                        "Small", "Medium", "Large"),
                    ScreenCalibrated = ReadBoolean(root, "ScreenCalibrated",
                        defaults.ScreenCalibrated),
                    CalibrationErrorPixels = ReadDouble(root, "CalibrationErrorPixels",
                        defaults.CalibrationErrorPixels),
                };
            }
            catch (Exception exception) when (exception is IOException ||
                                              exception is UnauthorizedAccessException ||
                                              exception is System.Xml.XmlException)
            {
                return defaults;
            }
        }

        public bool TrySave(OperatorPreferences preferences, out string errorMessage)
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var document = new XDocument(
                    new XElement("OperatorPreferences",
                        new XAttribute("version", "1"),
                        new XElement("Language", preferences.Language),
                        new XElement("Units", preferences.Units),
                        new XElement("KeyboardLayout", preferences.KeyboardLayout),
                        new XElement("MouseCursor", preferences.MouseCursor),
                        new XElement("DateTimeOffsetTicks", preferences.DateTimeOffsetTicks),
                        new XElement("FontSize", preferences.FontSize),
                        new XElement("ScreenCalibrated", preferences.ScreenCalibrated),
                        new XElement("CalibrationErrorPixels",
                            preferences.CalibrationErrorPixels.ToString(
                                CultureInfo.InvariantCulture))));
                document.Save(_filePath);
                errorMessage = null;
                return true;
            }
            catch (Exception exception) when (exception is IOException ||
                                              exception is UnauthorizedAccessException)
            {
                errorMessage = "Operator preferences could not be saved: " + exception.Message;
                return false;
            }
        }

        private static string ReadChoice(XElement root, string name, string fallback,
            params string[] allowed)
        {
            XElement element = root.Element(name);
            if (element == null)
            {
                return fallback;
            }

            foreach (string value in allowed)
            {
                if (string.Equals(element.Value, value, StringComparison.Ordinal))
                {
                    return value == "Italiana" ? "Italiano" : value;
                }
            }

            return fallback;
        }

        private static bool ReadBoolean(XElement root, string name, bool fallback)
        {
            bool value;
            XElement element = root.Element(name);
            return element != null && bool.TryParse(element.Value, out value) ? value : fallback;
        }

        private static long ReadLong(XElement root, string name, long fallback)
        {
            long value;
            XElement element = root.Element(name);
            return element != null && long.TryParse(element.Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        private static double ReadDouble(XElement root, string name, double fallback)
        {
            double value;
            XElement element = root.Element(name);
            return element != null && double.TryParse(element.Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out value) ? value : fallback;
        }
    }
}
