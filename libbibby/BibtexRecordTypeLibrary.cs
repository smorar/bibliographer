// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System.Collections;
using System.IO;
using System;

namespace libbibby
{
    public class BibtexRecordTypeLibrary
    {
        private static ArrayList records;

        public static int Count ()
        {
            return records.Count;
        }

        public static int Index (string name)
        {
            for (int i = 0; i < records.Count; i++)
                if (String.Compare (((BibtexRecordType)records[i]).name, name, true) == 0)
                    return i;
            return -1;
        }

        public static bool Contains (string name)
        {
            return (Index (name) != -1);
        }

        public static BibtexRecordType Get (string name)
        {
            int index = Index (name);
            if (index == -1)
                return null;
            return (BibtexRecordType)records[index];
        }

        public static BibtexRecordType GetWithIndex (int index)
        {
            if (index < 0 || index >= records.Count)
                return null;
            return (BibtexRecordType)records[index];
        }

        public static void Add (BibtexRecordType record)
        {
            records.Add (record);
        }

        public static string Filename = System.Environment.GetEnvironmentVariable ("BIBTEX_TYPE_LIB");

        public static void Save ()
        {
            // TODO: possibly make this safer (in case of crash during write?)
            
            StreamWriter stream = null;
            try {
                stream = new StreamWriter (Filename);
                if (stream == null) {
                    Debug.WriteLine (1, "Argh, couldn't open the file!");
                }
            } catch (System.IO.DirectoryNotFoundException e) {
                Debug.WriteLine (10, e.Message);
                Debug.WriteLine (1, "Directory {0} not found!", System.IO.Path.GetDirectoryName(Filename));
                System.IO.Directory.CreateDirectory (System.IO.Path.GetDirectoryName(Filename));
            }
            // catch some other exception if file can't be opened due to permission problems or something
            if (stream != null) {
                // good to go
                for (int i = 0; i < records.Count; i++) {
                    BibtexRecordType record = (BibtexRecordType)records[i];
                    stream.WriteLine (record.name);
                    stream.WriteLine (record.description);
                    stream.WriteLine (record.spec ? 1 : 0);
                    if (record.fields.Count > 0) {
                        stream.Write (record.fields[0]);
                        for (int j = 1; j < record.fields.Count; j++)
                            stream.Write ("," + record.fields[j]);
                    }
                    stream.WriteLine ();
                    if (record.optional.Count > 0) {
                        stream.Write (record.optional[0]);
                        for (int j = 0; j < record.optional.Count; j++)
                            stream.Write ("," + record.optional[j]);
                    }
                    stream.WriteLine ();
                    stream.WriteLine ();
                }
                stream.Close ();
            }
        }

        public static void Load ()
        {
            records = new ArrayList ();
            
            StreamReader stream = null;
            do {
                try {
                    stream = new StreamReader (Filename);
                    if (stream == null) {
                        Debug.WriteLine (1, "Argh, couldn't open the file!");
                    }
                    break;
                } catch (System.IO.DirectoryNotFoundException e) {
                    Debug.WriteLine (10, e.Message);
                    Debug.WriteLine (1, "Directory {0} not found! Creating it...", System.IO.Path.GetDirectoryName(Filename));
                    System.IO.Directory.CreateDirectory (System.IO.Path.GetDirectoryName(Filename));
                } catch (System.IO.FileNotFoundException e) {
                    Debug.WriteLine (10, e.Message);
                    Debug.WriteLine (1, "File {0} not found! Instantiating it...", Filename);
                    System.IO.Stream recStream = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibtex_records");
                    System.IO.FileStream outRecStream = new FileStream (Filename, FileMode.CreateNew);
                    byte[] data = new byte[recStream.Length];
                    recStream.Read (data, 0, (int)recStream.Length);
                    recStream.Close ();
                    outRecStream.Write (data, 0, data.Length);
                    outRecStream.Close ();
                }
            } while (true);
            
            if (stream != null) {
                do {
                    string recordName = stream.ReadLine ();
                    if (recordName == null) {
                        // end of file
                        break;
                    }
                    string description = stream.ReadLine ();
                    if (description == null)
                        break;
                    string spec = stream.ReadLine ();
                    if (spec == null)
                        break;
                    string temp = stream.ReadLine ();
                    if (temp == null)
                        break;
                    string[] fields = temp.Split (',');
                    temp = stream.ReadLine ();
                    if (temp == null)
                        break;
                    string[] required = temp.Split (',');
                    stream.ReadLine ();
                    // blank line between records
                    BibtexRecordType record = new BibtexRecordType ();
                    record.name = recordName;
                    record.description = description;
                    record.spec = (System.Convert.ToInt32 (spec) == 1);
                    StringArrayList sarray = new StringArrayList ();
                    for (int i = 0; i < fields.Length; i++)
                        sarray.Add (fields[i]);
                    record.fields = sarray;
                    IntArrayList iarray = new IntArrayList ();
                    for (int i = 0; i < required.Length; i++)
                        iarray.Add (System.Convert.ToInt32 (required[i]));
                    record.optional = iarray;
                    
                    records.Add (record);
                    
                    Debug.WriteLine (5, "Read in info for record '" + recordName + "'");
                } while (true);
                stream.Close ();
            }
        }
    }
}
