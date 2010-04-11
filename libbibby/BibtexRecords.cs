// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using System.IO;

namespace libbibby
{
    public class BibtexRecords : System.Collections.CollectionBase
    {
        public event System.EventHandler RecordAdded;
        public event System.EventHandler RecordDeleted;
        public event System.EventHandler RecordsModified;
        public event System.EventHandler RecordModified;
        public event System.EventHandler RecordURIModified;

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

        protected virtual void OnRecordModified (EventArgs e)
        {
            if (RecordModified != null)
                RecordModified (this, e);
        }

        protected virtual void OnRecordURIModified (EventArgs e)
        {
            if (RecordURIModified != null)
                RecordURIModified (this, e);
        }

        public BibtexRecord this[int index] {
            get { return ((BibtexRecord)(List[index])); }
            set { List[index] = value; }
        }

        public int Add (BibtexRecord record)
        {
            int ret = List.Add (record);
            
            //System.Console.WriteLine ("RecordAdded event emitted: Add {0}", record.GetKey ());
            this.OnRecordAdded (record, new EventArgs ());
            
            //System.Console.WriteLine ("RecordsModified event emitted: Add {0}", record.GetKey ());
            this.OnRecordsModified (new EventArgs ());
            return ret;
        }

        public void Insert (int index, BibtexRecord record)
        {
            List.Insert (index, record);
            //System.Console.WriteLine ("RecordAdded event emitted: Insert");
            this.OnRecordAdded (record, new EventArgs ());
            
            //System.Console.WriteLine ("RecordsModified event emitted: Insert");
            this.OnRecordsModified (new EventArgs ());
        }

        public void Remove (BibtexRecord record)
        {
            List.Remove (record);
            //System.Console.WriteLine ("RecordsDeleted event emitted: Remove");
            this.OnRecordDeleted (record, new EventArgs ());
            
            //System.Console.WriteLine ("RecordsModified event emitted: Remove");
            this.OnRecordsModified (new EventArgs ());
        }

        public bool Contains (BibtexRecord record)
        {
            return List.Contains (record);
        }

        // getAuthors method is to get a list of all of the authors for the side bar
        public StringArrayList GetAuthors ()
        {
            StringArrayList authorList = new StringArrayList ();
            for (int i = 0; i < this.Count; i++) {
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
            for (int i = 0; i < this.Count; i++) {
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
            for (int i = 0; i < this.Count; i++) {
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
            
            for (int i = 0; i < this.Count; i++) {
                output = output + ((BibtexRecord)this.List[i]).ToBibtexString ();
            }
            return output;
        }

        public void Save (string filename)
        {
            // Save to file
            StreamWriter output = new StreamWriter (filename);

//            IEnumerator iter = this.GetEnumerator();
//            while(iter.MoveNext())
//            {
//                output.Write(((BibtexRecord)iter.Current).ToBibtexString());
//            }

            output.Write (this.ToBibtexString ());
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
            for (int i = 0; i < this.Count; i++) {
                BibtexRecord record = (BibtexRecord)List[i];
                if (record.HasURI (uri))
                    return true;
            }
            return false;
        }
        
    }
}
