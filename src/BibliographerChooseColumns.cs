// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;

namespace bibliographer
{
    public partial class BibliographerChooseColumns : Gtk.Dialog
    {

        public BibliographerChooseColumns ()
        {
            this.Build ();
        }

        public void ConstructDialog (Gtk.TreeViewColumn[] columns)
        {
            int rows = 5;
            int i = 0;
            Gtk.VBox vbox;
            vbox = new Gtk.VBox ();
            columnChecklistHbox.Add (vbox);
            vbox.Show ();
            foreach (Gtk.TreeViewColumn column in columns) {
                Gtk.CheckButton checkbutton;
                
                checkbutton = new Gtk.CheckButton ();
                checkbutton.Data.Add ("column", column);
                checkbutton.Active = column.Visible;
                checkbutton.Label = column.Title;
                
                checkbutton.Clicked += OnCheckButtonClicked;
                
                checkbutton.Show ();
                
                vbox.Add (checkbutton);
                
                if (i == rows - 1) {
                    vbox = new Gtk.VBox ();
                    columnChecklistHbox.Add (vbox);
                    vbox.Show ();
                    i = 0;
                }
                i = i + 1;
            }
        }

        protected virtual void OnCheckButtonClicked (object o, System.EventArgs e)
        {
            Gtk.CheckButton checkbutton = (Gtk.CheckButton)o;
            
            Gtk.TreeViewColumn column = (Gtk.TreeViewColumn)checkbutton.Data["column"];
            
            column.Visible = checkbutton.Active;
            if (Config.KeyExists ("Columns/" + column.Title + "/width"))
                column.FixedWidth = Config.GetInt ("Columns/" + column.Title + "/width");
            else
                column.FixedWidth = 100;
        }
    }
}
