//
//  LitListStore.cs
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


    public class LitListStore : Gtk.ListStore
    {
        BibtexRecords bibtexRecords;

        public LitListStore (BibtexRecords btRecords)
        {
            var coltype = new GLib.GType[1];
            coltype[0] = (GLib.GType)typeof(BibtexRecord);
            ColumnTypes = coltype;

            if (btRecords == null)
            {
                // TODO: Throw a proper exception here!!
                throw (new Exception());
            }
            //if (btRecords == null)
            //    this.bibtexRecords = new BibtexRecords ();
            //else
            //    this.bibtexRecords = btRecords;
            
            //this.bibtexRecords.RecordAdded += this.OnRecordAdded;
            //this.bibtexRecords.RecordDeleted += this.OnRecordDeleted;
        }

        void OnRecordAdded (object o, EventArgs a)
        {
            Debug.WriteLine (5, "Record added in LitListStore");
            //BibtexRecord record = (BibtexRecord) o;
            Gtk.TreeIter iter = Append ();
            SetValue (iter, 0, o);
        }

        void OnRecordDeleted (object o, EventArgs a)
        {
            Debug.WriteLine (5, "Record deleted in LitListStore");
            var record = (BibtexRecord)o;
            
            Gtk.TreeIter iter = GetIter (record);
            Remove (ref iter);
        }

        public void SetBibtexRecords (BibtexRecords btRecords)
        {
            Clear ();
            
            bibtexRecords = btRecords;
            bibtexRecords.RecordAdded += OnRecordAdded;
            bibtexRecords.RecordDeleted += OnRecordDeleted;
            
            foreach (BibtexRecord record in btRecords) {
                Debug.WriteLine (5, "Inserting record");
                Gtk.TreeIter iter = Append ();
                SetValue (iter, 0, record);
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
            
            if (GetIterFirst (out iter)) {
                rec = GetValue (iter, 0) as BibtexRecord;
                
                if (rec.GetHashCode () == record.GetHashCode ()) {
                    return iter;
                }
                
                while (IterNext (ref iter)) {
                    rec = GetValue (iter, 0) as BibtexRecord;
                    
                    if (rec.GetHashCode () == record.GetHashCode ()) {
                        return iter;
                    }
                }
            }
            return iter;
        }
    }
}
