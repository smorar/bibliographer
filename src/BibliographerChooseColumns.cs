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
    
    
    public partial class BibliographerChooseColumns : Gtk.Dialog
    {
        
        public BibliographerChooseColumns()
        {
            this.Build();
        }
        
        public void ConstructDialog(Gtk.TreeViewColumn[] columns)
        {
            int rows = 5;
            int i = 0;
            Gtk.VBox vbox;
            vbox = new Gtk.VBox();
            columnChecklistHbox.Add(vbox);
            vbox.Show();
            foreach (Gtk.TreeViewColumn column in columns)
            {
                Gtk.CheckButton checkbutton;
    
                checkbutton = new Gtk.CheckButton();
                checkbutton.Data.Add("column", column);
                checkbutton.Active = column.Visible;
                checkbutton.Label = column.Title;
                
                checkbutton.Clicked += OnCheckButtonClicked;
                
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
        }

        protected virtual void OnCheckButtonClicked(object o, System.EventArgs e)
        {
            Gtk.CheckButton checkbutton = (Gtk.CheckButton) o;

            Gtk.TreeViewColumn column = (Gtk.TreeViewColumn) checkbutton.Data["column"];
            
            column.Visible = checkbutton.Active;
            if (Config.KeyExists("Columns/"+column.Title+"/width"))
                column.FixedWidth = Config.GetInt("Columns/"+column.Title+"/width");
            else
                column.FixedWidth = 100;
        }
    }
}