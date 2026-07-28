using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Lightweight runtime localization for the prototype. Static XAML text
    /// retains its English source in an attached property, allowing repeated
    /// language changes without rebuilding each view.
    /// </summary>
    public static partial class LocalizationManager
    {
        private static readonly DependencyProperty SourceTextProperty =
            DependencyProperty.RegisterAttached("SourceText", typeof(string),
                typeof(LocalizationManager), new PropertyMetadata(null));

        private static readonly Dictionary<string, Dictionary<string, string>> Translations =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private static string _language = "English";

        static LocalizationManager()
        {
            Add("fr",
                "Home", "Accueil", "Settings", "Paramètres",
                "Operator Preferences", "Préférences opérateur",
                "Cancel", "Annuler", "Apply", "Appliquer", "Change", "Modifier",
                "Calibrate", "Calibrer", "Confirm", "Confirmer", "Close", "Fermer",
                "Unsaved changes", "Modifications non enregistrées",
                "Please apply changes before exiting.", "Appliquez les modifications avant de quitter.",
                "Preferences saved", "Préférences enregistrées",
                "LANGUAGE AND REGION", "LANGUE ET RÉGION", "DISPLAY", "AFFICHAGE",
                "Language", "Langue", "Units", "Unités", "Date and Time", "Date et heure",
                "Keyboard Layout", "Disposition du clavier", "Mouse Cursor", "Curseur de souris",
                "Font Size", "Taille du texte", "Screen Calibration", "Calibrage de l’écran",
                "Required if touch offset occurs", "Requis en cas de décalage tactile",
                "Change Language", "Changer la langue", "Change Units", "Changer les unités",
                "Change Keyboard Layout", "Changer la disposition du clavier",
                "Change Mouse Cursor", "Changer le curseur de souris",
                "Change Font Size", "Changer la taille du texte",
                "Change Date and Time", "Changer la date et l’heure",
                "Change Setting", "Modifier le paramètre", "Current: ", "Actuel : ",
                "Metric (millimeters)", "Métrique (millimètres)",
                "Imperial (inches)", "Impérial (pouces)",
                "Millimeters", "Millimètres", "Inches", "Pouces",
                "Enabled", "Activé", "Disabled", "Désactivé",
                "Small", "Petite", "Medium", "Moyenne", "Medium (recommended)", "Moyenne (recommandée)",
                "Large", "Grande", "Not calibrated", "Non calibré", "Calibrated", "Calibré",
                "Date", "Date", "Time (24-hour)", "Heure (24 h)",
                "Choose a valid date and time.", "Choisissez une date et une heure valides.",
                "This changes the date and time displayed by the operator interface.",
                    "Cela modifie la date et l’heure affichées par l’interface opérateur.",
                "Global Menu", "Menu général", "GENERAL", "GÉNÉRAL",
                "Job / File menu", "Menu tâches / fichiers",
                "Error & Information", "Erreurs et informations",
                "Settings / Preferences", "Paramètres / Préférences",
                "CONFIGURATION", "CONFIGURATION", "Machine Line Configuration", "Configuration de la ligne",
                "ADVANCED", "AVANCÉ", "Technician Interface", "Interface technicien",
                "HELP", "AIDE", "Help / Manual", "Aide / Manuel",
                "About / Version", "À propos / Version",
                "Counter and Productivity", "Compteur et productivité",
                "Completed Sets", "Lots terminés", "Preset (0 = unlimited)", "Objectif (0 = illimité)",
                "Output / h", "Production / h", "Reset to zero", "Remettre à zéro",
                "Set target", "Définir l’objectif", "Current Jobs", "Tâches actuelles",
                "New job", "Nouvelle tâche", "Load job", "Charger la tâche",
                "Active Alerts", "Alertes actives", "View errors", "Voir les erreurs",
                "Machines", "Machines", "Purge", "Purger", "Start", "Démarrer",
                "Pause", "Pause", "Stop", "Arrêter", "Ready", "Prêt",
                "Running", "En marche", "Offline", "Hors ligne",
                "Calibrate Touch Screen", "Calibrer l’écran tactile",
                "Tap the centre of each target.", "Touchez le centre de chaque cible.",
                "Target {0} of {1}", "Cible {0} sur {1}",
                "Calibration complete", "Calibrage terminé");

            Add("nl",
                "Home", "Start", "Settings", "Instellingen",
                "Operator Preferences", "Operatorvoorkeuren",
                "Cancel", "Annuleren", "Apply", "Toepassen", "Change", "Wijzigen",
                "Calibrate", "Kalibreren", "Confirm", "Bevestigen", "Close", "Sluiten",
                "Unsaved changes", "Niet-opgeslagen wijzigingen",
                "Please apply changes before exiting.", "Pas wijzigingen toe voordat u afsluit.",
                "Preferences saved", "Voorkeuren opgeslagen",
                "LANGUAGE AND REGION", "TAAL EN REGIO", "DISPLAY", "WEERGAVE",
                "Language", "Taal", "Units", "Eenheden", "Date and Time", "Datum en tijd",
                "Keyboard Layout", "Toetsenbordindeling", "Mouse Cursor", "Muiscursor",
                "Font Size", "Tekstgrootte", "Screen Calibration", "Schermkalibratie",
                "Required if touch offset occurs", "Vereist bij een aanraakafwijking",
                "Change Language", "Taal wijzigen", "Change Units", "Eenheden wijzigen",
                "Change Keyboard Layout", "Toetsenbordindeling wijzigen",
                "Change Mouse Cursor", "Muiscursor wijzigen",
                "Change Font Size", "Tekstgrootte wijzigen",
                "Change Date and Time", "Datum en tijd wijzigen",
                "Change Setting", "Instelling wijzigen", "Current: ", "Huidig: ",
                "Metric (millimeters)", "Metrisch (millimeter)",
                "Imperial (inches)", "Imperiaal (inch)",
                "Millimeters", "Millimeter", "Inches", "Inch",
                "Enabled", "Ingeschakeld", "Disabled", "Uitgeschakeld",
                "Small", "Klein", "Medium", "Gemiddeld", "Medium (recommended)", "Gemiddeld (aanbevolen)",
                "Large", "Groot", "Not calibrated", "Niet gekalibreerd", "Calibrated", "Gekalibreerd",
                "Date", "Datum", "Time (24-hour)", "Tijd (24 uur)",
                "Choose a valid date and time.", "Kies een geldige datum en tijd.",
                "This changes the date and time displayed by the operator interface.",
                    "Dit wijzigt de datum en tijd van de operatorinterface.",
                "Global Menu", "Hoofdmenu", "GENERAL", "ALGEMEEN",
                "Job / File menu", "Taak-/bestandsmenu",
                "Error & Information", "Fouten en informatie",
                "Settings / Preferences", "Instellingen / Voorkeuren",
                "CONFIGURATION", "CONFIGURATIE", "Machine Line Configuration", "Machinelijn configureren",
                "ADVANCED", "GEAVANCEERD", "Technician Interface", "Technicusinterface",
                "HELP", "HELP", "Help / Manual", "Help / Handleiding",
                "About / Version", "Info / Versie",
                "Counter and Productivity", "Teller en productiviteit",
                "Completed Sets", "Voltooide sets", "Preset (0 = unlimited)", "Doel (0 = onbeperkt)",
                "Output / h", "Uitvoer / u", "Reset to zero", "Op nul zetten",
                "Set target", "Doel instellen", "Current Jobs", "Huidige taken",
                "New job", "Nieuwe taak", "Load job", "Taak laden",
                "Active Alerts", "Actieve meldingen", "View errors", "Fouten bekijken",
                "Machines", "Machines", "Purge", "Legen", "Start", "Starten",
                "Pause", "Pauzeren", "Stop", "Stoppen", "Ready", "Gereed",
                "Running", "Actief", "Offline", "Offline",
                "Calibrate Touch Screen", "Aanraakscherm kalibreren",
                "Tap the centre of each target.", "Tik op het midden van elke doelmarkering.",
                "Target {0} of {1}", "Doel {0} van {1}",
                "Calibration complete", "Kalibratie voltooid");

            Add("de",
                "Home", "Startseite", "Settings", "Einstellungen",
                "Operator Preferences", "Bedienereinstellungen",
                "Cancel", "Abbrechen", "Apply", "Anwenden", "Change", "Ändern",
                "Calibrate", "Kalibrieren", "Confirm", "Bestätigen", "Close", "Schließen",
                "Unsaved changes", "Nicht gespeicherte Änderungen",
                "Please apply changes before exiting.", "Bitte Änderungen vor dem Verlassen anwenden.",
                "Preferences saved", "Einstellungen gespeichert",
                "LANGUAGE AND REGION", "SPRACHE UND REGION", "DISPLAY", "ANZEIGE",
                "Language", "Sprache", "Units", "Einheiten", "Date and Time", "Datum und Uhrzeit",
                "Keyboard Layout", "Tastaturbelegung", "Mouse Cursor", "Mauszeiger",
                "Font Size", "Textgröße", "Screen Calibration", "Bildschirmkalibrierung",
                "Required if touch offset occurs", "Erforderlich bei Berührungsversatz",
                "Change Language", "Sprache ändern", "Change Units", "Einheiten ändern",
                "Change Keyboard Layout", "Tastaturbelegung ändern",
                "Change Mouse Cursor", "Mauszeiger ändern",
                "Change Font Size", "Textgröße ändern",
                "Change Date and Time", "Datum und Uhrzeit ändern",
                "Change Setting", "Einstellung ändern", "Current: ", "Aktuell: ",
                "Metric (millimeters)", "Metrisch (Millimeter)",
                "Imperial (inches)", "Imperial (Zoll)",
                "Millimeters", "Millimeter", "Inches", "Zoll",
                "Enabled", "Aktiviert", "Disabled", "Deaktiviert",
                "Small", "Klein", "Medium", "Mittel", "Medium (recommended)", "Mittel (empfohlen)",
                "Large", "Groß", "Not calibrated", "Nicht kalibriert", "Calibrated", "Kalibriert",
                "Date", "Datum", "Time (24-hour)", "Uhrzeit (24 Stunden)",
                "Choose a valid date and time.", "Wählen Sie ein gültiges Datum und eine Uhrzeit.",
                "This changes the date and time displayed by the operator interface.",
                    "Dies ändert Datum und Uhrzeit der Bedieneroberfläche.",
                "Global Menu", "Hauptmenü", "GENERAL", "ALLGEMEIN",
                "Job / File menu", "Auftrags-/Dateimenü",
                "Error & Information", "Fehler und Informationen",
                "Settings / Preferences", "Einstellungen / Präferenzen",
                "CONFIGURATION", "KONFIGURATION", "Machine Line Configuration", "Maschinenlinie konfigurieren",
                "ADVANCED", "ERWEITERT", "Technician Interface", "Technikeroberfläche",
                "HELP", "HILFE", "Help / Manual", "Hilfe / Handbuch",
                "About / Version", "Info / Version",
                "Counter and Productivity", "Zähler und Produktivität",
                "Completed Sets", "Fertige Sätze", "Preset (0 = unlimited)", "Ziel (0 = unbegrenzt)",
                "Output / h", "Ausgabe / h", "Reset to zero", "Auf null setzen",
                "Set target", "Ziel festlegen", "Current Jobs", "Aktuelle Aufträge",
                "New job", "Neuer Auftrag", "Load job", "Auftrag laden",
                "Active Alerts", "Aktive Meldungen", "View errors", "Fehler anzeigen",
                "Machines", "Maschinen", "Purge", "Leeren", "Start", "Starten",
                "Pause", "Pause", "Stop", "Stopp", "Ready", "Bereit",
                "Running", "In Betrieb", "Offline", "Offline",
                "Calibrate Touch Screen", "Touchscreen kalibrieren",
                "Tap the centre of each target.", "Tippen Sie auf die Mitte jedes Ziels.",
                "Target {0} of {1}", "Ziel {0} von {1}",
                "Calibration complete", "Kalibrierung abgeschlossen");

            Add("es",
                "Home", "Inicio", "Settings", "Ajustes",
                "Operator Preferences", "Preferencias del operador",
                "Cancel", "Cancelar", "Apply", "Aplicar", "Change", "Cambiar",
                "Calibrate", "Calibrar", "Confirm", "Confirmar", "Close", "Cerrar",
                "Unsaved changes", "Cambios sin guardar",
                "Please apply changes before exiting.", "Aplique los cambios antes de salir.",
                "Preferences saved", "Preferencias guardadas",
                "LANGUAGE AND REGION", "IDIOMA Y REGIÓN", "DISPLAY", "PANTALLA",
                "Language", "Idioma", "Units", "Unidades", "Date and Time", "Fecha y hora",
                "Keyboard Layout", "Distribución del teclado", "Mouse Cursor", "Cursor del ratón",
                "Font Size", "Tamaño del texto", "Screen Calibration", "Calibración de pantalla",
                "Required if touch offset occurs", "Necesario si hay desplazamiento táctil",
                "Change Language", "Cambiar idioma", "Change Units", "Cambiar unidades",
                "Change Keyboard Layout", "Cambiar distribución del teclado",
                "Change Mouse Cursor", "Cambiar cursor del ratón",
                "Change Font Size", "Cambiar tamaño del texto",
                "Change Date and Time", "Cambiar fecha y hora",
                "Change Setting", "Cambiar ajuste", "Current: ", "Actual: ",
                "Metric (millimeters)", "Métrico (milímetros)",
                "Imperial (inches)", "Imperial (pulgadas)",
                "Millimeters", "Milímetros", "Inches", "Pulgadas",
                "Enabled", "Activado", "Disabled", "Desactivado",
                "Small", "Pequeño", "Medium", "Mediano", "Medium (recommended)", "Mediano (recomendado)",
                "Large", "Grande", "Not calibrated", "Sin calibrar", "Calibrated", "Calibrado",
                "Date", "Fecha", "Time (24-hour)", "Hora (24 horas)",
                "Choose a valid date and time.", "Elija una fecha y hora válidas.",
                "This changes the date and time displayed by the operator interface.",
                    "Esto cambia la fecha y hora de la interfaz del operador.",
                "Global Menu", "Menú principal", "GENERAL", "GENERAL",
                "Job / File menu", "Menú de trabajos / archivos",
                "Error & Information", "Errores e información",
                "Settings / Preferences", "Ajustes / Preferencias",
                "CONFIGURATION", "CONFIGURACIÓN", "Machine Line Configuration", "Configuración de la línea",
                "ADVANCED", "AVANZADO", "Technician Interface", "Interfaz técnica",
                "HELP", "AYUDA", "Help / Manual", "Ayuda / Manual",
                "About / Version", "Acerca de / Versión",
                "Counter and Productivity", "Contador y productividad",
                "Completed Sets", "Conjuntos completados", "Preset (0 = unlimited)", "Objetivo (0 = ilimitado)",
                "Output / h", "Producción / h", "Reset to zero", "Restablecer a cero",
                "Set target", "Fijar objetivo", "Current Jobs", "Trabajos actuales",
                "New job", "Nuevo trabajo", "Load job", "Cargar trabajo",
                "Active Alerts", "Alertas activas", "View errors", "Ver errores",
                "Machines", "Máquinas", "Purge", "Purgar", "Start", "Iniciar",
                "Pause", "Pausar", "Stop", "Detener", "Ready", "Listo",
                "Running", "En marcha", "Offline", "Sin conexión",
                "Calibrate Touch Screen", "Calibrar pantalla táctil",
                "Tap the centre of each target.", "Toque el centro de cada objetivo.",
                "Target {0} of {1}", "Objetivo {0} de {1}",
                "Calibration complete", "Calibración completada");

            Add("it",
                "Home", "Home", "Settings", "Impostazioni",
                "Operator Preferences", "Preferenze operatore",
                "Cancel", "Annulla", "Apply", "Applica", "Change", "Modifica",
                "Calibrate", "Calibra", "Confirm", "Conferma", "Close", "Chiudi",
                "Unsaved changes", "Modifiche non salvate",
                "Please apply changes before exiting.", "Applica le modifiche prima di uscire.",
                "Preferences saved", "Preferenze salvate",
                "LANGUAGE AND REGION", "LINGUA E REGIONE", "DISPLAY", "SCHERMO",
                "Language", "Lingua", "Units", "Unità", "Date and Time", "Data e ora",
                "Keyboard Layout", "Layout tastiera", "Mouse Cursor", "Cursore del mouse",
                "Font Size", "Dimensione testo", "Screen Calibration", "Calibrazione schermo",
                "Required if touch offset occurs", "Necessario in caso di offset del tocco",
                "Change Language", "Cambia lingua", "Change Units", "Cambia unità",
                "Change Keyboard Layout", "Cambia layout tastiera",
                "Change Mouse Cursor", "Cambia cursore del mouse",
                "Change Font Size", "Cambia dimensione testo",
                "Change Date and Time", "Cambia data e ora",
                "Change Setting", "Modifica impostazione", "Current: ", "Attuale: ",
                "Metric (millimeters)", "Metrico (millimetri)",
                "Imperial (inches)", "Imperiale (pollici)",
                "Millimeters", "Millimetri", "Inches", "Pollici",
                "Enabled", "Attivato", "Disabled", "Disattivato",
                "Small", "Piccolo", "Medium", "Medio", "Medium (recommended)", "Medio (consigliato)",
                "Large", "Grande", "Not calibrated", "Non calibrato", "Calibrated", "Calibrato",
                "Date", "Data", "Time (24-hour)", "Ora (24 ore)",
                "Choose a valid date and time.", "Scegli una data e un’ora valide.",
                "This changes the date and time displayed by the operator interface.",
                    "Questo modifica la data e l’ora dell’interfaccia operatore.",
                "Global Menu", "Menu globale", "GENERAL", "GENERALE",
                "Job / File menu", "Menu lavori / file",
                "Error & Information", "Errori e informazioni",
                "Settings / Preferences", "Impostazioni / Preferenze",
                "CONFIGURATION", "CONFIGURAZIONE", "Machine Line Configuration", "Configurazione linea",
                "ADVANCED", "AVANZATE", "Technician Interface", "Interfaccia tecnico",
                "HELP", "AIUTO", "Help / Manual", "Aiuto / Manuale",
                "About / Version", "Informazioni / Versione",
                "Counter and Productivity", "Contatore e produttività",
                "Completed Sets", "Set completati", "Preset (0 = unlimited)", "Obiettivo (0 = illimitato)",
                "Output / h", "Produzione / h", "Reset to zero", "Azzera",
                "Set target", "Imposta obiettivo", "Current Jobs", "Lavori correnti",
                "New job", "Nuovo lavoro", "Load job", "Carica lavoro",
                "Active Alerts", "Avvisi attivi", "View errors", "Visualizza errori",
                "Machines", "Macchine", "Purge", "Spurga", "Start", "Avvia",
                "Pause", "Pausa", "Stop", "Arresta", "Ready", "Pronto",
                "Running", "In funzione", "Offline", "Offline",
                "Calibrate Touch Screen", "Calibra touchscreen",
                "Tap the centre of each target.", "Tocca il centro di ogni bersaglio.",
                "Target {0} of {1}", "Bersaglio {0} di {1}",
                "Calibration complete", "Calibrazione completata");

            AddStfoTranslations();
        }

        public static string CurrentLanguage => _language;

        public static string GetCode(string language)
        {
            switch (language)
            {
                case "Français": return "fr";
                case "Nederlands": return "nl";
                case "Deutsch": return "de";
                case "Español": return "es";
                case "Italiano":
                case "Italiana": return "it";
                default: return "en";
            }
        }

        public static string GetAbbreviation(string language)
        {
            return GetCode(language).ToUpperInvariant();
        }

        public static void SetLanguage(string language)
        {
            _language = string.IsNullOrWhiteSpace(language) ? "English" : language;
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(GetCode(_language) + "-" +
                    (GetCode(_language) == "en" ? "GB" : GetCode(_language).ToUpperInvariant()));
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.InvariantCulture;
            }

            CultureInfo.CurrentUICulture = culture;
        }

        public static string Translate(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            string code = GetCode(_language);
            Dictionary<string, string> language;
            string translated;
            return code != "en" && Translations.TryGetValue(code, out language) &&
                   language.TryGetValue(source, out translated)
                ? translated
                : source;
        }

        public static void SetLocalizedText(TextBlock textBlock, string source)
        {
            textBlock.SetValue(SourceTextProperty, source);
            textBlock.Text = Translate(source);
        }

        public static void Apply(DependencyObject root)
        {
            ApplyRecursive(root, new HashSet<DependencyObject>());
        }

        private static void ApplyRecursive(DependencyObject element,
            HashSet<DependencyObject> visited)
        {
            if (element == null || !visited.Add(element))
            {
                return;
            }

            // Snapshot both trees before changing text. Updating a Run can
            // mutate its parent's inline collection and invalidate a live
            // LogicalTreeHelper enumerator.
            var logicalChildren = new List<DependencyObject>();
            foreach (object child in LogicalTreeHelper.GetChildren(element))
            {
                var dependencyChild = child as DependencyObject;
                if (dependencyChild != null)
                {
                    logicalChildren.Add(dependencyChild);
                }
            }

            var visualChildren = new List<DependencyObject>();
            if (element is Visual || element is Visual3D)
            {
                int childCount = VisualTreeHelper.GetChildrenCount(element);
                for (int index = 0; index < childCount; index++)
                {
                    visualChildren.Add(VisualTreeHelper.GetChild(element, index));
                }
            }

            var run = element as Run;
            if (run != null && !BindingOperations.IsDataBound(run, Run.TextProperty))
            {
                ApplyString(element, Run.TextProperty, run.Text);
            }
            else
            {
                var textBlock = element as TextBlock;
                if (textBlock != null &&
                    !BindingOperations.IsDataBound(textBlock, TextBlock.TextProperty) &&
                    textBlock.Inlines.Count == 0)
                {
                    ApplyString(element, TextBlock.TextProperty, textBlock.Text);
                }

                var contentControl = element as ContentControl;
                if (contentControl != null && contentControl.Content is string)
                {
                    string source = GetOrRememberSource(contentControl, (string)contentControl.Content);
                    contentControl.Content = Translate(source);
                }

                var toolTipElement = element as FrameworkElement;
                if (toolTipElement != null && toolTipElement.ToolTip is string)
                {
                    string source = GetOrRememberSource(toolTipElement, (string)toolTipElement.ToolTip);
                    toolTipElement.ToolTip = Translate(source);
                }
            }

            foreach (DependencyObject child in visualChildren)
            {
                ApplyRecursive(child, visited);
            }

            foreach (DependencyObject child in logicalChildren)
            {
                ApplyRecursive(child, visited);
            }
        }

        private static void ApplyString(DependencyObject element, DependencyProperty property,
            string current)
        {
            string source = GetOrRememberSource(element, current);
            element.SetValue(property, Translate(source));
        }

        private static string GetOrRememberSource(DependencyObject element, string current)
        {
            string source = (string)element.GetValue(SourceTextProperty);
            if (source == null)
            {
                source = current;
                element.SetValue(SourceTextProperty, source);
            }
            return source;
        }

        private static void Add(string code, params string[] pairs)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index + 1 < pairs.Length; index += 2)
            {
                dictionary[pairs[index]] = pairs[index + 1];
            }
            Translations[code] = dictionary;
        }

        private static void AddTo(string code, params string[] pairs)
        {
            Dictionary<string, string> dictionary;
            if (!Translations.TryGetValue(code, out dictionary))
            {
                dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
                Translations[code] = dictionary;
            }

            for (int index = 0; index + 1 < pairs.Length; index += 2)
            {
                dictionary[pairs[index]] = pairs[index + 1];
            }
        }
    }
}
