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

namespace bibliographer
{
    public class SearchEntry : bibliographer.widget.SearchEntry
    {
        public new event System.EventHandler Changed;
        
        public SearchEntry()
        {
            this.BorderWidth = 6;
            this.WidthRequest = 200;

            Gtk.Menu searchMenu = this.Menu;

            Gtk.RadioMenuItem searchMenuItemAll = new Gtk.RadioMenuItem("All");
            Gtk.RadioMenuItem searchMenuItemAuthor = new Gtk.RadioMenuItem(searchMenuItemAll,"Author");
            Gtk.RadioMenuItem searchMenuItemTitle = new Gtk.RadioMenuItem(searchMenuItemAll,"Title");
            Gtk.RadioMenuItem searchMenuItemArticle = new Gtk.RadioMenuItem(searchMenuItemAll,"Article");

            searchMenuItemAll.Data.Add("searchField", BibtexSearchField.All);
            searchMenuItemAuthor.Data.Add("searchField", BibtexSearchField.Author);
            searchMenuItemTitle.Data.Add("searchField", BibtexSearchField.Title);
            searchMenuItemArticle.Data.Add("searchField", BibtexSearchField.Article);

            searchMenu.Add(searchMenuItemAll);
            searchMenu.Add(searchMenuItemAuthor);
            searchMenu.Add(searchMenuItemTitle);
            searchMenu.Add(searchMenuItemArticle);

            searchMenuItemAll.Activated += OnSearchEntryChanged;
            searchMenuItemAuthor.Activated += OnSearchEntryChanged;
            searchMenuItemTitle.Activated += OnSearchEntryChanged;
            searchMenuItemArticle.Activated += OnSearchEntryChanged;
            this.InnerEntry.Changed += OnSearchEntryChanged;

            this.Show();
        }

        public void Clear()
        {
            this.InnerEntry.Text = "";
        }
        
        protected virtual void OnSearchEntryChanged (object sender, System.EventArgs e)
        {
            if (this.Changed != null)
                this.Changed(sender, e);
        }
    }
}