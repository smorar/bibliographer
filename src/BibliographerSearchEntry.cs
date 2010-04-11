// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using libbibby;

namespace bibliographer
{
    public class SearchEntry : bibliographer.widget.SearchEntry
    {
        public new event System.EventHandler Changed;

        public SearchEntry ()
        {
            this.BorderWidth = 6;
            this.WidthRequest = 200;
            
            Gtk.Menu searchMenu = this.Menu;
            
            Gtk.RadioMenuItem searchMenuItemAll = new Gtk.RadioMenuItem ("All");
            Gtk.RadioMenuItem searchMenuItemAuthor = new Gtk.RadioMenuItem (searchMenuItemAll, "Author");
            Gtk.RadioMenuItem searchMenuItemTitle = new Gtk.RadioMenuItem (searchMenuItemAll, "Title");
            Gtk.RadioMenuItem searchMenuItemArticle = new Gtk.RadioMenuItem (searchMenuItemAll, "Article");
            
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
            this.InnerEntry.Changed += OnSearchEntryChanged;
            
            this.Show ();
        }

        public void Clear ()
        {
            this.InnerEntry.Text = "";
        }

        protected virtual void OnSearchEntryChanged (object sender, System.EventArgs e)
        {
            if (this.Changed != null)
                this.Changed (sender, e);
        }
    }
}
