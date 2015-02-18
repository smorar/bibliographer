//
//  BibtexRecordFieldTypeLibrary.cs
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

namespace libbibby
{
    public static class BibtexRecordFieldTypeLibrary
    {
        private static ArrayList fields;

        public static int Count ()
        {
            return fields.Count;
        }

        public static bool Contains (string name)
        {
            for (int i = 0; i < fields.Count; i++)
                if (((BibtexRecordFieldType)fields[i]).name == name)
                    return true;
            return false;
        }

        public static BibtexRecordFieldType Get (string name)
        {
            for (int i = 0; i < fields.Count; i++)
                if (((BibtexRecordFieldType)fields[i]).name == name)
                    return (BibtexRecordFieldType)fields[i];
            return null;
        }

        public static BibtexRecordFieldType GetWithIndex (int index)
        {
            if (index < 0 || index >= fields.Count)
                return null;
            return (BibtexRecordFieldType)fields[index];
        }

        public static void Add (BibtexRecordFieldType field)
        {
            fields.Add (field);
        }

        public static string Filename = System.Environment.GetEnvironmentVariable ("BIBTEX_FIELDTYPE_LIB");

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
            
            if (stream != null) {
                // good to go
                for (int i = 0; i < fields.Count; i++) {
                    BibtexRecordFieldType field = (BibtexRecordFieldType)fields[i];
                    stream.WriteLine (field.name);
                    stream.WriteLine (field.description);
                    stream.WriteLine (field.spec ? 1 : 0);
                    stream.WriteLine ();
                }
                stream.Close ();
            }
        }

        public static void Load ()
        {
            fields = new ArrayList ();
            
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
                    System.IO.Stream recStream = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibtex_fields");
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
                    string fieldName = stream.ReadLine ();
                    if (fieldName == null)
                        break;
                    string description = stream.ReadLine ();
                    if (description == null)
                        break;
                    string spec = stream.ReadLine ();
                    stream.ReadLine ();
                    // blank line between records
                    BibtexRecordFieldType field = new BibtexRecordFieldType ();
                    field.name = fieldName;
                    field.description = description;
                    field.spec = (System.Convert.ToInt32 (spec) == 1);
                    
                    fields.Add (field);
                    
                    Debug.WriteLine (5, "Read in info for field '" + fieldName + "'");
                } while (true);
                stream.Close ();
            }
        }
    }
}
