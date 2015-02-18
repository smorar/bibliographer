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
using libbibby;

namespace bibliographer
{

    public partial class BibliographerMainWindow : Gtk.Window
    {
        Gtk.Viewport viewportRequired;
        Gtk.Viewport viewportOptional;
        Gtk.Viewport viewportOther;
        Gtk.Viewport viewportBibliographerData;

        SearchEntry searchEntry;
        LitTreeView litTreeView;
        SidePaneTreeView sidePaneTreeView;

        BibtexRecords bibtexRecords;
        SidePaneTreeStore sidePaneStore;
        LitListStore litStore;
        Gtk.TreeModelFilter modelFilter;
        Gtk.TreeModelFilter fieldFilter;

        bool modified;
        bool new_selected_record;
        string file_name;
        string application_name;

        public AlterationMonitor am;

        static Gtk.TargetEntry[] target_table = { new Gtk.TargetEntry ("text/uri-list", 0, 0) };

        class FieldEntry : Gtk.Entry
        {
            public string field = "";
        }

        sealed class FieldButton : Gtk.Button
        {
            public string field = "";
        }

        public BibliographerMainWindow () : base(Gtk.WindowType.Toplevel)
        {
            var title = (System.Reflection.AssemblyTitleAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyTitleAttribute));
            application_name = title.Title;

            Build ();

            am = new AlterationMonitor ();

            // Set up main window defaults
            WidthRequest = 600;
            HeightRequest = 420;

            int width, height;

            if (Config.KeyExists ("window_width"))
                width = Config.GetInt ("window_width");
            else
                width = 600;
            if (Config.KeyExists ("window_height"))
                height = Config.GetInt ("window_height");
            else
                height = 420;
            Resize (width, height);
            if (Config.KeyExists ("window_maximized"))
			if (Config.GetBool ("window_maximized"))
				Maximize ();

            SetPosition (Gtk.WindowPosition.Center);

            viewportBibliographerData = new Gtk.Viewport ();
            viewportOptional = new Gtk.Viewport ();
            viewportOther = new Gtk.Viewport ();
            viewportRequired = new Gtk.Viewport ();

            viewportBibliographerData.BorderWidth = 2;
            viewportOptional.BorderWidth = 2;
            viewportOther.BorderWidth = 2;
            viewportRequired.BorderWidth = 2;

            scrolledwindowBibliographerData.Add (viewportBibliographerData);
            scrolledwindowOptional.Add (viewportOptional);
            scrolledwindowOther.Add (viewportOther);
            scrolledwindowRqdFields.Add (viewportRequired);

            // Main bibtex view list model
            bibtexRecords = new BibtexRecords ();

            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
            bibtexRecords.RecordURIAdded += OnBibtexRecordURIAdded;
            bibtexRecords.RecordURIModified += OnBibtexRecordURIModified;

            litStore = new LitListStore (bibtexRecords);

            modelFilter = new Gtk.TreeModelFilter (litStore, null);
            fieldFilter = new Gtk.TreeModelFilter (modelFilter, null);

            modelFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (ModelFilterListStore);
            fieldFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FieldFilterListStore);

            // Setup and add the LitTreeView
            litTreeView = new LitTreeView (fieldFilter);
            scrolledwindowTreeView.Add (litTreeView);
            // LitTreeView callbacks
            litTreeView.Selection.Changed += OnLitTreeViewSelectionChanged;
            litTreeView.DragDataReceived += OnLitTreeViewDragDataReceived;

            // Side Pane tree model
            sidePaneStore = new SidePaneTreeStore (bibtexRecords);
            sidePaneStore.SetSortColumnId (0, Gtk.SortType.Ascending);

            sidePaneTreeView = new SidePaneTreeView (sidePaneStore);
            scrolledwindowSidePane.Add (sidePaneTreeView);
            // SidePaneTreeView callbacks
            sidePaneTreeView.Selection.Changed += OnSidePaneTreeSelectionChanged;

            if (Config.KeyExists ("SideBar/visible")) {
                // Can't figure out how to get or set MenuItem viewSideBar's bool state, so
                // we just fire off an Activate event here instead.
                if (Config.GetBool ("SideBar/visible")) {
                    SidebarAction.Activate ();
                }
            }

            // Read cached sidePane width
            if (Config.KeyExists ("SideBar/width")) {
                var sidePane = (Gtk.HPaned)scrolledwindowSidePane.Parent;
                sidePane.Position = Config.GetInt ("SideBar/width");
            }

            // Set up comboRecordType items
            for (int i = 0; i < BibtexRecordTypeLibrary.Count (); i++)
                comboRecordType.InsertText (i, BibtexRecordTypeLibrary.GetWithIndex (i).name);

            // Set up drag and drop of files into litTreeView
            Gtk.Drag.DestSet (litTreeView, Gtk.DestDefaults.All, target_table, Gdk.DragAction.Copy);

            // Search entry
            searchEntry = new SearchEntry ();
            searchEntry.Changed += OnFilterEntryChanged;
            searchEntry.BorderWidth = 12;
            searchHbox.Add (searchEntry);

            UpdateMenuFileHistory ();

            // Activate new file

            FileNewAction.Activate ();
            EditRecordsAction.Activate ();
            ReconstructTabs ();
            ReconstructDetails ();
            // Now that we are configured, show the window
            Show ();
        }

        void BibtexGenerateKeySetStatus ()
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;

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

        bool FieldFilterListStore (Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            // TODO: add support for searching certain columns eg. Author / Journal / Title etc...
            // This is possible by implimenting a method such as record.searchField(field, text)

            var record = (BibtexRecord)model.GetValue (iter, 0);

            Gtk.TreeIter iterFilter;
            Gtk.TreeModel modelFilterListStore;

            if (sidePaneTreeView.Selection.GetSelected (out modelFilterListStore, out iterFilter)) {
                Gtk.TreeIter iterParent;
                string column, filter;
                if (((SidePaneTreeStore)modelFilterListStore).IterParent (out iterParent, iterFilter)) {
					column = ((SidePaneTreeStore)modelFilterListStore).GetValue (iterParent, 0) as string;
					filter = ((SidePaneTreeStore)modelFilterListStore).GetValue (iterFilter, 0) as string;
                    // Deal with authors
                    if (column.ToLower () == "author") {
                        //string authorstring = record.GetField(column.ToLower());
                        if (record != null) {
                            StringArrayList authors = record.GetAuthors ();
                            if (authors == null)
                                authors = new StringArrayList ();
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

        bool ModelFilterListStore (Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            BibtexSearchField sfield;
            sfield = BibtexSearchField.All;

            Gtk.Menu searchMenu = searchEntry.Menu;

            foreach (Gtk.RadioMenuItem menuItem in searchMenu.AllChildren) {
                if (menuItem.Active) {
                    sfield = (BibtexSearchField)menuItem.Data["searchField"];
                }
            }

			if (string.IsNullOrEmpty (searchEntry.InnerEntry.Text))
				return true;

            var record = (BibtexRecord)model.GetValue (iter, 0);

            if (record != null) {
				if (record.SearchRecord (searchEntry.InnerEntry.Text, sfield))
					return true;
				else {
					if ((sfield == BibtexSearchField.All) || (sfield == BibtexSearchField.Article)) {
						var index = (Tri)record.GetCustomDataField ("indexData");
						if (index != null) {
							//System.Console.WriteLine("Index tri data: " + index.ToString());
							if (index.IsSubString (searchEntry.InnerEntry.Text))
								return true;
						}
					}
				}
            }
            return false;
        }

        public void ReconstructDetails ()
        {
            // step 1: reset values
            recordIcon.Pixbuf = null;
            recordDetails.Text = null;

            // step 2: add new values
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
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
            // step 1: reset viewports
            viewportRequired.Forall (viewportRequired.Remove);
            viewportOptional.Forall (viewportOptional.Remove);
            viewportOther.Forall (viewportOther.Remove);
            viewportBibliographerData.Forall (viewportBibliographerData.Remove);

            // step 2: add stuff to the viewports
            var req = new Gtk.VBox (false, 5);
            var opt = new Gtk.VBox (false, 5);
            var other = new Gtk.VBox (false, 5);
            var bData = new Gtk.VBox (false, 5);

            Gtk.TreeIter iter;
            Gtk.TreeModel model;
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

                    // viewportRequired
                    var tableReq = new Gtk.Table (0, 2, false);
                    // TODO: process OR type fields
                    numItems = 0;
                    for (int i = 1; i < recordType.fields.Count; i++) {
                        int subNumItems = 0;
                        for (int j = 0; j < recordType.fields.Count; j++) {
                            if (recordType.optional[j] == i) {
                                subNumItems++;
                                if (subNumItems > 1) {
                                    numItems += 2;
                                    tableReq.NRows = numItems;
                                    var orLabel = new Gtk.Label ();
                                    orLabel.Markup = "<b>or</b>";
                                    tableReq.Attach (orLabel, 0, 2, numItems - 2, numItems - 1, 0, 0, 5, 5);
                                } else {
                                    numItems++;
                                    tableReq.NRows = numItems;
                                }
                                string fieldName = recordType.fields[j];
                                tableReq.Attach (new Gtk.Label (fieldName), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                                var textEntry = new FieldEntry ();
                                if (record.HasField (fieldName))
                                    textEntry.Text = record.GetField (fieldName);
                                tableReq.Attach (textEntry, 1, 2, numItems - 1, numItems, Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill, 0, 5, 5);
                                textEntry.field = fieldName;
                                textEntry.Changed += OnFieldChanged;
                            }
                        }
                        if (subNumItems == 0)
                            break;
                    }
                    req.PackStart (tableReq, false, false, 5);

                    //  viewportOptional
                    var tableOpt = new Gtk.Table (0, 2, false);
                    numItems = 0;
                    for (int i = 0; i < recordType.fields.Count; i++) {
                        if (recordType.optional[i] == 0) {
                            numItems++;
                            tableOpt.NRows = numItems;
                            tableOpt.Attach (new Gtk.Label (recordType.fields[i]), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                            var textEntry = new FieldEntry ();
                            if (record.HasField (recordType.fields[i]))
                                textEntry.Text = record.GetField (recordType.fields[i]);
                            tableOpt.Attach (textEntry, 1, 2, numItems - 1, numItems, Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill, 0, 5, 5);
                            textEntry.field = recordType.fields[i];
                            textEntry.Changed += OnFieldChanged;
                        }
                    }
                    opt.PackStart (tableOpt, false, false, 5);
                }

                // viewportOther
                var tableOther = new Gtk.Table (0, 3, false);
                numItems = 0;
                for (int i = 0; i < record.RecordFields.Count; i++) {
                    // doing this the hard way because we want to
                    // ignore case
                    bool found = false;
                    if (recordType != null)
                        for (int j = 0; j < recordType.fields.Count; j++)
                            if (String.Compare (((BibtexRecordField)record.RecordFields[i]).fieldName, recordType.fields[j], true) == 0) {
                                found = true;
                                break;
                            }
                    if (!found) {
                        // got one
                        string fieldName = ((BibtexRecordField)record.RecordFields[i]).fieldName;
                        bool inFieldLibrary = false;
                        for (int j = 0; j < BibtexRecordFieldTypeLibrary.Count (); j++)
                            if (String.Compare (fieldName, BibtexRecordFieldTypeLibrary.GetWithIndex (j).name, true) == 0) {
                                inFieldLibrary = true;
                                break;
                            }
                        numItems++;
                        tableOther.NRows = numItems;
                        var fieldLabel = new Gtk.Label ();
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
                        tableOther.Attach (textEntry, 1, 2, numItems - 1, numItems, Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill, 0, 5, 5);
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
                Gtk.ComboBox extraFields = Gtk.ComboBox.NewText ();
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
                    var hbox = new Gtk.HBox ();
                    hbox.PackStart (new Gtk.Label ("Add extra field:"), false, false, 5);
                    hbox.PackStart (extraFields, false, false, 5);
                    other.PackStart (hbox, false, false, 5);
                    extraFields.Changed += OnExtraFieldAdded;
                } else {
                    var noExtraFields = new Gtk.Label ();
                    noExtraFields.Markup = "<i>No extra fields</i>";
                    other.PackStart (noExtraFields, false, false, 5);
                }

                // viewportBibliographerData
                var uriHBox = new Gtk.HBox ();
                uriHBox.PackStart (new Gtk.Label ("Associated file:"), false, false, 5);
                var uriEntry = new FieldEntry ();
                if (record.HasURI())
                    uriEntry.Text = record.GetURI();
                uriEntry.field = BibtexRecord.BibtexFieldName.URI;
                uriEntry.Changed += OnFieldChanged;
                uriHBox.PackStart (uriEntry, false, false, 5);
                var uriBrowseButton = new Gtk.Button ("Browse");
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
            Title = application_name + " - " + file_name;
            modified = false;
        }

        void FileModified ()
        {
            //System.Console.WriteLine ("File modified setting file_name: {0}", file_name);
            Title = application_name + " - " + file_name + "*";
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
            am.SubscribeRecords (bibtexRecords);

            FileUnmodified ();
            // Disable editing of opened document
            ViewRecordsAction.Activate ();

            UpdateFileHistory (file_name);
        }

        Gtk.ResponseType FileSave ()
        {
            if (file_name != "Untitled.bib") {
                //BibtexListStoreParser btparser = new BibtexListStoreParser(
                //  store);
                //btparser.Save(file_name);
                bibtexRecords.Save (file_name);
                FileUnmodified ();

                return Gtk.ResponseType.Ok;
            } else {
                return FileSaveAs ();
            }
        }

        Gtk.ResponseType FileSaveAs ()
        {
            var fileSaveDialog = new Gtk.FileChooserDialog ("Save Bibtex file...", this, Gtk.FileChooserAction.Save, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);

            // TODO: filter for *.bib files only :)
            fileSaveDialog.Filter = new Gtk.FileFilter ();
            fileSaveDialog.Filter.AddPattern ("*.bib");

            if (Config.KeyExists ("bib_browse_path"))
                fileSaveDialog.SetCurrentFolder (Config.GetString ("bib_browse_path"));

            var response = (Gtk.ResponseType)fileSaveDialog.Run ();

            if (response == Gtk.ResponseType.Ok) {
                Config.SetString ("bib_browse_path", fileSaveDialog.CurrentFolder);
                if (fileSaveDialog.Filename != null) {
                    file_name = fileSaveDialog.Filename;
                    FileSave ();
                    UpdateFileHistory (file_name);
                    fileSaveDialog.Destroy ();
                    return Gtk.ResponseType.Ok;
                }
            }

            fileSaveDialog.Destroy ();
            return Gtk.ResponseType.Cancel;
        }

        void InsertFilesInDir (object o)
        {
            string dir = (string)o;
            string[] files = System.IO.Directory.GetFiles (dir);

            //double fraction = 1.0 / System.Convert.ToDouble(files.Length);

            foreach (string file in files) {
                while (Gtk.Application.EventsPending ())
                    Gtk.Application.RunIteration ();
                string uri = Gnome.Vfs.Uri.GetUriFromLocalPath (file);
				if (!bibtexRecords.HasURI (uri)) {
					Debug.WriteLine (5, "Adding new record with URI: {0}", uri);
					var record = new BibtexRecord ();
					bibtexRecords.Add (record);

					// Only set the uri field after the record has been added to bibtexRecords, so that the event is caught
					//System.Console.WriteLine("Setting URI: {0}", uri);
					record.SetURI (uri);
				}
            }
        }

        void UpdateRecordTypeCombo ()
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                string recordType = record.RecordType;
                new_selected_record = true;
                if (BibtexRecordTypeLibrary.Contains (recordType))
                    comboRecordType.Active = BibtexRecordTypeLibrary.Index (recordType);
                else
                    comboRecordType.Active = -1;

                comboRecordType.Activate ();
                entryReqBibtexKey.Text = record.GetKey ();
                buttonBibtexKeyGenerate.Sensitive = false;
                new_selected_record = false;
            }
        }

        void UpdateFileHistory (string filename)
        {
            if (!Config.KeyExists ("max_file_history_count"))
                Config.SetInt ("max_file_history_count", 5);

            int max = Config.GetInt ("max_file_history_count");
            if (max < 0) {
                max = 0;
                Config.SetInt ("max_file_history_count", max);
            }
            var tempHistory = (String[])Config.GetKey ("file_history");
            ArrayList history;
            if (tempHistory == null)
                history = new ArrayList ();
            else
                history = new ArrayList (tempHistory);

            //while (history.Count > max)
            //    history.RemoveAt(history.Count - 1);

            // check if this filename is already in our history
            // if so, it just gets bumped to top position,
            // otherwise bump everything down
            int bumpEnd;
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
            Config.SetKey ("file_history", history.ToArray ());

            UpdateMenuFileHistory ();
        }

        void UpdateMenuFileHistory ()
        {
            Debug.WriteLine (5, "File History - doing update...");

            var menus = menuBar.Children;
            foreach (Gtk.Widget menu in menus) {
                var menu_ = (Gtk.MenuItem)menu;
                if (menu_.Name == "FileAction") {
                    var file_menu = (Gtk.Menu)menu_.Submenu;
                    var file_menu_items = file_menu.Children;
                    foreach (Gtk.Widget file_menu_item in file_menu_items) {
                        var file_menu_item_ = (Gtk.MenuItem)file_menu_item;
                        if (file_menu_item_.Name == "RecentFilesAction") {
                            var recentFilesMenu = (Gtk.Menu)file_menu_item_.Submenu;
                            // step 1: clear the menu
                            foreach (Gtk.Widget w in recentFilesMenu) {
                                recentFilesMenu.Remove (w);
                            }

                            // step 2: add on items for history
                            bool gotOne = false;
                            if (Config.KeyExists ("max_file_history_count") && Config.KeyExists ("file_history")) {
                                object o = Config.GetKey ("file_history");
                                Debug.WriteLine (5, "{0}", o.GetType ());
                                var history = (String[])o;
                                for (int i = 0; i < history.Length; i++) {
                                    // Workaround for clearing history - check if history item is not an empty string
                                    if (history[i] != "") {
                                        string label = string.Format ("_{0} ", i + 1) + history [i];
                                        var item = new Gtk.MenuItem (label);
                                        item.Activated += OnFileHistoryActivate;
                                        item.Data.Add ("i", (IntPtr)i);
                                        recentFilesMenu.Append (item);
                                        gotOne = true;
                                    }
                                }
                            }
                            if (gotOne) {
                                recentFilesMenu.Append (new Gtk.SeparatorMenuItem ());
                                var accel = new Gtk.AccelGroup ();
                                var clear = new Gtk.ImageMenuItem ("gtk-clear", accel);

                                clear.Activated += OnClearFileHistory;
                                recentFilesMenu.Append (clear);
                            } else {
                                var none = new Gtk.MenuItem ("(none)");
                                // want to disable this somehow...
                                //none. = false;
                                recentFilesMenu.Append (none);
                            }

                            recentFilesMenu.ShowAll ();
                        }
                    }
                }
            }
        }

        void Quit ()
        {
            litTreeView.SaveColumnsState ();

            // Save sidebar visibility
            Config.SetBool ("SideBar/visible", scrolledwindowSidePane.Visible);
            var sidePane = (Gtk.HPaned)scrolledwindowSidePane.Parent;
            // Save sidebar width
            Config.SetInt ("SideBar/width", sidePane.Position);

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
            var dialog = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.None, "{0} has been modified. Do you want to save it?", file_name);
            dialog.AddButton ("Yes", Gtk.ResponseType.Yes);
            dialog.AddButton ("No", Gtk.ResponseType.No);
            dialog.AddButton ("Cancel", Gtk.ResponseType.Cancel);
            var msg_result = (Gtk.ResponseType)dialog.Run ();
            dialog.Destroy ();

            if (msg_result == Gtk.ResponseType.Yes) {
                Gtk.ResponseType save_result = FileSave ();
                if (save_result == Gtk.ResponseType.Ok)
                    return true; else if (save_result == Gtk.ResponseType.Cancel)
                    return false;
            } else if (msg_result == Gtk.ResponseType.No)
                return true; else if (msg_result == Gtk.ResponseType.Cancel)
                return false;
            else {
                var error = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok, "An unexpected error occurred in processing your response, please contact the developers!");
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
            if (am.Altered (record)) {
                //System.Console.WriteLine("Record altered: URI added");
                // Add record to get re-indexed
                am.SubscribeRecord (record);
            }
        }
        protected void OnBibtexRecordURIModified (object o, EventArgs a)
        {
            var record = (BibtexRecord)o;
            if (am.Altered (record)) {
                //System.Console.WriteLine("Record altered: URI modified");
                // Add record to get re-indexed
                am.SubscribeRecord (record);
            }
        }

        protected void OnComboRecordTypeChanged (object o, EventArgs a)
        {
            // the next check stops bad things from happening when
            // the user selects a new record in the list view,
            // causing the checkbox to get updated. In this case,
            // we really don't want to be calling this method
            if (new_selected_record)
                return;

            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                if (((Gtk.ComboBox)o).Active != -1) {
                    string bType = BibtexRecordTypeLibrary.GetWithIndex (((Gtk.ComboBox)o).Active).name;
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
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var combo = (Gtk.ComboBox)o;
                Gtk.TreeIter comboIter;
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
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var entry = (FieldEntry)o;
                var record = (BibtexRecord)model.GetValue (iter, 0);
                record.SetField (entry.field, entry.Text);
                model.EmitRowChanged (model.GetPath (iter), iter);
                BibtexGenerateKeySetStatus ();
            }
        }

        protected void OnFieldRemoved (object o, EventArgs a)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
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
            ab.Run ();
            ab.Destroy ();
        }

        protected void OnEntryReqBibtexKeyChanged (object sender, EventArgs e)
        {

            var bibtexEntry = (Gtk.Entry)sender;
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                record.SetKey (bibtexEntry.Text);
                model.EmitRowChanged (model.GetPath (iter), iter);
            }
        }

        protected void OnURIBrowseClicked (object o, EventArgs a)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected (out model, out iter)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);

                var fileOpenDialog = new Gtk.FileChooserDialog ("Associate file...", this, Gtk.FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);

                fileOpenDialog.Filter = new Gtk.FileFilter ();
                fileOpenDialog.Title = "Associate file...";
                fileOpenDialog.Filter.AddPattern ("*");

                if (record.HasField (BibtexRecord.BibtexFieldName.URI)) {
                    // If a file is associated with this directory, then open browse path in the files containing directory
                    fileOpenDialog.SetUri (record.GetURI ());
                } else if (Config.KeyExists ("uri_browse_path")) {
                    // Else, query config for stored path
                    fileOpenDialog.SetCurrentFolder (Config.GetString ("uri_browse_path"));
                }

                var result = (Gtk.ResponseType)fileOpenDialog.Run ();

                if (result == Gtk.ResponseType.Ok) {
                    record.SetField (BibtexRecord.BibtexFieldName.URI, Gnome.Vfs.Uri.GetUriFromLocalPath (fileOpenDialog.Filename));
                    Config.SetString (BibtexRecord.BibtexFieldName.URI, fileOpenDialog.CurrentFolder);
                    ReconstructTabs ();
                    ReconstructDetails ();
                }

                fileOpenDialog.Destroy ();
            }
        }

        protected void OnFileOpenActivated (object o, EventArgs a)
        {

            var fileOpenDialog = new Gtk.FileChooserDialog ("Open Bibtex File...", this, Gtk.FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);

            fileOpenDialog.Filter = new Gtk.FileFilter ();
            fileOpenDialog.Filter.AddPattern ("*.bib");

            // query config for stored path
            if (Config.KeyExists ("bib_browse_path"))
                fileOpenDialog.SetCurrentFolder (Config.GetString ("bib_browse_path"));

            var result = (Gtk.ResponseType)fileOpenDialog.Run ();

            if (result == Gtk.ResponseType.Ok)
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
            var item = (Gtk.MenuItem)o;
            int index = (int)Convert.ToUInt16 ((string)item.Data["i"].ToString ());
            if (Config.KeyExists ("max_file_history_count") && index >= 0 && index < Config.GetInt ("max_file_history_count") && Config.KeyExists ("file_history")) {
                var history = (string[])Config.GetKey ("file_history");
                if (index < history.Length) {
                    Debug.WriteLine (5, "Loading {0}", history[index]);
                    file_name = history[index];
                    FileOpen (file_name);
                    UpdateFileHistory (file_name);
                }
            }
        }

        protected void OnAddRecordActivated (object sender, EventArgs e)
        {
            Debug.WriteLine (5, "Adding a new record");
            //Debug.WriteLine(5, "Prev rec count: {0}", bibtexRecords.Count);
            Gtk.TreeIter litTreeViewIter, sidePaneIter;

            // Unfilter
            sidePaneStore.GetIterFirst (out sidePaneIter);
            sidePaneTreeView.SetCursor (sidePaneTreeView.Model.GetPath (sidePaneIter), sidePaneTreeView.GetColumn (0), false);

            // Clear search
            searchEntry.Clear ();

            if (bibtexRecords == null) {
                bibtexRecords = new BibtexRecords ();

                bibtexRecords.RecordsModified += OnBibtexRecordsModified;
                bibtexRecords.RecordURIAdded += OnBibtexRecordURIAdded;
                bibtexRecords.RecordURIModified += OnBibtexRecordURIModified;
            }

            var record = new BibtexRecord ();
            //System.Console.WriteLine ("Calling Add");
            bibtexRecords.Add (record);

            litTreeViewIter = litStore.GetIter (record);
            litTreeView.SetCursor (litTreeView.Model.GetPath (litTreeViewIter), litTreeView.GetColumn (0), false);

            BibtexGenerateKeySetStatus ();

            am.SubscribeRecord (record);
        }

        protected void OnRemoveRecordActivated (object sender, EventArgs e)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            Gtk.TreePath newpath;

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
                Gtk.TreeModel searchModel = ((Gtk.TreeModelSort)model).Model;
                Gtk.TreeIter searchIter = ((Gtk.TreeModelSort)model).ConvertIterToChildIter (iter);

                // Obtain the filter model
                Gtk.TreeModel filterModel = ((Gtk.TreeModelFilter)searchModel).Model;
                Gtk.TreeIter filterIter = ((Gtk.TreeModelFilter)searchModel).ConvertIterToChildIter (searchIter);

                // Obtain the real model
                Gtk.TreeModel realModel = ((Gtk.TreeModelFilter)filterModel).Model;
                Gtk.TreeIter realIter = ((Gtk.TreeModelFilter)filterModel).ConvertIterToChildIter (filterIter);

                // Delete record from the real model
                var record = ((Gtk.ListStore)realModel).GetValue (realIter, 0) as BibtexRecord;
                bibtexRecords.Remove (record);

            }
        }

        protected void OnAddRecordFromBibtexActivated (object sender, EventArgs e)
        {
            Gtk.TreeIter iter;

            var bibtexEntryDialog = new BibtexEntryDialog ();

            var result = (Gtk.ResponseType)bibtexEntryDialog.Run ();
            if (result == Gtk.ResponseType.Ok) {
                try {
                    var record = new BibtexRecord (bibtexEntryDialog.GetText ());
                    bibtexRecords.Add (record);

                    iter = litStore.GetIter (record);
                    litTreeView.Selection.SelectIter (iter);

                    BibtexGenerateKeySetStatus ();

                    am.SubscribeRecord (record);
                } catch (ParseException except) {
                    Debug.WriteLine (1, "Parse exception: {0}", except.GetReason ());
                }
            }

            bibtexEntryDialog.Destroy ();
        }

        protected void OnAddRecordFromClipboardActivated (object sender, EventArgs e)
        {
            Gtk.TreeIter iter;
            Gtk.Clipboard clipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("GDK_CLIPBOARD_SELECTION", false));
            string contents = clipboard.WaitForText ();
            var record = new BibtexRecord (contents);
            bibtexRecords.Add (record);

            iter = litStore.GetIter (record);
            litTreeView.Selection.SelectIter (iter);

            BibtexGenerateKeySetStatus ();

            am.SubscribeRecord (record);
        }

        protected void OnClearFileHistory (object o, EventArgs a)
        {
            Debug.WriteLine (5, "Clearing file history");
            var temp = new ArrayList ();
            // Workaround for clear file history bug - set history to contain a single empty string
            temp.Add ("");
            Config.SetKey ("file_history", temp.ToArray ());
            UpdateMenuFileHistory ();
        }

        protected void OnImportFolderActivated (object sender, EventArgs e)
        {
            var folderImportDialog = new Gtk.FileChooserDialog ("Import folder...", this, Gtk.FileChooserAction.SelectFolder, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);

            // query config for stored path
            if (Config.KeyExists ("bib_import_path"))
                folderImportDialog.SetCurrentFolder (Config.GetString ("bib_import_path"));

            var response = (Gtk.ResponseType)folderImportDialog.Run ();

            if (response == Gtk.ResponseType.Ok) {
                string folderImportPath = folderImportDialog.Filename;
                string folderImportCurrentPath = folderImportDialog.CurrentFolder;

                folderImportDialog.Hide ();

                while (Gtk.Application.EventsPending ())
                    Gtk.Application.RunIteration ();

                Config.SetString ("bib_import_path", folderImportCurrentPath);
                Debug.WriteLine (5, "Importing folder: {0}", folderImportPath);

                InsertFilesInDir (folderImportPath);
            }

            folderImportDialog.Destroy ();

        }

        protected void OnToggleSideBarActivated (object sender, EventArgs e)
        {
			if (SidebarAction.Active)
				scrolledwindowSidePane.Visible = true;
			else
				scrolledwindowSidePane.Visible = false;
        }

        protected void OnToggleRecordDetailsActivated (object sender, EventArgs e)
        {
			if (!RecordDetailsAction.Active) {
				recordDetailsView.Visible = false;
			} else {
				recordDetailsView.Visible = true;
			}
        }

        protected void OnToggleFullScreenActionActivated (object sender, EventArgs e)
        {

            if (FullScreenAction.Active) {
                Fullscreen ();
            } else {
                Unfullscreen ();
            }
        }

        protected void OnRadioViewRecordsActivated (object sender, EventArgs e)
        {
            if (ViewRecordsAction.Active) {
                recordView.Visible = true;
                recordEditor.Visible = false;
                vpane.Position = vpane.MaxPosition - 150;
                toggleEditRecords.Active = false;
            }
        }

        protected void OnRadioEditRecordsActivated (object sender, EventArgs e)
        {
            if (EditRecordsAction.Active) {
                recordView.Visible = false;
                recordEditor.Visible = true;
                ReconstructDetails ();
                vpane.Position = vpane.MaxPosition - 350;
                toggleEditRecords.Active = true;
            }
        }

        protected void OnChooseColumnsActivated (object sender, EventArgs e)
        {
            var chooseColumnsDialog = new BibliographerChooseColumns ();

            chooseColumnsDialog.ConstructDialog (litTreeView.Columns);
            chooseColumnsDialog.Run ();
            chooseColumnsDialog.Destroy ();
        }

        protected void OnLitTreeViewSelectionChanged (object o, EventArgs a)
        {
            //Console.WriteLine("Selection changed");
            Gtk.TreeIter iter;
            Gtk.TreeModel model;

            if (((Gtk.TreeSelection)o).GetSelected (out model, out iter)) {
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
            if (toggleEditRecords.Active) {
                EditRecordsAction.Active = true;
            } else {
                ViewRecordsAction.Active = true;
            }
        }

        protected void OnWindowDeleteEvent (object o, Gtk.DeleteEventArgs args)
        {
            args.RetVal = true;
            Quit ();
        }

        protected void OnFilterEntryChanged (object sender, EventArgs e)
        {
            // Filter when the filter entry text has changed
            modelFilter.Refilter ();
        }

        protected void OnButtonBibtexKeyGenerateClicked (object sender, EventArgs e)
        {
            //System.Console.WriteLine("Generate a Bibtex Key");

            Gtk.TreeIter iter;
            Gtk.TreeModel model;

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
                    var authorsurname = new ArrayList ();
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
                        bibtexkey = (string)(authorsurname.ToArray ())[0];
                    } else {
                        bibtexkey = (string)(authorsurname.ToArray ())[0] + "_etal";
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

        protected void OnLitTreeViewDragDataReceived (object o, Gtk.DragDataReceivedArgs args)
        {
            //Console.WriteLine("Data received is of type '" + args.SelectionData.Type.Name + "'");
            // the atom type we want is a text/uri-list
            // if we get anything else, bounce it
            if (args.SelectionData.Type.Name.CompareTo ("text/uri-list") != 0) {
                // wrong type
                return;
            }
            //DragDataReceivedArgs args
            string data = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data);
            //Split out multiple files

            string[] uri_list = System.Text.RegularExpressions.Regex.Split (data, "\r\n");
            Gtk.TreePath path;
            Gtk.TreeViewDropPosition drop_position;
            if (!litTreeView.GetDestRowAtPos (args.X, args.Y, out path, out drop_position)) {
                Debug.WriteLine (5, "Failed to drag and drop because of GetDestRowAtPos failure");
                return;
            }
            Gtk.TreeIter iter;
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
                if (u.Length > 0) {
                    Debug.WriteLine (5, "Associating file '" + u + "' with entry '" + record.GetKey () + "'");
                    record.SetField (BibtexRecord.BibtexFieldName.URI, u);
                    // TODO: disable debugging info
                    //Console.WriteLine("Importing: " + u);
                    //bibtexRecord record = new bibtexRecord(store, u);
                }
            }
            //if (litTreeView.Selection.GetSelected(out model, out check) && (iter == check))
            ReconstructTabs ();
            ReconstructDetails ();
        }

        protected void OnWindowSizeAllocated (object o, Gtk.SizeAllocatedArgs a)
        {
            if (!Config.GetBool ("window_maximized")) {
                Config.SetInt ("window_width", a.Allocation.Width);
                Config.SetInt ("window_height", a.Allocation.Height);
            }
        }

        protected void OnWindowStateChanged (object o, Gtk.WindowStateEventArgs a)
        {
            Gdk.EventWindowState gdk_event = a.Event;

            if (gdk_event.NewWindowState == Gdk.WindowState.Maximized) {
                Debug.WriteLine (10, "window has been maximized");
                Config.SetBool ("window_maximized", true);
            } else if (gdk_event.NewWindowState == 0) {
                Debug.WriteLine (10, "window is back to normal");
                Config.SetBool ("window_maximized", false);
                Resize (Config.GetInt ("window_width"), Config.GetInt ("window_height"));
            }
        }

    }
}
