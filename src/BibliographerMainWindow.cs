//
//  BibliographerMainWindow.cs
//
//  Author:
//       Sameer Morar <smorar@gmail.com>
//       Carl Hultquist <chultquist@gmail.com>
//
//  Copyright (c) 2005-2015 Bibliographer developers
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//


using System;
using System.Collections;
using Gtk;
using libbibby;

namespace bibliographer
{

    public class BibliographerMainWindow
    {
        protected Window window;
        protected Viewport viewportRequired;
        protected Viewport viewportOptional;
        protected Viewport viewportOther;
        protected Viewport viewportBibliographerData;
        protected Paned scrolledwindowSidePane;
        protected Box recordDetailsView;

        protected ScrolledWindow scrolledwindowSidePaneScrolledWindow;

        protected Statusbar bibliographerStatusBar;

        protected CheckMenuItem MenuViewSidebar;
        protected CheckMenuItem MenuViewRecordDetails;
        protected CheckMenuItem MenuFullScreen;
        protected MenuItem MenuFileRecentFiles;
        protected RadioMenuItem ViewRecordsAction;
        protected RadioMenuItem EditRecordsAction;
        protected ToggleButton ToggleEditRecords;

        protected Image recordIcon;
        protected Label recordDetails;

        protected Box RecordView;
        protected Box RecordEditor;

        protected SearchEntry searchEntry;
        protected Entry tempEntry;

        protected BibtexRecords bibtexRecords;
        protected SidePaneTreeStore sidePaneStore;
        protected LitListStore litStore;
        protected TreeModelFilter modelFilter;
        protected TreeModelFilter fieldFilter;

        protected ComboBoxText comboRecordType;
        protected Entry entryReqBibtexKey;
        protected Button buttonBibtexKeyGenerate;
        protected Label lblBibtexKey;
        protected Paned MainVpane;

        protected Builder gui;

        protected LitTreeView litTreeView;
        protected SidePaneTreeView sidePaneTreeView;
        protected Notebook notebookFields;
        protected BibliographerSettings windowSettings;
        protected BibliographerSettings sidebarSettings;
        protected BibliographerSettings filehandlingSettings;

        bool modified;
        bool new_selected_record;
        protected string file_name;
        protected string application_name;

        public AlterationMonitor am;

        static TargetEntry[] target_table = { new TargetEntry ("text/uri-list", 0, 0) };

        class FieldEntry : Entry
        {
            public string field = "";
        }

        sealed class FieldButton : Button
        {
            public string field = "";
        }

        public BibliographerMainWindow ()
        {
            System.Reflection.AssemblyTitleAttribute title;

            gui = new Builder ();
            System.IO.Stream guiStream = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibliographer.glade");
            try
            {
                var reader = new System.IO.StreamReader (guiStream);
                gui.AddFromString (reader.ReadToEnd());
                gui.Autoconnect (this);

                // TODO: move window and derived settings into apps.bibliographer.window
                windowSettings = new BibliographerSettings ("apps.bibliographer");
                sidebarSettings = new BibliographerSettings ("apps.bibliographer.sidebar");
                filehandlingSettings = new BibliographerSettings ("apps.bibliographer.filehandling");

                if (windowSettings.GetString ("data-directory") == "") {
                    // Set default data directory if none exists
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        windowSettings.SetString("data-directory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer");
                    }
                    else
                    {
                        windowSettings.SetString ("data-directory", Environment.GetEnvironmentVariable ("HOME") + "/.local/share/bibliographer/");
                    }
                }

                am = new AlterationMonitor ();

                // Set up main window defaults

                MenuItem MenuFileNew;
                MenuItem MenuFileOpen;
                MenuItem MenuFileSave;
                MenuItem MenuFileSaveAs;
                MenuItem MenuFileImportFolder;
                MenuItem MenuFileQuit;
                MenuItem MenuEditAddRecord;
                MenuItem MenuEditRemoveRecord;
                MenuItem MenuEditAddRecordFromBibtex;
                MenuItem MenuEditAddRecordClipboard;
                MenuItem MenuViewColumns;
                MenuItem MenuHelpAbout;

                ToolButton ToolFileNew;
                ToolButton ToolFileOpen;
                ToolButton ToolFileSave;
                ToolButton ToolFileSaveAs;
                ToolButton ToolAddRecord;
                ToolButton ToolRemoveRecord;


                ScrolledWindow scrolledwindowTreeView;
                Box searchHBox;

                window = (Window)gui.GetObject ("bibliographer.BibliographerMainWindow");
                MenuFileNew = (MenuItem)gui.GetObject ("FileNew");
                MenuFileOpen = (MenuItem)gui.GetObject ("FileOpen");
                MenuFileSave = (MenuItem)gui.GetObject ("FileSave");
                MenuFileSaveAs = (MenuItem)gui.GetObject ("FileSaveAs");
                MenuFileImportFolder = (MenuItem)gui.GetObject ("FileImportFolder");
                MenuFileRecentFiles = (MenuItem)gui.GetObject ("recentFilesMenu");
                MenuFileQuit = (MenuItem)gui.GetObject ("FileQuit");
                MenuEditAddRecord = (MenuItem)gui.GetObject ("EditAddRecord");
                MenuEditRemoveRecord = (MenuItem)gui.GetObject ("EditRemoveRecord");
                MenuEditAddRecordFromBibtex = (MenuItem)gui.GetObject ("EditAddRecordFromBibtex");
                MenuEditAddRecordClipboard = (MenuItem)gui.GetObject ("EditAddRecordClipboard");
                MenuViewSidebar = (CheckMenuItem)gui.GetObject ("ViewSidebar");
                MenuViewRecordDetails = (CheckMenuItem)gui.GetObject ("ViewRecordDetails");
                MenuViewColumns = (MenuItem)gui.GetObject ("ViewColumns");
                MenuFullScreen = (CheckMenuItem)gui.GetObject ("ViewFullScreen");
                MenuHelpAbout = (MenuItem)gui.GetObject ("HelpAbout");

                ToolFileNew = (ToolButton)gui.GetObject ("ToolNewFile");
                ToolFileOpen = (ToolButton)gui.GetObject ("ToolFileOpen");
                ToolFileSave = (ToolButton)gui.GetObject ("ToolFileSave");
                ToolFileSaveAs = (ToolButton)gui.GetObject ("ToolFileSaveAs");
                ToolAddRecord = (ToolButton)gui.GetObject ("ToolAddRecord");
                ToolRemoveRecord = (ToolButton)gui.GetObject ("ToolRemoveRecord");

                ViewRecordsAction = (RadioMenuItem)gui.GetObject ("ViewRecordsAction");
                EditRecordsAction = (RadioMenuItem)gui.GetObject ("EditRecordsAction");
                ToggleEditRecords = (ToggleButton)gui.GetObject ("ToggleEditRecords");

                scrolledwindowTreeView = (ScrolledWindow)gui.GetObject ("scrolledwindowTreeView");
                scrolledwindowSidePaneScrolledWindow = (ScrolledWindow)gui.GetObject ("scrolledwindowSidePaneScrolledWindow");
                scrolledwindowSidePane = (Paned)gui.GetObject ("scrolledwindowSidePane");
                viewportRequired = (Viewport)gui.GetObject ("viewportRequired");
                viewportOptional = (Viewport)gui.GetObject ("viewportOptional");
                viewportOther = (Viewport)gui.GetObject ("viewportOther");
                viewportBibliographerData = (Viewport)gui.GetObject ("viewportBibliographerData");
                notebookFields = (Notebook)gui.GetObject ("notebookFields");
                recordDetailsView = (Box)gui.GetObject ("recordDetailsView");
                MainVpane = (Paned)gui.GetObject ("MainVpane");

                comboRecordType = (ComboBoxText)gui.GetObject ("comboRecordType");
                searchHBox = (Box)gui.GetObject ("searchHBox");
                entryReqBibtexKey = (Entry)gui.GetObject ("entryReqBibtexKey");
                buttonBibtexKeyGenerate = (Button)gui.GetObject ("buttonBibtexKeyGenerate");
                lblBibtexKey = (Label)gui.GetObject ("lblBibtexKey");
                RecordEditor = (Box)gui.GetObject ("RecordEditor");
                RecordView = (Box)gui.GetObject ("RecordView");

                bibliographerStatusBar = (Statusbar)gui.GetObject("bibliographerStatusBar");

                MenuFileNew.Activated += OnFileNewActivated;
                MenuFileOpen.Activated += OnFileOpenActivated;
                MenuFileSave.Activated += OnFileSaveActivated;
                MenuFileSaveAs.Activated += OnFileSaveAsActivated;
                MenuFileImportFolder.Activated += OnFileImportFolderActivated;
                MenuFileQuit.Activated += OnFileQuitActivated;
                MenuEditAddRecord.Activated += OnAddRecordActivated;
                MenuEditRemoveRecord.Activated += OnRemoveRecordActivated;
                MenuEditAddRecordFromBibtex.Activated += OnAddRecordFromBibtexActivated;
                MenuEditAddRecordClipboard.Activated += OnAddRecordFromClipboardActivated;
                MenuViewSidebar.Activated += OnToggleSideBarActivated;
                MenuViewRecordDetails.Activated += OnToggleRecordDetailsActivated;
                MenuViewColumns.Activated += OnChooseColumnsActivated;
                MenuFullScreen.Activated += OnToggleFullScreenActionActivated;
                MenuHelpAbout.Activated += OnHelpAboutActivated;

                ToolFileNew.Clicked += OnFileNewActivated;
                ToolFileOpen.Clicked += OnFileOpenActivated;
                ToolFileSave.Clicked += OnFileSaveActivated;
                ToolFileSaveAs.Clicked += OnFileSaveAsActivated;
                ToolAddRecord.Clicked += OnAddRecordActivated;
                ToolRemoveRecord.Clicked += OnRemoveRecordActivated;

                ToggleEditRecords.Clicked += OnToggleEditRecordsActivated;
                EditRecordsAction.Activated += OnRadioEditRecordsActivated;
                ViewRecordsAction.Activated += OnRadioViewRecordsActivated;

                buttonBibtexKeyGenerate.Activated += OnButtonBibtexKeyGenerateClicked;
                comboRecordType.Changed += OnComboRecordTypeChanged;

                window.DeleteEvent += OnWindowDeleteEvent;
                window.StateChanged += OnWindowStateChanged;
                window.SizeAllocated += OnWindowSizeAllocated;

                window.Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("bibliographer.png"));
                title = (System.Reflection.AssemblyTitleAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), 
                                                                     typeof(System.Reflection.AssemblyTitleAttribute));
                application_name = title.Title;

                window.WidthRequest = 800;
                window.HeightRequest = 600;

                int wdth, hght;

                wdth = windowSettings.GetInt ("window-width");
                hght = windowSettings.GetInt ("window-height");

                window.Resize(width: wdth, height: hght);

                if (windowSettings.GetBoolean ("window-maximized"))
                    window.Maximize ();

                window.SetPosition (WindowPosition.Center);

                // Main bibtex view list model
                bibtexRecords = new BibtexRecords ();

                bibtexRecords.RecordsModified += OnBibtexRecordsModified;
                bibtexRecords.RecordURIAdded += OnBibtexRecordURIAdded;
                bibtexRecords.RecordURIModified += OnBibtexRecordURIModified;

                litStore = new LitListStore (bibtexRecords);

                modelFilter = new TreeModelFilter (litStore, null);
                fieldFilter = new TreeModelFilter (modelFilter, null);

                modelFilter.VisibleFunc = new TreeModelFilterVisibleFunc (ModelFilterListStore);
                fieldFilter.VisibleFunc = new TreeModelFilterVisibleFunc (FieldFilterListStore);

                // Setup and add the LitTreeView
                litTreeView = new LitTreeView (fieldFilter);
                scrolledwindowTreeView.Add (litTreeView);
                scrolledwindowTreeView.Visible = true;
                litTreeView.ShowAll ();
                // LitTreeView callbacks
                litTreeView.Selection.Changed += OnLitTreeViewSelectionChanged;
                litTreeView.DragDataReceived += OnLitTreeViewDragDataReceived;

                // Side Pane tree model
                sidePaneStore = new SidePaneTreeStore (bibtexRecords);
                sidePaneStore.SetSortColumnId (0, SortType.Ascending);

                sidePaneTreeView = new SidePaneTreeView (sidePaneStore);
                scrolledwindowSidePaneScrolledWindow.Add (sidePaneTreeView);
                // SidePaneTreeView callbacks
                sidePaneTreeView.Selection.Changed += OnSidePaneTreeSelectionChanged;

                if (sidebarSettings.GetBoolean ("visible")) {
                    MenuViewSidebar.Active = false;
                    MenuViewSidebar.Activate ();
                }

                MenuViewRecordDetails.Active = false;
                MenuViewRecordDetails.Activate ();

                // Read cached sidePane width
                scrolledwindowSidePane.Position = sidebarSettings.GetInt ("width");

                // Set up comboRecordType items
                for (int i = 0; i < BibtexRecordTypeLibrary.Count (); i++)
                    comboRecordType.InsertText (i, BibtexRecordTypeLibrary.GetWithIndex (i).name);

                // Set up drag and drop of files into litTreeView
                Drag.DestSet (litTreeView, DestDefaults.All, target_table, Gdk.DragAction.Copy);

                // Search entry
                searchEntry = new SearchEntry ();
                // TODO: Fix searchEntry
                //searchEntry.SearchChanged += OnFilterEntryChanged;
                //searchHBox.Add (searchEntry);
                var tempLabel = new Label();
                tempLabel.Text = "Search: ";
                searchHBox.Add(tempLabel);
                tempEntry = new Entry();
                tempEntry.WidthChars = 40;
                tempEntry.Changed += OnFilterEntryChanged;
                searchHBox.Add(tempEntry);
                var tempButton = new Button();
                tempButton.Relief = Gtk.ReliefStyle.None;
                tempButton.Label = "Clear";
                tempButton.Clicked += TempButton_Clicked;
                searchHBox.Add(tempButton);
                searchHBox.Expand = true;

                UpdateMenuFileHistory ();

                // Activate new file

                MenuFileNew.Activate ();
                ToggleEditRecords.Activate ();
                ReconstructTabs ();
                ReconstructDetails ();
                // Now that we are configured, show the window
                window.ShowAll ();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine ("GUI configuration file not found.\n" + e.Message);
            }
        }

        void TempButton_Clicked (object sender, EventArgs e)
        {
            tempEntry.Text = "";
        }

        void BibtexGenerateKeySetStatus ()
        {
            TreeIter iter;
            ITreeModel model;

            // Get selected bibtex record
            if (litTreeView.Selection.GetSelected (out model, out iter)) {

                var record = (BibtexRecord)model.GetValue (iter, 0);
                // Record has Author and Year fields
                if (record.HasField ("author") && record.HasField ("year")) {
                    // Author and year fields are not empty
                    if (!(string.IsNullOrEmpty (record.GetField ("author"))) && !(string.IsNullOrEmpty (record.GetField ("year")))) {
                        // Bibtex Entry is empty
                        if (entryReqBibtexKey.Text == "" | entryReqBibtexKey.Text == null)
                            buttonBibtexKeyGenerate.Sensitive = true;
                        else
                            // Bibtex Entry is not empty, so the generate key is not sensitive
                            buttonBibtexKeyGenerate.Sensitive = false;
                    } else
                        // Author and year fields are empty
                        buttonBibtexKeyGenerate.Sensitive = false;
                } else
                    // Record does not have Author and Year fields
                    buttonBibtexKeyGenerate.Sensitive = false;
            } else
                // A Bibtex record is not selected
                buttonBibtexKeyGenerate.Sensitive = false;
        }

        bool FieldFilterListStore (ITreeModel model, TreeIter iter)
        {
            // TODO: add support for searching certain columns eg. Author / Journal / Title etc...
            // This is possible by implimenting a method such as record.searchField(field, text)

            var record = (BibtexRecord)model.GetValue (iter, 0);

            TreeIter iterFilter;
            ITreeModel modelFilterListStore;

            if (sidePaneTreeView.Selection.GetSelected (out modelFilterListStore, out iterFilter)) {
                TreeIter iterParent;
                string column, filter;
                if (((SidePaneTreeStore)modelFilterListStore).IterParent (out iterParent, iterFilter)) {
                    column = ((SidePaneTreeStore)modelFilterListStore).GetValue (iterParent, 0) as string;
                    filter = ((SidePaneTreeStore)modelFilterListStore).GetValue (iterFilter, 0) as string;
                    // Deal with authors
                    if (column.ToLower () == "author") {
                        //string authorstring = record.GetField(column.ToLower());
                        if (record != null) {
                            StringArrayList authors = record.GetAuthors () ?? new StringArrayList ();
                            return authors.Contains (filter);
                        }
                        // Deal with other fields
                    } else {
                        if (record != null) {
                            return record.GetField (column.ToLower ()) == filter;
                        }
                    }
                    //System.Console.WriteLine(column + " -> " + filter);
                }
            }

            return true;
        }

        bool ModelFilterListStore (ITreeModel model, TreeIter iter)
        {
            BibtexSearchField sfield;
            sfield = BibtexSearchField.All;

            Menu searchMenu = searchEntry.Menu;

            foreach (RadioMenuItem menuItem in searchMenu.AllChildren) {
                if (menuItem.Active) {
                    sfield = (BibtexSearchField)menuItem.Data ["searchField"];
                }
            }

            if (string.IsNullOrEmpty (searchEntry.Text) && string.IsNullOrEmpty(tempEntry.Text))
                return true;

            var record = (BibtexRecord)model.GetValue (iter, 0);

            if (record != null) {
                if (record.SearchRecord (searchEntry.Text, sfield) && record.SearchRecord(tempEntry.Text, sfield))
                    return true;
                else {
                    if ((sfield == BibtexSearchField.All) || (sfield == BibtexSearchField.Article)) {
                        var index = (Tri)record.GetCustomDataField ("indexData");
                        if (index != null) {
                            //System.Console.WriteLine("Index tri data: " + index.ToString());
                            if (index.IsSubString (searchEntry.Text) && index.IsSubString(tempEntry.Text))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public void ReconstructDetails ()
        {
            recordIcon = (Image)gui.GetObject ("recordIcon");
            recordDetails = (Label)gui.GetObject ("recordDetails");

            // step 1: reset values
            recordIcon.Pixbuf = null;
            recordDetails.Text = null;

            // step 2: add new values
            TreeIter iter;
            ITreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                recordIcon.Pixbuf = (Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail");

                string text;

                // TODO: sort out some smart way of placing all the details
                // here
                text = "<b>";
                text = text + StringOps.TeXToUnicode (record.GetField ("title"));
                text = text + "</b>\n";
                text = text + StringOps.TeXToUnicode (record.GetField ("author"));
                text = text + "\n";
                text = text + StringOps.TeXToUnicode (record.GetField ("year"));
                recordDetails.Markup = text;
            }
        }

        public void ReconstructTabs ()
        {
            TreeIter iter;
            ITreeModel model;

            viewportRequired = (Viewport)gui.GetObject ("viewportRequired");
            viewportOptional = (Viewport)gui.GetObject ("viewportOptional");
            viewportOther = (Viewport)gui.GetObject ("viewportOther");
            viewportBibliographerData = (Viewport)gui.GetObject ("viewportBibliographerData");

            // step 1: reset viewports
            viewportRequired.Forall (viewportRequired.Remove);
            viewportOptional.Forall (viewportOptional.Remove);
            viewportOther.Forall (viewportOther.Remove);
            viewportBibliographerData.Forall (viewportBibliographerData.Remove);

            // step 2: add stuff to the viewports
            var req = new VBox (false, 5);
            var opt = new VBox (false, 5);
            var other = new VBox (false, 5);
            var bData = new VBox (false, 5);

            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                comboRecordType.Sensitive = true;

                if (comboRecordType.Active < 0) {
                    entryReqBibtexKey.Sensitive = false;
                    notebookFields.Sensitive = false;
                    buttonBibtexKeyGenerate.Sensitive = false;
                } else {
                    notebookFields.Sensitive = true;
                    entryReqBibtexKey.Sensitive = true;
                    BibtexGenerateKeySetStatus ();
                }
                //  Console.WriteLine("Combo box active: " + comboRecordType.Active);

                var record = (BibtexRecord)model.GetValue (iter, 0);
                uint numItems;
                BibtexRecordType recordType = null;
                if (BibtexRecordTypeLibrary.Contains (record.RecordType)) {
                    recordType = BibtexRecordTypeLibrary.Get (record.RecordType);

                    if (comboRecordType.ActiveText != record.RecordType) {
                        comboRecordType.Active = BibtexRecordTypeLibrary.Index (record.RecordType);
                    }

                    if (entryReqBibtexKey.Text == "") {
                        buttonBibtexKeyGenerate.Activate ();
                    }

                    // viewportRequired
                    var tableReq = new Table (0, 2, false);
                    // TODO: process OR type fields
                    numItems = 0;
                    for (int i = 1; i < recordType.fields.Count; i++) {
                        int subNumItems = 0;
                        for (int j = 0; j < recordType.fields.Count; j++) {
                            if ((recordType.optional [j] == i) && (recordType.optional[j] != 0) ){
                                subNumItems++;
                                if (subNumItems > 1) {
                                    numItems += 2;
                                    tableReq.NRows = numItems;
                                    var orLabel = new Label ();
                                    orLabel.Markup = "<b>or</b>";
                                    tableReq.Attach (orLabel, 0, 2, numItems - 2, numItems - 1, 0, 0, 5, 5);
                                } else {
                                    numItems++;
                                    tableReq.NRows = numItems;
                                }
                                string fieldName = recordType.fields [j];
                                tableReq.Attach (new Label (fieldName), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                                var textEntry = new FieldEntry ();
                                if (record.HasField (fieldName))
                                    textEntry.Text = record.GetField (fieldName);
                                tableReq.Attach (textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                                textEntry.field = fieldName;
                                textEntry.Changed += OnFieldChanged;
                            }
                        }
                        if (subNumItems == 0)
                            break;
                    }
                    req.PackStart (tableReq, false, false, 5);

                    //  viewportOptional
                    var tableOpt = new Table (0, 2, false);
                    numItems = 0;
                    for (int i = 0; i < recordType.fields.Count; i++) {
                        if (recordType.optional [i] == 0) {
                            numItems++;
                            tableOpt.NRows = numItems;
                            tableOpt.Attach (new Label (recordType.fields [i]), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                            var textEntry = new FieldEntry ();
                            if (record.HasField (recordType.fields [i]))
                                textEntry.Text = record.GetField (recordType.fields [i]);
                            tableOpt.Attach (textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                            textEntry.field = recordType.fields [i];
                            textEntry.Changed += OnFieldChanged;
                        }
                    }
                    opt.PackStart (tableOpt, false, false, 5);
                }

                // viewportOther
                var tableOther = new Table (0, 3, false);
                numItems = 0;
                for (int i = 0; i < record.RecordFields.Count; i++) {
                    // doing this the hard way because we want to
                    // ignore case
                    bool found = false;
                    if (recordType != null)
                        for (int j = 0; j < recordType.fields.Count; j++)
                            if (String.Compare (((BibtexRecordField)record.RecordFields [i]).fieldName, recordType.fields [j], true) == 0) {
                                found = true;
                                break;
                            }
                    if (!found) {
                        // got one
                        string fieldName = ((BibtexRecordField)record.RecordFields [i]).fieldName;
                        bool inFieldLibrary = false;
                        for (int j = 0; j < BibtexRecordFieldTypeLibrary.Count (); j++)
                            if (String.Compare (fieldName, BibtexRecordFieldTypeLibrary.GetWithIndex (j).name, true) == 0) {
                                inFieldLibrary = true;
                                break;
                            }
                        numItems++;
                        tableOther.NRows = numItems;
                        var fieldLabel = new Label ();
                        if (inFieldLibrary)
                            fieldLabel.Text = fieldName;
                        else
                            fieldLabel.Markup = "<span foreground=\"red\">" + fieldName + "</span>";
                        tableOther.Attach (fieldLabel, 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                        var textEntry = new FieldEntry ();
                        if (record.HasField (fieldName))
                            textEntry.Text = record.GetField (fieldName);
                        textEntry.field = fieldName;
                        textEntry.Changed += OnFieldChanged;
                        tableOther.Attach (textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                        var removeButton = new FieldButton ();
                        removeButton.Label = "Remove field";
                        removeButton.field = fieldName;
                        removeButton.Clicked += OnFieldRemoved;
                        removeButton.Activated += OnFieldRemoved;
                        tableOther.Attach (removeButton, 2, 3, numItems - 1, numItems, 0, 0, 5, 5);
                    }
                }
                other.PackStart (tableOther, false, false, 5);

                // also include a drop-down box of other fields that could be
                // added to this record
                ComboBoxText extraFields;
                extraFields = new ComboBoxText ();
                bool comboAdded = false;
                for (int i = 0; i < BibtexRecordFieldTypeLibrary.Count (); i++) {
                    string field = BibtexRecordFieldTypeLibrary.GetWithIndex (i).name;
                    bool found = false;
                    if (recordType != null) {
                        for (int j = 0; j < recordType.fields.Count; j++)
                            if (String.Compare (field, recordType.fields[j], true) == 0) {
                                found = true;
                                break;
                            }
                    }
                    if (found)
                        continue;
                    for (int j = 0; j < record.RecordFields.Count; j++)
                        if (String.Compare (field, ((BibtexRecordField)record.RecordFields[j]).fieldName, true) == 0) {
                            found = true;
                            break;
                        }
                    if (found)
                        continue;
                    extraFields.AppendText (field);
                    comboAdded = true;
                }
                if (comboAdded) {
                    var hbox = new HBox ();
                    hbox.PackStart (new Label ("Add extra field:"), false, false, 5);
                    hbox.PackStart (extraFields, false, false, 5);
                    other.PackStart (hbox, false, false, 5);
                    extraFields.Changed += OnExtraFieldAdded;
                } else {
                    var noExtraFields = new Label ();
                    noExtraFields.Markup = "<i>No extra fields</i>";
                    other.PackStart (noExtraFields, false, false, 5);
                }

                // viewportBibliographerData
                var uriHBox = new HBox ();
                uriHBox.PackStart (new Label ("Associated file:"), false, false, 5);
                var uriEntry = new FieldEntry ();
                if (record.HasURI ())
                    uriEntry.Text = record.GetURI ();
                uriEntry.field = BibtexRecord.BibtexFieldName.URI;
                uriEntry.FocusOutEvent += OnFieldChanged;
                uriHBox.PackStart (uriEntry, false, false, 5);
                var uriBrowseButton = new Button ("Browse");
                uriBrowseButton.Activated += OnURIBrowseClicked;
                uriBrowseButton.Clicked += OnURIBrowseClicked;
                uriHBox.PackStart (uriBrowseButton, false, false, 5);
                bData.PackStart (uriHBox, false, false, 5);
            } else {
                notebookFields.Sensitive = false;
                entryReqBibtexKey.Sensitive = false;
                buttonBibtexKeyGenerate.Sensitive = false;
                comboRecordType.Sensitive = false;
            }

            viewportRequired.Add (req);
            viewportRequired.ShowAll ();

            viewportOptional.Add (opt);
            viewportOptional.ShowAll ();

            viewportOther.Add (other);
            viewportOther.ShowAll ();

            viewportBibliographerData.Add (bData);
            viewportBibliographerData.ShowAll ();
        }

        void FileUnmodified ()
        {
            window.Title = application_name + " - " + file_name;
            modified = false;
        }

        void FileModified ()
        {
            //System.Console.WriteLine ("File modified setting file_name: {0}", file_name);
            window.Title = application_name + " - " + file_name + "*";
            modified = true;
        }

        public void FileOpen (string file_name)
        {
            bibtexRecords = BibtexRecords.Open (file_name);
            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
            bibtexRecords.RecordURIAdded += OnBibtexRecordURIAdded;
            bibtexRecords.RecordURIModified += OnBibtexRecordURIModified;

            litStore.SetBibtexRecords (bibtexRecords);
            sidePaneStore.SetBibtexRecords (bibtexRecords);

            this.file_name = file_name;

            am.FlushQueues ();
            am.SubscribeAlteredRecords (bibtexRecords);

            FileUnmodified ();
            // Disable editing of opened document
            ViewRecordsAction.Activate ();

            UpdateFileHistory (file_name);
        }

        ResponseType FileSave ()
        {
            if (file_name != "Untitled.bib") {
                //BibtexListStoreParser btparser = new BibtexListStoreParser(
                //  store);
                //btparser.Save(file_name);
                bibtexRecords.Save (file_name);
                FileUnmodified ();

                return ResponseType.Ok;
            } else {
                return FileSaveAs ();
            }
        }

        ResponseType FileSaveAs ()
        {
            var fileSaveDialog = new FileChooserDialog ("Save Bibtex file...", window, FileChooserAction.Save);

            fileSaveDialog.AddButton ("Cancel", ResponseType.Cancel);
            fileSaveDialog.AddButton ("Ok", ResponseType.Ok);
            // TODO: filter for *.bib files only :)
            fileSaveDialog.Filter = new FileFilter ();
            fileSaveDialog.Filter.AddPattern ("*.bib");

            fileSaveDialog.SetCurrentFolder (filehandlingSettings.GetString ("bib-browse-path"));

            var response = (ResponseType)fileSaveDialog.Run ();

            if (response == ResponseType.Ok) {
                filehandlingSettings.SetString ("bib-browse-path", fileSaveDialog.CurrentFolder);
                if (fileSaveDialog.Filename != null) {
                    file_name = fileSaveDialog.Filename;
                    FileSave ();
                    UpdateFileHistory (file_name);
                    fileSaveDialog.Destroy ();
            return ResponseType.Ok;
                }
            }

            fileSaveDialog.Destroy ();
            return ResponseType.Cancel;
        }

        void InsertFilesInDir (object o)
        {
            string dir = (string)o;
            string[] files = System.IO.Directory.GetFiles (dir);

            //double fraction = 1.0 / System.Convert.ToDouble(files.Length);

            foreach (string file in files) {
                while (Gtk.Application.EventsPending ())
                    Gtk.Application.RunIteration ();
                Uri fileUri = new Uri(file);
                if (!bibtexRecords.HasURI (fileUri.ToString())) {
                    Debug.WriteLine (5, "Adding new record with URI: {0}", fileUri.ToString());
                    var record = new BibtexRecord ();
                    record.DoiAdded += OnDOIRecordAdded;
                    record.RecordModified += OnRecordModified;
                    bibtexRecords.Add (record);

                    // Only set the uri field after the record has been added to bibtexRecords, so that the event is caught
                    //System.Console.WriteLine("Setting URI: {0}", uri);
                    record.SetURI (fileUri.ToString());
                }
            }
        }

        void UpdateRecordTypeCombo ()
        {
            TreeIter iter;
            ITreeModel model;

            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                string recordType = record.RecordType;
                new_selected_record = true;
                comboRecordType.Active = BibtexRecordTypeLibrary.Contains (recordType) ? BibtexRecordTypeLibrary.Index (recordType) : -1;

                comboRecordType.Activate ();
                entryReqBibtexKey.Text = record.GetKey ();
                buttonBibtexKeyGenerate.Sensitive = false;
                new_selected_record = false;
            }
        }

        void UpdateFileHistory (string filename)
        {
            int max, bumpEnd;
            string[] tempHistory;
            ArrayList history;

            max = filehandlingSettings.GetInt ("max-file-history-count");
            tempHistory = filehandlingSettings.GetStrv ("file-history");
            history = new ArrayList (tempHistory);

            while (history.Count > max)
                history.RemoveAt(history.Count - 1);

            // check if this filename is already in our history
            // if so, it just gets bumped to top position,
            // otherwise bump everything down
            for (bumpEnd = 0; bumpEnd < history.Count; bumpEnd++) {
                if ((string)history[bumpEnd] == filename) {
                    break;
                }
            }
            if (bumpEnd == max)
                bumpEnd--;
            if (history.Count < max && bumpEnd == history.Count)
                history.Add ("");
            //System.Console.WriteLine("bumpEnd set to {0}", bumpEnd);
            for (int cur = bumpEnd; cur > 0; cur--) {
                history[cur] = history[cur - 1];
            }
            history[0] = filename;
            filehandlingSettings.SetStrv ("file-history", (string[])history.ToArray(typeof(string)));

            UpdateMenuFileHistory ();
        }

        void UpdateMenuFileHistory ()
        {
            Debug.WriteLine (5, "File History - doing update...");

            Menu MenuFileRecentFilesChildren;
            MenuFileRecentFilesChildren = new Menu ();

            // step 1: clear the menu
            foreach (Widget w in MenuFileRecentFiles) {
                MenuFileRecentFiles.Remove (w);
            }
            MenuFileRecentFiles.Submenu = MenuFileRecentFilesChildren;
            MenuFileRecentFiles.Label = "Recent Files";

            // step 2: add on items for history
            bool gotOne = false;
            String[] history = filehandlingSettings.GetStrv ("file-history");

            for (int i = 0; i < history.Length; i++) {
                // Workaround for clearing history - check if history item is not an empty string
                if (history[i] != "") {
                    string label = string.Format ("_{0} ", i + 1) + history [i];
                    var item = new MenuItem (label);
                    item.Activated += OnFileHistoryActivate;
                    item.Data.Add ("i", (IntPtr)i);
                    MenuFileRecentFilesChildren.Add (item);
                    gotOne = true;
                }
            }
            if (gotOne) {
                MenuFileRecentFilesChildren.Add (new SeparatorMenuItem ());
                var clear = new MenuItem ("Clear");
                clear.Activated += OnClearFileHistory;
                MenuFileRecentFilesChildren.Add (clear);
            } else {
                var none = new MenuItem ("(none)");
                // want to disable this somehow...
                //none. = false;
                MenuFileRecentFilesChildren.Add (none);
            }

            MenuFileRecentFilesChildren.ShowAll ();
        }

        void Quit ()
        {
            litTreeView.SaveColumnsState ();

            // Save sidebar visibility
            sidebarSettings.SetBoolean("visible", scrolledwindowSidePaneScrolledWindow.Visible);
            // Save sidebar width
            // TODO: This is disabled due to growing sidebar bug
            //sidebarSettings.SetInt("width", scrolledwindowSidePane.Position);

            if (IsModified ()) {
                if (!ProcessModifiedData ())
                    return;
            }

            Gtk.Application.Quit ();
        }

        bool ProcessModifiedData ()
        {
            // display a dialog asking the user if they
            // want to save their changes (offering them
            // Yes/No/Cancel. If they choose Yes, call
            // FileSave and return true. If they choose
            // No, just return true. If they choose
            // Cancel, return false
            var dialog = new MessageDialog (window, DialogFlags.Modal, MessageType.Question, 
                             ButtonsType.None, "{0} has been modified. Do you want to save it?", file_name);
            dialog.AddButton ("Yes", ResponseType.Yes);
            dialog.AddButton ("No", ResponseType.No);
            dialog.AddButton ("Cancel", ResponseType.Cancel);
            var msg_result = (ResponseType)dialog.Run ();
            dialog.Destroy ();

            if (msg_result == ResponseType.Yes) {
                ResponseType save_result = FileSave ();
                if (save_result == ResponseType.Ok)
                    return true;
                else if (save_result == ResponseType.Cancel)
                    return false;
            } else if (msg_result == ResponseType.No)
                return true;
            else if (msg_result == ResponseType.Cancel)
                return false;
            else {
                var error = new MessageDialog (window, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, 
                                "An unexpected error occurred in processing your response, please contact the developers!");
                error.Run ();
                error.Destroy ();
                return false;
            }
            return false;
        }

        bool IsModified ()
        {
            return modified;
        }

        /* -----------------------------------------------------------------
           CALLBACKS
           ----------------------------------------------------------------- */

        protected void OnBibtexRecordURIAdded (object o, EventArgs a)
        {
            var record = (BibtexRecord)o;
            //if (am.Altered (record)) {
                //System.Console.WriteLine("Record altered: URI added");
                // Add record to get re-indexed
                am.SubscribeAlteredRecord (record);
            //}
        }

        protected void OnBibtexRecordURIModified (object o, EventArgs a)
        {
            //TODO: Is this necessary? - the record with a previous URI should still be monitored by the alteration monitor
            var record = (BibtexRecord)o;
            //if (am.Altered (record)) {
                //System.Console.WriteLine("Record altered: URI modified");
                // Add record to get re-indexed
                am.SubscribeAlteredRecord (record);
            //}
        }

        protected void OnComboRecordTypeChanged (object o, EventArgs a)
        {
            // the next check stops bad things from happening when
            // the user selects a new record in the list view,
            // causing the checkbox to get updated. In this case,
            // we really don't want to be calling this method
            if (new_selected_record)
                return;

            TreeIter iter;
            ITreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                if (((ComboBox)o).Active != -1) {
                    string bType = BibtexRecordTypeLibrary.GetWithIndex (((ComboBox)o).Active).name;
                    Debug.WriteLine (5, bType);
                    // TODO: fix me :-)
                    //model.SetValue(iter, (int)bibtexRecordField.bibtexField.BIBTEXTYPE,
                    //((int)bType).ToString());

                    ((BibtexRecord)model.GetValue (iter, 0)).RecordType = bType;
                    if (bType == "comment") {
                        lblBibtexKey.Text = "Comment";
                        notebookFields.Visible = false;
                        buttonBibtexKeyGenerate.Visible = false;
                    } else {
                        lblBibtexKey.Text = "BibTeX Key";
                        notebookFields.Visible = true;
                        buttonBibtexKeyGenerate.Visible = true;
                    }
                }
            // Sort out the behaviour of the Required and Optional fields
            // for each type of record
            // TODO: FIXME
            }
            ReconstructTabs ();
            ReconstructDetails ();
        }

        protected void OnSidePaneTreeSelectionChanged (object o, EventArgs a)
        {
            fieldFilter.Refilter ();
        }

        protected void OnBibtexRecordsModified (object o, EventArgs a)
        {
            // Refresh the Article Type field
            FileModified ();
            UpdateRecordTypeCombo ();
        }

        protected void OnExtraFieldAdded (object o, EventArgs a)
        {
            TreeIter iter;
            ITreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var combo = (ComboBox)o;
                TreeIter comboIter;
                if (combo.GetActiveIter (out comboIter)) {
                    var record = (BibtexRecord)model.GetValue (iter, 0);
                    record.SetField ((string)combo.Model.GetValue (comboIter, 0), "");
                    ReconstructTabs ();
                    ReconstructDetails ();
                }
            }
        }

        protected void OnFieldChanged (object o, EventArgs a)
        {
            TreeIter iter;
            ITreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var entry = (FieldEntry)o;
                var record = (BibtexRecord)model.GetValue (iter, 0);
                if (record.GetField (entry.field) != entry.Text) {
                    record.SetField (entry.field, entry.Text);
                    model.EmitRowChanged (model.GetPath (iter), iter);
                    BibtexGenerateKeySetStatus ();
                }
            }
        }

        protected void OnFieldRemoved (object o, EventArgs a)
        {
            TreeIter iter;
            ITreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var button = (FieldButton)o;
                var record = (BibtexRecord)model.GetValue (iter, 0);
                record.RemoveField (button.field);
                ReconstructTabs ();
                ReconstructDetails ();
            }
        }

        protected void OnFileQuitActivated (object sender, EventArgs e)
        {
            Quit ();
        }

        protected void OnFileNewActivated (object sender, EventArgs e)
        {
            if (IsModified ()) {
                if (!ProcessModifiedData ())
                    return;
            }

            am.FlushQueues ();

            bibtexRecords = new BibtexRecords ();

            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
            bibtexRecords.RecordURIAdded += OnBibtexRecordURIAdded;
            bibtexRecords.RecordURIModified += OnBibtexRecordURIModified;

            litStore.SetBibtexRecords (bibtexRecords);
            sidePaneStore.SetBibtexRecords (bibtexRecords);

            file_name = "Untitled.bib";
            FileUnmodified ();
        }

        protected void OnHelpAboutActivated (object sender, EventArgs e)
        {
            var ab = new AboutBox ();
            ab.TransientFor = window;
            ab.Run ();
            ab.Destroy ();
        }

        protected void OnEntryReqBibtexKeyChanged (object sender, EventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException ("e");

            var bibtexEntry = (Entry)sender;
            TreeIter iter;
            ITreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                record.SetKey (bibtexEntry.Text);
                model.EmitRowChanged (model.GetPath (iter), iter);
            }
        }

        protected void OnURIBrowseClicked (object o, EventArgs a)
        {
            TreeIter iter;
            ITreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);

                var fileOpenDialog = new FileChooserDialog ("Associate file...", window, FileChooserAction.Open);
                fileOpenDialog.AddButton ("Cancel", ResponseType.Cancel);
                fileOpenDialog.AddButton ("Ok", ResponseType.Ok);

                fileOpenDialog.Filter = new FileFilter ();
                fileOpenDialog.Title = "Associate file...";
                fileOpenDialog.Filter.AddPattern ("*");

                if (record.HasField (BibtexRecord.BibtexFieldName.URI)) {
                    // If a file is associated with this directory, then open browse path in the files containing directory
                    fileOpenDialog.SetUri (record.GetURI ());
                }
                else {
                    // Else, query config for stored path
                    fileOpenDialog.SetCurrentFolder (filehandlingSettings.GetString ("uri-browse-path"));
                }

                var result = (ResponseType)fileOpenDialog.Run ();

                if (result == ResponseType.Ok) {
                    
                    Uri fileUri;
                    fileUri = new Uri (fileOpenDialog.Filename);
                    record.SetField (BibtexRecord.BibtexFieldName.URI, fileUri.AbsoluteUri);
                    filehandlingSettings.SetString ("uri-browse-path", fileOpenDialog.CurrentFolder);
                    ReconstructTabs ();
                    ReconstructDetails ();

                }

                fileOpenDialog.Destroy ();
            }
        }

        protected void OnFileOpenActivated (object o, EventArgs a)
        {

            var fileOpenDialog = new FileChooserDialog ("Open Bibtex File...", window, FileChooserAction.Open);
            fileOpenDialog.AddButton ("Cancel", ResponseType.Cancel);
            fileOpenDialog.AddButton ("Ok", ResponseType.Ok);
            fileOpenDialog.Filter = new FileFilter ();
            fileOpenDialog.Filter.AddPattern ("*.bib");

            // query config for stored path
            fileOpenDialog.SetCurrentFolder (filehandlingSettings.GetString ("bib-browse-path"));

            var result = (ResponseType)fileOpenDialog.Run ();

            if (result == ResponseType.Ok)
                if (fileOpenDialog.Filename != null) {
                    file_name = fileOpenDialog.Filename;
                    FileOpen (file_name);
                    // TODO: Verify document integrity
                    // (search for conflicting bibtexkeys)
                    // (highlite any uncomplete entries)
                }
            fileOpenDialog.Destroy ();
        }

        protected void OnFileSaveActivated (object sender, EventArgs e)
        {
            FileSave ();
        }

        protected void OnFileSaveAsActivated (object sender, EventArgs e)
        {
            FileSaveAs ();
        }

        protected void OnFileHistoryActivate (object o, EventArgs a)
        {
            MenuItem item;
            string[] history;
            int index;

            item = (MenuItem)o;
            index = (int)Convert.ToUInt16 ((string)item.Data ["i"].ToString ());
            history = filehandlingSettings.GetStrv ("file-history");
            if (index < history.Length) {
                Debug.WriteLine (5, "Loading {0}", history[index]);
                file_name = history[index];
                FileOpen (file_name);
                UpdateFileHistory (file_name);
            }
        }

        protected void OnAddRecordActivated (object sender, EventArgs e)
        {
            Debug.WriteLine (5, "Adding a new record");
            //Debug.WriteLine(5, "Prev rec count: {0}", bibtexRecords.Count);
            TreeIter litTreeViewIter, sidePaneIter;

            // Unfilter
            sidePaneStore.GetIterFirst (out sidePaneIter);
            sidePaneTreeView.SetCursor (sidePaneTreeView.Model.GetPath (sidePaneIter), sidePaneTreeView.GetColumn (1), false);

            // Clear search
            searchEntry.Clear ();
            tempEntry.Text = "";

            if (bibtexRecords == null) {
                bibtexRecords = new BibtexRecords ();

                bibtexRecords.RecordsModified += OnBibtexRecordsModified;
                bibtexRecords.RecordURIAdded += OnBibtexRecordURIAdded;
                bibtexRecords.RecordURIModified += OnBibtexRecordURIModified;
            }

            var record = new BibtexRecord ();
            //System.Console.WriteLine ("Calling Add");
            record.DoiAdded += OnDOIRecordAdded;
            record.RecordModified += OnRecordModified;
            bibtexRecords.Add (record);

            fieldFilter.IterNthChild (out litTreeViewIter, fieldFilter.IterNChildren ()-1);
            litTreeView.SetCursor(fieldFilter.GetPath(litTreeViewIter), litTreeView.GetColumn (0), false);


            BibtexGenerateKeySetStatus ();

            am.SubscribeAlteredRecord (record);
        }

        protected void OnRemoveRecordActivated (object sender, EventArgs e)
        {
            TreeIter iter;
            ITreeModel model;
            TreePath newpath;

            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                // select next row, or previous row if we are on the last row
                newpath = model.GetPath (iter);
                newpath.Next ();
                litTreeView.SetCursor (newpath, litTreeView.GetColumn (0), false);
                if (!litTreeView.Selection.PathIsSelected (newpath)) {
                    newpath = model.GetPath (iter);
                    newpath.Prev ();
                    litTreeView.SetCursor (newpath, litTreeView.GetColumn (0), false);
                }

                // model and iter are TreeModelSort types
                // Obtain the search model
                ITreeModel searchModel = ((TreeModelSort)model).Model;
                TreeIter searchIter = ((TreeModelSort)model).ConvertIterToChildIter (iter);

                // Obtain the filter model
                ITreeModel filterModel = ((TreeModelFilter)searchModel).Model;
                TreeIter filterIter = ((TreeModelFilter)searchModel).ConvertIterToChildIter (searchIter);

                // Obtain the real model
                ITreeModel realModel = ((TreeModelFilter)filterModel).Model;
                TreeIter realIter = ((TreeModelFilter)filterModel).ConvertIterToChildIter (filterIter);

                // Delete record from the real model
                var record = ((ListStore)realModel).GetValue (realIter, 0) as BibtexRecord;
                bibtexRecords.Remove (record);

            }
        }

        protected void OnAddRecordFromBibtexActivated (object sender, EventArgs e)
        {
            TreeIter iter;
            Dialog EntryDialog;
            ResponseType result;
            TextView BibtexTextView;
            TextBuffer BibtexTextBuffer;

            EntryDialog = (Dialog)gui.GetObject ("BibtexEntryDialog");
            BibtexTextView = (TextView)gui.GetObject ("BibtexTextView");
            BibtexTextBuffer = BibtexTextView.Buffer;
            EntryDialog.AddButton ("Cancel", ResponseType.Cancel);
            EntryDialog.AddButton ("OK", ResponseType.Ok);

            result = (ResponseType)EntryDialog.Run ();

            if (result == ResponseType.Ok) {
                try {
                    var record = new BibtexRecord (BibtexTextBuffer.Text);
                    record.RecordModified += OnRecordModified;
                    bibtexRecords.Add (record);

                    iter = litStore.GetIter (record);
                    litTreeView.Selection.SelectIter (iter);

                    BibtexGenerateKeySetStatus ();

                    am.SubscribeAlteredRecord (record);
                } catch (ParseException except) {
                    Debug.WriteLine (1, "Parse exception: {0}", except.GetReason ());
                }
            }
            EntryDialog.Destroy ();
        }

        protected void OnAddRecordFromClipboardActivated (object sender, EventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException ("sender");
            if (e == null)
                throw new ArgumentNullException ("e");
            TreeIter iter;
            Clipboard clipboard;

            clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", true));
            if (clipboard.WaitIsTextAvailable()) {
                try{
                    var record = new BibtexRecord (clipboard.WaitForText());
                    record.RecordModified += OnRecordModified;
                    bibtexRecords.Add (record);

                    iter = litStore.GetIter (record);
                    litTreeView.Selection.SelectIter (iter);

                    BibtexGenerateKeySetStatus ();

                    am.SubscribeAlteredRecord (record);
                } catch (ParseException except) {
                    Debug.WriteLine (1, "Parse exception: {0}", except.GetReason ());
                }
            }
        }

        protected void OnClearFileHistory (object o, EventArgs a)
        {
            Debug.WriteLine (5, "Clearing file history");
            var temp = new ArrayList ();
            // Workaround for clear file history bug - set history to contain a single empty string
            temp.Add ("");
            filehandlingSettings.SetStrv ("file-history", (string[])temp.ToArray(typeof(string)));
            UpdateMenuFileHistory ();
        }

        protected void OnFileImportFolderActivated (object sender, EventArgs e)
        {
            var folderImportDialog = new FileChooserDialog ("Import folder...", window, FileChooserAction.SelectFolder);
            folderImportDialog.AddButton ("Cancel", ResponseType.Cancel);
            folderImportDialog.AddButton ("Ok", ResponseType.Ok);

            // query config for stored path
            folderImportDialog.SetCurrentFolder(filehandlingSettings.GetString("bib-import-path"));

            var response = (ResponseType)folderImportDialog.Run ();

            if (response == ResponseType.Ok) {
                string folderImportPath = folderImportDialog.Filename;
                string folderImportCurrentPath = folderImportDialog.CurrentFolder;

                folderImportDialog.Hide ();

                while (Gtk.Application.EventsPending ())
                    Gtk.Application.RunIteration ();

                filehandlingSettings.SetString ("bib-import-path", folderImportCurrentPath);
                Debug.WriteLine (5, "Importing folder: {0}", folderImportPath);

                InsertFilesInDir (folderImportPath);
            }

            folderImportDialog.Destroy ();

        }

        protected void OnToggleSideBarActivated (object sender, EventArgs e)
        {
            if (MenuViewSidebar.Active)
                scrolledwindowSidePaneScrolledWindow.Visible = true;
            else
                scrolledwindowSidePaneScrolledWindow.Visible = false;
        }

        protected void OnToggleRecordDetailsActivated (object sender, EventArgs e)
        {
            if (!MenuViewRecordDetails.Active) {
                recordDetailsView.Visible = false;
            } else {
                recordDetailsView.Visible = true;
            }
        }

        protected void OnToggleFullScreenActionActivated (object sender, EventArgs e)
        {

            if (MenuFullScreen.Active) {
                window.Fullscreen ();
            } else {
                window.Unfullscreen ();
            }
        }

        protected void OnRadioViewRecordsActivated (object sender, EventArgs e)
        {
            if (ViewRecordsAction.Active) {
                RecordView.Visible = true;
                RecordEditor.Visible = false;
                MainVpane.Position = MainVpane.MaxPosition - 180;
                ToggleEditRecords.Active = false;
            }
        }

        protected void OnRecordModified (object sender, EventArgs e)
        {
            ReconstructDetails();
        }

        protected void OnRadioEditRecordsActivated (object sender, EventArgs e)
        {
            if (EditRecordsAction.Active) {
                RecordView.Visible = false;
                RecordEditor.Visible = true;
                ReconstructDetails ();
                MainVpane.Position = MainVpane.MaxPosition - 350;
                ToggleEditRecords.Active = true;
            }
        }

        protected void OnChooseColumnsActivated (object sender, EventArgs e)
        {
            Dialog chooseColumnsDialog;

            chooseColumnsDialog = new BibliographerChooseColumns (litTreeView.Columns);
            chooseColumnsDialog.Run ();
            chooseColumnsDialog.Destroy ();
        }

        protected void OnLitTreeViewSelectionChanged (object o, EventArgs a)
        {
            //Console.WriteLine("Selection changed");
            TreeIter iter;
            ITreeModel model;

            if (((TreeSelection)o).GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                string recordType = record.RecordType;
                new_selected_record = true;
                if (BibtexRecordTypeLibrary.Contains (recordType))
                    comboRecordType.Active = BibtexRecordTypeLibrary.Index (recordType);
                else
                    comboRecordType.Active = -1;

                entryReqBibtexKey.Text = record.GetKey ();
                buttonBibtexKeyGenerate.Sensitive = false;
                new_selected_record = false;

                if (recordType == "comment") {
                    lblBibtexKey.Text = "Comment";
                    notebookFields.Visible = false;
                    buttonBibtexKeyGenerate.Visible = false;
                } else {
                    lblBibtexKey.Text = "BibTeX Key";
                    notebookFields.Visible = true;
                    buttonBibtexKeyGenerate.Visible = true;
                }

                // Interrogate ListStore for values
                // TODO: fix!
            } else {
                buttonBibtexKeyGenerate.Sensitive = false;
            }

            ReconstructTabs ();
            ReconstructDetails ();
        }

        protected void OnToggleEditRecordsActivated (object sender, EventArgs e)
        {
            if (ToggleEditRecords.Active) {
                EditRecordsAction.Active = true;
            } else {
                ViewRecordsAction.Active = true;
            }
        }

        protected void OnWindowDeleteEvent (object o, DeleteEventArgs args)
        {
            args.RetVal = true;
            Quit ();
        }

        protected void OnFilterEntryChanged (object sender, EventArgs e)
        {
            // Filter when the filter entry text has changed
            //System.Console.WriteLine("OnFilterEntryChanged");
            modelFilter.Refilter ();
        }

        protected void OnButtonBibtexKeyGenerateClicked (object sender, EventArgs e)
        {
            //System.Console.WriteLine("Generate a Bibtex Key");

            TreeIter iter;
            ITreeModel model;

            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                // TODO: check if this is the correct bibtexRecordFieldType
                // (it currently doesn't work for types that don't have an author
                // or year field)

                var record = (BibtexRecord)model.GetValue (iter, 0);
                string authors = record.HasField ("author") ? (record.GetField ("author").ToLower ()).Trim () : "";
                string year = record.HasField ("year") ? record.GetField ("year").Trim () : "";

                //System.Console.WriteLine("authors: " + authors);

                authors = authors.Replace (" and ", "&");
                string[] authorarray = authors.Split (("&").ToCharArray ());

                if (authorarray.Length > 0) {
                    var authorsurname = new StringArrayList ();
                    foreach (string author in authorarray) {
                        //System.Console.WriteLine(author);
                        // Deal with format of "Surname, Firstname ..."
                        if (author.IndexOf (",") > 0) {
                            string[] authorname = author.Split (',');
                            //System.Console.WriteLine("Surname: " + authorname[0]);
                            authorsurname.Add (authorname[0]);
                            // Deal with format of "Firstname ... Surname"
                        } else {
                            string[] authorname = author.Split (' ');
                            //System.Console.WriteLine("Surname: " + authorname[authorname.Length - 1]);
                            authorsurname.Add (authorname[authorname.Length - 1]);
                        }
                    }

                    string bibtexkey;

                    if (authorsurname.Count < 2) {
                        bibtexkey = authorsurname [0];
                    } else {
                        bibtexkey = authorsurname [0] + "_etal";
                    }
                    bibtexkey = bibtexkey + year;
                    // TODO: Check for and mitigate any duplication of keys

                    // Setting the bibtex key in the entry field and ListStore
                    entryReqBibtexKey.Text = bibtexkey;
                    record.SetKey (bibtexkey);
                    model.EmitRowChanged (model.GetPath (iter), iter);
                }
            }
            BibtexGenerateKeySetStatus ();
        }

        protected void OnLitTreeViewDragDataReceived (object o, DragDataReceivedArgs args)
        {
            // the atom type we want is a text/uri-list
            // if we get anything else, bounce it
            if (args.SelectionData.DataType.Name.CompareTo ("text/uri-list") != 0) {
                // wrong type
                return;
            }
            //DragDataReceivedArgs args
            string data = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data);
            //Split out multiple files

            string[] uri_list = System.Text.RegularExpressions.Regex.Split (data, "\r\n");
            TreePath path;
            TreeViewDropPosition drop_position;
            if (!litTreeView.GetDestRowAtPos (args.X, args.Y, out path, out drop_position)) {
                Debug.WriteLine (5, "Failed to drag and drop because of GetDestRowAtPos failure");
                return;
            }
            TreeIter iter;
            //, check;
            BibtexRecord record;
            //TreeModel model;
            if (!litTreeView.Model.GetIter (out iter, path)) {
                Debug.WriteLine (5, "Failed to drag and drop because of GetIter failure");
                return;
            }
            record = (BibtexRecord)litTreeView.Model.GetValue (iter, 0);
            //For each file
            foreach (string u in uri_list) {
                if (u.Length > 0 && u != "\0") {
                    Debug.WriteLine (5, "Associating file '" + u + "' with entry '" + record.GetKey () + "'");
                    record.SetField (BibtexRecord.BibtexFieldName.URI, u);
                    // TODO: disable debugging info
                    //Console.WriteLine("Importing: " + u);
                    //bibtexRecord record = new bibtexRecord(store, u);
                    record.DoiAdded += OnDOIRecordAdded;
                    //record.RecordModified += OnRecordModified;
                }
            }
            //if (litTreeView.Selection.GetSelected(out model, out check) && (iter == check))
            ReconstructTabs ();
            ReconstructDetails ();
        }

        protected void OnWindowSizeAllocated (object o, SizeAllocatedArgs a)
        {
            if (!windowSettings.GetBoolean ("window-maximized")) {
                windowSettings.SetInt ("window-width", a.Allocation.Width);
                windowSettings.SetInt ("window-height", a.Allocation.Height);
            }
        }

        protected void OnDOIRecordAdded (object o, EventArgs e)
        {
            var record = (BibtexRecord) o;
            am.SubscribeRecordForDOILookup (record);
        }

        protected void OnWindowStateChanged (object o, StateChangedArgs a)
        {
            Gdk.WindowState state = (Gdk.WindowState)window.State;

            if (state == Gdk.WindowState.Maximized) {
                Debug.WriteLine (10, "window has been maximized");
                windowSettings.SetBoolean ("window-maximized", true);
            } else {
                Debug.WriteLine (10, "window is not maixmized");
                windowSettings.SetBoolean ("window-maximized", false);
                window.Resize (windowSettings.GetInt ("window-width"), windowSettings.GetInt ("window-height"));
            }
        }

    }
}
