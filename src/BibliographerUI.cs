// Copyright 2005-2007 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Gtk;
using GLib;
using Glade;
using Gnome;

namespace bibliographer
{
public class BibliographerUI
{
    [Widget] Gtk.Window mainWindow;

    // File menu
    [Widget] MenuItem fileNew;
    [Widget] MenuItem fileOpen;
    [Widget] MenuItem fileQuit;
    [Widget] MenuItem fileSave;
    [Widget] MenuItem fileSaveAs;
    [Widget] MenuItem importFolder;
    [Widget] Menu recentFilesMenu;

    // Edit menu
    [Widget] MenuItem fromBibtexAddRecord;
    [Widget] MenuItem fromClipboardAddRecord;
    [Widget] MenuItem bibtexAddRecord;
    [Widget] MenuItem bibtexDelRecord;

    // View menu
    [Widget] RadioMenuItem viewRDVViewRecords;
    [Widget] RadioMenuItem viewRDVEditRecords;
    [Widget] CheckMenuItem viewRecordDetails;
    [Widget] CheckMenuItem viewFullScreen;
    [Widget] MenuItem viewColumns;

    // Help menu
    [Widget] MenuItem helpAbout;

    // Tool bar
    [Widget] ToolButton toolbuttonNew;
    [Widget] ToolButton toolbuttonOpen;
    [Widget] ToolButton toolbuttonSave;
    [Widget] ToolButton toolbuttonSaveAs;
    [Widget] ToolButton toolbuttonAddRecord;
    [Widget] ToolButton toolbuttonDelRecord;

    [Widget] Gtk.Entry searchEntry;

    // Tree view
    [Widget] Gtk.VPaned vpane;
    [Widget] TreeView litTreeView;

    // Record details
    [Widget] Gtk.VBox recordDetailsView;
    [Widget] Gtk.VBox recordView;
    [Widget] Gtk.Image recordIcon;
    [Widget] Gtk.Label recordDetails;
    [Widget] Gtk.VBox recordEditor;

    [Widget] Gtk.ToggleButton toggleEditRecords;

    // Choose Column Dialog
    [Widget] Gtk.Dialog chooseColumnsDialog;
    [Widget] Gtk.HBox columnChecklistHbox;
    //[Widget] Gtk.Button chooseColumnsCloseButton;

    // Side Bar widgets
    [Widget] Gtk.ScrolledWindow scrolledwindowSidePane;
    [Widget] Gtk.TreeView sidePaneTreeView;
    [Widget] Gtk.MenuItem viewSideBar;

    [Widget] ComboBox comboRecordType;
    [Widget] Button buttonReqBibtexKeyGenerate;
    [Widget] Gtk.Entry entryReqBibtexKey;

    // notebook things
    [Widget] Notebook notebookFields;
    [Widget] Viewport viewportRequired, viewportOptional, viewportOther, viewportBibliographerData;

    // Required fields form

    // Optional fields form

    // File open dialog
    [Widget] FileChooserDialog fileOpenDialog;
    [Widget] Gtk.Button fileOpenCancelButton;
    [Widget] Gtk.Button fileOpenOpenButton;

    // File save dialog
    [Widget] FileChooserDialog fileSaveDialog;
    [Widget] Button fileSaveCancelButton;
    [Widget] Button fileSaveSaveButton;

    // Folder import dialog
    [Widget] FileChooserDialog folderImportDialog;
    //[Widget] Button folderImportCancelButton;
    //[Widget] Button folderImportOpenButton;

    [Widget] Dialog bibtexEntryDialog;
    [Widget] TextView bibtexData;

    // data members
    private BibtexRecords bibtexRecords;
    private SidePaneTreeStore sidePaneStore;
    private LitListStore litStore;
    private TreeModelFilter modelFilter;
    private TreeModelFilter fieldFilter;
    private TreeModelSort sorter;
    private string file_name;
    private bool file_modified;
    private bool new_selected_record;

    // Thread related members
    public System.Threading.Thread indexerThread, alterationMonitorThread;
    private Queue indexerQueue, alterationMonitorQueue;

    string application_name = "Bibliographer";

    bool editRecordsMode;

    private static Gtk.TargetEntry []    target_table =
        new TargetEntry []    {
        new TargetEntry ("text/uri-list", 0, 0),
    };

    public BibliographerUI()
    {
        Glade.XML gxml = new Glade.XML (null, "gui.glade", "mainWindow", null);

        gxml.Autoconnect (this);

        //entryReqBibtexKey.Changed += OnBibtexKeyChanged;
		entryReqBibtexKey.Changed += OnBibtexKeyChanged;

        indexerThread = new System.Threading.Thread(new ThreadStart(IndexerThread));
        indexerQueue = new Queue();

        alterationMonitorThread = new System.Threading.Thread(new ThreadStart(AlterationMonitorThread));
        alterationMonitorQueue = new Queue();
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

    private void RenderColumnPixbufFromBibtexRecord(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
    {
		if (model != null)
		{
	        BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);

			if ((cell != null) && (record != null) && (record.GetSmallThumbnail() != null))
		        (cell as Gtk.CellRendererPixbuf).Pixbuf = record.GetSmallThumbnail();
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

    private void OnBibtexRecordsModified(object o, EventArgs a)
    {
        file_modified = true;
        // Refresh the Article Type field
        UpdateTitle();
        UpdateRecordTypeCombo();
    }
    
    private void UpdateRecordTypeCombo()
    {
        TreeIter iter;
        TreeModel model;
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
            buttonReqBibtexKeyGenerate.Sensitive = false;
            new_selected_record = false;
        }
    }
    
    private void OnColumnSort(object o, EventArgs a)
    {
        TreeViewColumn col = (TreeViewColumn) o;
        Debug.WriteLine(5, "OnColumnSort: Column ID is {0} and {1}", col.SortColumnId, col.SortOrder.ToString());
		
    	int sortColumn;
    	SortType sortType;
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

    public int StringCompare(TreeModel model, TreeIter tia, TreeIter tib)
    {
        BibtexRecord a = (BibtexRecord) model.GetValue(tia, 0);
        BibtexRecord b = (BibtexRecord) model.GetValue(tib, 0);
        string A, B;
        string sortString = "";
    	int sortColumn;
    	SortType sortType;
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
    
    public int StringCompareAuthor(TreeModel model, TreeIter tia, TreeIter tib)
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

    public void Init()
    {
        // Assign callbacks to events
        mainWindow.DeleteEvent += OnWindowDeleteEvent;

        fileNew.Activated += OnFileNewActivate;
        fileOpen.Activated += OnFileOpenActivate;
        fileSave.Activated += OnFileSaveActivate;
        fileSaveAs.Activated += OnFileSaveAsActivate;
        fileQuit.Activated += OnFileQuitActivate;
        importFolder.Activated += OnImportFolderActivate;
        fromBibtexAddRecord.Activated += OnAddRecordFromBibtex;
        fromClipboardAddRecord.Activated += OnAddRecordFromClipboard;
        bibtexAddRecord.Activated += OnAddRecord;
        bibtexDelRecord.Activated += OnDelRecord;
        viewRDVViewRecords.Activated += OnToggleViewRecordsActivated;
        viewRDVEditRecords.Activated += OnToggleEditRecordsActivated;
        viewRecordDetails.Activated += OnToggleRecordDetailsActivated;
        viewFullScreen.Activated += OnToggleFullScreenActivated;
        viewSideBar.Activated += OnToggleSideBarActivated;
        viewColumns.Activated += OnChooseColumnsActivated;
        helpAbout.Activated += OnHelpAboutActivate;

        // Toolbutton
        toolbuttonNew.Clicked += OnFileNewActivate;
        toolbuttonOpen.Clicked += OnFileOpenActivate;
        toolbuttonSave.Clicked += OnFileSaveActivate;
        toolbuttonSaveAs.Clicked += OnFileSaveAsActivate;
        toolbuttonAddRecord.Clicked += OnAddRecord;
        toolbuttonDelRecord.Clicked += OnDelRecord;

        // Search entry
        searchEntry.Changed += OnFilterEntryChanged;

        // Record details
        toggleEditRecords.Toggled += OnToggleEditRecords;
        recordDetails.UseMarkup = true;

        // Record type
        comboRecordType.Changed += OnComboRecordTypeChanged;
        // Bibtex key
        // TODO: fixup the callbacks
        buttonReqBibtexKeyGenerate.Clicked += OnButtonBibtexKeyGenerateClicked;
        //entryReqBibtexKey.Changed += OnEntryReqBibtexKeyChanged;

        // Required tab

        // Optional tab

        litTreeView.Selection.Changed += OnTreeViewSelectionChanged;
        litTreeView.DragDataReceived += OnDragDataReceived;
        litTreeView.DragMotion += OnDragMotion;
        litTreeView.DragLeave += OnDragLeave;
        litTreeView.RowActivated += OnRowActivated;

        mainWindow.WindowStateEvent += OnWindowStateEvent;
        mainWindow.SizeAllocated += OnWindowSizeAllocated;

        // Set up main window defaults
        mainWindow.WidthRequest = 600;
        mainWindow.HeightRequest = 600;

        int width, height;

        if (Config.KeyExists("window_width"))
            width = Config.GetInt("window_width");
        else
            width = 600;
        if (Config.KeyExists("window_height"))
            height = Config.GetInt("window_height");
        else
            height = 600;
        mainWindow.Resize(width, height);
        if (Config.KeyExists("window_maximized"))
        	if (Config.GetBool("window_maximized") == true)
	        	mainWindow.Maximize();

        mainWindow.SetPosition(Gtk.WindowPosition.Center);
        mainWindow.Title = application_name;
        mainWindow.Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("bibliographer.png"));

        // Main bibtex view list model
        //store = new ListStore(typeof(BibtexRecord));
        bibtexRecords = new BibtexRecords();
        bibtexRecords.RecordsModified += OnBibtexRecordsModified;
        litStore = new LitListStore(bibtexRecords);

        modelFilter = new Gtk.TreeModelFilter(litStore, null);
        fieldFilter = new Gtk.TreeModelFilter(modelFilter, null);

        modelFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (ModelFilterListStore);
        fieldFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FieldFilterListStore);

        sorter = new TreeModelSort(fieldFilter);

        litTreeView.Model = sorter;

        // TODO: Perform this more elegantly
        // Possibly, read out fields from the bibtex record spec file
        // and make a certain set of columns visible by default
        Gtk.TreeViewColumn [] columnarray;
        columnarray = new Gtk.TreeViewColumn[8];

        if (Config.KeyExists("Columns/Author/order") && Config.KeyExists("Columns/Title/order") && Config.KeyExists("Columns/Year/order") && Config.KeyExists("Columns/Journal/order") && Config.KeyExists("Columns/Bibtex Key/order") && Config.KeyExists("Columns/Volume/order") && Config.KeyExists("Columns/Pages/order"))
        {
            columnarray[Config.GetInt("Columns/Icon/order")] = new Gtk.TreeViewColumn("Icon", new CellRendererPixbuf(), "image");
            columnarray[Config.GetInt("Columns/Author/order")] = new Gtk.TreeViewColumn("Author", new CellRendererText(), "text");
            columnarray[Config.GetInt("Columns/Title/order")] = new Gtk.TreeViewColumn("Title", new CellRendererText(), "text");
            columnarray[Config.GetInt("Columns/Year/order")] = new Gtk.TreeViewColumn("Year", new CellRendererText(), "text");
            columnarray[Config.GetInt("Columns/Journal/order")] = new Gtk.TreeViewColumn("Journal", new CellRendererText(), "text");
            columnarray[Config.GetInt("Columns/Bibtex Key/order")] = new Gtk.TreeViewColumn("Bibtex Key", new CellRendererText(), "text");
            columnarray[Config.GetInt("Columns/Volume/order")] = new Gtk.TreeViewColumn("Volume", new CellRendererText(), "text");
            columnarray[Config.GetInt("Columns/Pages/order")] = new Gtk.TreeViewColumn("Pages", new CellRendererText(), "text");
        }
        else
        {
            columnarray[0] = new Gtk.TreeViewColumn("Icon", new CellRendererPixbuf(), "image");
            columnarray[1] = new Gtk.TreeViewColumn("Author", new CellRendererText(), "text");
            columnarray[2] = new Gtk.TreeViewColumn("Title", new CellRendererText(), "text");
            columnarray[3] = new Gtk.TreeViewColumn("Year", new CellRendererText(), "text");
            columnarray[4] = new Gtk.TreeViewColumn("Journal", new CellRendererText(), "text");
            columnarray[5] = new Gtk.TreeViewColumn("Bibtex Key", new CellRendererText(), "text");
            columnarray[6] = new Gtk.TreeViewColumn("Volume", new CellRendererText(), "text");
            columnarray[7] = new Gtk.TreeViewColumn("Pages", new CellRendererText(), "text");
        }

        foreach (TreeViewColumn column in columnarray)
        {
            litTreeView.AppendColumn(column);
        }
        litTreeView.HeadersClickable = true;

        Gtk.TreeCellDataFunc textDataFunc = new Gtk.TreeCellDataFunc(RenderColumnTextFromBibtexRecord);
        Gtk.TreeCellDataFunc pixmapDataFunc = new Gtk.TreeCellDataFunc(RenderColumnPixbufFromBibtexRecord);
        int id = 0;

        foreach (TreeViewColumn column in litTreeView.Columns)
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
        Gtk.TreeViewColumn col = new Gtk.TreeViewColumn("Filter", new CellRendererText(), "text");

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
                viewSideBar.Activate();
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
        Gtk.Drag.DestSet (litTreeView, DestDefaults.All, target_table,
                          Gdk.DragAction.Copy);

        UpdateMenuFileHistory();

        // Activate new file
        fileNew.Activate();
        viewRDVEditRecords.Activate();
        ReconstructTabs();
        ReconstructDetails();
        mainWindow.Show();
    }
    
    private void OnWindowSizeAllocated(object o, Gtk.SizeAllocatedArgs a)
    {
    	if (Config.GetBool("window_maximized") == false)
    	{
			Config.SetInt("window_width", a.Allocation.Width);
			Config.SetInt("window_height", a.Allocation.Height);
		}
    }

	private void OnWindowStateEvent(object o, WindowStateEventArgs a)
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
	        mainWindow.Resize(Config.GetInt("window_width"), Config.GetInt("window_height"));
		}
	}

    private void OnSidePaneTreeSelectionChanged(object o, EventArgs a)
    {
        fieldFilter.Refilter();
    }

    private void OnChooseColumnsActivated(object o, EventArgs a)
    {
        Glade.XML gxml = new Glade.XML (null, "gui.glade", "chooseColumnsDialog", null);
        gxml.BindFields(this);
        gxml.Autoconnect (this);

        int rows = 5;
        int i = 0;
        Gtk.VBox vbox;
        vbox = new Gtk.VBox();
        columnChecklistHbox.Add(vbox);
        vbox.Show();
        foreach (Gtk.TreeViewColumn column in litTreeView.Columns)
        {
            Gtk.CheckButton checkbutton;

            checkbutton = new Gtk.CheckButton();
            checkbutton.Active = column.Visible;
            checkbutton.Label = column.Title;
            checkbutton.Show();

            vbox.Add(checkbutton);

            if (i == rows - 1)
            {
                vbox = new Gtk.VBox();
                columnChecklistHbox.Add(vbox);
                vbox.Show();
                i = 0;
            }
            i = i + 1;
        }
        //chooseColumnsDialog.Show();
        Gtk.ResponseType result = (Gtk.ResponseType)chooseColumnsDialog.Run();
        if (result == Gtk.ResponseType.Close) {
            try {
                foreach (Gtk.VBox vbox1 in columnChecklistHbox.Children)
                {
                    foreach (Gtk.CheckButton checkbutton in vbox1.Children)
                    {
                        foreach (Gtk.TreeViewColumn column in litTreeView.Columns)
                        {
                            if (column.Title == checkbutton.Label)
                            {
                                column.Visible = checkbutton.Active;
                                if (Config.KeyExists("Columns/"+column.Title+"/width"))
                                    column.FixedWidth = Config.GetInt("Columns/"+column.Title+"/width");
                                else
                                    column.FixedWidth = 100;
                            }
                        }
                    }
                }
            } catch (ParseException e) {
                Debug.WriteLine(5, "Parse exception: {0}", e.GetReason());
            }
        }
        chooseColumnsDialog.Hide();
    }

    private void OnToggleSideBarActivated(object o, EventArgs a)
    {
        if (scrolledwindowSidePane.Visible == true)
            scrolledwindowSidePane.Visible = false;
        else
            scrolledwindowSidePane.Visible = true;
    }

    private void OnToggleEditRecordsActivated(object o, EventArgs a)
    {
        recordView.Visible = false;
        recordEditor.Visible = true;
        vpane.Position = vpane.MaxPosition - 350;
        if (this.editRecordsMode == false)
            toggleEditRecords.Active = true;
        this.editRecordsMode = true;
    }

    private void OnToggleViewRecordsActivated(object o, EventArgs a)
    {
        recordView.Visible = true;
        recordEditor.Visible = false;
        ReconstructDetails();
        vpane.Position = vpane.MaxPosition - 150;
        if (this.editRecordsMode == true)
            toggleEditRecords.Active = false;
        this.editRecordsMode = false;
    }

    private void OnToggleEditRecords(object o, EventArgs a)
    {
        ToggleButton button = (ToggleButton) o;
        if (button.Active == true)
        {
            if (this.editRecordsMode == false)
                viewRDVEditRecords.Active = true;
            this.editRecordsMode = true;
        }
        else
        {
            if (this.editRecordsMode == true)
                viewRDVViewRecords.Active = true;
            this.editRecordsMode = false;
        }
    }

    private void OnToggleFullScreenActivated(object o, EventArgs a)
    {
        CheckMenuItem button = (CheckMenuItem)o;

        if (button.Active == true)
        {
            mainWindow.Fullscreen();
        }
        else
        {
            mainWindow.Unfullscreen();
        }
    }

    private void OnToggleRecordDetailsActivated(object o, EventArgs a)
    {
        CheckMenuItem button = (CheckMenuItem)o;

        if (button.Active == false)
        {
            recordDetailsView.Visible = false;
        }
        else
        {
            recordDetailsView.Visible = true;
        }
    }

    private void OnFilterEntryChanged(object o, EventArgs a)
    {
        // Filter when the filter entry text has changed
        modelFilter.Refilter();
    }

    private bool FieldFilterListStore(Gtk.TreeModel model, Gtk.TreeIter iter)
    {
        // TODO: add support for searching certain columns eg. Author / Journal / Title etc...
        // This is possible by implimenting a method such as record.searchField(field, text)

        BibtexRecord record = (BibtexRecord) model.GetValue (iter, 0);

        TreeIter iterFilter;
        TreeModel modelFilter;

        if (sidePaneTreeView.Selection.GetSelected(out modelFilter, out iterFilter))
        {
            TreeIter iterParent;
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

    private void OnBibtexKeyChanged(object o, EventArgs a)
    {
        // the next check stops bad things from happening when
        // the user selects a new record in the list view,
        // causing the checkbox to get updated. In this case,
        // we really don't want to be calling this method
        if (new_selected_record == true)
            return;

        TreeIter iter;
        TreeModel model;

        BibtexGenerateKeySetStatus();
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            Gtk.Entry entry = (Gtk.Entry)o;
            BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
            record.SetKey(entry.Text);
        }
    }

    class FieldEntry : Gtk.Entry
    {
        public string field = "";
    };

    class FieldButton : Gtk.Button
    {
        public string field = "";
    }

    private void OnFieldChanged(object o, EventArgs a)
    {
        TreeIter iter;
        TreeModel model;
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            FieldEntry entry = (FieldEntry) o;
            BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
            record.SetField(entry.field, entry.Text);
            model.EmitRowChanged(model.GetPath(iter), iter);
            BibtexGenerateKeySetStatus();
        }
    }

    private void OnFieldRemoved(object o, EventArgs a)
    {
        TreeIter iter;
        TreeModel model;
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            FieldButton button = (FieldButton) o;
            BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
            record.RemoveField(button.field);
            ReconstructTabs();
            ReconstructDetails();
        }
    }

    private void OnExtraFieldAdded(object o, EventArgs a)
    {
        TreeIter iter;
        TreeModel model;
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            ComboBox combo = (ComboBox) o;
            TreeIter comboIter;
            if (combo.GetActiveIter(out comboIter))
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
                record.SetField((string) combo.Model.GetValue(comboIter, 0), "");
                ReconstructTabs();
                ReconstructDetails();
            }
        }
    }

    public void OnURIBrowseClicked(object o, EventArgs a)
    {
        TreeIter iter;
        TreeModel model;
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
            Glade.XML gxml2 = new Glade.XML (null, "gui.glade",
                                             "fileOpenDialog", null);
            gxml2.BindFields(this);
            gxml2.Autoconnect (this);

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

            switch (fileOpenDialog.Run()) {
            case (int) ResponseType.Ok:
                // got it
                record.SetField("bibliographer_uri", Gnome.Vfs.Uri.GetUriFromLocalPath(fileOpenDialog.Filename));
                Config.SetString("uri_browse_path", fileOpenDialog.CurrentFolder);
                ReconstructTabs();
                ReconstructDetails();
                break;
            }
            fileOpenDialog.Hide();
        }
    }

    public void ReconstructDetails()
    {
        // step 1: reset values
        recordIcon.Pixbuf = null;
        recordDetails.Text = null;

        // step 2: add new values
        TreeIter iter;
        TreeModel model;
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
        VBox req = new VBox(false, 5);
        VBox opt = new VBox(false, 5);
        VBox other = new VBox(false, 5);
        VBox bData = new VBox(false, 5);

        TreeIter iter;
        TreeModel model;
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            comboRecordType.Sensitive = true;

            if (comboRecordType.Active < 0)
            {
                entryReqBibtexKey.Sensitive = false;
                notebookFields.Sensitive = false;
                buttonReqBibtexKeyGenerate.Sensitive = false;
            }
            else
            {
                notebookFields.Sensitive = true;
                entryReqBibtexKey.Sensitive = true;
                BibtexGenerateKeySetStatus();
            }
            //	Console.WriteLine("Combo box active: " + comboRecordType.Active);

            BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
            uint numItems;
            BibtexRecordType recordType = null;
            if (BibtexRecordTypeLibrary.Contains(record.RecordType)) {
                recordType = BibtexRecordTypeLibrary.Get(record.RecordType);

                // viewportRequired
                Table tableReq = new Table(0, 2, false);
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
                                Label orLabel = new Label();
                                orLabel.Markup = "<b>or</b>";
                                tableReq.Attach(orLabel, 0, 2, numItems - 2, numItems - 1, 0, 0, 5, 5);
                            }
                            else
                            {
                                numItems++;
                                tableReq.NRows = numItems;
                            }
                            string fieldName = recordType.fields[j];
                            tableReq.Attach(new Label(fieldName), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                            FieldEntry textEntry = new FieldEntry();
                            if (record.HasField(fieldName))
                                textEntry.Text = record.GetField(fieldName);
                            tableReq.Attach(textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                            textEntry.field = fieldName;
                            //textEntry.Changed += OnFieldChanged;
							textEntry.Changed += OnFieldChanged;
                        }
                    }
                    if (subNumItems == 0)
                        break;
                }
                req.PackStart(tableReq, false, false, 5);

                // 	viewportOptional
                Table tableOpt = new Table(0, 2, false);
                numItems = 0;
                for (int i = 0; i < recordType.fields.Count; i++) {
                    if (recordType.optional[i] == 0) {
                        numItems++;
                        tableOpt.NRows = numItems;
                        tableOpt.Attach(new Label(recordType.fields[i]), 0, 1, numItems - 1, numItems, 0, 0, 5, 5);
                        FieldEntry textEntry = new FieldEntry();
                        if (record.HasField(recordType.fields[i]))
                            textEntry.Text = record.GetField(recordType.fields[i]);
                        tableOpt.Attach(textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
                        textEntry.field = recordType.fields[i];
                        textEntry.Changed += OnFieldChanged;
                    }
                }
                opt.PackStart(tableOpt, false, false, 5);
            }

            // viewportOther
            Table tableOther = new Table(0, 3, false);
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
                    Label fieldLabel = new Label();
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
                    tableOther.Attach(textEntry, 1, 2, numItems - 1, numItems, AttachOptions.Expand | AttachOptions.Fill, 0, 5, 5);
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
            ComboBox extraFields = ComboBox.NewText();
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
                HBox hbox = new HBox();
                hbox.PackStart(new Label("Add extra field:"), false, false, 5);
                hbox.PackStart(extraFields, false, false, 5);
                other.PackStart(hbox, false, false, 5);
                extraFields.Changed += OnExtraFieldAdded;
            }
            else
            {
                Label noExtraFields = new Label();
                noExtraFields.Markup = "<i>No extra fields</i>";
                other.PackStart(noExtraFields, false, false, 5);
            }

            // viewportBibliographerData
            HBox uriHBox = new HBox();
            uriHBox.PackStart(new Label("Associated file:"), false, false, 5);
            FieldEntry uriEntry = new FieldEntry();
            if (record.HasField("bibliographer_uri"))
                uriEntry.Text = record.GetField("bibliographer_uri");
            uriEntry.field = "bibliographer_uri";
            uriEntry.Changed += OnFieldChanged;
            uriHBox.PackStart(uriEntry, false, false, 5);
            Button uriBrowseButton = new Button("Browse");
            uriBrowseButton.Activated += OnURIBrowseClicked;
            uriBrowseButton.Clicked += OnURIBrowseClicked;
            uriHBox.PackStart(uriBrowseButton, false, false, 5);
            bData.PackStart(uriHBox, false, false, 5);
        }
        else
        {
            notebookFields.Sensitive = false;
            entryReqBibtexKey.Sensitive = false;
            buttonReqBibtexKeyGenerate.Sensitive = false;
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

    private void OnComboRecordTypeChanged (object o, EventArgs a)
    {
        // the next check stops bad things from happening when
        // the user selects a new record in the list view,
        // causing the checkbox to get updated. In this case,
        // we really don't want to be calling this method
        if (new_selected_record)
            return;

        TreeIter iter;
        TreeModel model;
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            if (((ComboBox) o).Active != -1) {
                string bType = BibtexRecordTypeLibrary.GetWithIndex(((ComboBox) o).Active).name;
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
    private void OnTreeViewSelectionChanged (object o, EventArgs a)
    {
        //Console.WriteLine("Selection changed");
        TreeIter iter;
        TreeModel model;

        if (((TreeSelection)o).GetSelected(out model, out iter))
        {
            BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
            string recordType = record.RecordType;
            new_selected_record = true;
            if (BibtexRecordTypeLibrary.Contains(recordType))
                comboRecordType.Active = BibtexRecordTypeLibrary.Index(recordType);
            else
                comboRecordType.Active = -1;

            entryReqBibtexKey.Text = record.GetKey();
            buttonReqBibtexKeyGenerate.Sensitive = false;
            new_selected_record = false;

            // Interrogate ListStore for values
            // TODO: fix!
        }
        else {
            buttonReqBibtexKeyGenerate.Sensitive = false;
        }
        ReconstructTabs();
        ReconstructDetails();
    }

    private void OnImportFolderActivate (object o, EventArgs a)
    {
        Glade.XML gxml2 = new Glade.XML (null, "gui.glade",
                                         "folderImportDialog", null);
        gxml2.BindFields(this);
        gxml2.Autoconnect(this);

        // query config for stored path
        if (Config.KeyExists("bib_import_path"))
            folderImportDialog.SetCurrentFolder(Config.GetString("bib_import_path"));

        switch (folderImportDialog.Run())
        {
        case (int) ResponseType.Ok:
        	string folderImportPath = folderImportDialog.Filename;
        	string folderImportCurrentPath = folderImportDialog.CurrentFolder;
	        folderImportDialog.Hide();
	        folderImportDialog.Destroy();
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration (); 
            
            Config.SetString("bib_import_path", folderImportCurrentPath);
            Debug.WriteLine(5, "Importing folder: {0}", folderImportPath);
            
            InsertFilesInDir(folderImportPath);
            
            break;
         case (int) ResponseType.Cancel:
	        folderImportDialog.Hide();
	        folderImportDialog.Destroy();
	        break;
        }
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

    private void OnFileOpenActivate (object o, EventArgs a)
    {

        Glade.XML gxml2 = new Glade.XML (null, "gui.glade",
                                         "fileOpenDialog", null);
        gxml2.BindFields(this);
        gxml2.Autoconnect (this);

        fileOpenDialog.Filter = new Gtk.FileFilter();
        fileOpenDialog.Filter.AddPattern("*.bib");

        fileOpenDialog.FileActivated += OnFileOpenFileActivated;
        fileOpenCancelButton.Clicked += OnFileOpenCancelButtonClicked;
        fileOpenOpenButton.Clicked += OnFileOpenFileActivated;

        // query config for stored path
        if (Config.KeyExists("bib_browse_path"))
            fileOpenDialog.SetCurrentFolder(Config.GetString("bib_browse_path"));

        fileOpenDialog.Run();
    }

    private void OnFileSaveActivate (object o, EventArgs a)
    {
        FileSave();
    }

    private void OnFileSaveAsActivate (object o, EventArgs a)
    {
        FileSaveAs();
    }

    private int FileSaveAs()
    {
        Glade.XML gxml2 = new Glade.XML (null, "gui.glade",
                                         "fileSaveDialog", null);
        gxml2.Autoconnect (this);

        // TODO: filter for *.bib files only :)
        fileSaveDialog.Filter = new Gtk.FileFilter();
        fileSaveDialog.Filter.AddPattern("*.bib");

        // TODO: connect up save buttons
        //fileSaveDialog.FileActivated += OnFileSaveFileActivated;
        fileSaveCancelButton.Clicked += OnFileSaveCancelButtonClicked;
        fileSaveSaveButton.Clicked += OnFileSaveFileActivated;

        if (Config.KeyExists("bib_browse_path"))
            fileSaveDialog.SetCurrentFolder(Config.GetString("bib_browse_path"));

        return fileSaveDialog.Run();
    }

    private void OnFileSaveFileActivated (object o, EventArgs a)
    {
        Config.SetString("bib_browse_path", fileSaveDialog.CurrentFolder);
        if (fileSaveDialog.Filename != null)
        {
            file_name = fileSaveDialog.Filename;
            FileSave();
            UpdateFileHistory(file_name);
            fileSaveDialog.Destroy();
        }
    }

    private int FileSave ()
    {
        if (file_name != "Untitled.bib")
        {
            //BibtexListStoreParser btparser = new BibtexListStoreParser(
            //	store);
            //btparser.Save(file_name);
            bibtexRecords.Save(file_name);
            file_modified = false;
            UpdateTitle();
            return (int) ResponseType.Ok;
        }
        else
        {
            return FileSaveAs();
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

    private void FileHistoryActivate(object o, EventArgs a)
    {
        MenuItem item = (MenuItem) o;
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

    private void ClearFileHistory(object o, EventArgs a)
    {
    	Debug.WriteLine(5, "Clearing file history");
        ArrayList temp = new ArrayList();
        // Workaround for clear file history bug - set history to contain a single empty string
        temp.Add("");
        Config.SetKey("file_history", temp.ToArray());
        UpdateMenuFileHistory();
    }

    private void UpdateMenuFileHistory()
    {
        Debug.WriteLine(5, "File History - doing update...");
        // step 1: clear the menu
        foreach (Widget w in recentFilesMenu)
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
	                MenuItem item = new MenuItem(label);
	                item.Activated += FileHistoryActivate;
					item.Data.Add("i", (System.IntPtr)i);
	                recentFilesMenu.Append(item);
	                gotOne = true;
                }
            }
        }
        if (gotOne)
        {
            recentFilesMenu.Append(new SeparatorMenuItem());
            MenuItem clear = new MenuItem("Clear Recent Files");
            clear.Activated += ClearFileHistory;
            recentFilesMenu.Append(clear);
        }
        else
        {
            MenuItem none = new MenuItem("(none)");
            // want to disable this somehow...
            //none. = false;
            recentFilesMenu.Append(none);
        }

        recentFilesMenu.ShowAll();
    }

    private void OnFileOpenFileActivated (object o, EventArgs a)
    {
        Config.SetString("bib_browse_path", fileOpenDialog.CurrentFolder);
        if (fileOpenDialog.Filename != null)
        {
            file_name = fileOpenDialog.Filename;
            this.FileOpen(file_name);
            UpdateFileHistory(file_name);
            fileOpenDialog.Destroy();
            // TODO: Verify document integrity
            // (search for conflicting bibtexkeys)
            // (highlite any uncomplete entries)
        }
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
        file_modified = false;
        UpdateTitle();
        // Disable editing of opened document
        //DisableEditing();
        viewRDVViewRecords.Activate();
        // Update side bar
    }

    private void OnFileOpenCancelButtonClicked (object o, EventArgs a)
    {
        fileOpenDialog.Destroy();
    }

    private void OnFileSaveCancelButtonClicked (object o, EventArgs a)
    {
        fileSaveDialog.Destroy();
    }

    private bool ProcessModifiedData()
    {
        // display a dialog asking the user if they
        // want to save their changes (offering them
        // Yes/No/Cancel. If they choose Yes, call
        // FileSave and return true. If they choose
        // No, just return true. If they choose
        // Cancel, return false
        MessageDialog dialog = new MessageDialog(mainWindow, DialogFlags.Modal, MessageType.Question, ButtonsType.None, "{0} has been modified. Do you want to save it?", file_name);
        dialog.AddButton("Yes", ResponseType.Yes);
        dialog.AddButton("No", ResponseType.No);
        dialog.AddButton("Cancel", ResponseType.Cancel);
        int result = dialog.Run();
        dialog.Hide();
        switch (result) {
        case (int) ResponseType.Yes:
            switch (FileSave()) {
            case (int) ResponseType.Ok:
                return true;
            case (int) ResponseType.Cancel:
                return false;
            default:
                return false;
            }
        case (int) ResponseType.No:
            return true;
        case (int) ResponseType.Cancel:
            return false;
        default:
            // eek
            MessageDialog error = new MessageDialog(mainWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "An unexpected error occurred in processing your response, please contact the developers!");
            error.Run();
            error.Hide();
            return false;
        }
    }

    private void OnFileNewActivate (object o, EventArgs a)
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
        file_modified = false;
        UpdateTitle();
    }

    private bool IsModified()
    {
        return file_modified;
    }

    private void UpdateTitle()
    {
        if (file_modified == false)
        {
            mainWindow.Title = application_name + " - " + file_name;
        }
        else
        {
            mainWindow.Title = application_name + " - " + file_name+"*";
        }
    }

    private void OnFileQuitActivate (object o, EventArgs a)
    {
        Quit();
    }

    private void OnWindowDeleteEvent (object o, DeleteEventArgs a)
    {
        Quit();
        a.RetVal = true;
    }

    private void Quit()
    {
        // Save column states
        int i = 0;
        foreach (TreeViewColumn column in litTreeView.Columns)
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

        Application.Quit ();
    }

    private void OnDragDataReceived (object o, DragDataReceivedArgs a)
    {
        //Console.WriteLine("Data received is of type '" + a.SelectionData.Type.Name + "'");
        // the atom type we want is a text/uri-list
        // if we get anything else, bounce it
        if (a.SelectionData.Type.Name.CompareTo("text/uri-list") != 0)
        {
            // wrong type
            return;
        }
        //DragDataReceivedArgs args
        string data = System.Text.Encoding.UTF8.GetString (
            a.SelectionData.Data);
        //Split out multiple files
        string []    uri_list = Regex.Split (data, "\r\n");
        TreePath path;
        TreeViewDropPosition drop_position;
        if (!litTreeView.GetDestRowAtPos(a.X, a.Y, out path, out drop_position))
        {
            Debug.WriteLine(5, "Failed to drag and drop because of GetDestRowAtPos failure");
            return;
        }
        TreeIter iter; //, check;
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

    private void OnDragMotion(object o, DragMotionArgs a)
    {
        // FIXME: how do we check from here if that drag has data that we want?

        TreePath path;
        TreeViewDropPosition drop_position;
        if (litTreeView.GetDestRowAtPos(a.X, a.Y, out path, out drop_position)) {
            litTreeView.SetDragDestRow(path, TreeViewDropPosition.IntoOrAfter);
        }
        else
            litTreeView.UnsetRowsDragDest();
    }

    private void OnDragLeave(object o, DragLeaveArgs a)
    {
        litTreeView.UnsetRowsDragDest();
    }

    private void OnRowActivated(object o, RowActivatedArgs a)
    {
        Debug.WriteLine(5, "Row activated");

        TreeIter iter;
        BibtexRecord record;

        if (!litTreeView.Model.GetIter(out iter, a.Path))
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

        List list = new List(typeof(String));
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
            MessageDialog md = new Gtk.MessageDialog (mainWindow,
                                                      DialogFlags.DestroyWithParent,
                                                      MessageType.Error,
                                                      ButtonsType.Close, "Error loading associated file:\n" + Gnome.Vfs.Uri.GetLocalPathFromUri(uriString));
            //int result = md.Run ();
            md.Run();
            md.Destroy();
            Debug.WriteLine(0, "Error loading associated file:\n{0}", Gnome.Vfs.Uri.GetLocalPathFromUri(uriString));
        }
    }

    private void OnAddRecord (object o, EventArgs a)
    {
        Debug.WriteLine(5, "Adding a new record");
        //Debug.WriteLine(5, "Prev rec count: {0}", bibtexRecords.Count);
        TreeIter iter;

        if (bibtexRecords == null)
        {
            bibtexRecords = new BibtexRecords();
            bibtexRecords.RecordsModified += OnBibtexRecordsModified;
        }

        BibtexRecord record = new BibtexRecord();
        bibtexRecords.Add(record);

        iter = litStore.GetIter(record);
        litTreeView.Selection.SelectIter(iter);

        BibtexGenerateKeySetStatus();

        System.Threading.Monitor.Enter(alterationMonitorQueue);
        alterationMonitorQueue.Enqueue(record);
        System.Threading.Monitor.Exit(alterationMonitorQueue);
    }

    private void OnAddRecordFromBibtex (object o, EventArgs a)
    {
        TreeIter iter;
        Glade.XML gxml2 = new Glade.XML (null, "gui.glade",
                                         "bibtexEntryDialog", null);
        gxml2.Autoconnect (this);

        ResponseType result = (ResponseType) bibtexEntryDialog.Run();
        if (result == ResponseType.Ok) {
            try {
                BibtexRecord record = new BibtexRecord(bibtexData.Buffer.Text);
                bibtexRecords.Add(record);

                iter = litStore.GetIter(record);
                litTreeView.Selection.SelectIter(iter);

                BibtexGenerateKeySetStatus();

                System.Threading.Monitor.Enter(alterationMonitorQueue);
                alterationMonitorQueue.Enqueue(record);
                System.Threading.Monitor.Exit(alterationMonitorQueue);
            } catch (ParseException e) {
                Debug.WriteLine(1, "Parse exception: {0}", e.GetReason());
            }
        }

        bibtexData.Buffer.Text = "";
        bibtexEntryDialog.Hide();
    }

    private void OnAddRecordFromClipboard (object o, EventArgs a)
    {
        TreeIter iter;
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

    private void BibtexGenerateKeySetStatus()
    {
        TreeIter iter;
        TreeModel model;

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
                        buttonReqBibtexKeyGenerate.Sensitive = true;
                    // Bibtex Entry is not empty, so the generate key is not sensitive
                    else
                        buttonReqBibtexKeyGenerate.Sensitive = false;
                }
                // Author and year fields are empty
                else
                    buttonReqBibtexKeyGenerate.Sensitive = false;
            }
            // Record does not have Author and Year fields
            else
                buttonReqBibtexKeyGenerate.Sensitive = false;
        }
        // A Bibtex record is not selected
        else
            buttonReqBibtexKeyGenerate.Sensitive = false;
    }

    private void OnDelRecord (object o, EventArgs a)
    {
        TreeIter iter;
        TreeModel model;

        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            // model and iter are TreeModelSort types
            // Obtain the search model
            TreeModel searchModel = ((TreeModelSort) model).Model;
            TreeIter searchIter = ((TreeModelSort) model).ConvertIterToChildIter(iter);

            // Obtain the filter model
            TreeModel filterModel = ((TreeModelFilter) searchModel).Model;
            TreeIter filterIter = ((TreeModelFilter) searchModel).ConvertIterToChildIter(searchIter);

            // Obtain the real model
            TreeModel realModel = ((TreeModelFilter) filterModel).Model;
            TreeIter realIter = ((TreeModelFilter) filterModel).ConvertIterToChildIter(filterIter);

            // Delete record from the real model
            //((ListStore) realModel).Remove(ref realIter);
            BibtexRecord record = ((ListStore) realModel).GetValue(realIter,0) as BibtexRecord;
            bibtexRecords.Remove(record);

        }
    }

    private void OnButtonBibtexKeyGenerateClicked (object o, EventArgs a)
    {
        TreeIter iter;
        TreeModel model;
        if (litTreeView.Selection.GetSelected(out model, out iter))
        {
            // TODO: check if this is the correct bibtexRecordFieldType
            // (it currently doesn't work for types that don't have an author
            // or year field)

            BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);
            string authors = record.HasField("author") ? (record.GetField("author").ToLower()).Trim() : "";
            string year = record.HasField("year") ? record.GetField("year").Trim() : "";

            //Console.WriteLine("authors: " + authors);

            authors = authors.Replace(" and ", "&");
            string []   authorarray = authors.Split(("&").ToCharArray());

            if (authorarray.Length > 0)
            {
                ArrayList authorsurname = new ArrayList();
                foreach (string author in authorarray)
                {
                    //Console.WriteLine(author);
                    // Deal with format of "Surname, Firstname ..."
                    if (author.IndexOf(",")>0)
                    {
                        string []   authorname = author.Split(',');
                        //Console.WriteLine("Surname: " + authorname[0]);
                        authorsurname.Add(authorname[0]);
                    }
                    // Deal with format of "Firstname ... Surname"
                    else
                    {
                        string []   authorname = author.Split(' ');
                        //Console.WriteLine("Surname: " + authorname[authorname.Length - 1]);
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

    private void OnHelpAboutActivate (object o, EventArgs a)
    {
        AboutBox ab = new AboutBox();
        ab.Run();
        ab.Destroy();
    }

}
}