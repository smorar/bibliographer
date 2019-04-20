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
using System.Collections.Generic;
using Gtk;
using libbibby;
using static bibliographer.Debug;

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
        protected Statusbar bibliographerStatusBar;

        protected ScrolledWindow scrolledwindowSidePaneScrolledWindow;

        protected CheckMenuItem MenuViewSidebar;
        protected CheckMenuItem MenuViewRecordDetails;
        protected CheckMenuItem MenuFullScreen;
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
        protected ScrolledWindow scrolledwindowTreeView;
        protected Notebook notebookFields;
        protected BibliographerSettings windowSettings;
        protected BibliographerSettings sidebarSettings;
        protected BibliographerSettings filehandlingSettings;

        //        bool modified;
        private bool new_selected_record;
        protected string file_name;
        protected string application_name;

        public AlterationMonitor am;

        private static TargetEntry [] target_table = { new TargetEntry ("text/uri-list", 0, 0) };

        private SuperTri searchData = new SuperTri ();
        private List<int> searchResult = new List<int> ();

        private class FieldEntry : Entry
        {
            public string field = "";
        }

        private sealed class FieldButton : Button
        {
            public string field = "";
        }

        public BibliographerMainWindow ()
        {
            System.Reflection.AssemblyTitleAttribute title;

            gui = new Builder ();
            System.IO.Stream guiStream = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibliographer.glade");
            try {
                System.IO.StreamReader reader = new System.IO.StreamReader (guiStream);
                gui.AddFromString (reader.ReadToEnd ());
                gui.Autoconnect (this);

                // TODO: move window and derived settings into apps.bibliographer.window
                windowSettings = new BibliographerSettings ("apps.bibliographer");
                sidebarSettings = new BibliographerSettings ("apps.bibliographer.sidebar");
                filehandlingSettings = new BibliographerSettings ("apps.bibliographer.filehandling");

                if (windowSettings.GetString ("data-directory") == "") {
                    // Set default data directory if none exists
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                        windowSettings.SetString ("data-directory", Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer");
                    } else {
                        windowSettings.SetString ("data-directory", Environment.GetEnvironmentVariable ("HOME") + "/.local/share/bibliographer/");
                    }
                }

                am = new AlterationMonitor ();

                string dataDirectory = windowSettings.GetString ("data-directory");
                DatabaseStoreStatic.Initialize (dataDirectory + "/" + "bibliographer.sqlite");

                // Set up main window defaults

                MenuItem MenuFileImportFolder;
                MenuItem MenuFileQuit;
                MenuItem MenuEditAddRecord;
                MenuItem MenuEditRemoveRecord;
                MenuItem MenuEditAddRecordFromBibtex;
                MenuItem MenuEditAddRecordClipboard;
                MenuItem MenuViewColumns;
                MenuItem MenuHelpAbout;

                ToolButton ToolAddRecord;
                ToolButton ToolRemoveRecord;


                Box searchHBox;

                window = (Window)gui.GetObject ("bibliographer.BibliographerMainWindow");
                MenuFileImportFolder = (MenuItem)gui.GetObject ("FileImportFolder");
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

                bibliographerStatusBar = (Statusbar)gui.GetObject ("bibliographerStatusBar");

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

                ToolAddRecord.Clicked += OnAddRecordActivated;
                ToolRemoveRecord.Clicked += OnRemoveRecordActivated;

                ToggleEditRecords.Clicked += OnToggleEditRecordsActivated;
                EditRecordsAction.Activated += OnRadioEditRecordsActivated;
                ViewRecordsAction.Activated += OnRadioViewRecordsActivated;

                buttonBibtexKeyGenerate.Activated += OnButtonBibtexKeyGenerateClicked;
                comboRecordType.Changed += OnComboRecordTypeChanged;

                window.DestroyEvent += OnWindowDestroyEvent;
                window.DeleteEvent += OnWindowDeleteEvent;
                window.StateChanged += OnWindowStateChanged;
                window.SizeAllocated += OnWindowSizeAllocated;

                window.Icon = new Gdk.Pixbuf (System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibliographer.png"));
                title = (System.Reflection.AssemblyTitleAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (),
                                                                     typeof (System.Reflection.AssemblyTitleAttribute));
                application_name = title.Title;

                window.WidthRequest = 800;
                window.HeightRequest = 600;

                int wdth, hght;

                wdth = windowSettings.GetInt ("window-width");
                hght = windowSettings.GetInt ("window-height");

                window.Resize (wdth, hght);

                if (windowSettings.GetBoolean ("window-maximized")) {
                    window.Maximize ();
                }

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
                for (int i = 0; i < BibtexRecordTypeLibrary.Count (); i++) {
                    comboRecordType.InsertText (i, BibtexRecordTypeLibrary.GetWithIndex (i).name);
                }

                // Set up drag and drop of files into litTreeView
                Drag.DestSet (litTreeView, DestDefaults.All, target_table, Gdk.DragAction.Copy);

                // Search entry
                searchEntry = new SearchEntry ();
                // TODO: Fix searchEntry
                //searchEntry.SearchChanged += OnFilterEntryChanged;
                //searchHBox.Add (searchEntry);
                Label tempLabel = new Label {
                    Text = "Search: "
                };
                searchHBox.Add (tempLabel);
                tempEntry = new Entry {
                    WidthChars = 40
                };
                //tempEntry.Activated += OnFilterEntryChanged;
                //tempEntry.FocusOutEvent += OnFilterEntryChanged;
                tempEntry.Changed += OnFilterEntryChanged;
                searchHBox.Add (tempEntry);
                Button tempButton = new Button {
                    Relief = ReliefStyle.None,
                    Label = "Clear"
                };
                tempButton.Clicked += TempButton_Clicked;
                searchHBox.Add (tempButton);
                searchHBox.Expand = true;

                LoadDatabase ();
                ToggleEditRecords.Activate ();
                ReconstructTabs ();
                ReconstructDetails ();
                // Now that we are configured, show the window
                window.ShowAll ();
            } catch (ArgumentNullException e) {
                WriteLine (0, "GUI configuration file not found.\n" + e.Message);
            }
        }

        private void TempButton_Clicked (object sender, EventArgs e)
        {
            tempEntry.Text = "";
            tempEntry.Activate ();
        }

        private void BibtexGenerateKeySetStatus ()
        {

            // Get selected bibtex record
            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {

                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                // Record has Author and Year fields
                if (record.HasField ("author") && record.HasField ("year")) {
                    // Author and year fields are not empty
                    if (!string.IsNullOrEmpty (record.GetField ("author")) && !string.IsNullOrEmpty (record.GetField ("year"))) {
                        // Bibtex Entry is empty
                        if (entryReqBibtexKey.Text == "" | entryReqBibtexKey.Text == null) {
                            buttonBibtexKeyGenerate.Sensitive = true;
                        } else {
                            // Bibtex Entry is not empty, so the generate key is not sensitive
                            buttonBibtexKeyGenerate.Sensitive = false;
                        }
                    } else {
                        // Author and year fields are empty
                        buttonBibtexKeyGenerate.Sensitive = false;
                    }
                } else {
                    // Record does not have Author and Year fields
                    buttonBibtexKeyGenerate.Sensitive = false;
                }
            } else {
                // A Bibtex record is not selected
                buttonBibtexKeyGenerate.Sensitive = false;
            }
        }

        private bool FieldFilterListStore (ITreeModel model, TreeIter iter)
        {
            // TODO: add support for searching certain columns eg. Author / Journal / Title etc...
            // This is possible by implimenting a method such as record.searchField(field, text)

            BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);

            if (sidePaneTreeView.Selection.GetSelected (out ITreeModel modelFilterListStore, out TreeIter iterFilter)) {
                string column, filter;
                if (((SidePaneTreeStore)modelFilterListStore).IterParent (out TreeIter iterParent, iterFilter)) {
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
                }
            }

            return true;
        }

        private bool ModelFilterListStore (ITreeModel model, TreeIter iter)
        {
            if (string.IsNullOrEmpty (tempEntry.Text)) {
                return true;
            }

            BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);

            WriteLine (10, "Records containing text {0} are:", tempEntry.Text);
            if (searchResult != null) {
                foreach (int res in searchResult) {
                    WriteLine (10, "{0}", res);
                }
                return searchResult.Contains (record.DbId ());
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
            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
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
            VBox req = new VBox (false, 5);
            VBox opt = new VBox (false, 5);
            VBox other = new VBox (false, 5);
            VBox bData = new VBox (false, 5);

            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
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

                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                uint numItems;
                BibtexRecordType recordType;
                if (BibtexRecordTypeLibrary.Contains (record.RecordType)) {
                    recordType = BibtexRecordTypeLibrary.Get (record.RecordType);

                    if (comboRecordType.ActiveText != record.RecordType) {
                        comboRecordType.Active = BibtexRecordTypeLibrary.Index (record.RecordType);
                    }

                    if (entryReqBibtexKey.Text == "") {
                        buttonBibtexKeyGenerate.Activate ();
                    }

                    // viewportRequired
                    Table tableReq = new Table (0, 2, false);
                    // TODO: process OR type fields
                    numItems = 0;
                    for (int i = 1; i < recordType.fields.Count; i++) {
                        int subNumItems = 0;
                        for (int j = 0; j < recordType.fields.Count; j++) {
                            if ((recordType.optional [j] == i) && (recordType.optional [j] != 0)) {
                                subNumItems++;
                                if (subNumItems > 1) {
                                    numItems += 2;
                                    tableReq.NRows = numItems;
                                    Label orLabel = new Label {
                                        Markup = "<b>or</b>"
                                    };
                                    tableReq.Attach (orLabel, 0, 2, numItems - 2, numItems - 1, 0, 0, 5, 5);
                                } else {
                                    numItems++;
                                    tableReq.NRows = numItems;
                                }
                                string fieldName = recordType.fields [j];
                                tableReq.Attach (new Label (fieldName), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                                FieldEntry textEntry = new FieldEntry ();
                                if (record.HasField (fieldName)) {
                                    textEntry.Text = record.GetField (fieldName);
                                }

                                tableReq.Attach (textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                                textEntry.field = fieldName;
                                textEntry.Activated += OnFieldChanged;
                                textEntry.FocusOutEvent += OnFieldChanged;
                                //textEntry.Changed += OnFieldChanged;
                            }
                        }
                        if (subNumItems == 0) {
                            break;
                        }
                    }
                    req.PackStart (tableReq, false, false, 5);

                    //  viewportOptional
                    Table tableOpt = new Table (0, 2, false);
                    numItems = 0;
                    for (int i = 0; i < recordType.fields.Count; i++) {
                        if (recordType.optional [i] == 0) {
                            numItems++;
                            tableOpt.NRows = numItems;
                            tableOpt.Attach (new Label (recordType.fields [i]), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                            FieldEntry textEntry = new FieldEntry ();
                            if (record.HasField (recordType.fields [i])) {
                                textEntry.Text = record.GetField (recordType.fields [i]);
                            }

                            tableOpt.Attach (textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                            textEntry.field = recordType.fields [i];
                            textEntry.Activated += OnFieldChanged;
                            textEntry.FocusOutEvent += OnFieldChanged;
                            //textEntry.Changed += OnFieldChanged;
                        }
                    }
                    opt.PackStart (tableOpt, false, false, 5);
                }

                // viewportOther
                //TODO: fix viewport other
                Table tableOther = new Table (0, 3, false);
                numItems = 0;
                if (record.recordFields != null) {
                    for (int i = 0; i < record.recordFields.Count; i++) {
                        // doing this the hard way because we want to
                        // ignore case
                        bool found = false;
                        if (BibtexRecordTypeLibrary.Contains (record.RecordType)) {
                            recordType = BibtexRecordTypeLibrary.Get (record.RecordType);
                            if (recordType != null) {
                                for (int j = 0; j < recordType.fields.Count; j++) {
                                    if (string.Compare (((BibtexRecordField)record.recordFields [i]).fieldName, recordType.fields [j], true) == 0) {
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!found) {
                            // got one
                            string fieldName = ((BibtexRecordField)record.recordFields [i]).fieldName;
                            bool inFieldLibrary = false;
                            for (int j = 0; j < BibtexRecordFieldTypeLibrary.Count (); j++) {
                                if (string.Compare (fieldName, BibtexRecordFieldTypeLibrary.GetWithIndex (j).name, true) == 0) {
                                    inFieldLibrary = true;
                                    break;
                                }
                            }

                            numItems++;
                            tableOther.NRows = numItems;
                            Label fieldLabel = new Label ();
                            if (inFieldLibrary) {
                                fieldLabel.Text = fieldName;
                            } else {
                                fieldLabel.Markup = "<span foreground=\"red\">" + fieldName + "</span>";
                            }

                            tableOther.Attach (fieldLabel, 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                            FieldEntry textEntry = new FieldEntry ();
                            if (record.HasField (fieldName)) {
                                textEntry.Text = record.GetField (fieldName);
                            }

                            textEntry.field = fieldName;
                            textEntry.Changed += OnFieldChanged;
                            tableOther.Attach (textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                            FieldButton removeButton = new FieldButton {
                                Label = "Remove field",
                                field = fieldName
                            };
                            removeButton.Clicked += OnFieldRemoved;
                            removeButton.Activated += OnFieldRemoved;
                            tableOther.Attach (removeButton, 2, 3, numItems - 1, numItems, 0, 0, 5, 5);
                        }
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
                    if (BibtexRecordTypeLibrary.Contains (record.RecordType)) {
                        recordType = BibtexRecordTypeLibrary.Get (record.RecordType);
                        if (recordType != null) {
                            for (int j = 0; j < recordType.fields.Count; j++) {
                                if (string.Compare (field, recordType.fields [j], true) == 0) {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (found) {
                        continue;
                    }

                    if (record.recordFields != null) {
                        for (int j = 0; j < record.recordFields.Count; j++) {
                            if (string.Compare (field, ((BibtexRecordField)record.recordFields [j]).fieldName, true) == 0) {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found) {
                        continue;
                    }

                    extraFields.AppendText (field);
                    comboAdded = true;
                }
                if (comboAdded) {
                    HBox hbox = new HBox ();
                    hbox.PackStart (new Label ("Add extra field:"), false, false, 5);
                    hbox.PackStart (extraFields, false, false, 5);
                    other.PackStart (hbox, false, false, 5);
                    extraFields.Changed += OnExtraFieldAdded;
                } else {
                    Label noExtraFields = new Label {
                        Markup = "<i>No extra fields</i>"
                    };
                    other.PackStart (noExtraFields, false, false, 5);
                }

                // viewportBibliographerData
                HBox uriHBox = new HBox ();
                uriHBox.PackStart (new Label ("Associated file:"), false, false, 5);
                FieldEntry uriEntry = new FieldEntry ();
                if (record.HasURI ()) {
                    uriEntry.Text = record.GetURI ();
                }

                uriEntry.field = "Filename";
                uriEntry.Activated += OnFieldChanged;
                uriEntry.FocusOutEvent += OnFieldChanged;
                uriHBox.PackStart (uriEntry, false, false, 5);
                Button uriBrowseButton = new Button ("Browse");
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

        private void InsertFilesInDir (object o)
        {
            string dir = (string)o;
            string [] files = System.IO.Directory.GetFiles (dir);

            //double fraction = 1.0 / System.Convert.ToDouble(files.Length);
            int num_files = files.Length;
            int num_processed = 0;
            foreach (string file in files) {
                while (Application.EventsPending ()) {
                    Application.RunIteration ();
                }

                string fileName = System.IO.Path.GetFileNameWithoutExtension (file);
                Uri fileUri = new Uri (file);
                if (!bibtexRecords.HasURI (fileUri.ToString ())) {
                    WriteLine (10, "Adding new record with URI: {0}", fileUri.ToString ());
                    BibtexRecord record = new BibtexRecord ();
                    record.DoiAdded += OnDOIRecordAdded;

                    bibtexRecords.Add (record);
                    record.RecordType = "misc";
                    // Set the title to the filename
                    record.SetField (BibtexRecord.BibtexFieldName.Title, fileName);
                    // Only set the uri field after the record has been added to bibtexRecords, so that the event is caught
                    record.SetURI (fileUri.ToString ());
                }
                num_processed += 1;
                UpdateStatusBarMessage (1, "Importing: " + num_processed + "/" + num_files + " files");
                WriteLine (5, "Inserted file: " + file);
            }
            UpdateStatusBarMessage (1, "");
        }

        private void UpdateRecordTypeCombo ()
        {

            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                string recordType = record.RecordType;
                new_selected_record = true;
                comboRecordType.Active = BibtexRecordTypeLibrary.Contains (recordType) ? BibtexRecordTypeLibrary.Index (recordType) : -1;

                comboRecordType.Activate ();
                entryReqBibtexKey.Text = record.GetKey ();
                buttonBibtexKeyGenerate.Sensitive = false;
                new_selected_record = false;
            }
        }

        private void Quit ()
        {
            litTreeView.SaveColumnsState ();

            // Save sidebar visibility
            sidebarSettings.SetBoolean ("visible", scrolledwindowSidePaneScrolledWindow.Visible);
            // Save sidebar width
            // TODO: This is disabled due to growing sidebar bug
            //sidebarSettings.SetInt("width", scrolledwindowSidePane.Position);

            Application.Quit ();
        }

        protected void LoadDatabase ()
        {
            am.FlushQueues ();

            bibtexRecords = new BibtexRecords ();

            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
            bibtexRecords.RecordURIAdded += OnBibtexRecordURIAdded;
            bibtexRecords.RecordURIModified += OnBibtexRecordURIModified;

            litStore.SetBibtexRecords (bibtexRecords);
            sidePaneStore.SetBibtexRecords (bibtexRecords);

            foreach (BibtexRecord record in bibtexRecords) {
                if (record.HasCustomDataField ("indexData")) {
                    if (record.GetCustomDataField ("indexData") != null) {
                        string indexData = (string)record.GetCustomDataField ("indexData");
                        Tri index = new Tri (indexData);
                        int id = record.DbId ();
                        searchData.AddTri (index, id);
                    }
                }
            }
        }

        /* -----------------------------------------------------------------
           CALLBACKS
           ----------------------------------------------------------------- */

        protected void OnBibtexRecordURIAdded (object o, EventArgs a)
        {
            BibtexRecord record = (BibtexRecord)o;
            WriteLine (10, "Subscribing new record to alteration thread for uri " + record.GetURI ());

            am.SubscribeAlteredRecord (record);
        }

        protected void OnBibtexRecordURIModified (object o, EventArgs a)
        {
            //TODO: Is this necessary? - the record with a previous URI should still be monitored by the alteration monitor
            BibtexRecord record = (BibtexRecord)o;
            WriteLine (10, "Subscribing updated record to alteration thread for uri " + record.GetURI ());

            am.SubscribeAlteredRecord (record);
        }

        protected void OnComboRecordTypeChanged (object o, EventArgs a)
        {
            // the next check stops bad things from happening when
            // the user selects a new record in the list view,
            // causing the checkbox to get updated. In this case,
            // we really don't want to be calling this method
            if (new_selected_record) {
                return;
            }

            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                if (((ComboBox)o).Active != -1) {
                    string bType = BibtexRecordTypeLibrary.GetWithIndex (((ComboBox)o).Active).name;
                    WriteLine (5, bType);
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
            UpdateRecordTypeCombo ();

            Gdk.Rectangle rect;
            rect = litTreeView.GetCellArea (null, litTreeView.GetColumn (1));
            litTreeView.Window.InvalidateRect (rect, true);

        }

        protected void OnExtraFieldAdded (object o, EventArgs a)
        {
            if (a == null) {
                throw new ArgumentNullException ();
            }

            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                ComboBox combo = (ComboBox)o;
                if (combo.GetActiveIter (out TreeIter comboIter)) {
                    BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                    record.SetField ((string)combo.Model.GetValue (comboIter, 0), "");
                    ReconstructTabs ();
                    ReconstructDetails ();
                }
            }
        }

        protected void OnFieldChanged (object o, EventArgs a)
        {
            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                FieldEntry entry = (FieldEntry)o;
                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                if (record.GetField (entry.field) != entry.Text) {
                    record.SetField (entry.field, entry.Text);
                    model.EmitRowChanged (model.GetPath (iter), iter);
                    BibtexGenerateKeySetStatus ();
                }
            }
        }

        protected void OnFieldRemoved (object o, EventArgs a)
        {
            if (a == null) {
                throw new ArgumentNullException ();
            }

            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                FieldButton button = (FieldButton)o;
                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                record.RemoveField (button.field);
                ReconstructTabs ();
                ReconstructDetails ();
            }
        }

        protected void OnFileQuitActivated (object sender, EventArgs e)
        {
            Quit ();
        }

        protected void OnHelpAboutActivated (object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox {
                TransientFor = window
            };
            ab.Run ();
            ab.Destroy ();
        }

        protected void OnEntryReqBibtexKeyChanged (object sender, EventArgs e)
        {
            if (e == null) {
                throw new ArgumentNullException ();
            }

            Entry bibtexEntry = (Entry)sender;
            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                record.SetKey (bibtexEntry.Text);
                model.EmitRowChanged (model.GetPath (iter), iter);
            }
        }

        protected void OnURIBrowseClicked (object o, EventArgs a)
        {
            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);

                FileChooserDialog fileOpenDialog = new FileChooserDialog ("Associate file...", window, FileChooserAction.Open);
                fileOpenDialog.AddButton ("Cancel", ResponseType.Cancel);
                fileOpenDialog.AddButton ("Ok", ResponseType.Ok);

                fileOpenDialog.Filter = new FileFilter ();
                fileOpenDialog.Title = "Associate file...";
                fileOpenDialog.Filter.AddPattern ("*");

                if (record.HasURI ()) {
                    // If a file is associated with this directory, then open browse path in the files containing directory
                    fileOpenDialog.SetUri (record.GetURI ());
                } else {
                    // Else, query config for stored path
                    if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                        fileOpenDialog.SetCurrentFolder (filehandlingSettings.GetString ("uri-browse-path"));
                    }
                }

                ResponseType result = (ResponseType)fileOpenDialog.Run ();

                if (result == ResponseType.Ok) {

                    Uri fileUri;
                    fileUri = new Uri (fileOpenDialog.Filename);
                    record.SetURI (fileUri.AbsoluteUri);
                    filehandlingSettings.SetString ("uri-browse-path", fileOpenDialog.CurrentFolder);
                    ReconstructTabs ();
                    ReconstructDetails ();

                }

                fileOpenDialog.Destroy ();
            }
        }

        protected void OnAddRecordActivated (object sender, EventArgs e)
        {
            WriteLine (5, "Adding a new record");
            //Debug.WriteLine(5, "Prev rec count: {0}", bibtexRecords.Count);

            // Unfilter
            sidePaneStore.GetIterFirst (out TreeIter sidePaneIter);
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

            BibtexRecord record = new BibtexRecord ();
            record.DoiAdded += OnDOIRecordAdded;
            bibtexRecords.Add (record);

            fieldFilter.IterNthChild (out TreeIter litTreeViewIter, fieldFilter.IterNChildren () - 1);
            litTreeView.SetCursor (fieldFilter.GetPath (litTreeViewIter), litTreeView.GetColumn (0), false);


            BibtexGenerateKeySetStatus ();

            am.SubscribeAlteredRecord (record);
        }

        protected void OnRemoveRecordActivated (object sender, EventArgs e)
        {
            TreePath newpath;

            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
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
                BibtexRecord record = ((ListStore)realModel).GetValue (realIter, 0) as BibtexRecord;
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
                    BibtexRecord record = new BibtexRecord (BibtexTextBuffer.Text);
                    bibtexRecords.Add (record);

                    iter = litStore.GetIter (record);
                    litTreeView.Selection.SelectIter (iter);

                    BibtexGenerateKeySetStatus ();

                    am.SubscribeAlteredRecord (record);
                } catch (ParseException except) {
                    WriteLine (1, "Parse exception: {0}", except.GetReason ());
                }
            }
            EntryDialog.Destroy ();
        }

        protected void OnAddRecordFromClipboardActivated (object sender, EventArgs e)
        {
            if (sender == null) {
                throw new ArgumentNullException ();
            }

            if (e == null) {
                throw new ArgumentNullException ();
            }

            TreeIter iter;
            Clipboard clipboard;

            clipboard = Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", true));
            if (clipboard.WaitIsTextAvailable ()) {
                try {
                    BibtexRecord record = new BibtexRecord (clipboard.WaitForText ());
                    bibtexRecords.Add (record);

                    iter = litStore.GetIter (record);
                    litTreeView.Selection.SelectIter (iter);

                    BibtexGenerateKeySetStatus ();

                    am.SubscribeAlteredRecord (record);
                } catch (ParseException except) {
                    WriteLine (1, "Parse exception: {0}", except.GetReason ());
                }
            }
        }

        protected void OnFileImportFolderActivated (object sender, EventArgs e)
        {
            FileChooserDialog folderImportDialog = new FileChooserDialog ("Import folder...", window, FileChooserAction.SelectFolder);
            folderImportDialog.AddButton ("Cancel", ResponseType.Cancel);
            folderImportDialog.AddButton ("Ok", ResponseType.Ok);

            // query config for stored path
            string importPath = filehandlingSettings.GetString ("bib-import-path");

            if (!System.IO.Directory.Exists (importPath)) {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                    folderImportDialog.SetCurrentFolder (importPath);
                }
            }

            ResponseType response = (ResponseType)folderImportDialog.Run ();

            if (response == ResponseType.Ok) {
                string folderImportPath = folderImportDialog.Filename;
                string folderImportCurrentPath = folderImportDialog.CurrentFolder;

                folderImportDialog.Hide ();

                while (Application.EventsPending ()) {
                    Application.RunIteration ();
                }

                filehandlingSettings.SetString ("bib-import-path", folderImportCurrentPath);
                WriteLine (5, "Importing folder: {0}", folderImportPath);

                InsertFilesInDir (folderImportPath);
            }

            folderImportDialog.Destroy ();
            WriteLine (5, "folderImportDialog destroyed.");
        }

        protected void OnToggleSideBarActivated (object sender, EventArgs e)
        {
            scrolledwindowSidePaneScrolledWindow.Visible = MenuViewSidebar.Active;
        }

        protected void OnToggleRecordDetailsActivated (object sender, EventArgs e)
        {
            recordDetailsView.Visible = MenuViewRecordDetails.Active;
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

            if (((TreeSelection)o).GetSelected (out ITreeModel model, out TreeIter iter)) {
                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                string recordType = record.RecordType;
                new_selected_record = true;
                comboRecordType.Active = BibtexRecordTypeLibrary.Contains (recordType) ? BibtexRecordTypeLibrary.Index (recordType) : -1;

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

        protected void OnWindowDestroyEvent (object o, DestroyEventArgs args)
        {
            args.RetVal = true;
            Quit ();
        }

        protected void OnWindowDeleteEvent (object o, DeleteEventArgs args)
        {
            args.RetVal = true;
            Quit ();
        }

        protected void OnFilterEntryChanged (object sender, EventArgs e)
        {
            WriteLine (5, "OnFilterEntryChanged");
            if (!string.IsNullOrEmpty (tempEntry.Text)) {
                searchResult = searchData.ContainsRecords (tempEntry.Text);
            } else {
                searchResult.Clear ();
            }
            modelFilter.Refilter ();
            searchResult.Clear ();
        }

        protected void OnButtonBibtexKeyGenerateClicked (object sender, EventArgs e)
        {
            WriteLine (5, "Generate a Bibtex Key");


            if (litTreeView.Selection.GetSelected (out ITreeModel model, out TreeIter iter)) {
                // TODO: check if this is the correct bibtexRecordFieldType
                // (it currently doesn't work for types that don't have an author
                // or year field)

                BibtexRecord record = (BibtexRecord)model.GetValue (iter, 0);
                string authors = record.HasField ("author") ? record.GetField ("author").ToLower ().Trim () : "";
                string year = record.HasField ("year") ? record.GetField ("year").Trim () : "";
                WriteLine (5, "authors: " + authors);

                authors = authors.Replace (" and ", "&");
                string [] authorarray = authors.Split ("&".ToCharArray ());

                if (authorarray.Length > 0) {
                    StringArrayList authorsurname = new StringArrayList ();
                    foreach (string author in authorarray) {
                        WriteLine (5, author);
                        // Deal with format of "Surname, Firstname ..."
                        if (author.IndexOf (",", StringComparison.CurrentCulture) > 0) {
                            string [] authorname = author.Split (',');
                            WriteLine (5, "Surname: " + authorname [0]);
                            authorsurname.Add (authorname [0]);
                            // Deal with format of "Firstname ... Surname"
                        } else {
                            string [] authorname = author.Split (' ');
                            WriteLine (5, "Surname: " + authorname [authorname.Length - 1]);
                            authorsurname.Add (authorname [authorname.Length - 1]);
                        }
                    }

                    string bibtexkey = authorsurname.Count < 2 ? authorsurname [0] : authorsurname [0] + "_etal";
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
            if (string.Compare (args.SelectionData.DataType.Name, "text/uri-list", StringComparison.CurrentCultureIgnoreCase) != 0) {
                // wrong type
                return;
            }
            //DragDataReceivedArgs args
            string data = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data);
            //Split out multiple files

            string [] uri_list = System.Text.RegularExpressions.Regex.Split (data, "\r\n");
            if (!litTreeView.GetDestRowAtPos (args.X, args.Y, out TreePath path, out TreeViewDropPosition drop_position)) {
                WriteLine (5, "Failed to drag and drop because of GetDestRowAtPos failure");
                return;
            }
            //, check;
            BibtexRecord record;
            //TreeModel model;
            if (!litTreeView.Model.GetIter (out TreeIter iter, path)) {
                WriteLine (5, "Failed to drag and drop because of GetIter failure");
                return;
            }
            record = (BibtexRecord)litTreeView.Model.GetValue (iter, 0);
            //For each file
            foreach (string u in uri_list) {
                if (u.Length > 0 && u != "\0") {
                    WriteLine (5, "Associating file '" + u + "' with entry '" + record.GetKey () + "'");
                    record.SetURI (u);
                    WriteLine (5, "Importing: " + u);
                    record.DoiAdded += OnDOIRecordAdded;
                }
            }
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
            BibtexRecord record = (BibtexRecord)o;
            LookupRecordData.LookupDOIData (record);
            am.SubscribeRecordForDOILookup (record);
        }

        protected void OnWindowStateChanged (object o, StateChangedArgs a)
        {
            Gdk.WindowState state = (Gdk.WindowState)window.State;

            if (state == Gdk.WindowState.Maximized) {
                WriteLine (10, "window has been maximized");
                windowSettings.SetBoolean ("window-maximized", true);
            } else {
                WriteLine (10, "window is not maixmized");
                windowSettings.SetBoolean ("window-maximized", false);
                window.Resize (windowSettings.GetInt ("window-width"), windowSettings.GetInt ("window-height"));
            }
        }

        protected void UpdateStatusBarMessage (uint context_id, string message)
        {
            bibliographerStatusBar.RemoveAll (context_id);
            bibliographerStatusBar.Push (context_id, message);
        }

    }
}
