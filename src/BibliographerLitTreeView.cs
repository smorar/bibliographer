// 
//  Copyright (C) 2009 smorar
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

namespace bibliographer
{
    class LitTreeView : Gtk.TreeView
    {
        private Gtk.TreeModelSort sorter;
            
        public LitTreeView(Gtk.TreeModel model)
        {
            sorter = new Gtk.TreeModelSort(model);
            
            this.Model = sorter;
            
            // TODO: Perform this more elegantly
            // Possibly, read out fields from the bibtex record spec file
            // and make a certain set of columns visible by default
            Gtk.TreeViewColumn [] columnarray;
            columnarray = new Gtk.TreeViewColumn[8];
    
            if (Config.KeyExists("Columns/Author/order") && 
                Config.KeyExists("Columns/Title/order") &&
                Config.KeyExists("Columns/Year/order") && 
                Config.KeyExists("Columns/Journal/order") && 
                Config.KeyExists("Columns/Bibtex Key/order") && 
                Config.KeyExists("Columns/Volume/order") && 
                Config.KeyExists("Columns/Pages/order"))
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
                this.AppendColumn(column);
            }
            this.HeadersClickable = true;
    
            Gtk.TreeCellDataFunc textDataFunc = new Gtk.TreeCellDataFunc(RenderColumnTextFromBibtexRecord);
            Gtk.TreeCellDataFunc pixmapDataFunc = new Gtk.TreeCellDataFunc(RenderColumnPixbufFromBibtexRecord);
            int id = 0;
    
            foreach (Gtk.TreeViewColumn column in this.Columns)
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
                    column.Title = "Icon";
                    column.SetCellDataFunc(column.CellRenderers[0], pixmapDataFunc);
                    column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
                    column.Expand = false;
                    column.Resizable = false;
                    column.Reorderable = false;
                    column.Clickable = false;
                    column.MinWidth = 20;
                    column.FixedWidth = 40;
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

            // Callbacks for the LitTreeView
            this.DragMotion += OnDragMotion;
            this.RowActivated += OnRowActivated;
            this.DragLeave += OnDragLeave;
            
            this.Show();
        }
        
        private void RenderColumnPixbufFromBibtexRecord(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            if (model != null)
            {
                BibtexRecord record = (BibtexRecord) model.GetValue(iter, 0);

                Gdk.Pixbuf pixbuf = record.GetSmallThumbnail();
    
                if ((cell != null) && (record != null))
                {
                    (cell as Gtk.CellRendererPixbuf).Pixbuf = pixbuf;
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
                        (cell as Gtk.CellRendererText).Text = StringOps.TeXToUnicode(record.GetField(column.Title));
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
                        (cell as Gtk.CellRendererText).Text = StringOps.TeXToUnicode(author_string);
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
        
        public void SaveColumnsState()
        {
            // Save column states
            int i = 0;
            foreach (Gtk.TreeViewColumn column in this.Columns)
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
        protected virtual void OnRowActivated (object o, Gtk.RowActivatedArgs args)
        {
            Debug.WriteLine(5, "Row activated");
    
            Gtk.TreeIter iter;
            BibtexRecord record;
    
            if (!this.Model.GetIter(out iter, args.Path))
            {
                Debug.WriteLine(5, "Failed to open record because of GetIter faliure");
                return;
            }
            record = (BibtexRecord) this.Model.GetValue(iter, 0);
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
                Gtk.MessageDialog md = new Gtk.MessageDialog ((Gtk.Window) this.Toplevel,
                                                          Gtk.DialogFlags.DestroyWithParent,
                                                          Gtk.MessageType.Error,
                                                          Gtk.ButtonsType.Close, "Error loading associated file:\n" + Gnome.Vfs.Uri.GetLocalPathFromUri(uriString));
                //int result = md.Run ();
                md.Run();
                md.Destroy();
                Debug.WriteLine(0, "Error loading associated file:\n{0}", Gnome.Vfs.Uri.GetLocalPathFromUri(uriString));
            }
        }
        
        protected virtual void OnDragLeave (object o, Gtk.DragLeaveArgs args)
        {
            this.UnsetRowsDragDest();
        }
        
        protected virtual void OnDragMotion (object o, Gtk.DragMotionArgs args)
        {
            // FIXME: how do we check from here if that drag has data that we want?
    
            Gtk.TreePath path;
            Gtk.TreeViewDropPosition drop_position;
            if (this.GetDestRowAtPos(args.X, args.Y, out path, out drop_position)) {
                this.SetDragDestRow(path, Gtk.TreeViewDropPosition.IntoOrAfter);
            }
            else
                this.UnsetRowsDragDest();
        }


    }
}
