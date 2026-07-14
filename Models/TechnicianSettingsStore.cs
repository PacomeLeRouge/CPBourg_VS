using System;
using System.IO;
using System.Xml.Linq;

namespace CPBourg.NextGenGui.Models
{
    /// <summary>
    /// Persists technician choices outside the installation directory, which
    /// may be read-only on the industrial PC. A malformed or incomplete file
    /// safely falls back to the approved defaults.
    /// </summary>
    public sealed class TechnicianSettingsStore
    {
        private readonly string _filePath;

        public TechnicianSettingsStore()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CPBourg", "NextGenGui", "technician-settings.xml"))
        {
        }

        internal TechnicianSettingsStore(string filePath)
        {
            _filePath = filePath;
        }

        public TechnicianSettings Load()
        {
            var defaults = TechnicianSettings.CreateDefaults();
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

                return new TechnicianSettings
                {
                    CalibrationSetOption = ReadChoice(root, "CalibrationSetOption",
                        defaults.CalibrationSetOption, "Always Process", "Always Reject"),
                    SaveAdjustmentsEnabled = ReadBoolean(root, "SaveAdjustmentsEnabled",
                        defaults.SaveAdjustmentsEnabled),
                    StitchHeadForm = ReadChoice(root, "StitchHeadForm",
                        defaults.StitchHeadForm, "Normal Stitch", "Loop Stitch"),
                    DisableStitchHead1 = ReadBoolean(root, "DisableStitchHead1",
                        defaults.DisableStitchHead1),
                    DisableStitchHead2 = ReadBoolean(root, "DisableStitchHead2",
                        defaults.DisableStitchHead2),
                    StitchSingleSheetEnabled = ReadBoolean(root, "StitchSingleSheetEnabled",
                        defaults.StitchSingleSheetEnabled),
                    PurgeOption = ReadChoice(root, "PurgeOption",
                        defaults.PurgeOption, "Ask Operator", "Always Process", "Always Purge"),
                    SheetInCompilerRestartAllowed = ReadBoolean(root,
                        "SheetInCompilerRestartAllowed", defaults.SheetInCompilerRestartAllowed),
                };
            }
            catch (IOException)
            {
                return defaults;
            }
            catch (UnauthorizedAccessException)
            {
                return defaults;
            }
            catch (System.Xml.XmlException)
            {
                return defaults;
            }
        }

        public bool TrySave(TechnicianSettings settings, out string errorMessage)
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var document = new XDocument(
                    new XElement("TechnicianSettings",
                        new XAttribute("version", "1"),
                        new XElement("CalibrationSetOption", settings.CalibrationSetOption),
                        new XElement("SaveAdjustmentsEnabled", settings.SaveAdjustmentsEnabled),
                        new XElement("StitchHeadForm", settings.StitchHeadForm),
                        new XElement("DisableStitchHead1", settings.DisableStitchHead1),
                        new XElement("DisableStitchHead2", settings.DisableStitchHead2),
                        new XElement("StitchSingleSheetEnabled", settings.StitchSingleSheetEnabled),
                        new XElement("PurgeOption", settings.PurgeOption),
                        new XElement("SheetInCompilerRestartAllowed",
                            settings.SheetInCompilerRestartAllowed)));

                document.Save(_filePath);
                errorMessage = null;
                return true;
            }
            catch (Exception exception) when (exception is IOException ||
                                              exception is UnauthorizedAccessException)
            {
                errorMessage = "The technician settings could not be saved: " + exception.Message;
                return false;
            }
        }

        private static bool ReadBoolean(XElement root, string name, bool fallback)
        {
            bool value;
            XElement element = root.Element(name);
            return element != null && bool.TryParse(element.Value, out value) ? value : fallback;
        }

        private static string ReadChoice(XElement root, string name, string fallback,
            params string[] allowedValues)
        {
            XElement element = root.Element(name);
            if (element == null)
            {
                return fallback;
            }

            foreach (string allowedValue in allowedValues)
            {
                if (string.Equals(element.Value, allowedValue, StringComparison.Ordinal))
                {
                    return allowedValue;
                }
            }

            return fallback;
        }
    }
}
