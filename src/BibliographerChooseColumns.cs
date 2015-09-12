//
//  BibliographerChooseColumns.cs
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
using Gtk;

namespace bibliographer
{
    public class BibliographerChooseColumns : Dialog
    {
        protected Box columnChecklistHbox;

        public BibliographerChooseColumns (TreeViewColumn[] columns)
        {
            Title = "Choose Columns";
            WidthRequest = 200;
            AddButton ("Close", ResponseType.Close);

            ContentArea.Add (new Label ("Visible Columns"));

            columnChecklistHbox = new Box (Orientation.Horizontal, 5);
            ContentArea.Add (columnChecklistHbox);

            const int rows = 8;
            int i = 0;
            VBox vbox;
            vbox = new VBox ();
            columnChecklistHbox.Add (vbox);
            vbox.Show ();
            foreach (TreeViewColumn column in columns) {
                CheckButton checkbutton;
                
                checkbutton = new CheckButton ();
                checkbutton.Data.Add ("column", column);
                checkbutton.Active = column.Visible;
                checkbutton.Label = column.Title;
                
                checkbutton.Clicked += OnCheckButtonClicked;
                
                checkbutton.Show ();
                
                vbox.Add (checkbutton);
                
                if (i == rows - 1) {
                    vbox = new VBox ();
                    columnChecklistHbox.Add (vbox);
                    vbox.Show ();
                    i = 0;
                }
                i = i + 1;
            }
            ShowAll ();
        }

        static void OnCheckButtonClicked (object o, EventArgs e)
        {
            var checkbutton = (CheckButton)o;
            
            var column = (TreeViewColumn)checkbutton.Data["column"];
            
            column.Visible = checkbutton.Active;
        }
    }
}
