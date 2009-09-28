// Copyright 2005-2007 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;

namespace bibliographer
{


public class LitListStore : Gtk.ListStore
{
    private BibtexRecords bibtexRecords;


    public LitListStore(BibtexRecords btRecords)
    {
        GLib.GType [] coltype = new GLib.GType[1];
        coltype[0] = (GLib.GType) typeof(BibtexRecord);
        this.ColumnTypes = coltype;

        if (btRecords == null)
            this.bibtexRecords = new BibtexRecords();
        else
            this.bibtexRecords = btRecords;

        this.bibtexRecords.RecordAdded += this.OnRecordAdded;
        this.bibtexRecords.RecordDeleted += this.OnRecordDeleted;
    }

    private void OnRecordAdded(object o, EventArgs a)
    {
        Debug.WriteLine(5, "Record added in LitListStore");
        this.UpdateListStore();
    }

    private void OnRecordDeleted(object o, EventArgs a)
    {
        Debug.WriteLine(5, "Record deleted in LitListStore");
        this.UpdateListStore();
    }

    public void SetBibtexRecords(BibtexRecords btRecords)
    {
        this.Clear();

        this.bibtexRecords = btRecords;
        this.bibtexRecords.RecordAdded += this.OnRecordAdded;
        this.bibtexRecords.RecordDeleted += this.OnRecordDeleted;

        this.UpdateListStore();
    }

    public BibtexRecords GetBibtexRecords()
    {
        return bibtexRecords;
    }

    private BibtexRecords GetListStoreBibtexRecords()
    {
        Gtk.TreeIter iter;
        BibtexRecords bibtexRecords = new BibtexRecords();

        if (this.GetIterFirst(out iter))
        {
            bibtexRecords.Add(this.GetValue(iter, 0) as BibtexRecord);
            while(this.IterNext(ref iter))
            {
                bibtexRecords.Add(this.GetValue(iter, 0) as BibtexRecord);
            }
        }

        return bibtexRecords;
    }

    public void UpdateListStore()
    {
        Debug.WriteLine(5, "Updating LitListStore");
        if (this.bibtexRecords != null)
        {
            Gtk.TreeIter treeIter;
            BibtexRecord rec;
				
            Debug.WriteLine(5, "bibtexRecords.Count: {0}", this.bibtexRecords.Count);
            if (this.GetIterFirst(out treeIter))
            {
				while (this.IterIsValid(treeIter))
				{
	                rec = this.GetValue(treeIter, 0) as BibtexRecord;
	                if (!this.bibtexRecords.Contains(rec))
					{
			        	this.Remove(ref treeIter);
					}
					if(this.IterIsValid(treeIter))
						this.IterNext(ref treeIter);
				}
            }

			BibtexRecords btRecords = this.GetListStoreBibtexRecords();
            for (int i = 0; i < this.bibtexRecords.Count; i++)
            {
                if (!btRecords.Contains(this.bibtexRecords[i]))
                {
                    Debug.WriteLine(5, "Inserting record into TreeIter at pos: {0}", i);
                    Gtk.TreeIter iter_ = this.Insert(i);
                    this.SetValue(iter_, 0,(BibtexRecord) this.bibtexRecords[i]);
                }
            }
        }
    }

    public Gtk.TreeIter GetIter(BibtexRecord record)
    {
        Gtk.TreeIter iter;
        BibtexRecord rec;

        if (this.GetIterFirst(out iter))
        {
            rec = this.GetValue(iter, 0) as BibtexRecord;

            if (rec.GetHashCode() == record.GetHashCode())
            {
                return iter;
            }

            while (this.IterNext(ref iter))
            {
                rec = this.GetValue(iter, 0) as BibtexRecord;

                if (rec.GetHashCode() == record.GetHashCode())
                {
                    return iter;
                }
            }
        }
        return iter;
    }
}
}
