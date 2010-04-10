// Copyright 2005-2007 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using System.IO;

namespace bibliographer
{
    public class BibtexRecords : System.Collections.CollectionBase
    {
        public event System.EventHandler RecordAdded;
        public event System.EventHandler RecordDeleted;
        public event System.EventHandler RecordsModified;
    	public event System.EventHandler RecordModified;
        
        protected virtual void OnRecordAdded(object o, EventArgs e)
        {
    		this.OnRecordsModified(new EventArgs());
            if (RecordAdded != null)
                RecordAdded(o, e);
        }

        protected virtual void OnRecordDeleted(object o, EventArgs e)
        {
    		this.OnRecordsModified(new EventArgs());
            if (RecordDeleted != null)
                RecordDeleted(o, e);
        }
    
        protected virtual void OnRecordsModified(EventArgs e)
        {
            if (RecordsModified != null)
                RecordsModified(this, e);
        }
    
        private void OnRecordModified(object o, EventArgs e)
        {
            Debug.WriteLine(5, "Record Modified");
    		
    		this.OnRecordsModified(new EventArgs());
    		if (RecordModified != null)
    			this.RecordModified(o, e);
    	}
    	
        public BibtexRecord this[int index]
        {
            get { return ((BibtexRecord)(List[index])); }
            set { List[index] = value; }
    		
        }
        
        public int Add(BibtexRecord record)
        {
            //System.Console.WriteLine("Record Added");
            record.RecordModified += OnRecordModified;
    
            int ret = List.Add(record);
            this.OnRecordAdded(record, new EventArgs());
            //this.OnRecordsModified(new EventArgs());
            return ret;
        }
    
        public void Insert(int index, BibtexRecord record)
        {
            List.Insert(index, record);
            this.OnRecordAdded(record, new EventArgs());
            //this.OnRecordsModified(new EventArgs());
        }
    
        public void Remove(BibtexRecord record)
        {
            List.Remove(record);
            this.OnRecordDeleted(record, new EventArgs());
            //this.OnRecordsModified(new EventArgs());
        }
    
        public bool Contains(BibtexRecord record)
        {
            return List.Contains(record);
        }
    
        // getAuthors method is to get a list of all of the authors for the side bar
        public StringArrayList GetAuthors()
        {
            StringArrayList authorList = new StringArrayList();
            for (int i = 0; i < this.Count; i++)
            {
                StringArrayList recordAuthors = ((BibtexRecord) List[i]).GetAuthors();
                // TODO: Parse recordAuthors array list and add to authorList if it is not currently in the list
                for (int j = 0; j < recordAuthors.Count; j++)
                {
                    if (!authorList.Contains(recordAuthors[j]))
                    {
                        authorList.Add(recordAuthors[j]);
                    }
                }
            }
            authorList.Sort();
            return authorList;
        }
    
        public StringArrayList GetYears()
        {
            StringArrayList years = new StringArrayList();
            for (int i = 0; i < this.Count; i++)
            {
                string year = ((BibtexRecord) List[i]).GetYear();
                // TODO: Parse recordAuthors array list and add to authorList if it is not currently in the list
                if (!years.Contains(year))
                {
                    years.Add(year);
                }
            }
            years.Sort();
            return years;
        }
    
        public StringArrayList GetJournals()
        {
            StringArrayList journals = new StringArrayList();
            for (int i = 0; i < this.Count; i++)
            {
                string journal = ((BibtexRecord) List[i]).GetJournal();
                // TODO: Parse recordAuthors array list and add to authorList if it is not currently in the list
                if (!journals.Contains(journal))
                {
                    journals.Add(journal);
                }
            }
            journals.Sort();
            return journals;
        }
    
        public string ToBibtexString()
        {
            string output = "";
    
            for (int i = 0; i < this.Count; i++)
            {
                output = output + ((BibtexRecord) this.List[i]).ToBibtexString();
            }
            return output;
        }
    
        public void Save(string filename)
        {
            // Save to file
            StreamWriter output = new StreamWriter(filename);
    
            /*IEnumerator iter = this.GetEnumerator();
               while(iter.MoveNext())
               {
                output.Write(((BibtexRecord)iter.Current).ToBibtexString());
               }*/
            output.Write(this.ToBibtexString());
            output.Close();
        }
    
        public static BibtexRecords Open(string filename)
        {
            StreamReader stream = new StreamReader(filename);
            //TODO: Check for other filetypes, and invoke other parsers (eg. endnote)
            BibtexRecords bibtexRecords = ParseBibtex(stream);
            stream.Close();
    
            return bibtexRecords;
        }
    
        private static BibtexRecords ParseBibtex(StreamReader stream)
        {
            BibtexRecords bibtexRecords = new BibtexRecords();
    
            int count = 1;
            while (stream.Peek() != -1) {
                BibtexRecord record;
                try {
                    record = new BibtexRecord(stream);
                } catch (ParseException e) {
                    if (e.GetReason() != "EOF")
                        Debug.WriteLine(1, String.Format("Error while parsing record {0:000} in file!\nError was: {1:000}\n", count, e.GetReason()));
						Debug.WriteLine(1, e.StackTrace);
                    break;
                }
                bibtexRecords.Add(record);
                count++;
            }
            return bibtexRecords;
        }
    
        public bool HasURI(string uri)
        {
            for (int i = 0; i < this.Count; i++)
            {
                BibtexRecord record = (BibtexRecord) List[i];
                if (record.HasURI(uri))
                    return true;
            }
            return false;
        }
    
    }
}
