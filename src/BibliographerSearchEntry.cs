//
//  BibliographerSearchEntry.cs
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

using libbibby;

namespace bibliographer
{

	public class SearchEntry : bibliographer.widget.SearchEntry
    {
        public new event System.EventHandler Changed;

        public SearchEntry ()
        {
            WidthRequest = 200;
            
            Gtk.Menu searchMenu = Menu;
            
            var searchMenuItemAll = new Gtk.RadioMenuItem ("All");
			var searchMenuItemAuthor = new Gtk.RadioMenuItem (searchMenuItemAll, "Author");
			var searchMenuItemTitle = new Gtk.RadioMenuItem (searchMenuItemAll, "Title");
			var searchMenuItemArticle = new Gtk.RadioMenuItem (searchMenuItemAll, "Article");
            
            searchMenuItemAll.Data.Add ("searchField", BibtexSearchField.All);
            searchMenuItemAuthor.Data.Add ("searchField", BibtexSearchField.Author);
            searchMenuItemTitle.Data.Add ("searchField", BibtexSearchField.Title);
            searchMenuItemArticle.Data.Add ("searchField", BibtexSearchField.Article);
            
            searchMenu.Add (searchMenuItemAll);
            searchMenu.Add (searchMenuItemAuthor);
            searchMenu.Add (searchMenuItemTitle);
            searchMenu.Add (searchMenuItemArticle);
            
            searchMenuItemAll.Activated += OnSearchEntryChanged;
            searchMenuItemAuthor.Activated += OnSearchEntryChanged;
            searchMenuItemTitle.Activated += OnSearchEntryChanged;
            searchMenuItemArticle.Activated += OnSearchEntryChanged;
			Changed += OnSearchEntryChanged;
            
            Show ();
        }

        public void Clear ()
        {
            Text = "";
        }

        protected virtual void OnSearchEntryChanged (object sender, System.EventArgs e)
        {
            if (Changed != null)
                Changed (sender, e);
        }
    }
}
