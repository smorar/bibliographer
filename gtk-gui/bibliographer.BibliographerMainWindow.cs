// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace bibliographer {
    
    
    public partial class BibliographerMainWindow {
        
        private Gtk.UIManager UIManager;
        
        private Gtk.Action FileAction;
        
        private Gtk.Action EditAction;
        
        private Gtk.Action ViewAction;
        
        private Gtk.Action HelpAction;
        
        private Gtk.Action FileNewAction;
        
        private Gtk.Action FileOpenAction;
        
        private Gtk.Action SaveAction;
        
        private Gtk.Action FileSaveAsAction;
        
        private Gtk.Action ImportFolderAction;
        
        private Gtk.Action RecentFilesAction;
        
        private Gtk.Action QuitAction;
        
        private Gtk.Action AddRecordAction;
        
        private Gtk.Action RemoveRecordAction;
        
        private Gtk.Action AddRecordFromBibTeXAction;
        
        private Gtk.Action AddRecordFromClipboardAction;
        
        private Gtk.ToggleAction SidebarAction;
        
        private Gtk.Action RecordDetailsViewAction;
        
        private Gtk.ToggleAction RecordDetailsAction;
        
        private Gtk.ToggleAction FullScreenAction;
        
        private Gtk.Action ColumnsAction;
        
        private Gtk.Action AboutAction;
        
        private Gtk.Action ToolbarNewAction;
        
        private Gtk.Action ToolbarOpenAction;
        
        private Gtk.Action ToolbarSaveAction;
        
        private Gtk.Action ToolbarSaveAsAction;
        
        private Gtk.Action ToolbarAddAction;
        
        private Gtk.Action ToolbarRemoveAction;
        
        private Gtk.RadioAction ViewRecordsAction;
        
        private Gtk.RadioAction EditRecordsAction;
        
        private Gtk.VBox vbox1;
        
        private Gtk.MenuBar menuBar;
        
        private Gtk.HBox hbox1;
        
        private Gtk.Toolbar toolBar1;
        
        private Gtk.HBox searchHbox;
        
        private Gtk.HPaned hpane;
        
        private Gtk.ScrolledWindow scrolledwindowSidePane;
        
        private Gtk.VBox vbox2;
        
        private Gtk.VPaned vpane;
        
        private Gtk.ScrolledWindow scrolledwindowTreeView;
        
        private Gtk.VBox recordDetailsView;
        
        private Gtk.VBox recordView;
        
        private Gtk.HBox recordViewHbox;
        
        private Gtk.Image recordIcon;
        
        private Gtk.Label recordDetails;
        
        private Gtk.VBox recordEditor;
        
        private Gtk.Table table1;
        
        private Gtk.Button buttonBibtexKeyGenerate;
        
        private Gtk.ComboBox comboRecordType;
        
        private Gtk.Entry entryReqBibtexKey;
        
        private Gtk.Label lblBibtexKey;
        
        private Gtk.Label lblRecordType;
        
        private Gtk.Notebook notebookFields;
        
        private Gtk.ScrolledWindow scrolledwindowRqdFields;
        
        private Gtk.Label lblNtbkRequired;
        
        private Gtk.ScrolledWindow scrolledwindowOptional;
        
        private Gtk.Label lblNtbkOptional;
        
        private Gtk.ScrolledWindow scrolledwindowOther;
        
        private Gtk.Label lblNtbkOther;
        
        private Gtk.ScrolledWindow scrolledwindowBibliographerData;
        
        private Gtk.Label lblNtbkBibliographerData;
        
        private Gtk.HButtonBox hbuttonbox1;
        
        private Gtk.ToggleButton toggleEditRecords;
        
        private Gtk.Statusbar statusbar;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget bibliographer.BibliographerMainWindow
            this.UIManager = new Gtk.UIManager();
            Gtk.ActionGroup w1 = new Gtk.ActionGroup("Default");
            this.FileAction = new Gtk.Action("FileAction", Mono.Unix.Catalog.GetString("_File"), null, null);
            this.FileAction.ShortLabel = Mono.Unix.Catalog.GetString("_File");
            w1.Add(this.FileAction, null);
            this.EditAction = new Gtk.Action("EditAction", Mono.Unix.Catalog.GetString("_Edit"), null, null);
            this.EditAction.ShortLabel = Mono.Unix.Catalog.GetString("_Edit");
            w1.Add(this.EditAction, null);
            this.ViewAction = new Gtk.Action("ViewAction", Mono.Unix.Catalog.GetString("_View"), null, null);
            this.ViewAction.ShortLabel = Mono.Unix.Catalog.GetString("_View");
            w1.Add(this.ViewAction, null);
            this.HelpAction = new Gtk.Action("HelpAction", Mono.Unix.Catalog.GetString("_Help"), null, null);
            this.HelpAction.ShortLabel = Mono.Unix.Catalog.GetString("_Help");
            w1.Add(this.HelpAction, null);
            this.FileNewAction = new Gtk.Action("FileNewAction", Mono.Unix.Catalog.GetString("_New"), null, "gtk-new");
            this.FileNewAction.ShortLabel = Mono.Unix.Catalog.GetString("_New");
            w1.Add(this.FileNewAction, null);
            this.FileOpenAction = new Gtk.Action("FileOpenAction", Mono.Unix.Catalog.GetString("_Open"), null, "gtk-open");
            this.FileOpenAction.ShortLabel = Mono.Unix.Catalog.GetString("_Open");
            w1.Add(this.FileOpenAction, null);
            this.SaveAction = new Gtk.Action("SaveAction", Mono.Unix.Catalog.GetString("_Save"), null, "gtk-save");
            this.SaveAction.ShortLabel = Mono.Unix.Catalog.GetString("_Save");
            w1.Add(this.SaveAction, null);
            this.FileSaveAsAction = new Gtk.Action("FileSaveAsAction", Mono.Unix.Catalog.GetString("Save _As"), null, "gtk-save-as");
            this.FileSaveAsAction.ShortLabel = Mono.Unix.Catalog.GetString("Save _As");
            w1.Add(this.FileSaveAsAction, null);
            this.ImportFolderAction = new Gtk.Action("ImportFolderAction", Mono.Unix.Catalog.GetString("Import _Folder"), null, null);
            this.ImportFolderAction.ShortLabel = Mono.Unix.Catalog.GetString("Import _Folder");
            w1.Add(this.ImportFolderAction, null);
            this.RecentFilesAction = new Gtk.Action("RecentFilesAction", Mono.Unix.Catalog.GetString("_Recent files"), null, null);
            this.RecentFilesAction.ShortLabel = Mono.Unix.Catalog.GetString("_Recent files");
            w1.Add(this.RecentFilesAction, null);
            this.QuitAction = new Gtk.Action("QuitAction", Mono.Unix.Catalog.GetString("_Quit"), null, "gtk-quit");
            this.QuitAction.ShortLabel = Mono.Unix.Catalog.GetString("_Quit");
            w1.Add(this.QuitAction, null);
            this.AddRecordAction = new Gtk.Action("AddRecordAction", Mono.Unix.Catalog.GetString("_Add record"), null, "gtk-add");
            this.AddRecordAction.ShortLabel = Mono.Unix.Catalog.GetString("_Add record");
            w1.Add(this.AddRecordAction, null);
            this.RemoveRecordAction = new Gtk.Action("RemoveRecordAction", Mono.Unix.Catalog.GetString("_Remove record"), null, "gtk-remove");
            this.RemoveRecordAction.ShortLabel = Mono.Unix.Catalog.GetString("_Remove record");
            w1.Add(this.RemoveRecordAction, null);
            this.AddRecordFromBibTeXAction = new Gtk.Action("AddRecordFromBibTeXAction", Mono.Unix.Catalog.GetString("Add record from _BibTeX..."), null, null);
            this.AddRecordFromBibTeXAction.ShortLabel = Mono.Unix.Catalog.GetString("Add record from _BibTeX...");
            w1.Add(this.AddRecordFromBibTeXAction, null);
            this.AddRecordFromClipboardAction = new Gtk.Action("AddRecordFromClipboardAction", Mono.Unix.Catalog.GetString("Add record from clipboard"), null, null);
            this.AddRecordFromClipboardAction.Sensitive = false;
            this.AddRecordFromClipboardAction.ShortLabel = Mono.Unix.Catalog.GetString("Add record from clipboard");
            w1.Add(this.AddRecordFromClipboardAction, null);
            this.SidebarAction = new Gtk.ToggleAction("SidebarAction", Mono.Unix.Catalog.GetString("_Sidebar"), null, null);
            this.SidebarAction.ShortLabel = Mono.Unix.Catalog.GetString("_Sidebar");
            w1.Add(this.SidebarAction, null);
            this.RecordDetailsViewAction = new Gtk.Action("RecordDetailsViewAction", Mono.Unix.Catalog.GetString("Record Details View"), null, null);
            this.RecordDetailsViewAction.ShortLabel = Mono.Unix.Catalog.GetString("Record Details View");
            w1.Add(this.RecordDetailsViewAction, null);
            this.RecordDetailsAction = new Gtk.ToggleAction("RecordDetailsAction", Mono.Unix.Catalog.GetString("Record _Details"), null, null);
            this.RecordDetailsAction.Active = true;
            this.RecordDetailsAction.ShortLabel = Mono.Unix.Catalog.GetString("Record _Details");
            w1.Add(this.RecordDetailsAction, null);
            this.FullScreenAction = new Gtk.ToggleAction("FullScreenAction", Mono.Unix.Catalog.GetString("_Full Screen"), null, null);
            this.FullScreenAction.ShortLabel = Mono.Unix.Catalog.GetString("_Full Screen");
            w1.Add(this.FullScreenAction, "<Mod2>F11");
            this.ColumnsAction = new Gtk.Action("ColumnsAction", Mono.Unix.Catalog.GetString("_Columns..."), null, null);
            this.ColumnsAction.ShortLabel = Mono.Unix.Catalog.GetString("_Columns...");
            w1.Add(this.ColumnsAction, null);
            this.AboutAction = new Gtk.Action("AboutAction", Mono.Unix.Catalog.GetString("_About"), null, "gtk-about");
            this.AboutAction.ShortLabel = Mono.Unix.Catalog.GetString("_About");
            w1.Add(this.AboutAction, null);
            this.ToolbarNewAction = new Gtk.Action("ToolbarNewAction", Mono.Unix.Catalog.GetString("_New"), null, "gtk-new");
            this.ToolbarNewAction.ShortLabel = Mono.Unix.Catalog.GetString("_New");
            w1.Add(this.ToolbarNewAction, null);
            this.ToolbarOpenAction = new Gtk.Action("ToolbarOpenAction", null, null, "gtk-open");
            w1.Add(this.ToolbarOpenAction, null);
            this.ToolbarSaveAction = new Gtk.Action("ToolbarSaveAction", null, null, "gtk-save");
            w1.Add(this.ToolbarSaveAction, null);
            this.ToolbarSaveAsAction = new Gtk.Action("ToolbarSaveAsAction", null, null, "gtk-save-as");
            w1.Add(this.ToolbarSaveAsAction, null);
            this.ToolbarAddAction = new Gtk.Action("ToolbarAddAction", null, null, "gtk-add");
            w1.Add(this.ToolbarAddAction, null);
            this.ToolbarRemoveAction = new Gtk.Action("ToolbarRemoveAction", null, null, "gtk-remove");
            w1.Add(this.ToolbarRemoveAction, null);
            this.ViewRecordsAction = new Gtk.RadioAction("ViewRecordsAction", Mono.Unix.Catalog.GetString("View Records"), null, null, 0);
            this.ViewRecordsAction.Group = new GLib.SList(System.IntPtr.Zero);
            this.ViewRecordsAction.ShortLabel = Mono.Unix.Catalog.GetString("View Records");
            w1.Add(this.ViewRecordsAction, null);
            this.EditRecordsAction = new Gtk.RadioAction("EditRecordsAction", Mono.Unix.Catalog.GetString("Edit Records"), null, null, 0);
            this.EditRecordsAction.Group = this.ViewRecordsAction.Group;
            this.EditRecordsAction.ShortLabel = Mono.Unix.Catalog.GetString("Edit Records");
            w1.Add(this.EditRecordsAction, null);
            this.UIManager.InsertActionGroup(w1, 0);
            this.AddAccelGroup(this.UIManager.AccelGroup);
            this.Name = "bibliographer.BibliographerMainWindow";
            this.Title = Mono.Unix.Catalog.GetString("BibliographerMainWindow");
            this.Icon = Gdk.Pixbuf.LoadFromResource("bibliographer.png");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.DefaultWidth = 640;
            this.DefaultHeight = 480;
            // Container child bibliographer.BibliographerMainWindow.Gtk.Container+ContainerChild
            this.vbox1 = new Gtk.VBox();
            this.vbox1.Name = "vbox1";
            // Container child vbox1.Gtk.Box+BoxChild
            this.UIManager.AddUiFromString("<ui><menubar name='menuBar'><menu name='FileAction' action='FileAction'><menuitem name='FileNewAction' action='FileNewAction'/><menuitem name='FileOpenAction' action='FileOpenAction'/><separator/><menuitem name='SaveAction' action='SaveAction'/><menuitem name='FileSaveAsAction' action='FileSaveAsAction'/><separator/><menuitem name='ImportFolderAction' action='ImportFolderAction'/><separator/><menu name='RecentFilesAction' action='RecentFilesAction'/><separator/><menuitem name='QuitAction' action='QuitAction'/></menu><menu name='EditAction' action='EditAction'><menuitem name='AddRecordAction' action='AddRecordAction'/><menuitem name='RemoveRecordAction' action='RemoveRecordAction'/><separator/><menuitem name='AddRecordFromBibTeXAction' action='AddRecordFromBibTeXAction'/><menuitem name='AddRecordFromClipboardAction' action='AddRecordFromClipboardAction'/></menu><menu name='ViewAction' action='ViewAction'><menuitem name='SidebarAction' action='SidebarAction'/><menu name='RecordDetailsViewAction' action='RecordDetailsViewAction'><menuitem name='ViewRecordsAction' action='ViewRecordsAction'/><menuitem name='EditRecordsAction' action='EditRecordsAction'/></menu><menuitem name='RecordDetailsAction' action='RecordDetailsAction'/><separator/><menuitem name='FullScreenAction' action='FullScreenAction'/><menuitem name='ColumnsAction' action='ColumnsAction'/></menu><menu name='HelpAction' action='HelpAction'><menuitem name='AboutAction' action='AboutAction'/></menu></menubar></ui>");
            this.menuBar = ((Gtk.MenuBar)(this.UIManager.GetWidget("/menuBar")));
            this.menuBar.Name = "menuBar";
            this.vbox1.Add(this.menuBar);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.vbox1[this.menuBar]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            // Container child hbox1.Gtk.Box+BoxChild
            this.UIManager.AddUiFromString("<ui><toolbar name='toolBar1'><toolitem name='ToolbarNewAction' action='ToolbarNewAction'/><toolitem name='ToolbarOpenAction' action='ToolbarOpenAction'/><toolitem name='ToolbarSaveAction' action='ToolbarSaveAction'/><toolitem name='ToolbarSaveAsAction' action='ToolbarSaveAsAction'/><separator/><toolitem name='ToolbarAddAction' action='ToolbarAddAction'/><toolitem name='ToolbarRemoveAction' action='ToolbarRemoveAction'/></toolbar></ui>");
            this.toolBar1 = ((Gtk.Toolbar)(this.UIManager.GetWidget("/toolBar1")));
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowArrow = false;
            this.toolBar1.ToolbarStyle = ((Gtk.ToolbarStyle)(0));
            this.toolBar1.IconSize = ((Gtk.IconSize)(3));
            this.hbox1.Add(this.toolBar1);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.hbox1[this.toolBar1]));
            w3.Position = 0;
            // Container child hbox1.Gtk.Box+BoxChild
            this.searchHbox = new Gtk.HBox();
            this.searchHbox.Name = "searchHbox";
            this.hbox1.Add(this.searchHbox);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.hbox1[this.searchHbox]));
            w4.Position = 1;
            w4.Expand = false;
            this.vbox1.Add(this.hbox1);
            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
            w5.Position = 1;
            w5.Expand = false;
            w5.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.hpane = new Gtk.HPaned();
            this.hpane.CanFocus = true;
            this.hpane.Name = "hpane";
            this.hpane.Position = 150;
            this.hpane.BorderWidth = ((uint)(2));
            // Container child hpane.Gtk.Paned+PanedChild
            this.scrolledwindowSidePane = new Gtk.ScrolledWindow();
            this.scrolledwindowSidePane.CanFocus = true;
            this.scrolledwindowSidePane.Name = "scrolledwindowSidePane";
            this.scrolledwindowSidePane.ShadowType = ((Gtk.ShadowType)(1));
            this.scrolledwindowSidePane.BorderWidth = ((uint)(2));
            this.hpane.Add(this.scrolledwindowSidePane);
            Gtk.Paned.PanedChild w6 = ((Gtk.Paned.PanedChild)(this.hpane[this.scrolledwindowSidePane]));
            w6.Resize = false;
            // Container child hpane.Gtk.Paned+PanedChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            // Container child vbox2.Gtk.Box+BoxChild
            this.vpane = new Gtk.VPaned();
            this.vpane.CanFocus = true;
            this.vpane.Name = "vpane";
            this.vpane.Position = 150;
            // Container child vpane.Gtk.Paned+PanedChild
            this.scrolledwindowTreeView = new Gtk.ScrolledWindow();
            this.scrolledwindowTreeView.HeightRequest = 200;
            this.scrolledwindowTreeView.CanFocus = true;
            this.scrolledwindowTreeView.Name = "scrolledwindowTreeView";
            this.scrolledwindowTreeView.ShadowType = ((Gtk.ShadowType)(1));
            this.scrolledwindowTreeView.BorderWidth = ((uint)(2));
            this.vpane.Add(this.scrolledwindowTreeView);
            Gtk.Paned.PanedChild w7 = ((Gtk.Paned.PanedChild)(this.vpane[this.scrolledwindowTreeView]));
            w7.Resize = false;
            // Container child vpane.Gtk.Paned+PanedChild
            this.recordDetailsView = new Gtk.VBox();
            this.recordDetailsView.Name = "recordDetailsView";
            this.recordDetailsView.BorderWidth = ((uint)(2));
            // Container child recordDetailsView.Gtk.Box+BoxChild
            this.recordView = new Gtk.VBox();
            this.recordView.Name = "recordView";
            // Container child recordView.Gtk.Box+BoxChild
            this.recordViewHbox = new Gtk.HBox();
            this.recordViewHbox.Name = "recordViewHbox";
            // Container child recordViewHbox.Gtk.Box+BoxChild
            this.recordIcon = new Gtk.Image();
            this.recordIcon.WidthRequest = 96;
            this.recordIcon.HeightRequest = 128;
            this.recordIcon.Name = "recordIcon";
            this.recordViewHbox.Add(this.recordIcon);
            Gtk.Box.BoxChild w8 = ((Gtk.Box.BoxChild)(this.recordViewHbox[this.recordIcon]));
            w8.Position = 0;
            w8.Expand = false;
            w8.Fill = false;
            // Container child recordViewHbox.Gtk.Box+BoxChild
            this.recordDetails = new Gtk.Label();
            this.recordDetails.Name = "recordDetails";
            this.recordDetails.Xpad = 10;
            this.recordDetails.Ypad = 10;
            this.recordDetails.Xalign = 0F;
            this.recordDetails.Yalign = 0F;
            this.recordDetails.LabelProp = Mono.Unix.Catalog.GetString("test");
            this.recordViewHbox.Add(this.recordDetails);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(this.recordViewHbox[this.recordDetails]));
            w9.Position = 1;
            this.recordView.Add(this.recordViewHbox);
            Gtk.Box.BoxChild w10 = ((Gtk.Box.BoxChild)(this.recordView[this.recordViewHbox]));
            w10.Position = 0;
            this.recordDetailsView.Add(this.recordView);
            Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(this.recordDetailsView[this.recordView]));
            w11.Position = 0;
            // Container child recordDetailsView.Gtk.Box+BoxChild
            this.recordEditor = new Gtk.VBox();
            this.recordEditor.Name = "recordEditor";
            // Container child recordEditor.Gtk.Box+BoxChild
            this.table1 = new Gtk.Table(((uint)(2)), ((uint)(3)), false);
            this.table1.Name = "table1";
            // Container child table1.Gtk.Table+TableChild
            this.buttonBibtexKeyGenerate = new Gtk.Button();
            this.buttonBibtexKeyGenerate.CanFocus = true;
            this.buttonBibtexKeyGenerate.Name = "buttonBibtexKeyGenerate";
            this.buttonBibtexKeyGenerate.UseUnderline = true;
            this.buttonBibtexKeyGenerate.Label = Mono.Unix.Catalog.GetString("Generate");
            this.table1.Add(this.buttonBibtexKeyGenerate);
            Gtk.Table.TableChild w12 = ((Gtk.Table.TableChild)(this.table1[this.buttonBibtexKeyGenerate]));
            w12.TopAttach = ((uint)(1));
            w12.BottomAttach = ((uint)(2));
            w12.LeftAttach = ((uint)(2));
            w12.RightAttach = ((uint)(3));
            w12.XPadding = ((uint)(10));
            w12.YPadding = ((uint)(10));
            w12.XOptions = ((Gtk.AttachOptions)(0));
            w12.YOptions = ((Gtk.AttachOptions)(0));
            // Container child table1.Gtk.Table+TableChild
            this.comboRecordType = Gtk.ComboBox.NewText();
            this.comboRecordType.Name = "comboRecordType";
            this.table1.Add(this.comboRecordType);
            Gtk.Table.TableChild w13 = ((Gtk.Table.TableChild)(this.table1[this.comboRecordType]));
            w13.LeftAttach = ((uint)(1));
            w13.RightAttach = ((uint)(3));
            w13.XPadding = ((uint)(10));
            w13.YPadding = ((uint)(10));
            w13.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.entryReqBibtexKey = new Gtk.Entry();
            this.entryReqBibtexKey.CanFocus = true;
            this.entryReqBibtexKey.Name = "entryReqBibtexKey";
            this.entryReqBibtexKey.IsEditable = true;
            this.entryReqBibtexKey.InvisibleChar = '●';
            this.table1.Add(this.entryReqBibtexKey);
            Gtk.Table.TableChild w14 = ((Gtk.Table.TableChild)(this.table1[this.entryReqBibtexKey]));
            w14.TopAttach = ((uint)(1));
            w14.BottomAttach = ((uint)(2));
            w14.LeftAttach = ((uint)(1));
            w14.RightAttach = ((uint)(2));
            w14.XPadding = ((uint)(10));
            w14.YPadding = ((uint)(10));
            w14.YOptions = ((Gtk.AttachOptions)(0));
            // Container child table1.Gtk.Table+TableChild
            this.lblBibtexKey = new Gtk.Label();
            this.lblBibtexKey.Name = "lblBibtexKey";
            this.lblBibtexKey.Xpad = 5;
            this.lblBibtexKey.Xalign = 1F;
            this.lblBibtexKey.LabelProp = Mono.Unix.Catalog.GetString("BibTeX Key");
            this.lblBibtexKey.Justify = ((Gtk.Justification)(1));
            this.table1.Add(this.lblBibtexKey);
            Gtk.Table.TableChild w15 = ((Gtk.Table.TableChild)(this.table1[this.lblBibtexKey]));
            w15.TopAttach = ((uint)(1));
            w15.BottomAttach = ((uint)(2));
            w15.XPadding = ((uint)(10));
            w15.YPadding = ((uint)(10));
            w15.XOptions = ((Gtk.AttachOptions)(4));
            w15.YOptions = ((Gtk.AttachOptions)(0));
            // Container child table1.Gtk.Table+TableChild
            this.lblRecordType = new Gtk.Label();
            this.lblRecordType.Name = "lblRecordType";
            this.lblRecordType.Xpad = 5;
            this.lblRecordType.Xalign = 1F;
            this.lblRecordType.LabelProp = Mono.Unix.Catalog.GetString("Record Type");
            this.lblRecordType.Justify = ((Gtk.Justification)(1));
            this.table1.Add(this.lblRecordType);
            Gtk.Table.TableChild w16 = ((Gtk.Table.TableChild)(this.table1[this.lblRecordType]));
            w16.XPadding = ((uint)(10));
            w16.YPadding = ((uint)(10));
            w16.XOptions = ((Gtk.AttachOptions)(4));
            w16.YOptions = ((Gtk.AttachOptions)(0));
            this.recordEditor.Add(this.table1);
            Gtk.Box.BoxChild w17 = ((Gtk.Box.BoxChild)(this.recordEditor[this.table1]));
            w17.Position = 0;
            w17.Expand = false;
            // Container child recordEditor.Gtk.Box+BoxChild
            this.notebookFields = new Gtk.Notebook();
            this.notebookFields.CanFocus = true;
            this.notebookFields.Name = "notebookFields";
            this.notebookFields.CurrentPage = 0;
            // Container child notebookFields.Gtk.Notebook+NotebookChild
            this.scrolledwindowRqdFields = new Gtk.ScrolledWindow();
            this.scrolledwindowRqdFields.CanFocus = true;
            this.scrolledwindowRqdFields.Name = "scrolledwindowRqdFields";
            this.scrolledwindowRqdFields.HscrollbarPolicy = ((Gtk.PolicyType)(2));
            this.scrolledwindowRqdFields.ShadowType = ((Gtk.ShadowType)(1));
            this.notebookFields.Add(this.scrolledwindowRqdFields);
            // Notebook tab
            this.lblNtbkRequired = new Gtk.Label();
            this.lblNtbkRequired.Name = "lblNtbkRequired";
            this.lblNtbkRequired.LabelProp = Mono.Unix.Catalog.GetString("Required fields");
            this.notebookFields.SetTabLabel(this.scrolledwindowRqdFields, this.lblNtbkRequired);
            this.lblNtbkRequired.ShowAll();
            // Container child notebookFields.Gtk.Notebook+NotebookChild
            this.scrolledwindowOptional = new Gtk.ScrolledWindow();
            this.scrolledwindowOptional.CanFocus = true;
            this.scrolledwindowOptional.Name = "scrolledwindowOptional";
            this.scrolledwindowOptional.HscrollbarPolicy = ((Gtk.PolicyType)(2));
            this.scrolledwindowOptional.ShadowType = ((Gtk.ShadowType)(1));
            this.notebookFields.Add(this.scrolledwindowOptional);
            Gtk.Notebook.NotebookChild w19 = ((Gtk.Notebook.NotebookChild)(this.notebookFields[this.scrolledwindowOptional]));
            w19.Position = 1;
            // Notebook tab
            this.lblNtbkOptional = new Gtk.Label();
            this.lblNtbkOptional.Name = "lblNtbkOptional";
            this.lblNtbkOptional.LabelProp = Mono.Unix.Catalog.GetString("Optional fields");
            this.notebookFields.SetTabLabel(this.scrolledwindowOptional, this.lblNtbkOptional);
            this.lblNtbkOptional.ShowAll();
            // Container child notebookFields.Gtk.Notebook+NotebookChild
            this.scrolledwindowOther = new Gtk.ScrolledWindow();
            this.scrolledwindowOther.CanFocus = true;
            this.scrolledwindowOther.Name = "scrolledwindowOther";
            this.scrolledwindowOther.HscrollbarPolicy = ((Gtk.PolicyType)(2));
            this.scrolledwindowOther.ShadowType = ((Gtk.ShadowType)(1));
            this.notebookFields.Add(this.scrolledwindowOther);
            Gtk.Notebook.NotebookChild w20 = ((Gtk.Notebook.NotebookChild)(this.notebookFields[this.scrolledwindowOther]));
            w20.Position = 2;
            // Notebook tab
            this.lblNtbkOther = new Gtk.Label();
            this.lblNtbkOther.Name = "lblNtbkOther";
            this.lblNtbkOther.LabelProp = Mono.Unix.Catalog.GetString("Other fields");
            this.notebookFields.SetTabLabel(this.scrolledwindowOther, this.lblNtbkOther);
            this.lblNtbkOther.ShowAll();
            // Container child notebookFields.Gtk.Notebook+NotebookChild
            this.scrolledwindowBibliographerData = new Gtk.ScrolledWindow();
            this.scrolledwindowBibliographerData.CanFocus = true;
            this.scrolledwindowBibliographerData.Name = "scrolledwindowBibliographerData";
            this.scrolledwindowBibliographerData.HscrollbarPolicy = ((Gtk.PolicyType)(2));
            this.scrolledwindowBibliographerData.ShadowType = ((Gtk.ShadowType)(1));
            this.notebookFields.Add(this.scrolledwindowBibliographerData);
            Gtk.Notebook.NotebookChild w21 = ((Gtk.Notebook.NotebookChild)(this.notebookFields[this.scrolledwindowBibliographerData]));
            w21.Position = 3;
            // Notebook tab
            this.lblNtbkBibliographerData = new Gtk.Label();
            this.lblNtbkBibliographerData.Name = "lblNtbkBibliographerData";
            this.lblNtbkBibliographerData.LabelProp = Mono.Unix.Catalog.GetString("Bibliographer data");
            this.notebookFields.SetTabLabel(this.scrolledwindowBibliographerData, this.lblNtbkBibliographerData);
            this.lblNtbkBibliographerData.ShowAll();
            this.recordEditor.Add(this.notebookFields);
            Gtk.Box.BoxChild w22 = ((Gtk.Box.BoxChild)(this.recordEditor[this.notebookFields]));
            w22.Position = 1;
            this.recordDetailsView.Add(this.recordEditor);
            Gtk.Box.BoxChild w23 = ((Gtk.Box.BoxChild)(this.recordDetailsView[this.recordEditor]));
            w23.Position = 1;
            // Container child recordDetailsView.Gtk.Box+BoxChild
            this.hbuttonbox1 = new Gtk.HButtonBox();
            this.hbuttonbox1.Name = "hbuttonbox1";
            this.hbuttonbox1.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
            this.toggleEditRecords = new Gtk.ToggleButton();
            this.toggleEditRecords.CanFocus = true;
            this.toggleEditRecords.Name = "toggleEditRecords";
            this.toggleEditRecords.UseUnderline = true;
            this.toggleEditRecords.Active = true;
            this.toggleEditRecords.Label = Mono.Unix.Catalog.GetString("E_dit Records");
            this.hbuttonbox1.Add(this.toggleEditRecords);
            Gtk.ButtonBox.ButtonBoxChild w24 = ((Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox1[this.toggleEditRecords]));
            w24.Expand = false;
            w24.Fill = false;
            this.recordDetailsView.Add(this.hbuttonbox1);
            Gtk.Box.BoxChild w25 = ((Gtk.Box.BoxChild)(this.recordDetailsView[this.hbuttonbox1]));
            w25.Position = 2;
            w25.Expand = false;
            w25.Fill = false;
            w25.Padding = ((uint)(2));
            this.vpane.Add(this.recordDetailsView);
            this.vbox2.Add(this.vpane);
            Gtk.Box.BoxChild w27 = ((Gtk.Box.BoxChild)(this.vbox2[this.vpane]));
            w27.Position = 0;
            this.hpane.Add(this.vbox2);
            this.vbox1.Add(this.hpane);
            Gtk.Box.BoxChild w29 = ((Gtk.Box.BoxChild)(this.vbox1[this.hpane]));
            w29.Position = 2;
            // Container child vbox1.Gtk.Box+BoxChild
            this.statusbar = new Gtk.Statusbar();
            this.statusbar.Name = "statusbar";
            this.statusbar.Spacing = 2;
            this.vbox1.Add(this.statusbar);
            Gtk.Box.BoxChild w30 = ((Gtk.Box.BoxChild)(this.vbox1[this.statusbar]));
            w30.Position = 3;
            w30.Expand = false;
            w30.Fill = false;
            this.Add(this.vbox1);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Hide();
            this.SizeAllocated += new Gtk.SizeAllocatedHandler(this.OnWindowSizeAllocated);
            this.WindowStateEvent += new Gtk.WindowStateEventHandler(this.OnWindowStateChanged);
            this.DeleteEvent += new Gtk.DeleteEventHandler(this.OnWindowDeleteEvent);
            this.FileNewAction.Activated += new System.EventHandler(this.OnFileNewActivated);
            this.FileOpenAction.Activated += new System.EventHandler(this.OnFileOpenActivated);
            this.SaveAction.Activated += new System.EventHandler(this.OnFileSaveActivated);
            this.FileSaveAsAction.Activated += new System.EventHandler(this.OnFileSaveAsActivated);
            this.ImportFolderAction.Activated += new System.EventHandler(this.OnImportFolderActivated);
            this.QuitAction.Activated += new System.EventHandler(this.OnFileQuitActivated);
            this.AddRecordAction.Activated += new System.EventHandler(this.OnAddRecordActivated);
            this.RemoveRecordAction.Activated += new System.EventHandler(this.OnRemoveRecordActivated);
            this.AddRecordFromBibTeXAction.Activated += new System.EventHandler(this.OnAddRecordFromBibtexActivated);
            this.AddRecordFromClipboardAction.Activated += new System.EventHandler(this.OnAddRecordFromClipboardActivated);
            this.SidebarAction.Activated += new System.EventHandler(this.OnToggleSideBarActivated);
            this.RecordDetailsAction.Activated += new System.EventHandler(this.OnToggleRecordDetailsActivated);
            this.FullScreenAction.Activated += new System.EventHandler(this.OnToggleFullScreenActionActivated);
            this.ColumnsAction.Activated += new System.EventHandler(this.OnChooseColumnsActivated);
            this.AboutAction.Activated += new System.EventHandler(this.OnHelpAboutActivated);
            this.ToolbarNewAction.Activated += new System.EventHandler(this.OnFileNewActivated);
            this.ToolbarOpenAction.Activated += new System.EventHandler(this.OnFileOpenActivated);
            this.ToolbarSaveAction.Activated += new System.EventHandler(this.OnFileSaveActivated);
            this.ToolbarSaveAsAction.Activated += new System.EventHandler(this.OnFileSaveAsActivated);
            this.ToolbarAddAction.Activated += new System.EventHandler(this.OnAddRecordActivated);
            this.ToolbarRemoveAction.Activated += new System.EventHandler(this.OnRemoveRecordActivated);
            this.ViewRecordsAction.Activated += new System.EventHandler(this.OnRadioViewRecordsActivated);
            this.EditRecordsAction.Activated += new System.EventHandler(this.OnRadioEditRecordsActivated);
            this.entryReqBibtexKey.Changed += new System.EventHandler(this.OnEntryReqBibtexKeyChanged);
            this.comboRecordType.Changed += new System.EventHandler(this.OnComboRecordTypeChanged);
            this.buttonBibtexKeyGenerate.Clicked += new System.EventHandler(this.OnButtonBibtexKeyGenerateClicked);
            this.toggleEditRecords.Toggled += new System.EventHandler(this.OnToggleEditRecordsActivated);
        }
    }
}
