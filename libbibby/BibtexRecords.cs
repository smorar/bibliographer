//
//  BibtexRecords.cs
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
using System.Threading;
using System.IO;

namespace libbibby
{
    public class BibtexRecords : System.Collections.CollectionBase
    {
        public event EventHandler RecordAdded;
        public event EventHandler RecordDeleted;
        public event EventHandler RecordsModified;
        public event EventHandler RecordModified;
        public event EventHandler RecordURIAdded;
        public event EventHandler RecordURIModified;

        protected virtual void OnRecordAdded (object o, EventArgs e)
        {
            //System.Console.WriteLine("RecordsModified event emitted: OnRecordAdded");
            if (RecordAdded != null)
                RecordAdded (o, e);
        }

        protected virtual void OnRecordDeleted (object o, EventArgs e)
        {
            //System.Console.WriteLine("RecordsModified event emitted: OnRecordDeleted");
            if (RecordDeleted != null)
                RecordDeleted (o, e);
        }

        protected virtual void OnRecordsModified (EventArgs e)
        {
            if (RecordsModified != null)
                RecordsModified (this, e);
        }

        protected virtual void OnRecordModified (object o, EventArgs e)
        {
            if (RecordModified != null)
                RecordModified (o, e);
        }

        protected virtual void OnRecordURIModified (object o, EventArgs e)
        {
            if (RecordURIModified != null)
                RecordURIModified (o, e);
        }

        protected virtual void OnRecordURIAdded (object o, EventArgs e)
        {
            if (RecordURIAdded != null)
                RecordURIAdded (o, e);
        }

        public BibtexRecord this[int index] {
            get { return ((BibtexRecord)(List[index])); }
            set {
                Monitor.Enter (this);
                List[index] = value; 
                Monitor.Exit (this);
                }
        }

        public int Add (BibtexRecord record)
        {
            Monitor.Enter (this);

            record.UriAdded += OnRecordURIAdded;
            record.UriUpdated += OnRecordURIModified;
            record.RecordModified += OnRecordModified;

            int ret = List.Add (record);
            
            //System.Console.WriteLine ("RecordAdded event emitted: Add {0}", record.GetKey ());
            OnRecordAdded (record, new EventArgs ());
            
            //System.Console.WriteLine ("RecordsModified event emitted: Add {0}", record.GetKey ());
            OnRecordsModified (new EventArgs ());

            Monitor.Exit (this);

            return ret;
        }

        public void Insert (int index, BibtexRecord record)
        {
            Monitor.Enter (this);

            record.UriAdded += OnRecordURIAdded;
            record.UriUpdated += OnRecordURIModified;
            record.RecordModified += OnRecordModified;

            List.Insert (index, record);
            //System.Console.WriteLine ("RecordAdded event emitted: Insert");
            OnRecordAdded (record, new EventArgs ());
            
            //System.Console.WriteLine ("RecordsModified event emitted: Insert");
            OnRecordsModified (new EventArgs ());

            Monitor.Exit (this);
        }

        public void Remove (BibtexRecord record)
        {
            Monitor.Enter (this);

            List.Remove (record);
            //System.Console.WriteLine ("RecordsDeleted event emitted: Remove");
            OnRecordDeleted (record, new EventArgs ());
            
            //System.Console.WriteLine ("RecordsModified event emitted: Remove");
            OnRecordsModified (new EventArgs ());

            Monitor.Exit (this);
        }

        public bool Contains (BibtexRecord record)
        {
            return List.Contains (record);
        }

        // getAuthors method is to get a list of all of the authors for the side bar
        public StringArrayList GetAuthors ()
        {
            var authorList = new StringArrayList ();
            for (int i = 0; i < Count; i++) {
                StringArrayList recordAuthors = ((BibtexRecord)List[i]).GetAuthors ();
                // TODO: Parse recordAuthors array list and add to authorList if it is not currently in the list
                for (int j = 0; j < recordAuthors.Count; j++) {
                    if (!authorList.Contains (recordAuthors[j])) {
                        authorList.Add (recordAuthors[j]);
                    }
                }
            }
            authorList.Sort ();
            return authorList;
        }

        public StringArrayList GetYears ()
        {
            var years = new StringArrayList ();
            for (int i = 0; i < Count; i++) {
                string year = ((BibtexRecord)List[i]).GetYear ();
                // TODO: Parse recordAuthors array list and add to authorList if it is not currently in the list
                if (!years.Contains (year)) {
                    years.Add (year);
                }
            }
            years.Sort ();
            return years;
        }

        public StringArrayList GetJournals ()
        {
            var journals = new StringArrayList ();
            for (int i = 0; i < Count; i++) {
                string journal = ((BibtexRecord)List[i]).GetJournal ();
                // TODO: Parse recordAuthors array list and add to authorList if it is not currently in the list
                if (!journals.Contains (journal)) {
                    journals.Add (journal);
                }
            }
            journals.Sort ();
            return journals;
        }

        public string ToBibtexString ()
        {
            string output = "";
            
            for (int i = 0; i < Count; i++) {
                output = output + ((BibtexRecord)List[i]).ToBibtexString ();
            }
            return output;
        }

        public void Save (string filename)
        {
            // Save to file
            var output = new StreamWriter (filename);

//            IEnumerator iter = this.GetEnumerator();
//            while(iter.MoveNext())
//            {
//                output.Write(((BibtexRecord)iter.Current).ToBibtexString());
//            }

            output.Write (ToBibtexString ());
            output.Close ();
        }

        public static BibtexRecords Open (string filename)
        {
            var stream = new StreamReader (filename);
            //TODO: Check for other filetypes, and invoke other parsers (eg. endnote)
            BibtexRecords bibtexRecords = ParseBibtex (stream);
            stream.Close ();

            return bibtexRecords;
        }

        static BibtexRecords ParseBibtex (StreamReader stream)
        {
            var bibtexRecords = new BibtexRecords ();
            
            int count = 1;
            while (stream.Peek () != -1) {
                BibtexRecord record;
                try {
                    record = new BibtexRecord (stream);
                } catch (ParseException e) {
                    if (e.GetReason () != "EOF")
                        Debug.WriteLine (1, String.Format ("Error while parsing record {0:000} in file!\nError was: {1:000}\n", count, e.GetReason ()));
                    break;
                }
                bibtexRecords.Add (record);
                count++;
            }
            return bibtexRecords;
        }

        public bool HasURI (string uri)
        {
            for (int i = 0; i < Count; i++) {
                var record = (BibtexRecord)List[i];
                if (record.HasURI (uri))
                    return true;
            }
            return false;
        }
        
    }
}
