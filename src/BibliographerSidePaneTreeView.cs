// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;

namespace bibliographer
{
    class SidePaneTreeView : Gtk.TreeView
    {
        public SidePaneTreeView (SidePaneTreeStore sidePaneStore)
        {
            this.Model = sidePaneStore;
            
            Gtk.TreeCellDataFunc filterTextDataFunc = new Gtk.TreeCellDataFunc (RenderFilterColumnTextFromBibtexRecords);
            Gtk.TreeViewColumn col = new Gtk.TreeViewColumn ("Filter", new Gtk.CellRendererText (), "text");
            
            col.SetCellDataFunc (col.CellRenderers[0], filterTextDataFunc);
            col.Sizing = Gtk.TreeViewColumnSizing.Autosize;
            
            this.AppendColumn (col);
            
            Gtk.TreePath path = sidePaneStore.GetPathAll ();
            this.SetCursor (path, col, false);
            
            // Callbacks for the sidePaneTreeView
            //this.Selection.Changed += OnSidePaneTreeSelectionChanged;
            
            this.Show ();
        }

        private void RenderFilterColumnTextFromBibtexRecords (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            //System.Console.WriteLine("Rendering cell");
            string val = (string)model.GetValue (iter, 0);
            //System.Console.WriteLine("Value = " + val);
            (cell as Gtk.CellRendererText).Text = StringOps.TeXToUnicode (val);
        }
        
    }
}
