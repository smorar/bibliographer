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
using System.Collections.Generic;
using System.IO;
using static libbibby.Debug;
using static System.Threading.Monitor;
using static libbibby.DatabaseStoreStatic;

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
            WriteLine (5, "RecordsModified event emitted: OnRecordAdded");
            RecordAdded?.Invoke (o, e);
        }

        protected virtual void OnRecordDeleted (object o, EventArgs e)
        {
            WriteLine (5, "RecordsModified event emitted: OnRecordDeleted");
            RecordDeleted?.Invoke (o, e);
        }

        protected virtual void OnRecordsModified (EventArgs e)
        {
            RecordsModified?.Invoke (this, e);
        }

        protected virtual void OnRecordModified (object o, EventArgs e)
        {
            RecordModified?.Invoke (o, e);
        }

        protected virtual void OnRecordURIModified (object o, EventArgs e)
        {
            RecordURIModified?.Invoke (o, e);
        }

        protected virtual void OnRecordURIAdded (object o, EventArgs e)
        {
            RecordURIAdded?.Invoke (o, e);
        }

        public BibtexRecord this[int index] {
            get => (BibtexRecord)List [index];
            set {
                Enter (this);
                List [index] = value;
                Exit (this);
            }
        }

        public BibtexRecords()
        {
            // Construct the bibtex records object by reading from the sqlite database
            List<int> recordIds = GetRecords ();

            foreach (int recordId in recordIds) {
                Enter (this);

                BibtexRecord record = new BibtexRecord (recordId);
                record.UriAdded += OnRecordURIAdded;
                record.UriUpdated += OnRecordURIModified;
                record.RecordModified += OnRecordModified;

                List.Add (record);
                Exit (this);
            }
        }

        public int Add (BibtexRecord record)
        {
            Enter (this);

            record.UriAdded += OnRecordURIAdded;
            record.UriUpdated += OnRecordURIModified;
            record.RecordModified += OnRecordModified;

            int ret = List.Add (record);
            WriteLine (5, "RecordAdded event emitted: Add {0}", record.GetKey ());
            OnRecordAdded (record, new EventArgs ());
            WriteLine (5, "RecordsModified event emitted: Add {0}", record.GetKey ());
            OnRecordsModified (new EventArgs ());
            Exit (this);

            return ret;
        }

        public void Remove (BibtexRecord record)
        {
            Enter (this);

            List.Remove (record);
            DeleteRecord (record.RecordId);
            WriteLine (5, "RecordsDeleted event emitted: Remove");
            OnRecordDeleted (record, new EventArgs ());
            WriteLine (5, "RecordsModified event emitted: Remove");
            OnRecordsModified (new EventArgs ());
            Exit (this);
        }

        public bool Contains (BibtexRecord record)
        {
            return List.Contains (record);
        }

        // getAuthors method is to get a list of all of the authors for the side bar
        public StringArrayList GetAuthors ()
        {
            StringArrayList authorList = new StringArrayList ();
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
            StringArrayList years = new StringArrayList ();
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
            StringArrayList journals = new StringArrayList ();
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
            StreamWriter output = new StreamWriter (filename);

            output.Write (ToBibtexString ());
            output.Close ();
        }

        public static BibtexRecords Open (string filename)
        {
            StreamReader stream = new StreamReader (filename);
            //TODO: Check for other filetypes, and invoke other parsers (eg. endnote)
            BibtexRecords bibtexRecords = ParseBibtex (stream);
            stream.Close ();

            return bibtexRecords;
        }

        private static BibtexRecords ParseBibtex (StreamReader stream)
        {
            BibtexRecords bibtexRecords = new BibtexRecords ();
            
            int count = 1;
            while (stream.Peek () != -1) {
                BibtexRecord record;
                try {
                    record = new BibtexRecord (stream);
                } catch (ParseException e) {
                    if (e.GetReason () != "EOF") {
                        WriteLine (1, string.Format ("Error while parsing record {0:000} in file!\nError was: {1:000}\n", count, e.GetReason ()));
                    }

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
                BibtexRecord record = (BibtexRecord)List[i];
                if (record.HasURI (uri)) {
                    return true;
                }
            }
            return false;
        }
        
    }
}
