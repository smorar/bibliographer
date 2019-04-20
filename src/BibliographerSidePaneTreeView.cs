//
//  BibliographerSidePaneTreeView.cs
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

namespace bibliographer
{
    public class SidePaneTreeView : Gtk.TreeView
    {
        public SidePaneTreeView (SidePaneTreeStore sidePaneStore)
        {
            Model = sidePaneStore;
            
            var filterTextDataFunc = new Gtk.TreeCellDataFunc (RenderFilterColumnTextFromBibtexRecords);
            var col = new Gtk.TreeViewColumn ("Filter", new Gtk.CellRendererText (), "text");
            
			col.SetCellDataFunc (col.Cells[0], filterTextDataFunc);
            col.Sizing = Gtk.TreeViewColumnSizing.Autosize;
            
            AppendColumn (col);
            
            Gtk.TreePath path = sidePaneStore.GetPathAll ();
            SetCursor (path, col, false);
            
            // Callbacks for the sidePaneTreeView
            //Selection.Changed += OnSidePaneTreeSelectionChanged;
            
            Show ();
        }

        static void RenderFilterColumnTextFromBibtexRecords (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
        {
            Debug.WriteLine(10, "Rendering cell");
            string val = (string)model.GetValue (iter, 0);
            Debug.WriteLine(10, "Value = " + val);
            (cell as Gtk.CellRendererText).Text = StringOps.TeXToUnicode (val);
        }
        
    }
}
