// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Threading;
using System.IO;
using System.Collections;
using Mono.Unix;

namespace bibliographer
{
    public class Cache
    {
        public static void Initialise ()
        {
            LoadCacheData ();
        }

        private static cacheSection LookupSection (string section)
        {
            cacheSection dummySection = new cacheSection (section);
            int index = sections.BinarySearch (dummySection, new sectionCompare ());
            if ((index < 0) || (index >= sections.Count)) {
                return null;
            }
            return ((cacheSection)sections[index]);
        }

        private static keySection LookupKey (string section, string key)
        {
            cacheSection cSection = LookupSection (section);
            if (cSection == null)
                return null;
            keySection dummyKey = new keySection (key, "");
            int index = cSection.keys.BinarySearch (dummyKey, new keyCompare ());
            if ((index < 0) || (index >= cSection.keys.Count)) {
                return null;
            }
            return ((keySection)cSection.keys[index]);
        }

        public static bool IsCached (string section, string key)
        {
            if (LookupKey (section, key) != null)
                return true;
            else
                return false;
        }

        public static string CachedFile (string section, string key)
        {
            keySection kSection = LookupKey (section, key);
            if (kSection == null)
                return "";
            else
                return kSection.filename;
        }

        public static string AddToCache (string section, string key)
        {
            keySection kSection = LookupKey (section, key);
            if (kSection != null)
                return kSection.filename;
            cacheSection cSection = LookupSection (section);
            if (cSection == null) {
                // add a new section
                sections.Add (new cacheSection (section));
                sections.Sort (new sectionCompare ());
                cSection = LookupSection (section);
                if (cSection == null) {
                    Debug.WriteLine (5, "Failing to add a new section in cache, that's messed up... :-(");
                    return "";
                }
            }
            string filename;
            Random random = new System.Random ();
            do {
                filename = Config.GetDataDir () + "cache/";
                string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                for (int i = 0; i < 20; i++)
                    filename = filename + chars[random.Next () % chars.Length];
                bool ok = false;
                try {
                    FileStream stream = new FileStream (filename, FileMode.CreateNew, FileAccess.Write);
                    stream.Close ();
                    ok = true;
                } catch (DirectoryNotFoundException e) {
                    Debug.WriteLine (10, e.Message);
                    
                    try {
                        System.IO.Directory.CreateDirectory (Config.GetDataDir ());
                    } catch (Exception e2) {
                        Debug.WriteLine (10, e2.Message);
                    }
                    try {
                        System.IO.Directory.CreateDirectory (Config.GetDataDir () + "cache");
                    } catch (Exception e2) {
                        Debug.WriteLine (10, e2.Message);
                        Debug.WriteLine (1, "Failed to create directory {0}", Config.GetDataDir () + "cache");
                    }
                } catch (IOException e) {
                    Debug.WriteLine (10, e.Message);
                    // file already exists
                }
                if (ok)
                    break;
            } while (true);
            cSection.keys.Add (new keySection (key, filename));
            cSection.keys.Sort (new keyCompare ());
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
            keySection kSection = LookupKey (section, key);
            if (kSection != null) {
                cacheSection cSection = LookupSection (section);
                Mono.Unix.Native.Syscall.unlink (kSection.filename);
                cSection.keys.Remove (kSection);
                if (cSection.keys.Count == 0)
                    sections.Remove (cSection);
                SaveCacheData ();
            }
        }

        // Private data & functions

        private class keySection
        {
            public keySection (string _key, string _filename)
            {
                key = _key;
                filename = _filename;
            }

            public string key;
            public string filename;
        }

        private class keyCompare : IComparer
        {
            public int Compare (object x, object y)
            {
                return string.Compare (((keySection)x).key, ((keySection)y).key);
            }
        }

        private class cacheSection
        {
            public cacheSection (string _section)
            {
                section = _section;
                keys = new ArrayList ();
            }

            public string section;
            public ArrayList keys;
        }

        private class sectionCompare : IComparer
        {
            public int Compare (object x, object y)
            {
                return string.Compare (((cacheSection)x).section, ((cacheSection)y).section);
            }
        }

        static ArrayList sections;

        private static void LoadCacheData ()
        {
            sections = new ArrayList ();
            StreamReader stream = null;
            try {
                stream = new StreamReader (new FileStream (Config.GetDataDir () + "cachedata", FileMode.Open, FileAccess.Read));
                
                // cache opened! let's read some data...
                cacheSection curSection = null;
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
                                if (((cacheSection)sections[i]).section == sectionName) {
                                    found = true;
                                    break;
                                }
                            }
                            if (found) {
                                Debug.WriteLine (5, "Duplicate section {0} in cache file!", sectionName);
                                curSection = null;
                                continue;
                            }
                            
                            if (curSection != null) {
                                curSection.keys.Sort (new keyCompare ());
                                sections.Add (curSection);
                            }
                            curSection = new cacheSection (sectionName);
                        } else {
                            if (curSection == null) {
                                // no active section, so skip
                                continue;
                            }
                            string[] fields = line.Split (' ');
                            curSection.keys.Add (new keySection (fields[0], fields[1]));
                        }
                    }
                }
                stream.Close ();
                if (curSection != null) {
                    sections.Add (curSection);
                }
                sections.Sort (new sectionCompare ());
            } catch (System.IO.DirectoryNotFoundException e) {
                Debug.WriteLine (10, e.Message);
                Debug.WriteLine (1, "Directory ~/.local/share/bibliographer/ not found! Creating it...");
                System.IO.Directory.CreateDirectory (Config.GetDataDir ());
            } catch (System.IO.FileNotFoundException e) {
                Debug.WriteLine (10, e.Message);
                // no cache, no problem-o :-)
            }
        }

        private static void SaveCacheData ()
        {
			cleanup_invalid_dirs();
			
            try {
                Monitor.Enter (sections);
                StreamWriter stream = new StreamWriter (new FileStream (Config.GetDataDir () + "cachedata", FileMode.OpenOrCreate, FileAccess.Write));
                
                for (int section = 0; section < sections.Count; section++) {
                    stream.WriteLine ("[{0}]", ((cacheSection)sections[section]).section);
                    for (int key = 0; key < ((cacheSection)sections[section]).keys.Count; key++) {
                        stream.WriteLine ("{0} {1}", ((keySection)((cacheSection)sections[section]).keys[key]).key, ((keySection)((cacheSection)sections[section]).keys[key]).filename);
                    }
                }
                
                stream.Close ();
                Monitor.Exit (sections);
            } catch (System.IO.DirectoryNotFoundException e) {
                Debug.WriteLine (10, e.Message);
                Debug.WriteLine (1, "Directory ~/.local/share/bibliographer/ not found! Creating it...");
                System.IO.Directory.CreateDirectory (Config.GetDataDir ());
            } catch (Exception e) {
                Debug.WriteLine (1, "Unhandled exception whilst trying to save cache: {0}", e);
            }
        }
		
		private static void cleanup_invalid_dirs()
		{
			try
			{
				if (System.IO.Directory.Exists("~/.bibliographer"))
				{
					Debug.WriteLine(1, "Deleting old ~/.bibliohrapher directory");
					System.IO.Directory.Delete("~/.bibliographer/");
				}
			} catch (System.IO.DirectoryNotFoundException e)
			{
				Debug.WriteLine (1, "Directory not found exception whilst trying to cleanup old directories: {0}", e);
			} catch (System.IO.FileNotFoundException e)
			{
				Debug.WriteLine (1, "File not found exception whilst trying to cleanup old directories: {0}", e);
			} catch (Exception e)
			{
				Debug.WriteLine (1, "Unhandled exception whilst trying to cleanup old directories: {0}", e);
			}
		}
    }
}
