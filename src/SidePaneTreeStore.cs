//
//  SidePaneTreeStore.cs
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

    public class SidePaneTreeStore : Gtk.TreeStore
    {
        Gtk.TreeIter iterAll, iterAuth, iterYear, iterJourn;
        BibtexRecords bibtexRecords;

        bool threadLock;

        public SidePaneTreeStore (BibtexRecords btRecords)
        {
            bibtexRecords = btRecords;
            
            bibtexRecords.RecordModified += OnBibtexRecordModified;
            bibtexRecords.RecordAdded += OnBibtexRecordAdded;
            bibtexRecords.RecordDeleted += OnBibtexRecordDeleted;
            
            var coltype = new GLib.GType[1];
            coltype[0] = (GLib.GType)typeof(string);
            ColumnTypes = coltype;
            
            Debug.WriteLine (5, "Adding Side Pane Filter Categories");
            
            iterAll = AppendNode ();
            SetValue (iterAll, 0, "All");
            iterAuth = AppendNode ();
            SetValue (iterAuth, 0, "Author");
            iterYear = AppendNode ();
            SetValue (iterYear, 0, "Year");
            iterJourn = AppendNode ();
            SetValue (iterJourn, 0, "Journal");
            
            InitialiseTreeStore ();
        }

        void OnBibtexRecordModified (object o, EventArgs e)
        {
            Debug.WriteLine (5, "BibtexRecordModified");
            BibtexRecord btRecord;
            btRecord = (BibtexRecord)o;
            
            Debug.WriteLine (10, btRecord.ToBibtexString ());
            
            UpdateTreeStore (btRecord);
        }

        void OnBibtexRecordAdded (object o, EventArgs e)
        {
            Debug.WriteLine (5, "BibtexRecordAdded");
            BibtexRecord btRecord;
            btRecord = (BibtexRecord)o;
            Debug.WriteLine (10, btRecord.ToBibtexString ());
            UpdateTreeStore (btRecord);
        }

        void OnBibtexRecordDeleted (object o, EventArgs e)
        {
            Debug.WriteLine (5, "BibtexRecordDeleted");
            BibtexRecord btRecord;
            btRecord = (BibtexRecord)o;
            Debug.WriteLine (10, btRecord.ToBibtexString ());
            UpdateTreeStore (btRecord);
        }

        public void InitialiseTreeStore ()
        {
            InitialiseAuthors ();
            InitialiseJournals ();
            InitialiseYears ();
        }

        public void UpdateTreeStore (BibtexRecord btRecord)
        {
            Debug.WriteLine (5, "Updating TreeStore");
            UpdateAuthors (btRecord);
            UpdateJournals (btRecord);
            UpdateYears (btRecord);
        }

        void InitialiseAuthors ()
        {
            string author;
            StringArrayList bibrecsAuthors;

            bibrecsAuthors = bibtexRecords.GetAuthors ();
            for (int ii = 0; ii < bibrecsAuthors.Count; ii++) {
                author = bibrecsAuthors[ii];
                
                // Insert Author
                StringArrayList treeAuthors = GetAuthors ();
                Debug.WriteLine (5, "Checking if Author {0} is in filter list", author);
                if ((!treeAuthors.Contains (author)) && (author != null) && (author != "")) {
                    // Tree does not contain the new author, so we're inserting it
                    Debug.WriteLine (5, "Inserting Author {0} into Filter List", author);
                    AppendValues (iterAuth, author);
                }
            }
            Debug.WriteLine (10, "Finished inserting Authors into Filter List");
            
            // Iterate through tree and remove authors that are not in the bibtexRecords
            Gtk.TreeIter iter;
            var treeAuth = new StringArrayList ();
            IterChildren (out iter, iterAuth);
            int n = IterNChildren (iterAuth);
            for (int i = 0; i < n; i++) {
                string author1 = (string)GetValue (iter, 0);
                if (bibrecsAuthors.Contains (author1) && !treeAuth.Contains (author1)) {
                    IterNext (ref iter);
                    treeAuth.Add (author1);
                } else {
                    Debug.WriteLine (5, "Removing Author {0} from Side Pane", author1);
                    Remove (ref iter);
                }
            }
        }

        void InitialiseJournals ()
        {
            string journal;
            StringArrayList bibrecsJournals;

            bibrecsJournals = bibtexRecords.GetJournals ();
            for (int ii = 0; ii < bibrecsJournals.Count; ii++) {
                journal = bibrecsJournals[ii];
                
                // Insert Journal
                StringArrayList treeJournals = GetJournals ();
                if ((!treeJournals.Contains (journal)) && (journal != null) && (journal != "")) {
                    // Tree does not contain the new journal, so we're inserting it
                    Debug.WriteLine (5, "Inserting Journal into Filter List");
                    AppendValues (iterJourn, journal);
                }
            }
            
            // Iterate through tree and remove journals that are not in the bibtexRecords
            var treeJourn = new StringArrayList ();
            Gtk.TreeIter iter;
            IterChildren (out iter, iterJourn);
            int n = IterNChildren (iterJourn);
            for (int i = 0; i < n; i++) {
                string journal1 = (string)GetValue (iter, 0);
                if (bibrecsJournals.Contains (journal1) && !treeJourn.Contains (journal1)) {
                    IterNext (ref iter);
                    treeJourn.Add (journal1);
                } else {
                    Debug.WriteLine (5, "Removing Journal {0} from Side Pane", journal1);
                    Remove (ref iter);
                }
            }
        }

        void InitialiseYears ()
        {
            string year;
            StringArrayList bibrecsYear;

			bibrecsYear = bibtexRecords.GetYears ();
            for (int ii = 0; ii < bibrecsYear.Count; ii++) {
                year = bibrecsYear[ii];
                
                // Insert Year
                StringArrayList treeYear = GetYears ();
                if ((!treeYear.Contains (year)) && (year != null) && (year != "")) {
                    // Tree does not contain the new year, so we're inserting it
                    Debug.WriteLine (5, "Inserting Year into Filter List");
                    AppendValues (iterYear, year);
                }
            }
            
            // Iterate through tree and remove years that are not in the bibtexRecords
            Gtk.TreeIter iter;
            var treeYr = new StringArrayList ();
            IterChildren (out iter, iterYear);
            int n = IterNChildren (iterYear);
            for (int i = 0; i < n; i++) {
                string year1 = (string)GetValue (iter, 0);
                if (bibrecsYear.Contains (year1) && !treeYr.Contains (year1)) {
                    IterNext (ref iter);
                    treeYr.Add (year1);
                } else {
                    Debug.WriteLine (5, "Removing Year {0} from Side Pane", year1);
                    Remove (ref iter);
                }
            }
        }

        void UpdateAuthors (BibtexRecord btRecord)
        {
            string author;
            StringArrayList bibrecAuthors;
            bibrecAuthors = btRecord.GetAuthors ();
            StringArrayList bibrecsAuthors;
            bibrecsAuthors = bibtexRecords.GetAuthors ();
            
            // Interate through updated record's authors
            for (int ii = 0; ii < bibrecAuthors.Count; ii++) {
                author = bibrecAuthors[ii];
                // Insert Author
                StringArrayList treeAuthors = GetAuthors ();
                if ((!treeAuthors.Contains (author)) && (author != null) && (author != "")) {
                    // Tree does not contain the new author, so we're inserting it
                    Debug.WriteLine (5, "Inserting Author {0} into Filter List", author);
                    AppendValues (iterAuth, author);
                }
            }
            
            // Iterate through tree and remove authors that are not in the bibtexRecords
            Gtk.TreeIter iter;
            var treeAuth = new StringArrayList ();
            IterChildren (out iter, iterAuth);
            int n = IterNChildren (iterAuth);
            for (int i = 0; i < n; i++) {
                string author1 = (string)GetValue (iter, 0);
                if (bibrecsAuthors.Contains (author1) && !treeAuth.Contains (author1)) {
                    IterNext (ref iter);
                    treeAuth.Add (author1);
                } else {
                    Debug.WriteLine (5, "Removing Author {0} from Side Pane", author1);
                    Remove (ref iter);
                }
            }
        }

        void UpdateJournals (BibtexRecord btRecord)
        {
            string journal;
            
            journal = btRecord.GetJournal ();
            StringArrayList bibrecsJournals;
            bibrecsJournals = bibtexRecords.GetJournals ();
            
            // Insert Journal
            StringArrayList treeJournals = GetJournals ();
            if ((!treeJournals.Contains (journal)) && (journal != null) && (journal != "")) {
                // Tree does not contain the new journal, so we're inserting it
                Debug.WriteLine (5, "Inserting Journal into Filter List");
                AppendValues (iterJourn, journal);
            }
            
            // Iterate through tree and remove journals that are not in the bibtexRecords
            var treeJourn = new StringArrayList ();
            Gtk.TreeIter iter;
            IterChildren (out iter, iterJourn);
            int n = IterNChildren (iterJourn);
            for (int i = 0; i < n; i++) {
                string journal1 = (string)GetValue (iter, 0);
                if (bibrecsJournals.Contains (journal1) && !treeJourn.Contains (journal1)) {
                    IterNext (ref iter);
                    treeJourn.Add (journal1);
                } else {
                    Debug.WriteLine (5, "Removing Journal {0} from Side Pane", journal1);
                    Remove (ref iter);
                }
            }
        }

        void UpdateYears (BibtexRecord btRecord)
        {
            string year;
            year = btRecord.GetYear ();
            
            StringArrayList bibrecsYear;
            bibrecsYear = bibtexRecords.GetYears ();
            // Insert Year
            StringArrayList treeYear = GetYears ();
            if ((!treeYear.Contains (year)) && (year != null) && (year != "")) {
                // Tree does not contain the new year, so we're inserting it
                Debug.WriteLine (5, "Inserting Year into Filter List");
                AppendValues (iterYear, year);
            }
            
            // Iterate through tree and remove years that are not in the bibtexRecords
            Gtk.TreeIter iter;
            var treeYr = new StringArrayList ();
            IterChildren (out iter, iterYear);
            int n = IterNChildren (iterYear);
            for (int i = 0; i < n; i++) {
                string year1 = (string)GetValue (iter, 0);
                if (bibrecsYear.Contains (year1) && !treeYr.Contains (year1)) {
                    IterNext (ref iter);
                    treeYr.Add (year1);
                } else {
                    Debug.WriteLine (5, "Removing Year {0} from Side Pane", year1);
                    Remove (ref iter);
                }
            }
        }

        public void UpdateTreeStore ()
        {
			if (!threadLock) {
				threadLock = true;
                
				if (bibtexRecords != null) {
					// Deal with authors
					StringArrayList bibAuthors;
					if (bibtexRecords.GetAuthors () == null)
						bibAuthors = new StringArrayList ();
					else
						bibAuthors = bibtexRecords.GetAuthors ();
					if (IterHasChild (iterAuth)) {
						Gtk.TreeIter iter;
						IterChildren (out iter, iterAuth);
						int n = IterNChildren (iterAuth);
						for (int i = 0; i < n; i++) {
							string author = (string)GetValue (iter, 0);
							if (bibAuthors.Contains (author))
								IterNext (ref iter);
							else {
								Debug.WriteLine (5, "Removing Author {0} from Side Pane", author);
								Remove (ref iter);
							}
						}
						StringArrayList treeAuthors = GetAuthors ();
						for (int i = 0; i < bibAuthors.Count; i++) {
							if (!(treeAuthors.Contains (bibAuthors [i]))) {
								// Add bibAuthor to the TreeStore
								if (bibAuthors [i] != "") {
									Debug.WriteLine (5, "Inserting Author {0} to Side Pane", bibAuthors [i]);
									Gtk.TreeIter insert = InsertNode (iterAuth, i);
									SetValue (insert, 0, bibAuthors [i]);
								}
							}
						}
					} else {
						Debug.WriteLine (5, "Generating Author Filter List");
						for (int i = 0; i < bibAuthors.Count; i++) {
							if (bibAuthors [i] != "")
								AppendValues (iterAuth, bibAuthors [i]);
						}
					}
                    
					// Deal with years
					StringArrayList bibYears;
					if (bibtexRecords.GetYears () == null)
						bibYears = new StringArrayList ();
					else
						bibYears = bibtexRecords.GetYears ();
                    
					if (IterHasChild (iterYear)) {
						Gtk.TreeIter iter;
						IterChildren (out iter, iterYear);
						for (int i = 0; i < IterNChildren (iterYear); i++) {
							string year = (string)GetValue (iter, 0);
                            
							if (bibYears.Contains (year))
								IterNext (ref iter);
							else {
								Debug.WriteLine (5, "Removing Year {0} from Side Pane", year);
								Remove (ref iter);
							}
						}
						StringArrayList treeYears = GetYears ();
						for (int i = 0; i < bibYears.Count; i++) {
							if (!(treeYears.Contains (bibYears [i]))) {
								// Add bibYear to the TreeStore
								if (bibYears [i] != "") {
									Debug.WriteLine (5, "Inserting Year {0} to Side Pane", bibYears [i]);
									Gtk.TreeIter insert = InsertNode (iterYear, i);
									SetValue (insert, 0, bibYears [i]);
								}
							}
						}
					} else {
						Debug.WriteLine (5, "Generating Year Filter List");
						for (int i = 0; i < bibYears.Count; i++) {
							if (bibYears [i] != "")
								AppendValues (iterYear, bibYears [i]);
						}
					}
                    
					// Deal with journals
					StringArrayList bibJournals;
					if (bibtexRecords.GetJournals () == null)
						bibJournals = new StringArrayList ();
					else
						bibJournals = bibtexRecords.GetJournals ();
                    
					if (IterHasChild (iterJourn)) {
						Gtk.TreeIter iter;
						IterChildren (out iter, iterJourn);
						for (int i = 0; i < IterNChildren (iterJourn); i++) {
							string journal = (string)GetValue (iter, 0);
                            
							if (bibYears.Contains (journal))
								IterNext (ref iter);
							else {
								Debug.WriteLine (5, "Removing Journal {0} from Side Pane", journal);
								Remove (ref iter);
							}
						}
						StringArrayList treeJournals = GetJournals ();
						for (int i = 0; i < bibJournals.Count; i++) {
							if (!(treeJournals.Contains (bibJournals [i]))) {
								// Add bibYear to the TreeStore
								if (bibJournals [i] != "") {
									Debug.WriteLine (5, "Inserting Journal {0} to Side Pane", bibJournals [i]);
									Gtk.TreeIter insert = InsertNode (iterJourn, i);
									SetValue (insert, 0, bibJournals [i]);
								}
							}
						}
					} else {
						Debug.WriteLine (5, "Generating Journal Filter List");
						for (int i = 0; i < bibJournals.Count; i++) {
							if (bibJournals [i] != "")
								AppendValues (iterJourn, bibJournals [i]);
						}
					}
				}
				threadLock = false;
			} else {
				Debug.WriteLine (2, "Thread is locked");
			}
        }

        public void SetBibtexRecords (BibtexRecords btRecords)
        {
            bibtexRecords = btRecords;
            bibtexRecords.RecordModified += OnBibtexRecordModified;
            bibtexRecords.RecordAdded += OnBibtexRecordAdded;
            bibtexRecords.RecordDeleted += OnBibtexRecordDeleted;
            InitialiseTreeStore ();
        }

        StringArrayList GetAuthors ()
        {
            Debug.WriteLine (10, "SidePaneTreeStore.GetAuthors()");
            var authors = new StringArrayList ();
            
            if (IterHasChild (iterAuth)) {
                Gtk.TreeIter iter;
                
                IterChildren (out iter, iterAuth);
                
                while (IterIsValid (iter)) {
                    string author = (string)GetValue (iter, 0);
                    Debug.WriteLine (10, "Add the Author {0} to an output list", author);
                    authors.Add (author);
                    if (!IterNext (ref iter))
                        break;
                }
            }
            authors.Sort ();
            return authors;
        }

        StringArrayList GetYears ()
        {
            var years = new StringArrayList ();
            
            if (IterHasChild (iterYear)) {
                
                Gtk.TreeIter iter;
                IterChildren (out iter, iterYear);
                while (IterIsValid (iter)) {
                    string year = (string)GetValue (iter, 0);
                    years.Add (year);
                    if (!IterNext (ref iter))
                        break;
                }
            }
            years.Sort ();
            return years;
        }

        StringArrayList GetJournals ()
        {
            var journals = new StringArrayList ();
            
            if (IterHasChild (iterJourn)) {
                Gtk.TreeIter iter;
                IterChildren (out iter, iterJourn);
                while (IterIsValid (iter)) {
                    string journal = (string)GetValue (iter, 0);
                    journals.Add (journal);
                    if (!IterNext (ref iter))
                        break;
                }
            }
            journals.Sort ();
            return journals;
        }

        public Gtk.TreePath GetPathAll ()
        {
            return GetPath (iterAll);
        }
    }
}
