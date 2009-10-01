// 
//  Copyright (C) 2005-2009 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
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
using System.Threading;

namespace bibliographer
{
    
    public partial class BibliographerMainWindow : Gtk.Window
    {
        Gtk.Viewport viewportRequired;
        Gtk.Viewport viewportOptional;
        Gtk.Viewport viewportOther;
        Gtk.Viewport viewportBibliographerData;
        
        private BibtexRecords bibtexRecords;
        private SidePaneTreeStore sidePaneStore;
        private LitListStore litStore;
        private Gtk.TreeModelFilter modelFilter;
        private Gtk.TreeModelFilter fieldFilter;
        private Gtk.TreeModelSort sorter;

        private bool modified;
        private bool new_selected_record;
        private string file_name;
        private string application_name;
        
        public System.Threading.Thread indexerThread, alterationMonitorThread;
        private Queue indexerQueue, alterationMonitorQueue;
        
        private static Gtk.TargetEntry []    target_table =
            new Gtk.TargetEntry [] {
            new Gtk.TargetEntry ("text/uri-list", 0, 0),
        };
        
        class FieldEntry : Gtk.Entry
        {
            public string field = "";
        };
    
        class FieldButton : Gtk.Button
        {
            public string field = "";
        }
        
        public BibliographerMainWindow() : base(Gtk.WindowType.Toplevel)
        {
            
            System.Reflection.AssemblyTitleAttribute title = (System.Reflection.AssemblyTitleAttribute) 
                Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(),
                                             typeof(System.Reflection.AssemblyTitleAttribute));
            application_name = title.Title;

            this.Build();
            
            indexerThread = new System.Threading.Thread(new ThreadStart(IndexerThread));
            indexerQueue = new Queue();
    
            alterationMonitorThread = new System.Threading.Thread(new ThreadStart(AlterationMonitorThread));
            alterationMonitorQueue = new Queue();

            // LitTreeView callbacks
            litTreeView.Selection.Changed += OnTreeViewSelectionChanged;
            
            // Set up main window defaults
            this.WidthRequest = 600;
            this.HeightRequest = 600;
    
            int width, height;
    
            if (Config.KeyExists("window_width"))
                width = Config.GetInt("window_width");
            else
                width = 600;
            if (Config.KeyExists("window_height"))
                height = Config.GetInt("window_height");
            else
                height = 600;
            this.Resize(width, height);
            if (Config.KeyExists("window_maximized"))
                if (Config.GetBool("window_maximized") == true)
                    this.Maximize();
    
            this.SetPosition(Gtk.WindowPosition.Center);
            //this.Title = application_name;
            //this.Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("bibliographer.png"));
    
            viewportBibliographerData = new Gtk.Viewport();
            viewportOptional = new Gtk.Viewport();
            viewportOther = new Gtk.Viewport();
            viewportRequired = new Gtk.Viewport();

            viewportBibliographerData.BorderWidth = 2;
            viewportOptional.BorderWidth = 2;
            viewportOther.BorderWidth = 2;
            viewportRequired.BorderWidth = 2;
            
            scrolledwindowBibliographerData.Add(viewportBibliographerData);
            scrolledwindowOptional.Add(viewportOptional);
            scrolledwindowOther.Add(viewportOther);
            scrolledwindowRqdFields.Add(viewportRequired);
            
            // Main bibtex view list model
            bibtexRecords = new BibtexRecords();
            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
            litStore = new LitListStore(bibtexRecords);
    
            modelFilter = new Gtk.TreeModelFilter(litStore, null);
            fieldFilter = new Gtk.TreeModelFilter(modelFilter, null);
    
            modelFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (ModelFilterListStore);
            fieldFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FieldFilterListStore);
    
            sorter = new Gtk.TreeModelSort(fieldFilter);
    
            litTreeView.Model = sorter;
    
            // TODO: Perform this more elegantly
            // Possibly, read out fields from the bibtex record spec file
            // and make a certain set of columns visible by default
            Gtk.TreeViewColumn [] columnarray;
            columnarray = new Gtk.TreeViewColumn[8];
    
            if (Config.KeyExists("Columns/Author/order") && Config.KeyExists("Columns/Title/order") && Config.KeyExists("Columns/Year/order") && Config.KeyExists("Columns/Journal/order") && Config.KeyExists("Columns/Bibtex Key/order") && Config.KeyExists("Columns/Volume/order") && Config.KeyExists("Columns/Pages/order"))
            {
                columnarray[Config.GetInt("Columns/Icon/order")] = new Gtk.TreeViewColumn("Icon", new Gtk.CellRendererPixbuf(), "image");
                columnarray[Config.GetInt("Columns/Author/order")] = new Gtk.TreeViewColumn("Author", new Gtk.CellRendererText(), "text");
                columnarray[Config.GetInt("Columns/Title/order")] = new Gtk.TreeViewColumn("Title", new Gtk.CellRendererText(), "text");
                columnarray[Config.GetInt("Columns/Year/order")] = new Gtk.TreeViewColumn("Year", new Gtk.CellRendererText(), "text");
                columnarray[Config.GetInt("Columns/Journal/order")] = new Gtk.TreeViewColumn("Journal", new Gtk.CellRendererText(), "text");
                columnarray[Config.GetInt("Columns/Bibtex Key/order")] = new Gtk.TreeViewColumn("Bibtex Key", new Gtk.CellRendererText(), "text");
                columnarray[Config.GetInt("Columns/Volume/order")] = new Gtk.TreeViewColumn("Volume", new Gtk.CellRendererText(), "text");
                columnarray[Config.GetInt("Columns/Pages/order")] = new Gtk.TreeViewColumn("Pages", new Gtk.CellRendererText(), "text");
            }
            else
            {
                columnarray[0] = new Gtk.TreeViewColumn("Icon", new Gtk.CellRendererPixbuf(), "image");
                columnarray[1] = new Gtk.TreeViewColumn("Author", new Gtk.CellRendererText(), "text");
                columnarray[2] = new Gtk.TreeViewColumn("Title", new Gtk.CellRendererText(), "text");
                columnarray[3] = new Gtk.TreeViewColumn("Year", new Gtk.CellRendererText(), "text");
                columnarray[4] = new Gtk.TreeViewColumn("Journal", new Gtk.CellRendererText(), "text");
                columnarray[5] = new Gtk.TreeViewColumn("Bibtex Key", new Gtk.CellRendererText(), "text");
                columnarray[6] = new Gtk.TreeViewColumn("Volume", new Gtk.CellRendererText(), "text");
                columnarray[7] = new Gtk.TreeViewColumn("Pages", new Gtk.CellRendererText(), "text");
            }
    
            foreach (Gtk.TreeViewColumn column in columnarray)
            {
                litTreeView.AppendColumn(column);
            }
            litTreeView.HeadersClickable = true;
    
            Gtk.TreeCellDataFunc textDataFunc = new Gtk.TreeCellDataFunc(RenderColumnTextFromBibtexRecord);
            Gtk.TreeCellDataFunc pixmapDataFunc = new Gtk.TreeCellDataFunc(RenderColumnPixbufFromBibtexRecord);
            int id = 0;
    
            foreach (Gtk.TreeViewColumn column in litTreeView.Columns)
            {
                // Check for persistant settings
                // Checking for column visibility
                if (Config.KeyExists("Columns/"+column.Title+"/visible"))
                {
                    column.Visible = Config.GetBool("Columns/"+column.Title+"/visible");
                }
                else
                {
                    // Default visible columns
                    string[] defaultVisible = {"Icon", "Author", "Title", "Year", "Journal", "Bibtex Key", "Volume", "Pages"};
    
                    if (System.Array.IndexOf(defaultVisible, column.Title) >= 0)
                    {
                        column.Visible = true;
                        //Config.SetBool("Columns/"+column.Title+"/visible",true);
                    }
                    else
                    {
                        column.Visible = false;
                        //Config.SetBool("Columns/"+column.Title+"/visible",false);
                    }
                }
                if (column.Title == "Icon")
                {
                    column.SetCellDataFunc(column.CellRenderers[0], pixmapDataFunc);
                    column.Sizing = Gtk.TreeViewColumnSizing.Autosize;
                    column.Expand = false;
                    column.Resizable = false;
                    column.Reorderable = false;
                    column.Clickable = false;
                }
                else if (column.Title == "Author")
                {
                    column.SetCellDataFunc(column.CellRenderers[0], textDataFunc);
                    column.Reorderable = true;
                    // Checking for column widths
                    column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
                    column.Resizable = true;
                    if (Config.KeyExists("Columns/"+column.Title+"/width"))
                        column.FixedWidth = Config.GetInt("Columns/"+column.Title+"/width");
                    else
                        column.FixedWidth = 100;
                    column.Clickable = true;
                    column.SortColumnId = id;
                    sorter.SetSortFunc(id, StringCompareAuthor);
                    column.Clicked += OnColumnSort;
                }
                else
                {
                    column.SetCellDataFunc(column.CellRenderers[0], textDataFunc);
                    column.Reorderable = true;
                    // Checking for column widths
                    column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
                    column.Resizable = true;
                    if (Config.KeyExists("Columns/"+column.Title+"/width"))
                        column.FixedWidth = Config.GetInt("Columns/"+column.Title+"/width");
                    else
                        column.FixedWidth = 100;
                    column.Clickable = true;
                    column.SortColumnId = id;
                    sorter.SetSortFunc(id, StringCompare);
                    column.Clicked += OnColumnSort;
                }
                id++;
            }
    
            // Side Pane tree model
            sidePaneStore = new SidePaneTreeStore(bibtexRecords);
            sidePaneStore.SetSortColumnId(0, Gtk.SortType.Ascending);
    
            sidePaneTreeView.Model = sidePaneStore;
    
            Gtk.TreeCellDataFunc filterTextDataFunc = new Gtk.TreeCellDataFunc(RenderFilterColumnTextFromBibtexRecords);
            Gtk.TreeViewColumn col = new Gtk.TreeViewColumn("Filter", new Gtk.CellRendererText(), "text");
    
            col.SetCellDataFunc(col.CellRenderers[0], filterTextDataFunc);
            col.Sizing = Gtk.TreeViewColumnSizing.Autosize;
    
            sidePaneTreeView.AppendColumn(col);
    
            sidePaneTreeView.Selection.Changed += OnSidePaneTreeSelectionChanged;
    
            if (Config.KeyExists("SideBar/visible"))
            {
                // Can't figure out how to get or set MenuItem viewSideBar's bool state, so
                // we just fire off an Activate event here instead.
                if (Config.GetBool("SideBar/visible") == true)
                {
                    SidebarAction.Activate();
                }
            }
    
            // Read cached sidePane width
            if (Config.KeyExists("SideBar/width"))
            {
                Gtk.HPaned hpane = (Gtk.HPaned)scrolledwindowSidePane.Parent;
                hpane.Position = Config.GetInt("SideBar/width");
            }
    
            Gtk.TreePath path = sidePaneStore.GetPathAll();
            sidePaneTreeView.SetCursor(path, col, false);
    
            // Set up comboRecordType items
            for (int i = 0; i < BibtexRecordTypeLibrary.Count(); i++)
                comboRecordType.InsertText(i, BibtexRecordTypeLibrary.GetWithIndex(i).name);
    
            // Set up drag and drop of files into litTreeView
            Gtk.Drag.DestSet (litTreeView, Gtk.DestDefaults.All, target_table,
                              Gdk.DragAction.Copy);

            UpdateMenuFileHistory();
    
            // Activate new file

            FileNewAction.Activate();
            EditRecordsAction.Activate();
            ReconstructTabs();
            ReconstructDetails();
            // Now that we are configured, show the window
            this.Show();
        }

        private void BibtexGenerateKeySetStatus()
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
    
            // Get selected bibtex record
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
    
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                // Record has Author and Year fields
                if (record.HasField("author") && record.HasField("year"))
                {
                    // Author and year fields are not empty
                    if (!(record.GetField("author") == "" || record.GetField("author") == null) && !(record.GetField("year") == "" || record.GetField("year") == null))
                    {
                        // Bibtex Entry is empty
                        if (entryReqBibtexKey.Text == "" | entryReqBibtexKey.Text == null)
                            buttonBibtexKeyGenerate.Sensitive = true;
                        // Bibtex Entry is not empty, so the generate key is not sensitive
                        else
                            buttonBibtexKeyGenerate.Sensitive = false;
                    }
                    // Author and year fields are empty
                    else
                        buttonBibtexKeyGenerate.Sensitive = false;
                }
                // Record does not have Author and Year fields
                else
                    buttonBibtexKeyGenerate.Sensitive = false;
            }
            // A Bibtex record is not selected
            else
                buttonBibtexKeyGenerate.Sensitive = false;
        }
        
        public void IndexerThread()
        {
            Debug.WriteLine(5, "Indexer thread started");
            try {
                do {
                    System.Threading.Monitor.Enter(indexerQueue);
                    while (indexerQueue.Count > 0) {
                        BibtexRecord record = (BibtexRecord) indexerQueue.Dequeue();
                        System.Threading.Monitor.Exit(indexerQueue);
                        record.Index();
                        System.Threading.Monitor.Enter(indexerQueue);
                    }
                    System.Threading.Monitor.Exit(indexerQueue);
                    System.Threading.Thread.Sleep(100);
                } while (true);
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine(5, "Indexer thread terminated");
            }
        }
        
        public void AlterationMonitorThread() {
            Debug.WriteLine(5, "Alteration monitor thread started");
            try {
                do {
                    System.Threading.Monitor.Enter(alterationMonitorQueue);
                    while (alterationMonitorQueue.Count > 0) {
                        BibtexRecord record = (BibtexRecord) alterationMonitorQueue.Dequeue();
                        System.Threading.Monitor.Exit(alterationMonitorQueue);
                        // FIXME: do the alteration monitoring stuff
                        // FIXME: if continuous monitoring is enabled, then
                        // the entry should be requeued
                        if (record.Altered()) {
                            System.Threading.Monitor.Enter(indexerQueue);
                            indexerQueue.Enqueue(record);
                            System.Threading.Monitor.Exit(indexerQueue);
                        }
                        System.Threading.Thread.Sleep(100);
                        System.Threading.Monitor.Enter(alterationMonitorQueue);
                        alterationMonitorQueue.Enqueue(record);
                    }
                    System.Threading.Monitor.Exit(alterationMonitorQueue);
                    System.Threading.Thread.Sleep(100);
                } while (true);
            }
            catch (ThreadAbortException) {
                Debug.WriteLine(5, "Alteration monitor thread terminated");
            }
        }
        
         private bool FieldFilterListStore(Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            // TODO: add support for searching certain columns eg. Author / Journal / Title etc...
            // This is possible by implimenting a method such as record.searchField(field, text)
    
            BibtexRecord record = (BibtexRecord) model.GetValue (iter, 0);
    
            Gtk.TreeIter iterFilter;
            Gtk.TreeModel modelFilter;
    
            if (sidePaneTreeView.Selection.GetSelected(out modelFilter, out iterFilter))
            {
                Gtk.TreeIter iterParent;
                string column, filter;
                if (((SidePaneTreeStore) modelFilter).IterParent(out iterParent, iterFilter))
                {
                    column = ((SidePaneTreeStore) modelFilter).GetValue(iterParent, 0) as string;
                    filter = ((SidePaneTreeStore) modelFilter).GetValue(iterFilter, 0) as string;
                    // Deal with authors
                    if (column.ToLower() == "author")
                    {
                        //string authorstring = record.GetField(column.ToLower());
                        if (record != null)
                        {
                            StringArrayList authors = record.GetAuthors();
                            if (authors == null)
                                authors = new StringArrayList();
                            if (authors.Contains(filter))
                                return true;
                            else
                                return false;
                        }
                    }
                    // Deal with other fields
                    else
                    {
                        if (record != null)
                        {
                            if (record.GetField(column.ToLower()) == filter)
                                return true;
                            else
                                return false;
                        }
                    }
                    //System.Console.WriteLine(column + " -> " + filter);
                }
            }
    
            //if (record.searchField(
            return true;
        }
    
        private bool ModelFilterListStore(Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            if (searchEntry.Text == "" || searchEntry.Text == null)
                return true;
    
            BibtexRecord record = (BibtexRecord) model.GetValue (iter, 0);
    
            if (record != null)
            {
                if (record.SearchRecord(searchEntry.Text) == true)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        
        private void RenderColumnPixbufFromBibtexRecord(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            if (model != null)
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
    
                //if ((cell != null) && (record != null) && (record.GetSmallThumbnail() != null))
                if ((cell != null) && (record != null) && (record.HasURI()))
                {
                    (cell as Gtk.CellRendererPixbuf).Pixbuf = record.GetSmallThumbnail();
                }
            }
        }
    
        private void RenderColumnTextFromBibtexRecord(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            // See here for an example of how you can highlight cells
            // based on something todo with the entry
            //
            // TODO: extend this feature to highlight entries that
            // are missing required fields
    
            if ((model != null) && (column != null) && (column.Title != null))
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                if (record != null)
                {
                    if (record.HasField(column.Title) && column.Title != "Author")
                    {
                        (cell as Gtk.CellRendererText).Text = record.GetField(column.Title);
                        (cell as Gtk.CellRendererText).Background = "white";
                        (cell as Gtk.CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
                    }
                    else if (record.HasField(column.Title) && column.Title == "Author")
                    {
                        StringArrayList authors = record.GetAuthors();
                        string author_string = "";
                        foreach (string author in authors)
                        {
                            if (author_string == "")
                                author_string = author;
                            else
                                author_string = String.Concat(author_string,"; " , author);
                        }
                        (cell as Gtk.CellRendererText).Text = author_string;
                        (cell as Gtk.CellRendererText).Background = "white";
                        (cell as Gtk.CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
                    }
                    else
                    {
                        (cell as Gtk.CellRendererText).Text = "";
                        // could highlight important fields that are missing data too,
                        // for e.g. the line below:
                        //(cell as Gtk.CellRendererText).Background = "green";
                    }
                    if (!BibtexRecordTypeLibrary.Contains(record.RecordType)) {
                        (cell as Gtk.CellRendererText).Foreground = "red";
                    }
                    else {
                        (cell as Gtk.CellRendererText).Foreground = "black";
                    }
                }
            }
        }
        
        public void ReconstructDetails()
        {
            // step 1: reset values
            recordIcon.Pixbuf = null;
            recordDetails.Text = null;
    
            // step 2: add new values
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                recordIcon.Pixbuf = record.GetLargeThumbnail();
    
                string text;
    
                // TODO: sort out some smart way of placing all the details
                // here
                text = "<b>";
                text = text + record.GetField("title");
                text = text + "</b>\n";
                text = text + record.GetField("author");
                text = text + "\n";
                text = text + record.GetField("year");
                recordDetails.Markup = text;
            }
        }
    
        public void ReconstructTabs()
        {
            // step 1: reset viewports
            viewportRequired.Forall(viewportRequired.Remove);
            viewportOptional.Forall(viewportOptional.Remove);
            viewportOther.Forall(viewportOther.Remove);
            viewportBibliographerData.Forall(viewportBibliographerData.Remove);
    
            // step 2: add stuff to the viewports
            Gtk.VBox req = new Gtk.VBox(false, 5);
            Gtk.VBox opt = new Gtk.VBox(false, 5);
            Gtk.VBox other = new Gtk.VBox(false, 5);
            Gtk.VBox bData = new Gtk.VBox(false, 5);
    
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                comboRecordType.Sensitive = true;
    
                if (comboRecordType.Active < 0)
                {
                    entryReqBibtexKey.Sensitive = false;
                    notebookFields.Sensitive = false;
                    buttonBibtexKeyGenerate.Sensitive = false;
                }
                else
                {
                    notebookFields.Sensitive = true;
                    entryReqBibtexKey.Sensitive = true;
                    BibtexGenerateKeySetStatus();
                }
                //  Console.WriteLine("Combo box active: " + comboRecordType.Active);
    
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                uint numItems;
                BibtexRecordType recordType = null;
                if (BibtexRecordTypeLibrary.Contains(record.RecordType)) {
                    recordType = BibtexRecordTypeLibrary.Get(record.RecordType);

                    // viewportRequired
                    Gtk.Table tableReq = new Gtk.Table(0, 2, false);
                    // TODO: process OR type fields
                    numItems = 0;
                    for (int i = 1; i < recordType.fields.Count; i++)
                    {
                        int subNumItems = 0;
                        for (int j = 0; j < recordType.fields.Count; j++)
                        {
                            if (recordType.optional[j] == i) {
                                subNumItems++;
                                if (subNumItems > 1)
                                {
                                    numItems += 2;
                                    tableReq.NRows = numItems;
                                    Gtk.Label orLabel = new Gtk.Label();
                                    orLabel.Markup = "<b>or</b>";
                                    tableReq.Attach(orLabel, 0, 2, numItems - 2, numItems - 1, 0, 0, 5, 5);
                                }
                                else
                                {
                                    numItems++;
                                    tableReq.NRows = numItems;
                                }
                                string fieldName = recordType.fields[j];
                                tableReq.Attach(new Gtk.Label(fieldName), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                                FieldEntry textEntry = new FieldEntry();
                                if (record.HasField(fieldName))
                                    textEntry.Text = record.GetField(fieldName);
                                tableReq.Attach(textEntry, 1, 2, numItems - 1, numItems, Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill, 0, 5, 5);
                                textEntry.field = fieldName;
                                textEntry.Changed += OnFieldChanged;
                            }
                        }
                        if (subNumItems == 0)
                            break;
                    }
                    req.PackStart(tableReq, false, false, 5);
    
                    //  viewportOptional
                    Gtk.Table tableOpt = new Gtk.Table(0, 2, false);
                    numItems = 0;
                    for (int i = 0; i < recordType.fields.Count; i++) {
                        if (recordType.optional[i] == 0) {
                            numItems++;
                            tableOpt.NRows = numItems;
                            tableOpt.Attach(new Gtk.Label(recordType.fields[i]), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                            FieldEntry textEntry = new FieldEntry();
                            if (record.HasField(recordType.fields[i]))
                                textEntry.Text = record.GetField(recordType.fields[i]);
                            tableOpt.Attach(textEntry, 1, 2, numItems - 1, numItems, Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill, 0, 5, 5);
                            textEntry.field = recordType.fields[i];
                            textEntry.Changed += OnFieldChanged;
                        }
                    }
                    opt.PackStart(tableOpt, false, false, 5);
                }
    
                // viewportOther
                Gtk.Table tableOther = new Gtk.Table(0, 3, false);
                numItems = 0;
                for (int i = 0; i < record.RecordFields.Count; i++) {
                    // doing this the hard way because we want to
                    // ignore case
                    bool found = false;
                    if (recordType != null)
                        for (int j = 0; j < recordType.fields.Count; j++)
                            if (String.Compare(((BibtexRecordField) record.RecordFields[i]).fieldName, recordType.fields[j], true) == 0) {
                                found = true;
                                break;
                            }
                    if (!found) {
                        // got one
                        string fieldName = ((BibtexRecordField) record.RecordFields[i]).fieldName;
                        bool inFieldLibrary = false;
                        for (int j = 0; j < BibtexRecordFieldTypeLibrary.Count(); j++)
                            if (String.Compare(fieldName, BibtexRecordFieldTypeLibrary.GetWithIndex(j).name, true) == 0)
                            {
                                inFieldLibrary = true;
                                break;
                            }
                        numItems++;
                        tableOther.NRows = numItems;
                        Gtk.Label fieldLabel = new Gtk.Label();
                        if (inFieldLibrary)
                            fieldLabel.Text = fieldName;
                        else
                            fieldLabel.Markup = "<span foreground=\"red\">" + fieldName + "</span>";
                        tableOther.Attach(fieldLabel, 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                        FieldEntry textEntry = new FieldEntry();
                        if (record.HasField(fieldName))
                            textEntry.Text = record.GetField(fieldName);
                        textEntry.field = fieldName;
                        textEntry.Changed += OnFieldChanged;
                        tableOther.Attach(textEntry, 1, 2, numItems - 1, numItems, Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill, 0, 5, 5);
                        FieldButton removeButton = new FieldButton();
                        removeButton.Label = "Remove field";
                        removeButton.field = fieldName;
                        removeButton.Clicked += OnFieldRemoved;
                        removeButton.Activated += OnFieldRemoved;
                        tableOther.Attach(removeButton, 2, 3, numItems - 1, numItems, 0, 0, 5, 5);
                    }
                }
                other.PackStart(tableOther, false, false, 5);
    
                // also include a drop-down box of other fields that could be
                // added to this record
                Gtk.ComboBox extraFields = Gtk.ComboBox.NewText();
                bool comboAdded = false;
                for (int i = 0; i < BibtexRecordFieldTypeLibrary.Count(); i++)
                {
                    string field = BibtexRecordFieldTypeLibrary.GetWithIndex(i).name;
                    bool found = false;
                    if (recordType != null) {
                        for (int j = 0; j < recordType.fields.Count; j++)
                            if (String.Compare(field, recordType.fields[j], true) == 0)
                            {
                                found = true;
                                break;
                            }
                    }
                    if (found)
                        continue;
                    for (int j = 0; j < record.RecordFields.Count; j++)
                        if (String.Compare(field, ((BibtexRecordField) record.RecordFields[j]).fieldName, true) == 0)
                        {
                            found = true;
                            break;
                        }
                    if (found)
                        continue;
                    extraFields.AppendText(field);
                    comboAdded = true;
                }
                if (comboAdded)
                {
                    Gtk.HBox hbox = new Gtk.HBox();
                    hbox.PackStart(new Gtk.Label("Add extra field:"), false, false, 5);
                    hbox.PackStart(extraFields, false, false, 5);
                    other.PackStart(hbox, false, false, 5);
                    extraFields.Changed += OnExtraFieldAdded;
                }
                else
                {
                    Gtk.Label noExtraFields = new Gtk.Label();
                    noExtraFields.Markup = "<i>No extra fields</i>";
                    other.PackStart(noExtraFields, false, false, 5);
                }
    
                // viewportBibliographerData
                Gtk.HBox uriHBox = new Gtk.HBox();
                uriHBox.PackStart(new Gtk.Label("Associated file:"), false, false, 5);
                FieldEntry uriEntry = new FieldEntry();
                if (record.HasField("bibliographer_uri"))
                    uriEntry.Text = record.GetField("bibliographer_uri");
                uriEntry.field = "bibliographer_uri";
                uriEntry.Changed += OnFieldChanged;
                uriHBox.PackStart(uriEntry, false, false, 5);
                Gtk.Button uriBrowseButton = new Gtk.Button("Browse");
                uriBrowseButton.Activated += OnURIBrowseClicked;
                uriBrowseButton.Clicked += OnURIBrowseClicked;
                uriHBox.PackStart(uriBrowseButton, false, false, 5);
                bData.PackStart(uriHBox, false, false, 5);
            }
            else
            {
                notebookFields.Sensitive = false;
                entryReqBibtexKey.Sensitive = false;
                buttonBibtexKeyGenerate.Sensitive = false;
                comboRecordType.Sensitive = false;
            }
    
            viewportRequired.Add(req);
            viewportRequired.ShowAll();
    
            viewportOptional.Add(opt);
            viewportOptional.ShowAll();
    
            viewportOther.Add(other);
            viewportOther.ShowAll();
    
            viewportBibliographerData.Add(bData);
            viewportBibliographerData.ShowAll();
        }
    
        public int StringCompare(Gtk.TreeModel model, Gtk.TreeIter tia, Gtk.TreeIter tib)
        {
            BibtexRecord a = (BibtexRecord) model.GetValue(tia, 0);
            BibtexRecord b = (BibtexRecord) model.GetValue(tib, 0);
            string A, B;
            string sortString = "";
            int sortColumn;
            Gtk.SortType sortType;
            sorter.GetSortColumnId(out sortColumn, out sortType);
            
            switch (sortColumn)
            {
                case 1:
                    sortString = "Author";
                    break;
                case 2:
                    sortString = "Title";
                    break;
                case 3:
                    sortString = "Year";
                    break;
                case 4:
                    sortString = "Journal";
                    break;
                case 5:
                    sortString = "Bibtex Key";
                    break;
                case 6:
                    sortString = "Volume";
                    break;
                case 7:
                    sortString = "Pages";
                    break;
            }
    
            if (a != null)
                if (a.HasField(sortString))
                    A = a.GetField(sortString);
                else
                    A = "";
            else
                A = "";
            if (b != null)
                if (b.HasField(sortString))
                    B = b.GetField(sortString);
                else
                    B = "";
            else
                B = "";
            Debug.WriteLine(10, "sortString: {0} Comparing {1} and {2}", sortString, A, B); 
            return String.Compare(A, B);
        }
        
        public int StringCompareAuthor(Gtk.TreeModel model, Gtk.TreeIter tia, Gtk.TreeIter tib)
        {
            BibtexRecord a = (BibtexRecord) model.GetValue(tia, 0);
            BibtexRecord b = (BibtexRecord) model.GetValue(tib, 0);
            string A, B;
            if (a != null)
                if (a.GetAuthors().Count > 0)
                    A = a.GetAuthors()[0];
                else
                    A = "";
            else
                A = "";
            if (b != null)
                if (b.GetAuthors().Count > 0)
                    B = b.GetAuthors()[0];
                else
                    B = "";
            else
                B = "";
            Debug.WriteLine(10, "Comparing {1} and {2}", A, B); 
            return String.Compare(A, B);
        }
        
        private void RenderFilterColumnTextFromBibtexRecords(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            //System.Console.WriteLine("Rendering cell");
            string val = (string) model.GetValue(iter,0);
            //System.Console.WriteLine("Value = " + val);
            (cell as Gtk.CellRendererText).Text = val;
        }
        
        private void FileUnmodified()
        {
            this.Title = application_name + " - " + file_name;
            modified = false;
        }
        
        private void FileModified()
        {
            this.Title = application_name + " - " + file_name + "*";
            modified = true;
        }
        
        public void FileOpen(string file_name)
        {
            bibtexRecords = BibtexRecords.Open(file_name);
            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
    
            litStore.SetBibtexRecords(bibtexRecords);
            sidePaneStore.SetBibtexRecords(bibtexRecords);
    
            this.file_name = file_name;
    
            System.Threading.Monitor.Enter(alterationMonitorQueue);
            alterationMonitorQueue.Clear();
            System.Threading.Monitor.Exit(alterationMonitorQueue);
            System.Threading.Monitor.Enter(indexerQueue);
            indexerQueue.Clear();
            System.Threading.Monitor.Exit(indexerQueue);
    
            System.Threading.Monitor.Enter(alterationMonitorQueue);
            foreach (BibtexRecord record in bibtexRecords)
                alterationMonitorQueue.Enqueue(record);
            System.Threading.Monitor.Exit(alterationMonitorQueue);
            FileUnmodified();
            // Disable editing of opened document
            ViewRecordsAction.Activate();
            
            UpdateFileHistory(file_name);
        }
        
        private Gtk.ResponseType FileSave ()
        {
            if (file_name != "Untitled.bib")
            {
                //BibtexListStoreParser btparser = new BibtexListStoreParser(
                //  store);
                //btparser.Save(file_name);
                bibtexRecords.Save(file_name);
                FileUnmodified();
                
                return Gtk.ResponseType.Ok;
            }
            else
            {
                return FileSaveAs();
            }
        }
        
        private Gtk.ResponseType FileSaveAs()
        {
            Gtk.FileChooserDialog fileSaveDialog = new Gtk.FileChooserDialog("Save Bibtex file...", this, Gtk.FileChooserAction.Save, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);

            // TODO: filter for *.bib files only :)
            fileSaveDialog.Filter = new Gtk.FileFilter();
            fileSaveDialog.Filter.AddPattern("*.bib");

            if (Config.KeyExists("bib_browse_path"))
                fileSaveDialog.SetCurrentFolder(Config.GetString("bib_browse_path"));

            Gtk.ResponseType response = (Gtk.ResponseType) fileSaveDialog.Run();

            if (response == Gtk.ResponseType.Ok)
            {
                Config.SetString("bib_browse_path", fileSaveDialog.CurrentFolder);
                if (fileSaveDialog.Filename != null)
                {
                    file_name = fileSaveDialog.Filename;
                    FileSave();
                    UpdateFileHistory(file_name);
                    fileSaveDialog.Destroy();
                    return Gtk.ResponseType.Ok;
                }
            }
            
            fileSaveDialog.Destroy();
            return Gtk.ResponseType.Cancel;
        }
        
        private void InsertFilesInDir(object o)
        {
            string dir = (string) o;
            string [] files = System.IO.Directory.GetFiles(dir);
    
            //double fraction = 1.0 / System.Convert.ToDouble(files.Length);
    
            foreach (string file in files)
            {
                while (Gtk.Application.EventsPending ())
                    Gtk.Application.RunIteration ();
                string uri = Gnome.Vfs.Uri.GetUriFromLocalPath(file);
                if (bibtexRecords.HasURI(uri) == false)
                {
                    Debug.WriteLine(5, "Adding new record with URI: {0}", uri);
                    BibtexRecord record = new BibtexRecord();
                    record.SetField("bibliographer_uri", uri);
                    bibtexRecords.Add(record);
                }
            }
        }
        
        private void UpdateRecordTypeCombo()
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                string recordType = record.RecordType;
                new_selected_record = true;
                if (BibtexRecordTypeLibrary.Contains(recordType))
                    comboRecordType.Active = BibtexRecordTypeLibrary.Index(recordType);
                else
                    comboRecordType.Active = -1;
                
                comboRecordType.Activate();
                entryReqBibtexKey.Text = record.GetKey();
                buttonBibtexKeyGenerate.Sensitive = false;
                new_selected_record = false;
            }
        }
        
        private void UpdateFileHistory(string filename)
        {
            if (!Config.KeyExists("max_file_history_count"))
                Config.SetInt("max_file_history_count", 5);
    
            int max = Config.GetInt("max_file_history_count");
            if (max < 0)
            {
                max = 0;
                Config.SetInt("max_file_history_count", max);
            }
            System.String[] tempHistory = (System.String[])Config.GetKey("file_history");
            ArrayList history;
            if (tempHistory == null)
                history = new ArrayList();
            else
                history = new ArrayList(tempHistory);
    
            //while (history.Count > max)
            //    history.RemoveAt(history.Count - 1);
    
            // check if this filename is already in our history
            // if so, it just gets bumped to top position,
            // otherwise bump everything down
            int bumpEnd;
            for (bumpEnd = 0; bumpEnd < history.Count; bumpEnd++)
            {
                if ((string) history[bumpEnd] == filename)
                {
                    break;
                }
            }
            if (bumpEnd == max)
                bumpEnd--;
            if (history.Count < max && bumpEnd == history.Count)
                history.Add("");
            //System.Console.WriteLine("bumpEnd set to {0}", bumpEnd);
            for (int cur = bumpEnd; cur > 0; cur--)
            {
                history[cur] = history[cur - 1];
            }
            history[0] = filename;
            Config.SetKey("file_history", history.ToArray());

            UpdateMenuFileHistory();
        }
        
        private void UpdateMenuFileHistory()
        {
            Debug.WriteLine(5, "File History - doing update...");
            
            Gtk.Widget[] menus = (Gtk.Widget[]) menuBar.Children;
            foreach (Gtk.Widget menu in menus)
            {
                Gtk.MenuItem menu_ = (Gtk.MenuItem) menu;
                if (menu_.Name == "FileAction")
                {
                    Gtk.Menu file_menu = (Gtk.Menu) menu_.Submenu;
                    Gtk.Widget[] file_menu_items = (Gtk.Widget[]) file_menu.Children;
                    foreach (Gtk.Widget file_menu_item in file_menu_items)
                    {
                        Gtk.MenuItem file_menu_item_ = (Gtk.MenuItem) file_menu_item;
                        if (file_menu_item_.Name == "RecentFilesAction")
                        {
                            Gtk.Menu recentFilesMenu = (Gtk.Menu) file_menu_item_.Submenu;
                            // step 1: clear the menu
                            foreach (Gtk.Widget w in recentFilesMenu)
                            {
                                recentFilesMenu.Remove(w);
                            }
                    
                            // step 2: add on items for history
                            bool gotOne = false;
                            if (Config.KeyExists("max_file_history_count") && Config.KeyExists("file_history")) {
                                object o = Config.GetKey("file_history");
                                Debug.WriteLine(5, "{0}", o.GetType());
                                System.String[] history = (System.String[])o;
                                for (int i = 0; i < history.Length; i++)
                                {
                                    // Workaround for clearing history - check if history item is not an empty string
                                    if (history[i] != "")
                                    {
                                        string label = string.Format("{0} ", i + 1) + (string) history[i];
                                        Gtk.MenuItem item = new Gtk.MenuItem(label);
                                        item.Activated += OnFileHistoryActivate;
                                        item.Data.Add("i", (System.IntPtr)i);
                                        recentFilesMenu.Append(item);
                                        gotOne = true;
                                    }
                                }
                            }
                            if (gotOne)
                            {
                                recentFilesMenu.Append(new Gtk.SeparatorMenuItem());
                                Gtk.MenuItem clear = new Gtk.MenuItem("Clear Recent Files");
                                clear.Activated += OnClearFileHistory;
                                recentFilesMenu.Append(clear);
                            }
                            else
                            {
                                Gtk.MenuItem none = new Gtk.MenuItem("(none)");
                                // want to disable this somehow...
                                //none. = false;
                                recentFilesMenu.Append(none);
                            }
                    
                            recentFilesMenu.ShowAll();
                        }
                    }
                }
            }
        }
        
        private void Quit()
        {
            // Save column states
            int i = 0;
            foreach (Gtk.TreeViewColumn column in litTreeView.Columns)
            {
                Config.SetBool("Columns/"+column.Title+"/visible", column.Visible);
                Config.SetInt("Columns/"+column.Title+"/order", i);
                // Prevent columns from persisting with a width less than 10 pixels
                if (column.Width >= 40)
                {
                    Config.SetInt("Columns/"+column.Title+"/width", column.Width);
                }
                else
                {
                    Config.SetInt("Columns/"+column.Title+"/width", 40);
                }
                i = i + 1;
            }
    
            // Save sidebar visibility
            Config.SetBool("SideBar/visible", scrolledwindowSidePane.Visible);
            Gtk.HPaned hpane = (Gtk.HPaned)scrolledwindowSidePane.Parent;
            // Save sidebar width
            Config.SetInt("SideBar/width", hpane.Position);
    
            if (IsModified())
            {
                if (!ProcessModifiedData())
                    return;
            }
    
            Gtk.Application.Quit ();
        }
        
        private bool ProcessModifiedData()
        {
            // display a dialog asking the user if they
            // want to save their changes (offering them
            // Yes/No/Cancel. If they choose Yes, call
            // FileSave and return true. If they choose
            // No, just return true. If they choose
            // Cancel, return false
            Gtk.MessageDialog dialog = new Gtk.MessageDialog(this, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.None, "{0} has been modified. Do you want to save it?", file_name);
            dialog.AddButton("Yes", Gtk.ResponseType.Yes);
            dialog.AddButton("No", Gtk.ResponseType.No);
            dialog.AddButton("Cancel", Gtk.ResponseType.Cancel);
            Gtk.ResponseType msg_result = (Gtk.ResponseType) dialog.Run();
            dialog.Hide();

            if (msg_result == Gtk.ResponseType.Yes)
            {
                Gtk.ResponseType save_result = FileSave();
                if (save_result == Gtk.ResponseType.Ok)
                    return true;
                else if (save_result == Gtk.ResponseType.Cancel)
                    return false;
            }
            else if (msg_result == Gtk.ResponseType.No)
                return true;
            else if (msg_result == Gtk.ResponseType.Cancel)
                return false;
            else
            {
                Gtk.MessageDialog error = new Gtk.MessageDialog(this, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok, "An unexpected error occurred in processing your response, please contact the developers!");
                error.Run();
                error.Destroy();
                return false;
            }
            return false;
        }
        
        private bool IsModified()
        {
            return modified;
        }

        /* ----------------------------------------------------------------- */
        /* CALLBACKS                                                         */
        /* ----------------------------------------------------------------- */
        
        protected virtual void OnColumnSort(object o, EventArgs a)
        {
            Gtk.TreeViewColumn col = (Gtk.TreeViewColumn) o;
            Debug.WriteLine(5, "OnColumnSort: Column ID is {0} and {1}", col.SortColumnId, col.SortOrder.ToString());
            
            int sortColumn;
            Gtk.SortType sortType;
            sorter.GetSortColumnId(out sortColumn, out sortType);
            if (sortColumn == -1)
            {
                sorter.SetSortColumnId(-1, col.SortOrder);
            }
            else
            {
                sorter.SetSortColumnId(col.SortColumnId, col.SortOrder);
            }
        }
        
        protected virtual void OnComboRecordTypeChanged (object o, EventArgs a)
        {
            // the next check stops bad things from happening when
            // the user selects a new record in the list view,
            // causing the checkbox to get updated. In this case,
            // we really don't want to be calling this method
            if (new_selected_record)
                return;
    
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                if (((Gtk.ComboBox) o).Active != -1) {
                    string bType = BibtexRecordTypeLibrary.GetWithIndex(((Gtk.ComboBox) o).Active).name;
                    Debug.WriteLine(5, bType);
                    // TODO: fix me :-)
                    //model.SetValue(iter, (int)bibtexRecordField.bibtexField.BIBTEXTYPE,
                    //((int)bType).ToString());
    
                    ((BibtexRecord) model.GetValue(iter, 0)).RecordType = bType;
                }
                // Sort out the behaviour of the Required and Optional fields
                // for each type of record
                // TODO: FIXME
            }
            ReconstructTabs();
            ReconstructDetails();
        }
        
        protected virtual void OnSidePaneTreeSelectionChanged(object o, EventArgs a)
        {
            fieldFilter.Refilter();
        }
        
        protected virtual void OnBibtexRecordsModified(object o, EventArgs a)
        {
            // Refresh the Article Type field
            FileModified();
            UpdateRecordTypeCombo();
        }
        
        protected virtual void OnExtraFieldAdded(object o, EventArgs a)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                Gtk.ComboBox combo = (Gtk.ComboBox) o;
                Gtk.TreeIter comboIter;
                if (combo.GetActiveIter(out comboIter))
                {
                    BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                    record.SetField((string) combo.Model.GetValue(comboIter, 0), "");
                    ReconstructTabs();
                    ReconstructDetails();
                }
            }
        }
        
        protected virtual void OnFieldChanged(object o, EventArgs a)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                FieldEntry entry = (FieldEntry) o;
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                record.SetField(entry.field, entry.Text);
                model.EmitRowChanged(model.GetPath(iter), iter);
                BibtexGenerateKeySetStatus();
            }
        }
        
        protected virtual void OnFieldRemoved(object o, EventArgs a)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                FieldButton button = (FieldButton) o;
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                record.RemoveField(button.field);
                ReconstructTabs();
                ReconstructDetails();
            }
        }
        
        protected virtual void OnFileQuitActivated (object sender, System.EventArgs e)
        {
            Quit();
        }
        
        protected virtual void OnFileNewActivated (object sender, System.EventArgs e)
        {
            if (IsModified())
            {
                if (!ProcessModifiedData())
                    return;
            }
    
            System.Threading.Monitor.Enter(alterationMonitorQueue);
            alterationMonitorQueue.Clear();
            System.Threading.Monitor.Exit(alterationMonitorQueue);
            System.Threading.Monitor.Enter(indexerQueue);
            indexerQueue.Clear();
            System.Threading.Monitor.Exit(indexerQueue);
    
            bibtexRecords = new BibtexRecords();
            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
    
            litStore.SetBibtexRecords(bibtexRecords);
            sidePaneStore.SetBibtexRecords(bibtexRecords);
    
            //if (file_name == null)
            file_name = "Untitled.bib";
            FileUnmodified();
        }

        protected virtual void OnHelpAboutActivated (object sender, System.EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.Run();
            ab.Destroy();
        }

        protected virtual void OnEntryReqBibtexKeyChanged (object sender, System.EventArgs e)
        {
            
            Gtk.Entry bibtexEntry = (Gtk.Entry) sender;
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                record.SetKey(bibtexEntry.Text);
                model.EmitRowChanged(model.GetPath(iter), iter);
            }
        }
        
        protected virtual void OnURIBrowseClicked(object o, EventArgs a)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                
                Gtk.FileChooserDialog fileOpenDialog = new Gtk.FileChooserDialog("Associate file...", this, Gtk.FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);
                
                fileOpenDialog.Filter = new Gtk.FileFilter();
                fileOpenDialog.Title = "Associate file...";
                fileOpenDialog.Filter.AddPattern("*");
                
                if (record.HasField("bibliographer_uri"))
                {
                    // If a file is associated with this directory, then open browse path in the files containing directory
                    fileOpenDialog.SetUri(record.GetURI());
                }
                else if (Config.KeyExists("uri_browse_path"))
                {
                    // Else, query config for stored path
                    fileOpenDialog.SetCurrentFolder(Config.GetString("uri_browse_path"));
                }

                Gtk.ResponseType result = (Gtk.ResponseType) fileOpenDialog.Run();

                if (result == Gtk.ResponseType.Ok)
                {
                    record.SetField("bibliographer_uri", Gnome.Vfs.Uri.GetUriFromLocalPath(fileOpenDialog.Filename));
                    Config.SetString("uri_browse_path", fileOpenDialog.CurrentFolder);
                    ReconstructTabs();
                    ReconstructDetails();
                }
                   
                fileOpenDialog.Destroy();
            }
        }

        protected virtual void OnFileOpenActivated (object o, EventArgs a)
        {
    
            Gtk.FileChooserDialog fileOpenDialog = new Gtk.FileChooserDialog("Open Bibtex File...", this, Gtk.FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);
    
            fileOpenDialog.Filter = new Gtk.FileFilter();
            fileOpenDialog.Filter.AddPattern("*.bib");
    
            // query config for stored path
            if (Config.KeyExists("bib_browse_path"))
                fileOpenDialog.SetCurrentFolder(Config.GetString("bib_browse_path"));
    
            Gtk.ResponseType result = (Gtk.ResponseType) fileOpenDialog.Run();

            if (result == Gtk.ResponseType.Ok)
                if (fileOpenDialog.Filename != null)
                {
                    file_name = fileOpenDialog.Filename;
                    this.FileOpen(file_name);
                    // TODO: Verify document integrity
                    // (search for conflicting bibtexkeys)
                    // (highlite any uncomplete entries)
                }
            fileOpenDialog.Destroy();
        }

        protected virtual void OnFileSaveActivated (object sender, System.EventArgs e)
        {
            FileSave();
        }

        protected virtual void OnFileSaveAsActivated (object sender, System.EventArgs e)
        {
            FileSaveAs();
        }
        
        private void OnFileHistoryActivate(object o, EventArgs a)
        {
            Gtk.MenuItem item = (Gtk.MenuItem) o;
            int index = (int) System.Convert.ToUInt16((string) item.Data["i"].ToString());
            if (Config.KeyExists("max_file_history_count") && index >= 0 && index < Config.GetInt("max_file_history_count") && Config.KeyExists("file_history"))
            {
                string[] history = (string[])Config.GetKey("file_history");
                if (index < history.Length)
                {
                    Debug.WriteLine(5, "Loading {0}", history[index]);
                    file_name = (string) history[index];
                    this.FileOpen(file_name);
                    UpdateFileHistory(file_name);
                }
            }
        }

        protected virtual void OnAddRecordActivated (object sender, System.EventArgs e)
        {
            Debug.WriteLine(5, "Adding a new record");
            //Debug.WriteLine(5, "Prev rec count: {0}", bibtexRecords.Count);
            Gtk.TreeIter iter;
    
            if (bibtexRecords == null)
            {
                bibtexRecords = new BibtexRecords();
                bibtexRecords.RecordsModified += OnBibtexRecordsModified;
            }
    
            BibtexRecord record = new BibtexRecord();
            bibtexRecords.Add(record);
    
            iter = litStore.GetIter(record);

            // TODO: Unfilter
            // TODO: Remove search
            // TODO: Unsort and then resort
            litTreeView.SetCursor(litStore.GetPath(iter),litTreeView.GetColumn(0),false);
            
            BibtexGenerateKeySetStatus();
    
            System.Threading.Monitor.Enter(alterationMonitorQueue);
            alterationMonitorQueue.Enqueue(record);
            System.Threading.Monitor.Exit(alterationMonitorQueue);
        }

        protected virtual void OnRemoveRecordActivated (object sender, System.EventArgs e)
        {
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            Gtk.TreePath newpath;
    
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                // select next row, or previous row if we are on the last row
                newpath = model.GetPath(iter);
                newpath.Next();
                litTreeView.SetCursor(newpath, litTreeView.GetColumn(0), false);
                if (!litTreeView.Selection.PathIsSelected(newpath))
                {
                    newpath = model.GetPath(iter);
                    newpath.Prev();
                    litTreeView.SetCursor(newpath, litTreeView.GetColumn(0), false);
                }
    
                // model and iter are TreeModelSort types
                // Obtain the search model
                Gtk.TreeModel searchModel = ((Gtk.TreeModelSort) model).Model;
                Gtk.TreeIter searchIter = ((Gtk.TreeModelSort) model).ConvertIterToChildIter(iter);
    
                // Obtain the filter model
                Gtk.TreeModel filterModel = ((Gtk.TreeModelFilter) searchModel).Model;
                Gtk.TreeIter filterIter = ((Gtk.TreeModelFilter) searchModel).ConvertIterToChildIter(searchIter);
    
                // Obtain the real model
                Gtk.TreeModel realModel = ((Gtk.TreeModelFilter) filterModel).Model;
                Gtk.TreeIter realIter = ((Gtk.TreeModelFilter) filterModel).ConvertIterToChildIter(filterIter);
    
                // Delete record from the real model
                //((ListStore) realModel).Remove(ref realIter);
                BibtexRecord record = ((Gtk.ListStore) realModel).GetValue(realIter,0) as BibtexRecord;
                bibtexRecords.Remove(record);
    
            }
        }

        protected virtual void OnAddRecordFromBibtexActivated (object sender, System.EventArgs e)
        {
            Gtk.TreeIter iter;

            BibtexEntryDialog bibtexEntryDialog = new BibtexEntryDialog();
            
            Gtk.ResponseType result = (Gtk.ResponseType) bibtexEntryDialog.Run();
            if (result == Gtk.ResponseType.Ok) {
                try {
                    BibtexRecord record = new BibtexRecord(bibtexEntryDialog.GetText());
                    bibtexRecords.Add(record);
    
                    iter = litStore.GetIter(record);
                    litTreeView.Selection.SelectIter(iter);
    
                    BibtexGenerateKeySetStatus();
    
                    System.Threading.Monitor.Enter(alterationMonitorQueue);
                    alterationMonitorQueue.Enqueue(record);
                    System.Threading.Monitor.Exit(alterationMonitorQueue);
                } catch (ParseException except) {
                    Debug.WriteLine(1, "Parse exception: {0}", except.GetReason());
                }
            }
    
            bibtexEntryDialog.Destroy();
        }

        protected virtual void OnAddRecordFromClipboardActivated (object sender, System.EventArgs e)
        {
            Gtk.TreeIter iter;
            Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("GDK_CLIPBOARD_SELECTION", false));
            string contents = clipboard.WaitForText();
            BibtexRecord record = new BibtexRecord(contents);
            bibtexRecords.Add(record);
    
            iter = litStore.GetIter(record);
            litTreeView.Selection.SelectIter(iter);
    
            BibtexGenerateKeySetStatus();
    
            System.Threading.Monitor.Enter(alterationMonitorQueue);
            alterationMonitorQueue.Enqueue(record);
            System.Threading.Monitor.Exit(alterationMonitorQueue);
        }
        
        protected virtual void OnClearFileHistory(object o, EventArgs a)
        {
            Debug.WriteLine(5, "Clearing file history");
            ArrayList temp = new ArrayList();
            // Workaround for clear file history bug - set history to contain a single empty string
            temp.Add("");
            Config.SetKey("file_history", temp.ToArray());
            UpdateMenuFileHistory();
        }

        protected virtual void OnImportFolderActivated (object sender, System.EventArgs e)
        {
            Gtk.FileChooserDialog folderImportDialog = new Gtk.FileChooserDialog("Import folder...", this, Gtk.FileChooserAction.SelectFolder, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Ok, Gtk.ResponseType.Ok);
    
            // query config for stored path
            if (Config.KeyExists("bib_import_path"))
                folderImportDialog.SetCurrentFolder(Config.GetString("bib_import_path"));

            Gtk.ResponseType response = (Gtk.ResponseType) folderImportDialog.Run();

            if (response == Gtk.ResponseType.Ok)
            {
                string folderImportPath = folderImportDialog.Filename;
                string folderImportCurrentPath = folderImportDialog.CurrentFolder;
                
                folderImportDialog.Hide();
                
                while (Gtk.Application.EventsPending ())
                    Gtk.Application.RunIteration (); 
                
                Config.SetString("bib_import_path", folderImportCurrentPath);
                Debug.WriteLine(5, "Importing folder: {0}", folderImportPath);
                
                InsertFilesInDir(folderImportPath);
            }
            
            folderImportDialog.Destroy();
            
        }

        protected virtual void OnToggleSideBarActivated (object sender, System.EventArgs e)
        {
            if (SidebarAction.Active == true)
                scrolledwindowSidePane.Visible = true;
            else
                scrolledwindowSidePane.Visible = false;
        }

        protected virtual void OnToggleRecordDetailsActivated (object sender, System.EventArgs e)
        {
            if (RecordDetailsAction.Active == false)
            {
                recordDetailsView.Visible = false;
            }
            else
            {
                recordDetailsView.Visible = true;
            }
        }

        protected virtual void OnToggleFullScreenActionActivated (object sender, System.EventArgs e)
        {
    
            if (FullScreenAction.Active == true)
            {
                this.Fullscreen();
            }
            else
            {
                this.Unfullscreen();
            }
        }

        protected virtual void OnRadioViewRecordsActivated (object sender, System.EventArgs e)
        {
            if (ViewRecordsAction.Active)
            {
                recordView.Visible = true;
                recordEditor.Visible = false;
                vpane.Position = vpane.MaxPosition - 150;
                toggleEditRecords.Active = false;
            }
        }

        protected virtual void OnRadioEditRecordsActivated (object sender, System.EventArgs e)
        {
            if (EditRecordsAction.Active)
            {
                recordView.Visible = false;
                recordEditor.Visible = true;
                ReconstructDetails();
                vpane.Position = vpane.MaxPosition - 350;
                toggleEditRecords.Active = true;
            }
        }

        protected virtual void OnChooseColumnsActivated (object sender, System.EventArgs e)
        {
            BibliographerChooseColumns chooseColumnsDialog = new BibliographerChooseColumns();

            chooseColumnsDialog.ConstructDialog(litTreeView.Columns);
            chooseColumnsDialog.Run();
            chooseColumnsDialog.Destroy();
        }
        
        protected virtual void OnTreeViewSelectionChanged (object o, EventArgs a)
        {
            //Console.WriteLine("Selection changed");
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
    
            if (((Gtk.TreeSelection)o).GetSelected(out model, out iter))
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                string recordType = record.RecordType;
                new_selected_record = true;
                if (BibtexRecordTypeLibrary.Contains(recordType))
                    comboRecordType.Active = BibtexRecordTypeLibrary.Index(recordType);
                else
                    comboRecordType.Active = -1;
    
                entryReqBibtexKey.Text = record.GetKey();
                buttonBibtexKeyGenerate.Sensitive = false;
                new_selected_record = false;
    
                // Interrogate ListStore for values
                // TODO: fix!
            }
            else {
                buttonBibtexKeyGenerate.Sensitive = false;
            }
            ReconstructTabs();
            ReconstructDetails();
        }

        protected virtual void OnToggleEditRecordsActivated (object sender, System.EventArgs e)
        {
            if (toggleEditRecords.Active == true)
            {
                    EditRecordsAction.Active = true;
            }
            else
            {
                    ViewRecordsAction.Active = true;
            }
        }

        protected virtual void OnWindowDeleteEvent (object o, Gtk.DeleteEventArgs args)
        {
            Quit();
        }

        protected virtual void OnFilterEntryChanged (object sender, System.EventArgs e)
        {
            // Filter when the filter entry text has changed
            modelFilter.Refilter();
        }

        protected virtual void OnButtonBibtexKeyGenerateClicked (object sender, System.EventArgs e)
        {
            //System.Console.WriteLine("Generate a Bibtex Key");
            
            Gtk.TreeIter iter;
            Gtk.TreeModel model;
            
            if (litTreeView.Selection.GetSelected(out model, out iter))
            {
                // TODO: check if this is the correct bibtexRecordFieldType
                // (it currently doesn't work for types that don't have an author
                // or year field)
    
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                string authors = record.HasField("author") ? (record.GetField("author").ToLower()).Trim() : "";
                string year = record.HasField("year") ? record.GetField("year").Trim() : "";
    
                //System.Console.WriteLine("authors: " + authors);
    
                authors = authors.Replace(" and ", "&");
                string []   authorarray = authors.Split(("&").ToCharArray());
    
                if (authorarray.Length > 0)
                {
                    ArrayList authorsurname = new ArrayList();
                    foreach (string author in authorarray)
                    {
                        //System.Console.WriteLine(author);
                        // Deal with format of "Surname, Firstname ..."
                        if (author.IndexOf(",")>0)
                        {
                            string []   authorname = author.Split(',');
                            //System.Console.WriteLine("Surname: " + authorname[0]);
                            authorsurname.Add(authorname[0]);
                        }
                        // Deal with format of "Firstname ... Surname"
                        else
                        {
                            string []   authorname = author.Split(' ');
                            //System.Console.WriteLine("Surname: " + authorname[authorname.Length - 1]);
                            authorsurname.Add(authorname[authorname.Length - 1]);
                        }
                    }
    
                    string bibtexkey;
    
                    if (authorsurname.Count < 2)
                    {
                        bibtexkey = (string)(authorsurname.ToArray())[0];
                    }
                    else
                    {
                        bibtexkey = (string)(authorsurname.ToArray())[0]+"_etal";
                    }
                    bibtexkey = bibtexkey + year;
                    // TODO: Check for and mitigate any duplication of keys
    
                    // Setting the bibtex key in the entry field and ListStore
                    entryReqBibtexKey.Text = bibtexkey;
                    record.SetKey(bibtexkey);
                    model.EmitRowChanged(model.GetPath(iter), iter);
                }
            }
            BibtexGenerateKeySetStatus();
        }

        protected virtual void OnLitTreeViewDragDataReceived (object o, Gtk.DragDataReceivedArgs args)
        {
            //Console.WriteLine("Data received is of type '" + args.SelectionData.Type.Name + "'");
            // the atom type we want is a text/uri-list
            // if we get anything else, bounce it
            if (args.SelectionData.Type.Name.CompareTo("text/uri-list") != 0)
            {
                // wrong type
                return;
            }
            //DragDataReceivedArgs args
            string data = System.Text.Encoding.UTF8.GetString (
                args.SelectionData.Data);
            //Split out multiple files
            
            string []    uri_list = System.Text.RegularExpressions.Regex.Split (data, "\r\n");
            Gtk.TreePath path;
            Gtk.TreeViewDropPosition drop_position;
            if (!litTreeView.GetDestRowAtPos(args.X, args.Y, out path, out drop_position))
            {
                Debug.WriteLine(5, "Failed to drag and drop because of GetDestRowAtPos failure");
                return;
            }
            Gtk.TreeIter iter; //, check;
            BibtexRecord record;
            //TreeModel model;
            if (!litTreeView.Model.GetIter(out iter, path))
            {
                Debug.WriteLine(5, "Failed to drag and drop because of GetIter failure");
                return;
            }
            record = (BibtexRecord) litTreeView.Model.GetValue(iter, 0);
            //For each file
            foreach (string u in uri_list)
            {
                if (u.Length > 0)
                {
                    Debug.WriteLine(5, "Associating file '" + u + "' with entry '" + record.GetKey() + "'");
                    record.SetField("bibliographer_uri", u);
                    // TODO: disable debugging info
                    //Console.WriteLine("Importing: " + u);
                    //bibtexRecord record = new bibtexRecord(store, u);
                }
            }
            //if (litTreeView.Selection.GetSelected(out model, out check) && (iter == check))
            ReconstructTabs();
            ReconstructDetails();
        }

        protected virtual void OnLitTreeViewDragMotion (object o, Gtk.DragMotionArgs args)
        {
            // FIXME: how do we check from here if that drag has data that we want?
    
            Gtk.TreePath path;
            Gtk.TreeViewDropPosition drop_position;
            if (litTreeView.GetDestRowAtPos(args.X, args.Y, out path, out drop_position)) {
                litTreeView.SetDragDestRow(path, Gtk.TreeViewDropPosition.IntoOrAfter);
            }
            else
                litTreeView.UnsetRowsDragDest();
        }

        protected virtual void OnLitTreeViewDragLeave (object o, Gtk.DragLeaveArgs args)
        {
            litTreeView.UnsetRowsDragDest();
        }

        protected virtual void OnLitTreeViewRowActivated (object o, Gtk.RowActivatedArgs args)
        {
            Debug.WriteLine(5, "Row activated");
    
            Gtk.TreeIter iter;
            BibtexRecord record;
    
            if (!litTreeView.Model.GetIter(out iter, args.Path))
            {
                Debug.WriteLine(5, "Failed to open record because of GetIter faliure");
                return;
            }
            record = (BibtexRecord) litTreeView.Model.GetValue(iter, 0);
            string uriString = record.GetURI();
            if (uriString == null || uriString == "")
            {
                Debug.WriteLine(5, "Selected record does not have a URI field");
                return;
            }
            Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri(uriString);
    
            GLib.List list = new GLib.List(typeof(String));
            list.Append(uriString);
            if (System.IO.File.Exists(Gnome.Vfs.Uri.GetLocalPathFromUri(uriString)))
            {
                Gnome.Vfs.MimeApplication app = Gnome.Vfs.Mime.GetDefaultApplication(uri.MimeType.Name);
                if (app != null)
                    app.Launch(list);
                return;
            }
            else
            {
                Gtk.MessageDialog md = new Gtk.MessageDialog (this,
                                                          Gtk.DialogFlags.DestroyWithParent,
                                                          Gtk.MessageType.Error,
                                                          Gtk.ButtonsType.Close, "Error loading associated file:\n" + Gnome.Vfs.Uri.GetLocalPathFromUri(uriString));
                //int result = md.Run ();
                md.Run();
                md.Destroy();
                Debug.WriteLine(0, "Error loading associated file:\n{0}", Gnome.Vfs.Uri.GetLocalPathFromUri(uriString));
            }
        }
        
        private void OnWindowSizeAllocated(object o, Gtk.SizeAllocatedArgs a)
        {
            if (Config.GetBool("window_maximized") == false)
            {
                Config.SetInt("window_width", a.Allocation.Width);
                Config.SetInt("window_height", a.Allocation.Height);
            }
        }
    
        private void OnWindowStateChanged(object o, Gtk.WindowStateEventArgs a)
        {
            Gdk.EventWindowState gdk_event = a.Event; 
            if (gdk_event.NewWindowState == Gdk.WindowState.Maximized)
            {
                Debug.WriteLine(10, "window has been maximized");
                Config.SetBool("window_maximized", true);
            }
            else if (gdk_event.NewWindowState == 0)
            {
                Debug.WriteLine(10, "window is back to normal");
                Config.SetBool("window_maximized", false);
                this.Resize(Config.GetInt("window_width"), Config.GetInt("window_height"));
            }
        }

    }
}
