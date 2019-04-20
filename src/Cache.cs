//
//  Cache.cs
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
using System.Collections;
using static bibliographer.Debug;

namespace bibliographer
{
    public static class Cache
    {
        private static ArrayList sections;

        public static void Initialise ()
        {
            LoadCacheData ();
        }

        private static CacheSection LookupSection (string section)
        {
            CacheSection dummySection = new CacheSection (section);
            int index = sections.BinarySearch (dummySection, new SectionCompare ());
            return (index < 0) || (index >= sections.Count) ? null : (CacheSection)sections [index];
        }

        private static KeySection LookupKey (string section, string key)
        {
            CacheSection cSection = LookupSection (section);
            if (cSection == null) {
                return null;
            }

            KeySection dummyKey = new KeySection (key, "");
            int index = cSection.keys.BinarySearch (dummyKey, new KeyCompare ());
            return (index < 0) || (index >= cSection.keys.Count) ? null : (KeySection)cSection.keys[index];
        }

        public static bool IsCached (string section, string key)
        {
            return LookupKey (section, key) != null;
        }

        public static string CachedFile (string section, string key)
        {
            KeySection kSection = LookupKey (section, key);
            return kSection == null ? "" : kSection.filename;
        }

        public static string AddToCache (string section, string key)
        {

            BibliographerSettings settings;
            KeySection kSection;
            CacheSection cSection;
            Random random;
            string datadir, filename;

            settings = new BibliographerSettings ("apps.bibliographer");

            kSection = LookupKey (section, key);
            if (kSection != null) {
                return kSection.filename;
            }

            cSection = LookupSection (section);

            if (cSection == null) {
                // add a new section
                sections.Add (new CacheSection (section));
                sections.Sort (new SectionCompare ());
                cSection = LookupSection (section);
                if (cSection == null) {
                    WriteLine (5, "Failing to add a new section in cache, that's messed up... :-(");
                    return "";
                }
            }

            datadir = settings.GetString ("data-directory");
            random = new Random ();
            do {
                filename = datadir + "/cache/";

                const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                for (int i = 0; i < 20; i++) {
                    filename = filename + chars[random.Next () % chars.Length];
                }

                bool ok = false;
                try {
                    FileStream stream = new FileStream (filename, FileMode.CreateNew, FileAccess.Write);
                    stream.Close ();
                    ok = true;
                } catch (DirectoryNotFoundException e) {
                    WriteLine (10, e.Message);
                    
                    try {
                        Directory.CreateDirectory (datadir);
                    } catch (Exception e2) {
                        WriteLine (10, e2.Message);
                    }
                    try {
                        Directory.CreateDirectory (datadir + "/cache");
                    } catch (Exception e2) {
                        WriteLine (10, e2.Message);
                        WriteLine (1, "Failed to create directory {0}", datadir + "/cache");
                    }
                } catch (IOException e) {
                    WriteLine (10, e.Message);
                    // file already exists
                }
                if (ok) {
                    break;
                }
            } while (true);
            cSection.keys.Add (new KeySection (key, filename));
            cSection.keys.Sort (new KeyCompare ());
            SaveCacheData ();
            return filename;
        }

        // if the key is found in the cache, then return the
        // associated filename. Otherwise add the key and
        // return the new filename.
        public static string Filename (string section, string key)
        {
            // NOTES: AddToCache already does this, so just call
            // that. This function exists simply to make this
            // functionality explicit, in the event that we want
            // AddToCache to not behave like this at some stage
            return AddToCache (section, key);
        }

        public static void RemoveFromCache (string section, string key)
        {
            KeySection kSection = LookupKey (section, key);
            if (kSection != null) {
                CacheSection cSection = LookupSection (section);
                if (File.Exists(kSection.filename))
                {
                    File.Delete(kSection.filename);
                }
                cSection.keys.Remove (kSection);
                if (cSection.keys.Count == 0) {
                    sections.Remove (cSection);
                }

                SaveCacheData ();
            }
        }

        // Private data & functions

        private class KeySection
        {
            public KeySection (string _key, string _filename)
            {
                key = _key;
                filename = _filename;
            }

            public string key;
            public string filename;
        }

        private class KeyCompare : IComparer
        {
            public int Compare (object x, object y)
            {
                return string.Compare (((KeySection)x).key, ((KeySection)y).key);
            }
        }

        private class CacheSection
        {
            public CacheSection (string _section)
            {
                section = _section;
                keys = new ArrayList ();
            }

            public string section;
            public ArrayList keys;
        }

        private class SectionCompare : IComparer
        {
            public int Compare (object x, object y)
            {
                return string.Compare (((CacheSection)x).section, ((CacheSection)y).section);
            }
        }

        private static void LoadCacheData ()
        {
            BibliographerSettings settings;
            CacheSection curSection;
            StreamReader stream;
            string datadir;

            settings = new BibliographerSettings ("apps.bibliographer");
            datadir = settings.GetString ("data-directory");

            sections = new ArrayList ();

            try {
                stream = new StreamReader (new FileStream (datadir + "/cachedata", FileMode.Open, FileAccess.Read));
                
                // cache opened! let's read some data...
                curSection = null;
                while (stream.Peek () > -1) {
                    string line = stream.ReadLine ();
                    if (line != "") {
                        if (line[0] == '[') {
                            // new section
                            char[] splits = { '[', ']' };
                            string sectionName = line.Split (splits)[1];
                            
                            // check that we don't already have this section name
                            bool found = false;
                            for (int i = 0; i < sections.Count; i++) {
                                if (((CacheSection)sections[i]).section == sectionName) {
                                    found = true;
                                    break;
                                }
                            }
                            if (found) {
                                WriteLine (5, "Duplicate section {0} in cache file!", sectionName);
                                curSection = null;
                                continue;
                            }
                            
                            if (curSection != null) {
                                curSection.keys.Sort (new KeyCompare ());
                                sections.Add (curSection);
                            }
                            curSection = new CacheSection (sectionName);
                        } else {
                            if (curSection == null) {
                                // no active section, so skip
                                continue;
                            }
                            string[] fields = line.Split (' ');
                            curSection.keys.Add (new KeySection (fields[0], fields[1]));
                        }
                    }
                }
                stream.Close ();
                if (curSection != null) {
                    sections.Add (curSection);
                }
                sections.Sort (new SectionCompare ());
            } catch (DirectoryNotFoundException e) {
                WriteLine (10, e.Message);
                WriteLine (1, "Directory ~/.local/share/bibliographer/ not found! Creating it...");
                Directory.CreateDirectory (datadir);
            } catch (FileNotFoundException e) {
                WriteLine (10, e.Message);
                // no cache, no problem-o :-)
            }
        }

        private static void SaveCacheData ()
        {
            BibliographerSettings settings;
            string datadir;

            settings = new BibliographerSettings ("apps.bibliographer");
            datadir = settings.GetString ("data-directory");

            Cleanup_invalid_dirs ();
			
            try {
                Monitor.Enter (sections);
                StreamWriter stream = new StreamWriter (new FileStream (datadir + "/cachedata", FileMode.OpenOrCreate, FileAccess.Write));
                
                for (int section = 0; section < sections.Count; section++) {
                    stream.WriteLine ("[{0}]", ((CacheSection)sections[section]).section);
                    for (int key = 0; key < ((CacheSection)sections[section]).keys.Count; key++) {
                        stream.WriteLine ("{0} {1}", ((KeySection)((CacheSection)sections[section]).keys[key]).key, ((KeySection)((CacheSection)sections[section]).keys[key]).filename);
                    }
                }
                
                stream.Close ();
                Monitor.Exit (sections);
            } catch (DirectoryNotFoundException e) {
                WriteLine (10, e.Message);
                WriteLine (1, "Directory ~/.local/share/bibliographer/ not found! Creating it...");
                Directory.CreateDirectory (datadir);
            } catch (Exception e) {
                WriteLine (1, "Unhandled exception whilst trying to save cache: {0}", e);
            }
        }

        private static void Cleanup_invalid_dirs ()
		{
			try
			{
				if (Directory.Exists("~/.bibliographer"))
				{
                    WriteLine (1, "Deleting old ~/.bibliographer directory");
					Directory.Delete("~/.bibliographer/");
				}
			} catch (DirectoryNotFoundException e)
			{
                WriteLine (1, "Directory not found exception whilst trying to cleanup old directories: {0}", e);
			} catch (FileNotFoundException e)
			{
                WriteLine (1, "File not found exception whilst trying to cleanup old directories: {0}", e);
			} catch (Exception e)
			{
                WriteLine (1, "Unhandled exception whilst trying to cleanup old directories: {0}", e);
			}
		}
    }
}
