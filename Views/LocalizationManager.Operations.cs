namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Operational-screen vocabulary used by Jobs, Errors, and Technician.
    /// Keeping these entries separate from the shell and STFO catalogs makes
    /// it easier to audit complete screen coverage.
    /// </summary>
    public static partial class LocalizationManager
    {
        private static void AddOperationsTranslations()
        {
            // Jobs / File Menu and dialogs.
            Op("Jobs", "Tâches", "Taken", "Aufträge", "Trabajos", "Lavori");
            Op("Jobs / File Menu", "Tâches / Menu fichiers", "Taken / Bestandsmenu", "Aufträge / Dateimenü", "Trabajos / Menú de archivos", "Lavori / Menu file");
            Op("Saved Jobs", "Tâches enregistrées", "Opgeslagen taken", "Gespeicherte Aufträge", "Trabajos guardados", "Lavori salvati");
            Op("Search jobs", "Rechercher des tâches", "Taken zoeken", "Aufträge suchen", "Buscar trabajos", "Cerca lavori");
            Op("Filter", "Filtrer", "Filteren", "Filtern", "Filtrar", "Filtra");
            Op("Status", "État", "Status", "Status", "Estado", "Stato");
            Op("Selected job is ready to open.", "La tâche sélectionnée est prête à être ouverte.", "De geselecteerde taak kan worden geopend.", "Der ausgewählte Auftrag kann geöffnet werden.", "El trabajo seleccionado está listo para abrirse.", "Il lavoro selezionato è pronto per essere aperto.");
            Op("Summary", "Résumé", "Samenvatting", "Zusammenfassung", "Resumen", "Riepilogo");
            Op("Name", "Nom", "Naam", "Name", "Nombre", "Nome");
            Op("Pages", "Pages", "Pagina's", "Seiten", "Páginas", "Pagine");
            Op(" pages", " pages", " pagina's", " Seiten", " páginas", " pagine");
            Op("Format", "Format", "Formaat", "Format", "Formato", "Formato");
            Op("Comment", "Commentaire", "Opmerking", "Kommentar", "Comentario", "Commento");
            Op("Ready for reprint", "Prêt pour réimpression", "Gereed voor herdruk", "Bereit zum Nachdruck", "Listo para reimpresión", "Pronto per la ristampa");
            Op("Barcode ID", "ID code-barres", "Barcode-ID", "Barcode-ID", "ID de código de barras", "ID codice a barre");
            Op("Last modified", "Dernière modification", "Laatst gewijzigd", "Zuletzt geändert", "Última modificación", "Ultima modifica");
            Op("Open Job", "Ouvrir la tâche", "Taak openen", "Auftrag öffnen", "Abrir trabajo", "Apri lavoro");
            Op("Save as New", "Enregistrer comme nouvelle", "Opslaan als nieuw", "Als neu speichern", "Guardar como nuevo", "Salva come nuovo");
            Op("Remove Job", "Supprimer la tâche", "Taak verwijderen", "Auftrag entfernen", "Eliminar trabajo", "Rimuovi lavoro");
            Op("View Log", "Afficher le journal", "Logboek bekijken", "Protokoll anzeigen", "Ver registro", "Visualizza registro");
            Op("Scan Barcode ID", "Scanner l’ID code-barres", "Barcode-ID scannen", "Barcode-ID scannen", "Escanear ID de código de barras", "Scansiona ID codice a barre");
            Op("Add Comment", "Ajouter un commentaire", "Opmerking toevoegen", "Kommentar hinzufügen", "Añadir comentario", "Aggiungi commento");
            Op("Add or update a comment for the selected job.", "Ajoutez ou modifiez un commentaire pour la tâche sélectionnée.", "Voeg een opmerking toe of werk deze bij voor de geselecteerde taak.", "Fügen Sie einen Kommentar für den ausgewählten Auftrag hinzu oder ändern Sie ihn.", "Añada o actualice un comentario para el trabajo seleccionado.", "Aggiungi o aggiorna un commento per il lavoro selezionato.");
            Op("Selected Job", "Tâche sélectionnée", "Geselecteerde taak", "Ausgewählter Auftrag", "Trabajo seleccionado", "Lavoro selezionato");
            Op("Maximum: 4 lines", "Maximum : 4 lignes", "Maximaal: 4 regels", "Maximal: 4 Zeilen", "Máximo: 4 líneas", "Massimo: 4 righe");
            Op("Save Comment", "Enregistrer le commentaire", "Opmerking opslaan", "Kommentar speichern", "Guardar comentario", "Salva commento");
            Op("Scan a book barcode now. Most USB barcode scanners type the ID and send Enter automatically.", "Scannez maintenant le code-barres d’un livre. La plupart des lecteurs USB saisissent l’ID et valident automatiquement.", "Scan nu de barcode van een boek. De meeste USB-scanners voeren de ID in en drukken automatisch op Enter.", "Scannen Sie jetzt den Barcode eines Buchs. Die meisten USB-Scanner geben die ID ein und senden automatisch die Eingabetaste.", "Escanee ahora el código de barras de un libro. La mayoría de los lectores USB introducen el ID y pulsan Intro automáticamente.", "Scansiona ora il codice a barre di un libro. La maggior parte degli scanner USB inserisce l’ID e preme automaticamente Invio.");
            Op("Find Job", "Rechercher la tâche", "Taak zoeken", "Auftrag suchen", "Buscar trabajo", "Trova lavoro");
            Op("Job Log", "Journal de la tâche", "Taaklogboek", "Auftragsprotokoll", "Registro del trabajo", "Registro lavoro");
            Op("Review this job's activity, then export it to any folder or removable USB drive.", "Consultez l’activité de cette tâche, puis exportez-la vers un dossier ou une clé USB.", "Bekijk de activiteit van deze taak en exporteer deze naar een map of verwisselbaar USB-station.", "Prüfen Sie die Aktivität dieses Auftrags und exportieren Sie sie in einen Ordner oder auf einen USB-Datenträger.", "Revise la actividad de este trabajo y expórtela a una carpeta o unidad USB extraíble.", "Controlla l’attività del lavoro ed esportala in una cartella o unità USB rimovibile.");
            Op("Date & Time", "Date et heure", "Datum en tijd", "Datum und Uhrzeit", "Fecha y hora", "Data e ora");
            Op("Action", "Action", "Actie", "Aktion", "Acción", "Azione");
            Op("Details", "Détails", "Details", "Details", "Detalles", "Dettagli");
            Op("Export Log...", "Exporter le journal...", "Logboek exporteren...", "Protokoll exportieren...", "Exportar registro...", "Esporta registro...");
            Op("Export Job Log", "Exporter le journal de la tâche", "Taaklogboek exporteren", "Auftragsprotokoll exportieren", "Exportar registro del trabajo", "Esporta registro lavoro");
            Op("CSV log (*.csv)", "Journal CSV (*.csv)", "CSV-logboek (*.csv)", "CSV-Protokoll (*.csv)", "Registro CSV (*.csv)", "Registro CSV (*.csv)");
            Op("Text log (*.txt)", "Journal texte (*.txt)", "Tekstlogboek (*.txt)", "Textprotokoll (*.txt)", "Registro de texto (*.txt)", "Registro di testo (*.txt)");
            Op("All files (*.*)", "Tous les fichiers (*.*)", "Alle bestanden (*.*)", "Alle Dateien (*.*)", "Todos los archivos (*.*)", "Tutti i file (*.*)");
            Op("Dimensions", "Dimensions", "Afmetingen", "Abmessungen", "Dimensiones", "Dimensioni");
            Op("Timestamp", "Horodatage", "Tijdstempel", "Zeitstempel", "Marca de tiempo", "Data e ora");
            Op("Open the selected job and load its settings.", "Ouvrez la tâche sélectionnée et chargez ses réglages.", "Open de geselecteerde taak en laad de instellingen.", "Öffnen Sie den ausgewählten Auftrag und laden Sie dessen Einstellungen.", "Abra el trabajo seleccionado y cargue sus ajustes.", "Apri il lavoro selezionato e carica le impostazioni.");
            Op("RUN ADJUSTMENTS", "RÉGLAGES DE PRODUCTION", "PRODUCTIEAANPASSINGEN", "LAUFANPASSUNGEN", "AJUSTES DE PRODUCCIÓN", "REGOLAZIONI DI PRODUZIONE");
            Op("Load saved RUN adjustments", "Charger les réglages de production enregistrés", "Opgeslagen productieaanpassingen laden", "Gespeicherte Laufanpassungen laden", "Cargar ajustes de producción guardados", "Carica regolazioni di produzione salvate");
            Op("Apply the saved RUN adjustments for this job.", "Appliquez les réglages de production enregistrés pour cette tâche.", "Pas de opgeslagen productieaanpassingen voor deze taak toe.", "Wenden Sie die gespeicherten Laufanpassungen für diesen Auftrag an.", "Aplique los ajustes de producción guardados para este trabajo.", "Applica le regolazioni di produzione salvate per questo lavoro.");
            Op("Current Setup", "Configuration actuelle", "Huidige configuratie", "Aktuelle Konfiguration", "Configuración actual", "Configurazione corrente");
            Op("Machine Line", "Ligne de machines", "Machinelijn", "Maschinenlinie", "Línea de máquinas", "Linea macchina");
            Op("Job to Remove", "Tâche à supprimer", "Te verwijderen taak", "Zu entfernender Auftrag", "Trabajo que eliminar", "Lavoro da rimuovere");
            Op("Remove the selected job from the system.", "Supprimez la tâche sélectionnée du système.", "Verwijder de geselecteerde taak uit het systeem.", "Entfernen Sie den ausgewählten Auftrag aus dem System.", "Elimine el trabajo seleccionado del sistema.", "Rimuovi il lavoro selezionato dal sistema.");
            Op("This action cannot be reverted. The job will be permanently removed.", "Cette action est irréversible. La tâche sera définitivement supprimée.", "Deze actie kan niet ongedaan worden gemaakt. De taak wordt permanent verwijderd.", "Diese Aktion kann nicht rückgängig gemacht werden. Der Auftrag wird dauerhaft entfernt.", "Esta acción no se puede deshacer. El trabajo se eliminará permanentemente.", "Questa azione non può essere annullata. Il lavoro verrà rimosso definitivamente.");
            Op("Save As New Job", "Enregistrer comme nouvelle tâche", "Opslaan als nieuwe taak", "Als neuen Auftrag speichern", "Guardar como nuevo trabajo", "Salva come nuovo lavoro");
            Op("Save the current machine setup as a new job.", "Enregistrez la configuration actuelle de la machine comme nouvelle tâche.", "Sla de huidige machineconfiguratie op als nieuwe taak.", "Speichern Sie die aktuelle Maschinenkonfiguration als neuen Auftrag.", "Guarde la configuración actual de la máquina como un nuevo trabajo.", "Salva la configurazione corrente della macchina come nuovo lavoro.");
            Op("Definition", "Définition", "Definitie", "Definition", "Definición", "Definizione");
            Op(" pages · ", " pages · ", " pagina's · ", " Seiten · ", " páginas · ", " pagine · ");
            Op("Job Name", "Nom de la tâche", "Taaknaam", "Auftragsname", "Nombre del trabajo", "Nome lavoro");
            Op("Job names must be unique.", "Les noms de tâches doivent être uniques.", "Taaknamen moeten uniek zijn.", "Auftragsnamen müssen eindeutig sein.", "Los nombres de trabajo deben ser únicos.", "I nomi dei lavori devono essere univoci.");
            Op("Preset Book Format", "Format de livre prédéfini", "Vooraf ingesteld boekformaat", "Voreingestelltes Buchformat", "Formato de libro predefinido", "Formato libro predefinito");
            Op("Book Dimensions", "Dimensions du livre", "Boekafmetingen", "Buchabmessungen", "Dimensiones del libro", "Dimensioni libro");
            Op("Width (mm)", "Largeur (mm)", "Breedte (mm)", "Breite (mm)", "Anchura (mm)", "Larghezza (mm)");
            Op("Length (mm)", "Longueur (mm)", "Lengte (mm)", "Länge (mm)", "Longitud (mm)", "Lunghezza (mm)");
            Op("New Name", "Nouveau nom", "Nieuwe naam", "Neuer Name", "Nombre nuevo", "Nuovo nome");
            Op("Save Job", "Enregistrer la tâche", "Taak opslaan", "Auftrag speichern", "Guardar trabajo", "Salva lavoro");
            Op("This will overwrite the existing job and cannot be undone.", "La tâche existante sera remplacée. Cette action est irréversible.", "Hiermee wordt de bestaande taak overschreven. Dit kan niet ongedaan worden gemaakt.", "Der bestehende Auftrag wird überschrieben. Dies kann nicht rückgängig gemacht werden.", "Esto sobrescribirá el trabajo existente y no se puede deshacer.", "Il lavoro esistente verrà sovrascritto e l’azione non può essere annullata.");
            Op("Custom", "Personnalisé", "Aangepast", "Benutzerdefiniert", "Personalizado", "Personalizzato");
            Op("Variant", "Variante", "Variant", "Variante", "Variante", "Variante");
            Op("New Job", "Nouvelle tâche", "Nieuwe taak", "Neuer Auftrag", "Nuevo trabajo", "Nuovo lavoro");
            Op("No job selected.", "Aucune tâche sélectionnée.", "Geen taak geselecteerd.", "Kein Auftrag ausgewählt.", "No hay ningún trabajo seleccionado.", "Nessun lavoro selezionato.");
            Op("Last action: {0} (stub - not yet connected to job storage)", "Dernière action : {0} (prototype non connecté au stockage)", "Laatste actie: {0} (prototype, nog niet gekoppeld aan opslag)", "Letzte Aktion: {0} (Prototyp, noch nicht mit dem Speicher verbunden)", "Última acción: {0} (prototipo aún no conectado al almacenamiento)", "Ultima azione: {0} (prototipo non ancora collegato all’archivio)");
            Op("No saved job uses barcode ID “{0}”. Check the book and scan again.", "Aucune tâche enregistrée n’utilise l’ID code-barres « {0} ». Vérifiez le livre et recommencez.", "Geen opgeslagen taak gebruikt barcode-ID ‘{0}’. Controleer het boek en scan opnieuw.", "Kein gespeicherter Auftrag verwendet die Barcode-ID „{0}“. Prüfen Sie das Buch und scannen Sie erneut.", "Ningún trabajo guardado usa el ID de código de barras «{0}». Compruebe el libro y vuelva a escanear.", "Nessun lavoro salvato usa l’ID codice a barre “{0}”. Controlla il libro e ripeti la scansione.");
            Op("Barcode matched “{0}”. It is ready to open.", "Le code-barres correspond à « {0} ». La tâche est prête à être ouverte.", "Barcode komt overeen met ‘{0}’. De taak kan worden geopend.", "Der Barcode entspricht „{0}“. Der Auftrag kann geöffnet werden.", "El código de barras coincide con «{0}». El trabajo está listo para abrirse.", "Il codice a barre corrisponde a “{0}”. Il lavoro è pronto per essere aperto.");
            Op("Barcode {0} matched job “{1}”.", "Le code-barres {0} correspond à la tâche « {1} ».", "Barcode {0} komt overeen met taak ‘{1}’.", "Barcode {0} entspricht Auftrag „{1}“.", "El código de barras {0} coincide con el trabajo «{1}».", "Il codice a barre {0} corrisponde al lavoro “{1}”.");
            Op("Job log exported to {0}", "Journal de la tâche exporté vers {0}", "Taaklogboek geëxporteerd naar {0}", "Auftragsprotokoll nach {0} exportiert", "Registro del trabajo exportado a {0}", "Registro lavoro esportato in {0}");
            Op("Loaded job “{0}” with saved RUN adjustments.", "Tâche « {0} » chargée avec les réglages de production enregistrés.", "Taak ‘{0}’ geladen met opgeslagen productieaanpassingen.", "Auftrag „{0}“ mit gespeicherten Laufanpassungen geladen.", "Trabajo «{0}» cargado con los ajustes de producción guardados.", "Lavoro “{0}” caricato con le regolazioni di produzione salvate.");
            Op("Loaded job “{0}”.", "Tâche « {0} » chargée.", "Taak ‘{0}’ geladen.", "Auftrag „{0}“ geladen.", "Trabajo «{0}» cargado.", "Lavoro “{0}” caricato.");
            Op("Comment Saved!", "Commentaire enregistré !", "Opmerking opgeslagen!", "Kommentar gespeichert!", "¡Comentario guardado!", "Commento salvato!");
            Op("The comment for “{0}” has been successfully saved.", "Le commentaire de « {0} » a été enregistré.", "De opmerking voor ‘{0}’ is opgeslagen.", "Der Kommentar für „{0}“ wurde gespeichert.", "El comentario de «{0}» se ha guardado.", "Il commento per “{0}” è stato salvato.");
            Op("New Job Saved!", "Nouvelle tâche enregistrée !", "Nieuwe taak opgeslagen!", "Neuer Auftrag gespeichert!", "¡Nuevo trabajo guardado!", "Nuovo lavoro salvato!");
            Op("The new job “{0}” was saved as {1} with {2} pages.", "La nouvelle tâche « {0} » a été enregistrée au format {1} avec {2} pages.", "De nieuwe taak ‘{0}’ is opgeslagen als {1} met {2} pagina's.", "Der neue Auftrag „{0}“ wurde als {1} mit {2} Seiten gespeichert.", "El nuevo trabajo «{0}» se guardó como {1} con {2} páginas.", "Il nuovo lavoro “{0}” è stato salvato come {1} con {2} pagine.");
            Op("Job Removed!", "Tâche supprimée !", "Taak verwijderd!", "Auftrag entfernt!", "¡Trabajo eliminado!", "Lavoro rimosso!");
            Op("The job “{0}” has been successfully removed.", "La tâche « {0} » a été supprimée.", "De taak ‘{0}’ is verwijderd.", "Der Auftrag „{0}“ wurde entfernt.", "El trabajo «{0}» se ha eliminado.", "Il lavoro “{0}” è stato rimosso.");
            Op("Scan or enter a barcode ID first.", "Scannez ou saisissez d’abord un ID code-barres.", "Scan of voer eerst een barcode-ID in.", "Scannen Sie zuerst eine Barcode-ID oder geben Sie sie ein.", "Escanee o introduzca primero un ID de código de barras.", "Scansiona o inserisci prima un ID codice a barre.");
            Op("Log saved to {0}", "Journal enregistré dans {0}", "Logboek opgeslagen in {0}", "Protokoll gespeichert unter {0}", "Registro guardado en {0}", "Registro salvato in {0}");
            Op("The log could not be saved: {0}", "Le journal n’a pas pu être enregistré : {0}", "Het logboek kon niet worden opgeslagen: {0}", "Das Protokoll konnte nicht gespeichert werden: {0}", "No se pudo guardar el registro: {0}", "Impossibile salvare il registro: {0}");
            Op("Job saved", "Tâche enregistrée", "Taak opgeslagen", "Auftrag gespeichert", "Trabajo guardado", "Lavoro salvato");
            Op("Barcode scanned", "Code-barres scanné", "Barcode gescand", "Barcode gescannt", "Código de barras escaneado", "Codice a barre scansionato");
            Op("Comment updated", "Commentaire mis à jour", "Opmerking bijgewerkt", "Kommentar aktualisiert", "Comentario actualizado", "Commento aggiornato");
            Op("Job loaded", "Tâche chargée", "Taak geladen", "Auftrag geladen", "Trabajo cargado", "Lavoro caricato");
            Op("Loaded as the current production job.", "Chargée comme tâche de production actuelle.", "Geladen als huidige productietaak.", "Als aktueller Produktionsauftrag geladen.", "Cargado como trabajo de producción actual.", "Caricato come lavoro di produzione corrente.");
            Op("{0}, {1} pages", "{0}, {1} pages", "{0}, {1} pagina's", "{0}, {1} Seiten", "{0}, {1} páginas", "{0}, {1} pagine");
            Op("Barcode {0} matched this saved job.", "Le code-barres {0} correspond à cette tâche enregistrée.", "Barcode {0} komt overeen met deze opgeslagen taak.", "Barcode {0} entspricht diesem gespeicherten Auftrag.", "El código de barras {0} coincide con este trabajo guardado.", "Il codice a barre {0} corrisponde a questo lavoro salvato.");
            Op("OK", "OK", "OK", "OK", "Aceptar", "OK");
            Op("Job Name Already Exists!", "Ce nom de tâche existe déjà !", "Taaknaam bestaat al!", "Auftragsname ist bereits vorhanden!", "¡El nombre del trabajo ya existe!", "Il nome del lavoro esiste già!");
            Op("A job with the entered name already exists. Do you want to replace the existing job with the current setup?", "Une tâche portant ce nom existe déjà. Voulez-vous la remplacer par la configuration actuelle ?", "Er bestaat al een taak met deze naam. Wilt u deze vervangen door de huidige configuratie?", "Ein Auftrag mit diesem Namen ist bereits vorhanden. Möchten Sie ihn durch die aktuelle Konfiguration ersetzen?", "Ya existe un trabajo con el nombre introducido. ¿Desea reemplazarlo por la configuración actual?", "Esiste già un lavoro con il nome inserito. Vuoi sostituirlo con la configurazione corrente?");
            Op("Set Number of Pages", "Définir le nombre de pages", "Aantal pagina's instellen", "Seitenzahl festlegen", "Establecer número de páginas", "Imposta numero di pagine");
            Op("Enter the total number of pages in the new job.", "Saisissez le nombre total de pages de la nouvelle tâche.", "Voer het totale aantal pagina's van de nieuwe taak in.", "Geben Sie die Gesamtseitenzahl des neuen Auftrags ein.", "Introduzca el número total de páginas del nuevo trabajo.", "Inserisci il numero totale di pagine del nuovo lavoro.");
            Op("Enter a whole number from 1 to 2,000.", "Saisissez un nombre entier compris entre 1 et 2 000.", "Voer een geheel getal van 1 tot 2.000 in.", "Geben Sie eine ganze Zahl zwischen 1 und 2.000 ein.", "Introduzca un número entero entre 1 y 2.000.", "Inserisci un numero intero da 1 a 2.000.");
            Op("Set Book Width", "Définir la largeur du livre", "Boekbreedte instellen", "Buchbreite festlegen", "Establecer anchura del libro", "Imposta larghezza libro");
            Op("Set Book Length", "Définir la longueur du livre", "Boeklengte instellen", "Buchlänge festlegen", "Establecer longitud del libro", "Imposta lunghezza libro");
            Op("Width ({0})", "Largeur ({0})", "Breedte ({0})", "Breite ({0})", "Anchura ({0})", "Larghezza ({0})");
            Op("Length ({0})", "Longueur ({0})", "Lengte ({0})", "Länge ({0})", "Longitud ({0})", "Lunghezza ({0})");
            Op("Enter the physical book format dimension in inches.", "Saisissez la dimension physique du livre en pouces.", "Voer de fysieke boekafmeting in inches in.", "Geben Sie die physische Buchabmessung in Zoll ein.", "Introduzca la dimensión física del libro en pulgadas.", "Inserisci la dimensione fisica del libro in pollici.");
            Op("Enter the physical book format dimension in millimetres.", "Saisissez la dimension physique du livre en millimètres.", "Voer de fysieke boekafmeting in millimeters in.", "Geben Sie die physische Buchabmessung in Millimetern ein.", "Introduzca la dimensión física del libro en milímetros.", "Inserisci la dimensione fisica del libro in millimetri.");
            Op("Dimensions do not match a standard preset; this job will be displayed as Custom.", "Les dimensions ne correspondent à aucun format standard ; cette tâche sera affichée comme Personnalisé.", "De afmetingen komen niet overeen met een standaardformaat; deze taak wordt als Aangepast weergegeven.", "Die Abmessungen entsprechen keinem Standardformat; dieser Auftrag wird als Benutzerdefiniert angezeigt.", "Las dimensiones no coinciden con un formato estándar; este trabajo se mostrará como Personalizado.", "Le dimensioni non corrispondono a un formato standard; il lavoro verrà visualizzato come Personalizzato.");
            Op("Dimensions match the {0} preset.", "Les dimensions correspondent au format prédéfini {0}.", "De afmetingen komen overeen met het vooraf ingestelde formaat {0}.", "Die Abmessungen entsprechen der Voreinstellung {0}.", "Las dimensiones coinciden con el formato predefinido {0}.", "Le dimensioni corrispondono al formato predefinito {0}.");
            Op("Enter a unique job name.", "Saisissez un nom de tâche unique.", "Voer een unieke taaknaam in.", "Geben Sie einen eindeutigen Auftragsnamen ein.", "Introduzca un nombre de trabajo único.", "Inserisci un nome lavoro univoco.");
            Op("Pages and both dimensions must be greater than zero.", "Le nombre de pages et les deux dimensions doivent être supérieurs à zéro.", "Het aantal pagina's en beide afmetingen moeten groter zijn dan nul.", "Seitenzahl und beide Abmessungen müssen größer als null sein.", "Las páginas y ambas dimensiones deben ser mayores que cero.", "Il numero di pagine e le due dimensioni devono essere maggiori di zero.");

            // Errors & Information.
            Op("Errors", "Erreurs", "Fouten", "Fehler", "Errores", "Errori");
            Op("Active Messages", "Messages actifs", "Actieve berichten", "Aktive Meldungen", "Mensajes activos", "Messaggi attivi");
            Op("Critical", "Critique", "Kritiek", "Kritisch", "Crítico", "Critico");
            Op("Warning", "Avertissement", "Waarschuwing", "Warnung", "Advertencia", "Avviso");
            Op("Info", "Info", "Info", "Info", "Información", "Info");
            Op("Resolved", "Résolu", "Opgelost", "Behoben", "Resuelto", "Risolto");
            Op("Severity", "Gravité", "Ernst", "Schweregrad", "Gravedad", "Gravità");
            Op("Source", "Source", "Bron", "Quelle", "Origen", "Origine");
            Op("Module/Job", "Module/Tâche", "Module/Taak", "Modul/Auftrag", "Módulo/Trabajo", "Modulo/Lavoro");
            Op("Time", "Heure", "Tijd", "Uhrzeit", "Hora", "Ora");
            Op("No active messages", "Aucun message actif", "Geen actieve berichten", "Keine aktiven Meldungen", "No hay mensajes activos", "Nessun messaggio attivo");
            Op("All clear! No messages at this time.", "Tout est en ordre ! Aucun message actuellement.", "Alles in orde! Er zijn momenteel geen berichten.", "Alles in Ordnung! Derzeit liegen keine Meldungen vor.", "¡Todo correcto! No hay mensajes en este momento.", "Tutto regolare! Nessun messaggio al momento.");
            Op("Clear", "Effacer", "Wissen", "Löschen", "Borrar", "Cancella");
            Op("Error: {0}", "Erreur : {0}", "Fout: {0}", "Fehler: {0}", "Error: {0}", "Errore: {0}");
            Op("Related Module", "Module associé", "Gerelateerde module", "Zugehöriges Modul", "Módulo relacionado", "Modulo correlato");
            Op("Related Job", "Tâche associée", "Gerelateerde taak", "Zugehöriger Auftrag", "Trabajo relacionado", "Lavoro correlato");
            Op("Description", "Description", "Beschrijving", "Beschreibung", "Descripción", "Descrizione");
            Op("Machine", "Machine", "Machine", "Maschine", "Máquina", "Macchina");
            Op("None", "Aucun", "Geen", "Keine", "Ninguno", "Nessuno");
            Op("Cover open on Module 3", "Capot ouvert sur le module 3", "Kap open op module 3", "Abdeckung an Modul 3 geöffnet", "Cubierta abierta en el módulo 3", "Copertura aperta sul modulo 3");
            Op("Cover open on Module 4", "Capot ouvert sur le module 4", "Kap open op module 4", "Abdeckung an Modul 4 geöffnet", "Cubierta abierta en el módulo 4", "Copertura aperta sul modulo 4");
            Op("Cover open on Module 6", "Capot ouvert sur le module 6", "Kap open op module 6", "Abdeckung an Modul 6 geöffnet", "Cubierta abierta en el módulo 6", "Copertura aperta sul modulo 6");
            Op("The access cover on BPM Module 3 is open. Verify all covers are fully closed.", "Le capot d’accès du module BPM 3 est ouvert. Vérifiez que tous les capots sont complètement fermés.", "De toegangskap van BPM-module 3 is open. Controleer of alle kappen volledig gesloten zijn.", "Die Zugangsabdeckung an BPM-Modul 3 ist geöffnet. Prüfen Sie, ob alle Abdeckungen vollständig geschlossen sind.", "La cubierta de acceso del módulo BPM 3 está abierta. Compruebe que todas las cubiertas estén completamente cerradas.", "La copertura di accesso del modulo BPM 3 è aperta. Verificare che tutte le coperture siano completamente chiuse.");
            Op("The access cover on BPM Module 4 is open. Verify all covers are fully closed.", "Le capot d’accès du module BPM 4 est ouvert. Vérifiez que tous les capots sont complètement fermés.", "De toegangskap van BPM-module 4 is open. Controleer of alle kappen volledig gesloten zijn.", "Die Zugangsabdeckung an BPM-Modul 4 ist geöffnet. Prüfen Sie, ob alle Abdeckungen vollständig geschlossen sind.", "La cubierta de acceso del módulo BPM 4 está abierta. Compruebe que todas las cubiertas estén completamente cerradas.", "La copertura di accesso del modulo BPM 4 è aperta. Verificare che tutte le coperture siano completamente chiuse.");
            Op("The access cover on BPM Module 6 is open. Verify all covers are fully closed.", "Le capot d’accès du module BPM 6 est ouvert. Vérifiez que tous les capots sont complètement fermés.", "De toegangskap van BPM-module 6 is open. Controleer of alle kappen volledig gesloten zijn.", "Die Zugangsabdeckung an BPM-Modul 6 ist geöffnet. Prüfen Sie, ob alle Abdeckungen vollständig geschlossen sind.", "La cubierta de acceso del módulo BPM 6 está abierta. Compruebe que todas las cubiertas estén completamente cerradas.", "La copertura di accesso del modulo BPM 6 è aperta. Verificare che tutte le coperture siano completamente chiuse.");
            Op("Machine needs setup", "La machine doit être configurée", "Machine moet worden ingesteld", "Maschine muss eingerichtet werden", "La máquina necesita configuración", "La macchina richiede configurazione");
            Op("The machine requires initial setup before it can run a job.", "La machine nécessite une configuration initiale avant de pouvoir exécuter une tâche.", "De machine moet eerst worden ingesteld voordat een taak kan worden uitgevoerd.", "Die Maschine muss zunächst eingerichtet werden, bevor ein Auftrag ausgeführt werden kann.", "La máquina requiere una configuración inicial antes de poder ejecutar un trabajo.", "La macchina richiede una configurazione iniziale prima di poter eseguire un lavoro.");

            // Machine Line Configuration and Add Module workflow.
            Op("Machine Line", "Ligne de machines", "Machinelijn", "Maschinenlinie", "Línea de máquinas", "Linea macchina");
            Op("Previous module", "Module précédent", "Vorige module", "Vorheriges Modul", "Módulo anterior", "Modulo precedente");
            Op("Next module", "Module suivant", "Volgende module", "Nächstes Modul", "Módulo siguiente", "Modulo successivo");
            Op("No modules configured.", "Aucun module configuré.", "Geen modules geconfigureerd.", "Keine Module konfiguriert.", "No hay módulos configurados.", "Nessun modulo configurato.");
            Op("Add Module", "Ajouter un module", "Module toevoegen", "Modul hinzufügen", "Añadir módulo", "Aggiungi modulo");
            Op("Add a module", "Ajouter un module", "Een module toevoegen", "Ein Modul hinzufügen", "Añadir un módulo", "Aggiungi un modulo");
            Op("Selected Module", "Module sélectionné", "Geselecteerde module", "Ausgewähltes Modul", "Módulo seleccionado", "Modulo selezionato");
            Op("No module selected.", "Aucun module sélectionné.", "Geen module geselecteerd.", "Kein Modul ausgewählt.", "No hay ningún módulo seleccionado.", "Nessun modulo selezionato.");
            Op("Position", "Position", "Positie", "Position", "Posición", "Posizione");
            Op("Speed", "Vitesse", "Snelheid", "Geschwindigkeit", "Velocidad", "Velocità");
            Op("Line Actions", "Actions de la ligne", "Lijnacties", "Linienaktionen", "Acciones de la línea", "Azioni linea");
            Op("Remove Module", "Supprimer le module", "Module verwijderen", "Modul entfernen", "Eliminar módulo", "Rimuovi modulo");
            Op("Replace Module", "Remplacer le module", "Module vervangen", "Modul ersetzen", "Sustituir módulo", "Sostituisci modulo");
            Op("Review & Confirm", "Vérifier et confirmer", "Controleren en bevestigen", "Prüfen und bestätigen", "Revisar y confirmar", "Controlla e conferma");
            Op("Configuration Status", "État de la configuration", "Configuratiestatus", "Konfigurationsstatus", "Estado de configuración", "Stato configurazione");
            Op("No status.", "Aucun état.", "Geen status.", "Kein Status.", "Sin estado.", "Nessuno stato.");
            Op("Registered", "Enregistré", "Geregistreerd", "Registriert", "Registrado", "Registrato");
            Op("Not Registered", "Non enregistré", "Niet geregistreerd", "Nicht registriert", "No registrado", "Non registrato");
            Op("{0} of {1}", "{0} sur {1}", "{0} van {1}", "{0} von {1}", "{0} de {1}", "{0} di {1}");
            Op("Unsaved changes — review and confirm.", "Modifications non enregistrées — vérifiez et confirmez.", "Niet-opgeslagen wijzigingen — controleren en bevestigen.", "Nicht gespeicherte Änderungen — prüfen und bestätigen.", "Cambios sin guardar — revise y confirme.", "Modifiche non salvate — controlla e conferma.");
            Op("Configuration confirmed.", "Configuration confirmée.", "Configuratie bevestigd.", "Konfiguration bestätigt.", "Configuración confirmada.", "Configurazione confermata.");
            Op("All available module types are already on the line.", "Tous les types de modules disponibles sont déjà sur la ligne.", "Alle beschikbare moduletypen staan al op de lijn.", "Alle verfügbaren Modultypen befinden sich bereits in der Linie.", "Todos los tipos de módulos disponibles ya están en la línea.", "Tutti i tipi di modulo disponibili sono già sulla linea.");
            Op("{0} is already on the line and cannot be added again.", "{0} est déjà sur la ligne et ne peut pas être ajouté à nouveau.", "{0} staat al op de lijn en kan niet opnieuw worden toegevoegd.", "{0} befindet sich bereits in der Linie und kann nicht erneut hinzugefügt werden.", "{0} ya está en la línea y no se puede añadir de nuevo.", "{0} è già sulla linea e non può essere aggiunto di nuovo.");
            Op("Pending: Added {0} ({1}). Select Review & Confirm when finished.", "En attente : {0} ajouté ({1}). Sélectionnez Vérifier et confirmer lorsque vous avez terminé.", "In behandeling: {0} toegevoegd ({1}). Selecteer Controleren en bevestigen wanneer u klaar bent.", "Ausstehend: {0} hinzugefügt ({1}). Wählen Sie anschließend Prüfen und bestätigen.", "Pendiente: se añadió {0} ({1}). Seleccione Revisar y confirmar cuando termine.", "In sospeso: aggiunto {0} ({1}). Al termine seleziona Controlla e conferma.");
            Op("Pending: Module removed. Select Review & Confirm when finished.", "En attente : module supprimé. Sélectionnez Vérifier et confirmer lorsque vous avez terminé.", "In behandeling: module verwijderd. Selecteer Controleren en bevestigen wanneer u klaar bent.", "Ausstehend: Modul entfernt. Wählen Sie anschließend Prüfen und bestätigen.", "Pendiente: módulo eliminado. Seleccione Revisar y confirmar cuando termine.", "In sospeso: modulo rimosso. Al termine seleziona Controlla e conferma.");
            Op("No unused module type is available for replacement.", "Aucun type de module inutilisé n’est disponible pour le remplacement.", "Er is geen ongebruikt moduletype beschikbaar als vervanging.", "Kein unbenutzter Modultyp ist als Ersatz verfügbar.", "No hay ningún tipo de módulo sin usar disponible para sustituirlo.", "Nessun tipo di modulo inutilizzato è disponibile per la sostituzione.");
            Op("Pending: Replaced {0} with {1}. Select Review & Confirm when finished.", "En attente : {0} remplacé par {1}. Sélectionnez Vérifier et confirmer lorsque vous avez terminé.", "In behandeling: {0} vervangen door {1}. Selecteer Controleren en bevestigen wanneer u klaar bent.", "Ausstehend: {0} durch {1} ersetzt. Wählen Sie anschließend Prüfen und bestätigen.", "Pendiente: se sustituyó {0} por {1}. Seleccione Revisar y confirmar cuando termine.", "In sospeso: {0} sostituito con {1}. Al termine seleziona Controlla e conferma.");
            Op("Confirm Machine Line", "Confirmer la ligne de machines", "Machinelijn bevestigen", "Maschinenlinie bestätigen", "Confirmar línea de máquinas", "Conferma linea macchina");
            Op("Review complete. Enter the technician PIN once to apply {0} and publish the configuration.", "Vérification terminée. Saisissez une fois le code PIN technicien pour appliquer {0} et publier la configuration.", "Controle voltooid. Voer eenmaal de technicus-PIN in om {0} toe te passen en de configuratie te publiceren.", "Prüfung abgeschlossen. Geben Sie einmal die Techniker-PIN ein, um {0} anzuwenden und die Konfiguration zu veröffentlichen.", "Revisión completa. Introduzca una vez el PIN técnico para aplicar {0} y publicar la configuración.", "Controllo completato. Inserisci una volta il PIN tecnico per applicare {0} e pubblicare la configurazione.");
            Op("an empty machine line", "une ligne de machines vide", "een lege machinelijn", "eine leere Maschinenlinie", "una línea de máquinas vacía", "una linea macchina vuota");
            Op("{0} module", "{0} module", "{0} module", "{0} Modul", "{0} módulo", "{0} modulo");
            Op("{0} modules", "{0} modules", "{0} modules", "{0} Module", "{0} módulos", "{0} moduli");
            Op("Machine line configuration confirmed and applied.", "Configuration de la ligne confirmée et appliquée.", "Machinelijnconfiguratie bevestigd en toegepast.", "Maschinenlinienkonfiguration bestätigt und angewendet.", "Configuración de la línea confirmada y aplicada.", "Configurazione della linea confermata e applicata.");
            Op("Add a machine module to the line.", "Ajoutez un module machine à la ligne.", "Voeg een machinemodule toe aan de lijn.", "Fügen Sie der Linie ein Maschinenmodul hinzu.", "Añada un módulo de máquina a la línea.", "Aggiungi un modulo macchina alla linea.");
            Op("Available Modules:", "Modules disponibles :", "Beschikbare modules:", "Verfügbare Module:", "Módulos disponibles:", "Moduli disponibili:");
            Op("Module Selected: ", "Module sélectionné : ", "Geselecteerde module: ", "Ausgewähltes Modul: ", "Módulo seleccionado: ", "Modulo selezionato: ");
            Op("Select Position:", "Sélectionner la position :", "Positie selecteren:", "Position auswählen:", "Seleccionar posición:", "Seleziona posizione:");
            Op("Review Module:", "Vérifier le module :", "Module controleren:", "Modul prüfen:", "Revisar módulo:", "Controlla modulo:");
            Op("Selected Module:", "Module sélectionné :", "Geselecteerde module:", "Ausgewähltes Modul:", "Módulo seleccionado:", "Modulo selezionato:");
            Op("Position:", "Position :", "Positie:", "Position:", "Posición:", "Posizione:");
            Op("This module will remain pending until the complete machine line is reviewed and authorized.", "Ce module restera en attente jusqu’à la vérification et l’autorisation de toute la ligne.", "Deze module blijft in behandeling totdat de volledige machinelijn is gecontroleerd en geautoriseerd.", "Dieses Modul bleibt ausstehend, bis die gesamte Maschinenlinie geprüft und autorisiert wurde.", "Este módulo permanecerá pendiente hasta que se revise y autorice toda la línea.", "Questo modulo rimarrà in sospeso finché l’intera linea non sarà controllata e autorizzata.");
            Op("Continue", "Continuer", "Doorgaan", "Weiter", "Continuar", "Continua");
            Op("Before {0}", "Avant {0}", "Vóór {0}", "Vor {0}", "Antes de {0}", "Prima di {0}");
            Op("After {0}", "Après {0}", "Na {0}", "Nach {0}", "Después de {0}", "Dopo {0}");
            Op("Start of line", "Début de la ligne", "Begin van de lijn", "Linienanfang", "Inicio de la línea", "Inizio della linea");
            Op("Feeder", "Margeur", "Invoer", "Anleger", "Alimentador", "Alimentatore");
            Op("Booklet Maker", "Brocheuse", "Boekjesmaker", "Broschürenfertiger", "Creador de folletos", "Fascicolatore");
            Op("Stacker", "Empileur", "Stapelaar", "Stapler", "Apilador", "Impilatore");
            Op("Trimmer", "Massicot", "Snijder", "Schneider", "Cortadora", "Rifilatore");

            // Technician Interface.
            Op("Technician Mode", "Mode technicien", "Technicusmodus", "Technikermodus", "Modo técnico", "Modalità tecnico");
            Op("Status:", "État :", "Status:", "Status:", "Estado:", "Stato:");
            Op("Machine:", "Machine :", "Machine:", "Maschine:", "Máquina:", "Macchina:");
            Op("Access:", "Accès :", "Toegang:", "Zugriff:", "Acceso:", "Accesso:");
            Op("Protected", "Protégé", "Beveiligd", "Geschützt", "Protegido", "Protetto");
            Op("Granted", "Autorisé", "Toegestaan", "Gewährt", "Concedido", "Consentito");
            Op("1.  Calibration Set Option", "1.  Option d’étalonnage des lots", "1.  Kalibratieoptie voor sets", "1.  Kalibrieroption für Sätze", "1.  Opción de calibración de conjuntos", "1.  Opzione calibrazione set");
            Op("2.  Save Adjustments", "2.  Enregistrer les réglages", "2.  Aanpassingen opslaan", "2.  Anpassungen speichern", "2.  Guardar ajustes", "2.  Salva regolazioni");
            Op("3.  Stitch Head Form", "3.  Forme de la tête de piqûre", "3.  Vorm van de hechtkop", "3.  Form des Heftkopfs", "3.  Forma del cabezal de grapado", "3.  Forma testa di cucitura");
            Op("4.  Stitch Detection Control", "4.  Contrôle de détection des piqûres", "4.  Hechtdetectie", "4.  Heftklammererkennung", "4.  Control de detección de grapado", "4.  Controllo rilevamento cucitura");
            Op("5.  Stitch Single Sheet", "5.  Piqûre d’une feuille unique", "5.  Eén vel hechten", "5.  Einzelblatt heften", "5.  Grapar una sola hoja", "5.  Cucitura foglio singolo");
            Op("6.  Purge Option", "6.  Option de purge", "6.  Leegoptie", "6.  Entleeroption", "6.  Opción de purga", "6.  Opzione spurgo");
            Op("7.  Sheet in Compiler Restart", "7.  Feuille présente au redémarrage", "7.  Vel bij herstart verzamelaar", "7.  Blatt bei Sammler-Neustart", "7.  Hoja al reiniciar compilador", "7.  Foglio al riavvio fascicolatore");
            Op("Always Process", "Toujours traiter", "Altijd verwerken", "Immer verarbeiten", "Procesar siempre", "Elabora sempre");
            Op("Always Reject", "Toujours rejeter", "Altijd weigeren", "Immer auswerfen", "Rechazar siempre", "Rifiuta sempre");
            Op("Normal Stitch", "Piqûre normale", "Normale hechting", "Normale Heftung", "Grapado normal", "Cucitura normale");
            Op("Loop Stitch", "Piqûre à boucle", "Lushechting", "Ringösenheftung", "Grapado en bucle", "Cucitura ad anello");
            Op("Ask Operator", "Demander à l’opérateur", "Operator vragen", "Bediener fragen", "Preguntar al operador", "Chiedi all’operatore");
            Op("Always Purge", "Toujours purger", "Altijd legen", "Immer entleeren", "Purgar siempre", "Spurga sempre");
            Op("Disable Head 1", "Désactiver la tête 1", "Kop 1 uitschakelen", "Kopf 1 deaktivieren", "Desactivar cabezal 1", "Disabilita testa 1");
            Op("Disable Head 2", "Désactiver la tête 2", "Kop 2 uitschakelen", "Kopf 2 deaktivieren", "Desactivar cabezal 2", "Disabilita testa 2");
            Op("Allowed", "Autorisé", "Toegestaan", "Zulässig", "Permitido", "Consentito");
            Op("Forbidden", "Interdit", "Verboden", "Verboten", "Prohibido", "Vietato");
            Op("Current Speed", "Vitesse actuelle", "Huidige snelheid", "Aktuelle Geschwindigkeit", "Velocidad actual", "Velocità attuale");
            Op("Technician Actions", "Actions technicien", "Technicusacties", "Technikeraktionen", "Acciones técnicas", "Azioni tecnico");
            Op("Reset BBM", "Réinitialiser le BBM", "BBM resetten", "BBM zurücksetzen", "Restablecer BBM", "Reimposta BBM");
            Op("Return machine to home position", "Ramener la machine en position initiale", "Machine naar uitgangspositie terugbrengen", "Maschine in Grundstellung fahren", "Devolver la máquina a la posición inicial", "Riporta la macchina alla posizione iniziale");
            Op("Stitch Pulse", "Impulsion de piqûre", "Hechtpuls", "Heftimpuls", "Impulso de grapado", "Impulso cucitura");
            Op("Run 1 stitch head cycle", "Exécuter 1 cycle de tête de piqûre", "Voer 1 hechtkopcyclus uit", "1 Heftkopfzyklus ausführen", "Ejecutar 1 ciclo del cabezal de grapado", "Esegui 1 ciclo della testa di cucitura");
            Op("Shipping Position", "Position de transport", "Transportpositie", "Transportposition", "Posición de transporte", "Posizione di trasporto");
            Op("Prepare machine for transport", "Préparer la machine pour le transport", "Machine voorbereiden op transport", "Maschine für den Transport vorbereiten", "Preparar la máquina para el transporte", "Prepara la macchina per il trasporto");
            Op("Technical Access", "Accès technique", "Technische toegang", "Technischer Zugriff", "Acceso técnico", "Accesso tecnico");
            Op("Password protected — skilled personnel only", "Protégé par mot de passe — personnel qualifié uniquement", "Beveiligd met wachtwoord — alleen bevoegd personeel", "Passwortgeschützt — nur für Fachpersonal", "Protegido con contraseña — solo personal cualificado", "Protetto da password — solo personale qualificato");
            Op("Enter your technician code to unlock protected actions.", "Saisissez votre code technicien pour déverrouiller les actions protégées.", "Voer uw technicuscode in om beveiligde acties te ontgrendelen.", "Geben Sie Ihren Technikercode ein, um geschützte Aktionen freizuschalten.", "Introduzca su código técnico para desbloquear las acciones protegidas.", "Inserisci il codice tecnico per sbloccare le azioni protette.");
            Op("Enter Technician Code:", "Saisir le code technicien :", "Technicuscode invoeren:", "Technikercode eingeben:", "Introducir código técnico:", "Inserisci codice tecnico:");
            Op("Unlock", "Déverrouiller", "Ontgrendelen", "Entsperren", "Desbloquear", "Sblocca");
            Op("Technician settings saved", "Réglages technicien enregistrés", "Technicusinstellingen opgeslagen", "Technikereinstellungen gespeichert", "Ajustes técnicos guardados", "Impostazioni tecnico salvate");
            Op("Defaults restored — select Save to keep them", "Valeurs par défaut restaurées — sélectionnez Enregistrer pour les conserver", "Standaardwaarden hersteld — selecteer Opslaan om ze te behouden", "Standardwerte wiederhergestellt — zum Beibehalten Speichern wählen", "Valores predeterminados restaurados — seleccione Guardar para conservarlos", "Valori predefiniti ripristinati — seleziona Salva per mantenerli");
            Op("{0} command prepared (prototype only)", "Commande {0} préparée (prototype uniquement)", "Opdracht {0} voorbereid (alleen prototype)", "Befehl {0} vorbereitet (nur Prototyp)", "Comando {0} preparado (solo prototipo)", "Comando {0} preparato (solo prototipo)");
            Op("Technical access granted", "Accès technique autorisé", "Technische toegang verleend", "Technischer Zugriff gewährt", "Acceso técnico concedido", "Accesso tecnico consentito");
        }

        private static void Op(string source, string fr, string nl, string de, string es, string it)
        {
            AddTo("fr", source, fr);
            AddTo("nl", source, nl);
            AddTo("de", source, de);
            AddTo("es", source, es);
            AddTo("it", source, it);
        }
    }
}
