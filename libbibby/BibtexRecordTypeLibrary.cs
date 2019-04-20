//
//  BibtexRecordTypeLibrary.cs
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

using System.Collections;
using System.IO;
using System;

namespace libbibby
{
    public static class BibtexRecordTypeLibrary
    {
        static ArrayList records;

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
            return index == -1 ? null : (BibtexRecordType)records [index];
        }

        public static BibtexRecordType GetWithIndex (int index)
        {
            if (index < 0 || index >= records.Count) {
                return null;
            }
            return (BibtexRecordType)records[index];
        }

        public static void Add (BibtexRecordType record)
        {
            records.Add (record);
        }

        public static string Filename = Environment.GetEnvironmentVariable ("BIBTEX_TYPE_LIB");

        public static void Save ()
        {
            // TODO: possibly make this safer (in case of crash during write?)
            
            StreamWriter stream;
            stream = null;
            try {
                stream = new StreamWriter (Filename);
                if (stream == null) {
                    Debug.WriteLine (1, "Argh, couldn't open the file!");
                }
            } catch (DirectoryNotFoundException e) {
                Debug.WriteLine (10, e.Message);
                Debug.WriteLine (1, "Directory {0} not found!", Path.GetDirectoryName(Filename));
                Directory.CreateDirectory (Path.GetDirectoryName(Filename));
            }
            // catch some other exception if file can't be opened due to permission problems or something
            if (stream != null) {
                // good to go
                for (int i = 0; i < records.Count; i++) {
                    var record = (BibtexRecordType)records[i];
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
            while (true) {
                try {
                    stream = new StreamReader (Filename);
                    if (stream == null) {
                        Debug.WriteLine (1, "Argh, couldn't open the file!");
                    }
                    break;
                }
                catch (DirectoryNotFoundException e) {
                    Debug.WriteLine (10, e.Message);
                    Debug.WriteLine (1, "Directory {0} not found! Creating it...", Path.GetDirectoryName (Filename));
                    Directory.CreateDirectory (Path.GetDirectoryName (Filename));
                }
                catch (FileNotFoundException e) {
                    Debug.WriteLine (10, e.Message);
                    Debug.WriteLine (1, "File {0} not found! Instantiating it...", Filename);
                    Stream recStream = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibtex_records");
                    var outRecStream = new FileStream (Filename, FileMode.CreateNew);
                    var data = new byte[recStream.Length];
                    recStream.Read (data, 0, (int)recStream.Length);
                    recStream.Close ();
                    outRecStream.Write (data, 0, data.Length);
                    outRecStream.Close ();
                }
            }
            
            if (stream != null) {
                while (true) {
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
                    var record = new BibtexRecordType ();
                    record.name = recordName;
                    record.description = description;
                    record.spec = (Convert.ToInt32 (spec) == 1);
                    var sarray = new StringArrayList ();
                    for (int i = 0; i < fields.Length; i++)
                        sarray.Add (fields [i]);
                    record.fields = sarray;
                    var iarray = new IntArrayList ();
                    for (int i = 0; i < required.Length; i++)
                        iarray.Add (Convert.ToInt32 (required [i]));
                    record.optional = iarray;
                    records.Add (record);
                    Debug.WriteLine (5, "Read in info for record '" + recordName + "'");
                }
                stream.Close ();
            }
        }
    }
}
