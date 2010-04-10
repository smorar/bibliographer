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
	class SidePaneTreeView : Gtk.TreeView
	{
		public SidePaneTreeView(SidePaneTreeStore sidePaneStore)
		{
			this.Model = sidePaneStore;
			
            Gtk.TreeCellDataFunc filterTextDataFunc = new Gtk.TreeCellDataFunc(RenderFilterColumnTextFromBibtexRecords);
            Gtk.TreeViewColumn col = new Gtk.TreeViewColumn("Filter", new Gtk.CellRendererText(), "text");
    
            col.SetCellDataFunc(col.CellRenderers[0], filterTextDataFunc);
            col.Sizing = Gtk.TreeViewColumnSizing.Autosize;
    
            this.AppendColumn(col);
    
            Gtk.TreePath path = sidePaneStore.GetPathAll();
            this.SetCursor(path, col, false);
			
			// Callbacks for the sidePaneTreeView
            //this.Selection.Changed += OnSidePaneTreeSelectionChanged;
			
			this.Show();
		}
		
        private void RenderFilterColumnTextFromBibtexRecords(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            //System.Console.WriteLine("Rendering cell");
            string val = (string) model.GetValue(iter,0);
            //System.Console.WriteLine("Value = " + val);
            (cell as Gtk.CellRendererText).Text = StringOps.TeXToUnicode(val);
        }
        
	}
}
