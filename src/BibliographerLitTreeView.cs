//
//  BibliographerLitTreeView.cs
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
using libbibby;

namespace bibliographer
{
    public class LitTreeView : Gtk.TreeView
    {
        protected Gtk.TreeModelSort sorter;
        protected BibliographerSettings columnsIconSettings;
        protected BibliographerSettings columnsAuthorSettings;
        protected BibliographerSettings columnsTitleSettings;
        protected BibliographerSettings columnsYearSettings;
        protected BibliographerSettings columnsJournalSettings;
        protected BibliographerSettings columnsBibtexKeySettings;
        protected BibliographerSettings columnsVolumeSettings;
        protected BibliographerSettings columnsPagesSettings;

        public LitTreeView (Gtk.ITreeModel model)
        {
            sorter = new Gtk.TreeModelSort (model);

            columnsIconSettings = new BibliographerSettings ("apps.bibliographer.columns.icon");
            columnsAuthorSettings = new BibliographerSettings ("apps.bibliographer.columns.author");
            columnsTitleSettings = new BibliographerSettings ("apps.bibliographer.columns.title");
            columnsYearSettings = new BibliographerSettings ("apps.bibliographer.columns.year");
            columnsJournalSettings = new BibliographerSettings ("apps.bibliographer.columns.journal");
            columnsBibtexKeySettings = new BibliographerSettings ("apps.bibliographer.columns.bibtexkey");
            columnsVolumeSettings = new BibliographerSettings ("apps.bibliographer.columns.volume");
            columnsPagesSettings = new BibliographerSettings ("apps.bibliographer.columns.pages");

            Model = sorter;
            
            Gtk.CellRendererPixbuf columnIconRenderer;
            Gtk.CellRendererText columnAuthorRenderer;
            Gtk.CellRendererText columnTitleRenderer;
            Gtk.CellRendererText columnYearRenderer;
            Gtk.CellRendererText columnJournalRenderer;
            Gtk.CellRendererText columnBibtexKeyRenderer;
            Gtk.CellRendererText columnVolumeRenderer;
            Gtk.CellRendererText columnPagesRenderer;

            columnIconRenderer = new Gtk.CellRendererPixbuf ();
            columnAuthorRenderer = new Gtk.CellRendererText ();
            columnTitleRenderer = new Gtk.CellRendererText ();
            columnYearRenderer = new Gtk.CellRendererText ();
            columnJournalRenderer = new Gtk.CellRendererText ();
            columnBibtexKeyRenderer = new Gtk.CellRendererText ();
            columnVolumeRenderer = new Gtk.CellRendererText ();
            columnPagesRenderer = new Gtk.CellRendererText ();

            AppendColumn ("Icon", columnIconRenderer, "image");
            AppendColumn ("Author", columnAuthorRenderer, "text");
            AppendColumn ("Title", columnTitleRenderer, "text");
            AppendColumn ("Year", columnYearRenderer, "text");
            AppendColumn ("Journal", columnJournalRenderer, "text");
            AppendColumn ("Bibtex Key", columnBibtexKeyRenderer, "text");
            AppendColumn ("Volume", columnVolumeRenderer, "text");
            AppendColumn ("Pages", columnPagesRenderer, "text");

            HeadersClickable = true;

			var textDataFunc = new Gtk.TreeCellDataFunc (RenderColumnTextFromBibtexRecord);
            var pixmapDataFunc = new Gtk.TreeCellDataFunc (RenderColumnPixbufFromBibtexRecord);

            int idx = 0;
            foreach (Gtk.TreeViewColumn column in Columns) {
                column.Expand = false;
                column.Reorderable = true;
                column.Resizable = true;
                column.Clickable = true;

                if (column.Title == "Icon") {
                    column.FixedWidth = columnsIconSettings.GetInt ("width");
                    column.Visible = columnsIconSettings.GetBoolean ("visible");
					column.SetCellDataFunc (column.Cells[0], pixmapDataFunc);
                    column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
                    column.Resizable = false;
                    column.Reorderable = false;
                    column.Clickable = false;
                    column.MinWidth = 20;
                } else if (column.Title == "Author") {
                    column.FixedWidth = columnsAuthorSettings.GetInt ("width");
                    column.Visible = columnsAuthorSettings.GetBoolean ("visible");
					column.SetCellDataFunc (column.Cells[0], textDataFunc);
                    column.SortColumnId = 1;
                    sorter.SetSortFunc (1, StringCompareAuthor);
                    column.Clicked += OnColumnSort;
                    if (column != Columns[columnsAuthorSettings.GetInt ("order") - 1])
                        MoveColumnAfter (column, Columns[columnsAuthorSettings.GetInt ("order") - 1]);
                } else if (column.Title == "Title") {
                    column.Expand = true;
                    column.FixedWidth = columnsTitleSettings.GetInt ("width");
                    column.Visible = columnsTitleSettings.GetBoolean ("visible");
                    column.SetCellDataFunc (column.Cells[0], textDataFunc);
                    column.SortColumnId = 2;
                    sorter.SetSortFunc (2, StringCompare);
                    column.Clicked += OnColumnSort;
                    if (column != Columns[columnsTitleSettings.GetInt ("order") - 1])
                        MoveColumnAfter (column, Columns[columnsTitleSettings.GetInt ("order") - 1]);
                } else if (column.Title == "Year") {
                    column.FixedWidth = columnsYearSettings.GetInt ("width");
                    column.Visible = columnsYearSettings.GetBoolean ("visible");
                    column.SetCellDataFunc (column.Cells[0], textDataFunc);
                    column.SortColumnId = 3;
                    sorter.SetSortFunc (3, StringCompare);
                    column.Clicked += OnColumnSort;
                    if (column != Columns[columnsYearSettings.GetInt ("order") - 1])
                        MoveColumnAfter (column, Columns[columnsYearSettings.GetInt ("order") - 1]);
                } else if (column.Title == "Journal") {
                    column.FixedWidth = columnsJournalSettings.GetInt ("width");
                    column.Visible = columnsJournalSettings.GetBoolean ("visible");
                    column.SetCellDataFunc (column.Cells[0], textDataFunc);
                    column.SortColumnId = 4;
                    sorter.SetSortFunc (4, StringCompare);
                    column.Clicked += OnColumnSort;
                    if (column != Columns[columnsJournalSettings.GetInt ("order") - 1])
                        MoveColumnAfter (column, Columns[columnsJournalSettings.GetInt ("order") - 1]);
                } else if (column.Title == "Bibtex Key") {
                    column.FixedWidth = columnsBibtexKeySettings.GetInt ("width");
                    column.Visible = columnsBibtexKeySettings.GetBoolean ("visible");
                    column.SetCellDataFunc (column.Cells[0], textDataFunc);
                    column.SortColumnId = 5;
                    sorter.SetSortFunc (5, StringCompare);
                    column.Clicked += OnColumnSort;
                    if (column != Columns[columnsBibtexKeySettings.GetInt ("order") - 1])
                        MoveColumnAfter (column, Columns[columnsBibtexKeySettings.GetInt ("order") - 1]);
                } else if (column.Title == "Volume") {
                    column.FixedWidth = columnsVolumeSettings.GetInt ("width");
                    column.Visible = columnsVolumeSettings.GetBoolean ("visible");
                    column.SetCellDataFunc (column.Cells[0], textDataFunc);
                    column.SortColumnId = 6;
                    sorter.SetSortFunc (6, StringCompare);
                    column.Clicked += OnColumnSort;
                    if (column != Columns[columnsVolumeSettings.GetInt ("order") - 1])
                        MoveColumnAfter (column, Columns[columnsVolumeSettings.GetInt ("order") - 1]);
                } else if (column.Title == "Pages") {
                    column.FixedWidth = columnsPagesSettings.GetInt ("width");
                    column.Visible = columnsPagesSettings.GetBoolean ("visible");
                    column.SetCellDataFunc (column.Cells[0], textDataFunc);
                    column.SortColumnId = 7;
                    sorter.SetSortFunc (7, StringCompare);
                    column.Clicked += OnColumnSort;
                    if (column != Columns[columnsPagesSettings.GetInt ("order") - 1])
                        MoveColumnAfter (column, Columns[columnsPagesSettings.GetInt ("order") - 1]);
                }
                idx++;
            }

            //RedrawColumns ();

            // Callbacks for the LitTreeView
            ColumnsChanged += OnColumnsChanged;
            DragMotion += OnDragMotion;
            RowActivated += OnRowActivated;
            DragLeave += OnDragLeave;
            
            Show ();
        }

        static void RenderColumnPixbufFromBibtexRecord (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
        {
            if (model != null) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                
                var pixbuf = (Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail");
                
                if ((cell != null) && (record != null)) {
                    (cell as Gtk.CellRendererPixbuf).Pixbuf = pixbuf;
                }
            }
        }

        static void RenderColumnTextFromBibtexRecord (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
        {
            // See here for an example of how you can highlight cells
            // based on something todo with the entry
            //
            // TODO: extend this feature to highlight entries that
            // are missing required fields
            
            if ((model != null) && (column != null) && (column.Title != null)) {
                var record = (BibtexRecord)model.GetValue (iter, 0);
                if (record != null) {
                    if (record.HasField (column.Title) && column.Title != "Author") {
                        (cell as Gtk.CellRendererText).Text = StringOps.TeXToUnicode (record.GetField (column.Title));
                        (cell as Gtk.CellRendererText).Background = "white";
                        (cell as Gtk.CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
                    } else if (record.HasField (column.Title) && column.Title == "Author") {
                        StringArrayList authors = record.GetAuthors ();
                        string author_string = "";
                        foreach (string author in authors) {
                            if (author_string == "")
                                author_string = author;
                            else
                                author_string = String.Concat (author_string, "; ", author);
                        }
                        (cell as Gtk.CellRendererText).Text = StringOps.TeXToUnicode (author_string);
                        (cell as Gtk.CellRendererText).Background = "white";
                        (cell as Gtk.CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
                    } else {
                        (cell as Gtk.CellRendererText).Text = "";
                        // could highlight important fields that are missing data too,
                        // for e.g. the line below:
                        //(cell as Gtk.CellRendererText).Background = "green";
                    }
                    if (!BibtexRecordTypeLibrary.Contains (record.RecordType)) {
                        (cell as Gtk.CellRendererText).Foreground = "red";
                    } else {
                        (cell as Gtk.CellRendererText).Foreground = "black";
                    }
                }
            }
        }

        public int StringCompare (Gtk.ITreeModel model, Gtk.TreeIter tia, Gtk.TreeIter tib)
        {
            var a = (BibtexRecord)model.GetValue (tia, 0);
            var b = (BibtexRecord)model.GetValue (tib, 0);
            string A, B;
            string sortString = "";
            int sortColumn;
            Gtk.SortType sortType;
            sorter.GetSortColumnId (out sortColumn, out sortType);
            
            switch (sortColumn) {
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
                if (a.HasField (sortString))
                    A = a.GetField (sortString);
                else
                    A = "";
            else
                A = "";
            if (b != null)
                if (b.HasField (sortString))
                    B = b.GetField (sortString);
                else
                    B = "";
            else
                B = "";
            Debug.WriteLine (10, "sortString: {0} Comparing {1} and {2}", sortString, A, B);
            return String.Compare (A.ToLower(), B.ToLower());
        }

        public int StringCompareAuthor (Gtk.ITreeModel model, Gtk.TreeIter tia, Gtk.TreeIter tib)
        {
            var a = (BibtexRecord)model.GetValue (tia, 0);
            var b = (BibtexRecord)model.GetValue (tib, 0);
            string A, B;
            if (a != null)
                if (a.GetAuthors ().Count > 0)
                    A = a.GetAuthors ()[0];
                else
                    A = "";
            else
                A = "";
            if (b != null)
                if (b.GetAuthors ().Count > 0)
                    B = b.GetAuthors ()[0];
                else
                    B = "";
            else
                B = "";
            Debug.WriteLine (10, "Comparing {1} and {2}", A, B);
            return String.Compare (A.ToLower(), B.ToLower());
        }

        public void SaveColumnsState ()
        {
            // Save column states
            int i = 0;
            foreach (Gtk.TreeViewColumn column in Columns) {
                if (column.Title == "Icon") {
                    columnsIconSettings.SetInt ("order", i);
                    columnsIconSettings.SetInt ("width", column.FixedWidth);
                    columnsIconSettings.SetBoolean ("visible", column.Visible);
                }
                else if (column.Title == "Author") {
                    columnsAuthorSettings.SetInt ("order", i);
                    columnsAuthorSettings.SetInt ("width", column.FixedWidth);
                    columnsAuthorSettings.SetBoolean ("visible", column.Visible);
                }
                else if (column.Title == "Title") {
                    columnsTitleSettings.SetInt ("order", i);
                    columnsTitleSettings.SetInt ("width", column.FixedWidth);
                    columnsTitleSettings.SetBoolean ("visible", column.Visible);
                }
                else if (column.Title == "Year") {
                    columnsYearSettings.SetInt ("order", i);
                    columnsYearSettings.SetInt ("width", column.FixedWidth);
                    columnsYearSettings.SetBoolean ("visible", column.Visible);
                }
                else if (column.Title == "Journal") {
                    columnsJournalSettings.SetInt ("order", i);
                    columnsJournalSettings.SetInt ("width", column.FixedWidth);
                    columnsJournalSettings.SetBoolean ("visible", column.Visible);
                }
                else if (column.Title == "Bibtex Key") {
                    columnsBibtexKeySettings.SetInt ("order", i);
                    columnsBibtexKeySettings.SetInt ("width", column.FixedWidth);
                    columnsBibtexKeySettings.SetBoolean ("visible", column.Visible);
                }
                else if (column.Title == "Volume") {
                    columnsVolumeSettings.SetInt ("order", i);
                    columnsVolumeSettings.SetInt ("width", column.FixedWidth);
                    columnsVolumeSettings.SetBoolean ("visible", column.Visible);
                }
                else if (column.Title == "Pages") {
                    columnsPagesSettings.SetInt ("order", i);
                    columnsPagesSettings.SetInt ("width", column.FixedWidth);
                    columnsPagesSettings.SetBoolean ("visible", column.Visible);
                }
                i++;
            }
        }

        /* ----------------------------------------------------------------- */
        /* CALLBACKS                                                         */
        /* ----------------------------------------------------------------- */

        protected virtual void OnColumnsChanged (object o, EventArgs args)
        {
            SaveColumnsState ();
        }

        protected virtual void OnColumnSort (object o, EventArgs a)
        {
            var col = (Gtk.TreeViewColumn)o;
            Debug.WriteLine (5, "OnColumnSort: Column ID is {0} and {1}", col.SortColumnId, col.SortOrder.ToString ());
            
            int sortColumn;
            Gtk.SortType sortType;
            sorter.GetSortColumnId (out sortColumn, out sortType);
            if (sortColumn == -1) {
                sorter.SetSortColumnId (-1, col.SortOrder);
            } else {
                sorter.SetSortColumnId (col.SortColumnId, col.SortOrder);
            }
        }

        protected virtual void OnRowActivated (object o, Gtk.RowActivatedArgs args)
        {
            Debug.WriteLine (5, "Row activated");
            
            Gtk.TreeIter iter;
            BibtexRecord record;

            if (!Model.GetIter (out iter, args.Path)) {
                Debug.WriteLine (5, "Failed to open record because of GetIter faliure");
                return;
            }
            record = (BibtexRecord)Model.GetValue (iter, 0);
            string uriString = record.GetURI ();
			if (string.IsNullOrEmpty (uriString)) {
				Debug.WriteLine (5, "Selected record does not have a URI field");
				return;
			}

            var uri = new Uri (uriString);
			var list = new GLib.List (typeof(String));
            list.Append (uriString);

			if (System.IO.File.Exists (uri.LocalPath)) {
                
                

                bool uncertain;
                string result;
                byte data;
                ulong data_size;

                data_size = 0;

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // TODO: Does this work under linux?
                    System.Diagnostics.Process.Start(@uri.ToString());
                }
                else
                {

                    result = GLib.ContentType.Guess(uri.ToString(), out data, data_size, out uncertain);

                    if (result != null & result != "" & !uncertain)
                    {


                        GLib.IAppInfo app;

                        app = GLib.AppInfoAdapter.GetDefaultForType(result, true);

                        if (app != null)
                        {

                            GLib.AppLaunchContext appContext;

                            appContext = new GLib.AppLaunchContext();
                            app.LaunchUris(list, appContext);
                            return;
                        }
                    }
                }
            } else {
				var md = new Gtk.MessageDialog ((Gtk.Window)Toplevel, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Error, Gtk.ButtonsType.Close, "Error loading associated file:\n" + uri.LocalPath);
                md.Run ();
                md.Destroy ();

                Debug.WriteLine (0, "Error loading associated file:\n{0}", uri.LocalPath);
            }
        }

        protected virtual void OnDragLeave (object o, Gtk.DragLeaveArgs args)
        {
            UnsetRowsDragDest ();
        }

        protected virtual void OnDragMotion (object o, Gtk.DragMotionArgs args)
        {
            // FIXME: how do we check from here if that drag has data that we want?
            
            Gtk.TreePath path;
            Gtk.TreeViewDropPosition drop_position;
            if (GetDestRowAtPos (args.X, args.Y, out path, out drop_position)) {
                SetDragDestRow (path, Gtk.TreeViewDropPosition.IntoOrAfter);
            } else
                UnsetRowsDragDest ();
        }
        
    }
}
