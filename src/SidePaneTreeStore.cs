// Copyright 2005-2007 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;

namespace bibliographer
{

public class SidePaneTreeStore : Gtk.TreeStore, Gtk.TreeSortable
{
    Gtk.TreeIter iterAll, iterAuth, iterYear, iterJourn;
    BibtexRecords bibtexRecords;
    
    private bool threadLock = false;
    
    public SidePaneTreeStore(BibtexRecords btRecords)
    {
        bibtexRecords = btRecords;

        bibtexRecords.RecordModified += OnBibtexRecordModified;
        bibtexRecords.RecordAdded += OnBibtexRecordAdded;
        bibtexRecords.RecordDeleted += OnBibtexRecordDeleted;

        GLib.GType [] coltype = new GLib.GType[1];
        coltype[0] = (GLib.GType) typeof(string);
        this.ColumnTypes = coltype;

        Debug.WriteLine(5, "Adding Side Pane Filter Categories");
        
        iterAll = this.AppendNode();
        this.SetValue(iterAll, 0, "All");
        iterAuth = this.AppendNode();
        this.SetValue(iterAuth, 0, "Author");
        iterYear = this.AppendNode();
        this.SetValue(iterYear, 0, "Year");
        iterJourn = this.AppendNode();
        this.SetValue(iterJourn, 0, "Journal");
        
        InitialiseTreeStore();
    }

    private void OnBibtexRecordModified(object o, EventArgs e)
    {
        Debug.WriteLine(5, "BibtexRecordModified");
		BibtexRecord btRecord;
		btRecord = (BibtexRecord) o;
		
		Debug.WriteLine(10, btRecord.ToBibtexString());
		
        UpdateTreeStore(btRecord);
    }
    
    private void OnBibtexRecordAdded(object o, EventArgs e)
    {
        Debug.WriteLine(5, "BibtexRecordAdded");
    	BibtexRecord btRecord;
    	btRecord = (BibtexRecord) o;
		Debug.WriteLine(10, btRecord.ToBibtexString());
		UpdateTreeStore(btRecord);
    }

    private void OnBibtexRecordDeleted(object o, EventArgs e)
    {
        Debug.WriteLine(5, "BibtexRecordDeleted");
    	BibtexRecord btRecord;
    	btRecord = (BibtexRecord) o;
		Debug.WriteLine(10, btRecord.ToBibtexString());
		UpdateTreeStore(btRecord);
    }

	public void InitialiseTreeStore()
	{
		InitialiseAuthors();
		InitialiseJournals();
		InitialiseYears();
	}

	public void UpdateTreeStore(BibtexRecord btRecord)
	{
		Debug.WriteLine(5, "Updating TreeStore");
		UpdateAuthors(btRecord);
		UpdateJournals(btRecord);
		UpdateYears(btRecord);
	}
	
	private void InitialiseAuthors()
	{
		string author;
		StringArrayList bibrecsAuthors = new StringArrayList();
		bibrecsAuthors = bibtexRecords.GetAuthors();
		for (int ii = 0; ii < bibrecsAuthors.Count; ii++)
		{
			author = bibrecsAuthors[ii];
			
			// Insert Author
			Debug.WriteLine(5, "Inserting Author into Filter List");
			StringArrayList treeAuthors = this.GetAuthors();
			if (((treeAuthors.Contains(author)) == false) && (author != null) && (author != ""))
			{
				// Tree does not contain the new author, so we're inserting it
				this.AppendValues(iterAuth, author);
			}
		}
		
		// Iterate through tree and remove authors that are not in the bibtexRecords
		Gtk.TreeIter iter;
		StringArrayList treeAuth = new StringArrayList();
		this.IterChildren(out iter, iterAuth);
		int n = this.IterNChildren(iterAuth);
		for (int i = 0; i < n; i++)
		{
			string author1 = (string) this.GetValue(iter, 0);
			if (bibrecsAuthors.Contains(author1) && treeAuth.Contains(author1) == false)
			{
				this.IterNext(ref iter);
				treeAuth.Add(author1);
			}
			else
			{
				Debug.WriteLine(5, "Removing Author {0} from Side Pane", author1);
				this.Remove(ref iter);
			}
		}
	}
	
	private void InitialiseJournals()
	{
		string journal;
		StringArrayList bibrecsJournals = new StringArrayList();
		bibrecsJournals = bibtexRecords.GetJournals();
		for (int ii = 0; ii < bibrecsJournals.Count; ii++)
		{
			journal = bibrecsJournals[ii];
			
			// Insert Journal
			Debug.WriteLine(5, "Inserting Journal into Filter List");
			StringArrayList treeJournals = this.GetJournals();
			if (((treeJournals.Contains(journal)) == false) && (journal != null) && (journal != ""))
			{
				// Tree does not contain the new journal, so we're inserting it
				this.AppendValues(iterJourn, journal);
			}
		}
		
		// Iterate through tree and remove journals that are not in the bibtexRecords
		StringArrayList treeJourn = new StringArrayList();
		Gtk.TreeIter iter;
		this.IterChildren(out iter, iterJourn);
		int n = this.IterNChildren(iterJourn);
		for (int i = 0; i < n; i++)
		{
			string journal1 = (string) this.GetValue(iter, 0);
			if (bibrecsJournals.Contains(journal1) && treeJourn.Contains(journal1) == false)
			{
				this.IterNext(ref iter);
				treeJourn.Add(journal1);
			}	
			else
			{
				Debug.WriteLine(5, "Removing Journal {0} from Side Pane", journal1);
				this.Remove(ref iter);
			}
		}
	}
	
	private void InitialiseYears()
	{
		string year;
		StringArrayList bibrecsYear = new StringArrayList();
		bibrecsYear = bibtexRecords.GetYears();
		for (int ii = 0; ii < bibrecsYear.Count; ii++)
		{
			year = bibrecsYear[ii];
			
			// Insert Year
			Debug.WriteLine(5, "Inserting Year into Filter List");
			StringArrayList treeYear = this.GetYears();
			if (((treeYear.Contains(year)) == false) && (year != null) && (year != ""))
			{
				// Tree does not contain the new year, so we're inserting it
				this.AppendValues(iterYear, year);
			}
		}
		
		// Iterate through tree and remove years that are not in the bibtexRecords
		Gtk.TreeIter iter;
		StringArrayList treeYr = new StringArrayList();
		this.IterChildren(out iter, iterYear);
		int n = this.IterNChildren(iterYear);
		for (int i = 0; i < n; i++)
		{
			string year1 = (string) this.GetValue(iter, 0);
			if (bibrecsYear.Contains(year1) && treeYr.Contains(year1) == false)
			{
				this.IterNext(ref iter);
				treeYr.Add(year1);
			}
			else
			{
				Debug.WriteLine(5, "Removing Year {0} from Side Pane", year1);
				this.Remove(ref iter);
			}
		}
	}
	
	private void UpdateAuthors(BibtexRecord btRecord)
	{
		string author;
		StringArrayList bibrecAuthors = new StringArrayList();
		bibrecAuthors = btRecord.GetAuthors();
		StringArrayList bibrecsAuthors = new StringArrayList();
		bibrecsAuthors = bibtexRecords.GetAuthors();
		
		// Interate through updated record's authors
		for (int ii = 0; ii < bibrecAuthors.Count; ii++)
		{
			author = bibrecAuthors[ii];
			// Insert Author
			Debug.WriteLine(5, "Inserting Author into Filter List");
			StringArrayList treeAuthors = this.GetAuthors();
			if (((treeAuthors.Contains(author)) == false) && (author != null) && (author != ""))
			{
				// Tree does not contain the new author, so we're inserting it
				this.AppendValues(iterAuth, author);
			}
		}
		
		// Iterate through tree and remove authors that are not in the bibtexRecords
		Gtk.TreeIter iter;
		StringArrayList treeAuth = new StringArrayList();
		this.IterChildren(out iter, iterAuth);
		int n = this.IterNChildren(iterAuth);
		for (int i = 0; i < n; i++)
		{
			string author1 = (string) this.GetValue(iter, 0);
			if (bibrecsAuthors.Contains(author1) && treeAuth.Contains(author1) == false)
			{
				this.IterNext(ref iter);
				treeAuth.Add(author1);
			}
			else
			{
				Debug.WriteLine(5, "Removing Author {0} from Side Pane", author1);
				this.Remove(ref iter);
			}
		}
	}
	
	private void UpdateJournals(BibtexRecord btRecord)
	{
		string journal;
		
		journal = btRecord.GetJournal();
		StringArrayList bibrecsJournals = new StringArrayList();
		bibrecsJournals = bibtexRecords.GetJournals();
		
		// Insert Journal
		Debug.WriteLine(5, "Inserting Journal into Filter List");
		StringArrayList treeJournals = this.GetJournals();
		if (((treeJournals.Contains(journal)) == false) && (journal != null) && (journal != ""))
		{
			// Tree does not contain the new journal, so we're inserting it
			this.AppendValues(iterJourn, journal);
		}
		
		// Iterate through tree and remove journals that are not in the bibtexRecords
		StringArrayList treeJourn = new StringArrayList();
		Gtk.TreeIter iter;
		this.IterChildren(out iter, iterJourn);
		int n = this.IterNChildren(iterJourn);
		for (int i = 0; i < n; i++)
		{
			string journal1 = (string) this.GetValue(iter, 0);
			if (bibrecsJournals.Contains(journal1) && treeJourn.Contains(journal1) == false)
			{
				this.IterNext(ref iter);
				treeJourn.Add(journal1);
			}	
			else
			{
				Debug.WriteLine(5, "Removing Journal {0} from Side Pane", journal1);
				this.Remove(ref iter);
			}
		}
	}
	
	private void UpdateYears(BibtexRecord btRecord)
	{
		string year;
		year = btRecord.GetYear();
		
		StringArrayList bibrecsYear = new StringArrayList();
		bibrecsYear = bibtexRecords.GetYears();
		// Insert Year
		Debug.WriteLine(5, "Inserting Year into Filter List");
		StringArrayList treeYear = this.GetYears();
		if (((treeYear.Contains(year)) == false) && (year != null) && (year != ""))
		{
			// Tree does not contain the new year, so we're inserting it
			this.AppendValues(iterYear, year);
		}
		
		// Iterate through tree and remove years that are not in the bibtexRecords
		Gtk.TreeIter iter;
		StringArrayList treeYr = new StringArrayList();
		this.IterChildren(out iter, iterYear);
		int n = this.IterNChildren(iterYear);
		for (int i = 0; i < n; i++)
		{
			string year1 = (string) this.GetValue(iter, 0);
			if (bibrecsYear.Contains(year1) && treeYr.Contains(year1) == false)
			{
				this.IterNext(ref iter);
				treeYr.Add(year1);
			}
			else
			{
				Debug.WriteLine(5, "Removing Year {0} from Side Pane", year1);
				this.Remove(ref iter);
			}
		}
	}
	
    public void UpdateTreeStore()
    {
        if (this.threadLock == false)
        {
            this.threadLock = true;
            
        if (bibtexRecords != null)
        {
            // Deal with authors
            StringArrayList bibAuthors;
            if (bibtexRecords.GetAuthors() == null)
                bibAuthors = new StringArrayList();
            else
                bibAuthors = (StringArrayList) bibtexRecords.GetAuthors();
            if (this.IterHasChild(iterAuth))
            {
                Gtk.TreeIter iter;
                this.IterChildren(out iter, iterAuth);
                int n = this.IterNChildren(iterAuth);
                for (int i = 0; i < n; i++)
                {
                    string author = (string) this.GetValue(iter, 0);
                    if (bibAuthors.Contains(author))
                        this.IterNext(ref iter);
                    else
                    {
                        Debug.WriteLine(5, "Removing Author {0} from Side Pane", author);
                        this.Remove(ref iter);
                    }
                }
                StringArrayList treeAuthors = this.GetAuthors();
                for (int i = 0; i < bibAuthors.Count; i++)
                {
                    if (!(treeAuthors.Contains(bibAuthors[i])))
                    {
                        // Add bibAuthor to the TreeStore
                        if (bibAuthors[i] != "")
                        {
                            Debug.WriteLine(5, "Inserting Author {0} to Side Pane", bibAuthors[i]);
                            Gtk.TreeIter insert = this.InsertNode(iterAuth, i);
                            this.SetValue(insert, 0, bibAuthors[i]);
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine(5, "Generating Author Filter List");
                for (int i = 0; i < bibAuthors.Count; i++)
                {
                    if (bibAuthors[i] != "")
                        this.AppendValues(iterAuth, bibAuthors[i]);
                }
            }

            // Deal with years
            StringArrayList bibYears;
            if (bibtexRecords.GetYears() == null)
                bibYears = new StringArrayList();
            else
                bibYears = (StringArrayList) bibtexRecords.GetYears();

            if (this.IterHasChild(iterYear))
            {
                Gtk.TreeIter iter;
                this.IterChildren(out iter, iterYear);
                for (int i = 0; i < this.IterNChildren(iterYear); i++)
                {
                    string year = (string) this.GetValue(iter, 0);

                    if (bibYears.Contains(year))
                        this.IterNext(ref iter);
                    else
                    {
                        Debug.WriteLine(5, "Removing Year {0} from Side Pane", year);
                        this.Remove(ref iter);
                    }
                }
                StringArrayList treeYears = this.GetYears();
                for (int i = 0; i < bibYears.Count; i++)
                {
                    if (!(treeYears.Contains(bibYears[i])))
                    {
                        // Add bibYear to the TreeStore
                        if (bibYears[i] != "")
                        {
                            Debug.WriteLine(5, "Inserting Year {0} to Side Pane", bibYears[i]);
                            Gtk.TreeIter insert = this.InsertNode(iterYear, i);
                            this.SetValue(insert, 0, bibYears[i]);
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine(5, "Generating Year Filter List");
                for (int i = 0; i < bibYears.Count; i++)
                {
                    if (bibYears[i] != "")
                        this.AppendValues(iterYear, bibYears[i]);
                }
            }

            // Deal with journals
            StringArrayList bibJournals;
            if (bibtexRecords.GetJournals() == null)
                bibJournals = new StringArrayList();
            else
                bibJournals = (StringArrayList) bibtexRecords.GetJournals();

            if (this.IterHasChild(iterJourn))
            {
                Gtk.TreeIter iter;
                this.IterChildren(out iter, iterJourn);
                for (int i = 0; i < this.IterNChildren(iterJourn); i++)
                {
                    string journal = (string) this.GetValue(iter, 0);

                    if (bibYears.Contains(journal))
                        this.IterNext(ref iter);
                    else
                    {
                        Debug.WriteLine(5, "Removing Journal {0} from Side Pane", journal);
                        this.Remove(ref iter);
                    }
                }
                StringArrayList treeJournals = this.GetJournals();
                for (int i = 0; i < bibJournals.Count; i++)
                {
                    if (!(treeJournals.Contains(bibJournals[i])))
                    {
                        // Add bibYear to the TreeStore
                        if (bibJournals[i] != "")
                        {
                            Debug.WriteLine(5, "Inserting Journal {0} to Side Pane", bibJournals[i]);
                            Gtk.TreeIter insert = this.InsertNode(iterJourn, i);
                            this.SetValue(insert, 0, bibJournals[i]);
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine(5, "Generating Journal Filter List");
                for (int i = 0; i < bibJournals.Count; i++)
                {
                    if (bibJournals[i] != "")
                        this.AppendValues(iterJourn, bibJournals[i]);
                }
            }
        }
        this.threadLock = false;
        }
			else
			{
				Debug.WriteLine(2, "Thread is locked");
			}
    }

    public void SetBibtexRecords(BibtexRecords btRecords)
    {
        bibtexRecords = btRecords;
        bibtexRecords.RecordModified += OnBibtexRecordModified;
        bibtexRecords.RecordAdded += OnBibtexRecordAdded;
        bibtexRecords.RecordDeleted += OnBibtexRecordDeleted;
        InitialiseTreeStore();
    }

    private StringArrayList GetAuthors()
    {
        StringArrayList authors = new StringArrayList();

        if (this.IterHasChild(iterAuth))
        {
            Gtk.TreeIter iter;
            this.IterChildren(out iter, iterAuth);
			while(this.IterIsValid(iter))
            {
                string author = (string) this.GetValue(iter, 0);
                authors.Add(author);
                this.IterNext(ref iter);
            }
        }
        authors.Sort();
        return authors;
    }

    private StringArrayList GetYears()
    {
        StringArrayList years = new StringArrayList();

        if (this.IterHasChild(iterYear))
        {

            Gtk.TreeIter iter;
            this.IterChildren(out iter, iterYear);
			while(this.IterIsValid(iter))
            {
                string year = (string) this.GetValue(iter, 0);
                years.Add(year);
                this.IterNext(ref iter);
            }
        }
        years.Sort();
        return years;
    }

    private StringArrayList GetJournals()
    {
        StringArrayList journals = new StringArrayList();

        if (this.IterHasChild(iterJourn))
        {
            Gtk.TreeIter iter;
            this.IterChildren(out iter, iterJourn);
			while(this.IterIsValid(iter))
            {
                string journal = (string) this.GetValue(iter, 0);
                journals.Add(journal);
                this.IterNext(ref iter);
            }
        }
        journals.Sort();
        return journals;
    }

    public Gtk.TreePath GetPathAll()
    {
        return this.GetPath(iterAll);
    }
}
}
