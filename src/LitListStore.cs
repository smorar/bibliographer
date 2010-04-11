// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using libbibby;

namespace bibliographer
{


    public class LitListStore : Gtk.ListStore
    {
        private BibtexRecords bibtexRecords;

        public LitListStore (BibtexRecords btRecords)
        {
            GLib.GType[] coltype = new GLib.GType[1];
            coltype[0] = (GLib.GType)typeof(BibtexRecord);
            this.ColumnTypes = coltype;
            
            if (btRecords == null)
                this.bibtexRecords = new BibtexRecords ();
            else
                this.bibtexRecords = btRecords;
            
            this.bibtexRecords.RecordAdded += this.OnRecordAdded;
            this.bibtexRecords.RecordDeleted += this.OnRecordDeleted;
        }

        private void OnRecordAdded (object o, EventArgs a)
        {
            Debug.WriteLine (5, "Record added in LitListStore");
            BibtexRecord record = (BibtexRecord)o;
            Gtk.TreeIter iter = this.Append ();
            this.SetValue (iter, 0, o);
        }

        private void OnRecordDeleted (object o, EventArgs a)
        {
            Debug.WriteLine (5, "Record deleted in LitListStore");
            BibtexRecord record = (BibtexRecord)o;
            
            Gtk.TreeIter iter = this.GetIter (record);
            this.Remove (ref iter);
        }

        public void SetBibtexRecords (BibtexRecords btRecords)
        {
            this.Clear ();
            
            this.bibtexRecords = btRecords;
            this.bibtexRecords.RecordAdded += this.OnRecordAdded;
            this.bibtexRecords.RecordDeleted += this.OnRecordDeleted;
            
            foreach (BibtexRecord record in btRecords) {
                Debug.WriteLine (5, "Inserting record");
                Gtk.TreeIter iter = this.Append ();
                this.SetValue (iter, 0, record);
            }
        }

        public BibtexRecords GetBibtexRecords ()
        {
            return bibtexRecords;
        }

        public Gtk.TreeIter GetIter (BibtexRecord record)
        {
            Gtk.TreeIter iter;
            BibtexRecord rec;
            
            if (this.GetIterFirst (out iter)) {
                rec = this.GetValue (iter, 0) as BibtexRecord;
                
                if (rec.GetHashCode () == record.GetHashCode ()) {
                    return iter;
                }
                
                while (this.IterNext (ref iter)) {
                    rec = this.GetValue (iter, 0) as BibtexRecord;
                    
                    if (rec.GetHashCode () == record.GetHashCode ()) {
                        return iter;
                    }
                }
            }
            return iter;
        }
    }
}
