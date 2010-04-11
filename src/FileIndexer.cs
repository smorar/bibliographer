// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Gnome;
using Gnome.Vfs;
using libbibby;

namespace bibliographer
{
    public class FileIndexer
    {
        private static StringArrayList GetProcessOutput (String command, String args)
        {
            StringArrayList result = new StringArrayList ();
            
            //    Console.WriteLine("Command: " + command);
            //    Console.WriteLine("args: " + args);
            
            System.Diagnostics.Process proc = new Process ();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = command;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            try {
                proc.Start ();
                
                do {
                    String line = proc.StandardOutput.ReadLine ();
                    if (line != null)
                        result.Add (line);
                    else
                        
                        break;
                } while (true);
                
                if (proc.HasExited) {
                    if (proc.ExitCode == 0)
                        return result;
                    else {
                        Debug.WriteLine (5, "Running of program '{0}' with args '{1}' failed with exit code {2}", command, args, proc.ExitCode);
                        return null;
                    }
                } else {
                    proc.Dispose ();
                    Debug.WriteLine (5, "Read From File process, '{0}' did not exit, so it was killed", command);
                    return result;
                }
            } catch (InvalidOperationException e) {
                Debug.WriteLine (1, "Caught InvalidOperationException");
                Debug.WriteLine (10, e.ToString ());
                return null;
            } catch (FileNotFoundException e) {
                Debug.WriteLine (1, "Cannot Index file. Application '{0}' not found.", command);
                Debug.WriteLine (10, e.ToString ());
                return null;
            // Why is this exception being thrown under linux???
            } catch (System.ComponentModel.Win32Exception e) {
                Debug.WriteLine (1, "Cannot Index file. Application '{0}' not found.", command);
                Debug.WriteLine (10, e.ToString ());
                return null;
            } catch (Exception e) {
                Debug.WriteLine (1, "Caught Unhandled Exception");
                Debug.WriteLine (10, e.ToString ());
                return null;
            }
            
        }

        /*
        private static StringArrayList ReadFromFile(String filename)
        {
            System.IO.StreamReader stream = new System.IO.StreamReader(new System.IO.FileStream(filename, System.IO.FileMode.Open));
            StringArrayList result = new StringArrayList();
            do {
                String line = stream.ReadLine();
                if (line != null)
                    result.Add(line);
                else
                    break;
            } while (true);
            stream.Close();
            return result;
        }
        */

        private static StringArrayList GetTextualExtractor (MimeType mimeType)
        {
            StringArrayList extractor = new StringArrayList ();
            
            if (Config.KeyExists ("textual_extractor") == false) {
                ArrayList def_extractors = new ArrayList ();
                // Set application defaults
                def_extractors.Add ("application/pdf:pdftotext:{0} -");
                def_extractors.Add ("application/msword:antiword:{0}");
                def_extractors.Add ("application/postscript:pstotext:{0}");
                def_extractors.Add ("text/plain:cat:{0}");
                
                Config.Initialise ();
                Config.SetKey ("textual_extractor", def_extractors.ToArray ());
            }
            
            // TODO Move Config class into a utils library
            string[] extractors = (string[])Config.GetKey ("textual_extractor");
            string[] output;
            
            foreach (string entry in extractors) {
                output = entry.Split (':');
                if (output[0] == mimeType.ToString ()) {
                    extractor.Add (output[1]);
                    extractor.Add (output[2]);
                }
            }
            
            Debug.WriteLine (5, "textual extractor determined: " + extractor[0]);
            
            return extractor;
        }

        public static StringArrayList GetTextualData (string URI)
        {
            Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (URI);
            MimeType mimeType = new MimeType (uri);
            
            Debug.WriteLine (5, "Indexing a file of MimeType: " + mimeType.Name);
            
            StringArrayList textualData = null;
            StringArrayList extractor;
            
            extractor = GetTextualExtractor (mimeType);
            
            if (extractor.Count == 2) {
                Debug.WriteLine (5, "Textual extractor is {0}", extractor[0]);
                string extractor_options = "";
                extractor_options = String.Format (extractor[1], '"' + Gnome.Vfs.Uri.GetLocalPathFromUri (URI) + '"');
                Debug.WriteLine (5, "extractor options are {0}", extractor_options);
                textualData = GetProcessOutput (extractor[0], extractor_options);
            }
            
            return textualData;
        }

        public static Tri Index (String URI)
        {
            //System.Console.WriteLine("Obtaining index from URI: " + URI);
            Tri index = new Tri ();
            
            StringArrayList textualData = GetTextualData (URI);
            
            if (textualData != null) {
                //System.Console.WriteLine("Converted textual data is as follows:\n---\n");
                for (int line = 0; line < textualData.Count; line++) {
                    while (Gtk.Application.EventsPending ())
                        Gtk.Application.RunIteration ();
                    String data = ((String)textualData[line]).ToLower ();
                    data = Regex.Replace (data, "[^\\w\\.@-]", " ");
                    data = Regex.Replace (data, "[\\d]", " ");
                    //System.Console.WriteLine(data);
                    String[] tokens = data.Split (' ');
                    foreach (String token in tokens)
                        index.AddString (token);
                }
                //System.Console.WriteLine("\n---");
            } else
                System.Console.WriteLine ("Got null back for index data :-(");
            Debug.WriteLine (5, "Got null back for index data :-(");
            
            return index;
        }

        public static void Index (BibtexRecord record)
        {
            //System.Console.WriteLine("Indexing " + record.GetKey());
            Tri index;
            string cacheKey;
            
            if (record.HasCustomDataField ("cacheKey") == false) {
                // No cacheKey - so generate it and index
                
                if (record.HasField ("bibliographer_last_uri") && record.HasField ("bibliographer_last_md5")) {
                    cacheKey = record.GetField ("bibliographer_last_uri") + "<" + record.GetField ("bibliographer_last_md5") + ">";
                    record.SetCustomDataField ("cacheKey", cacheKey);
                    
                    index = Index (record.GetURI ().ToString ());
                    StreamWriter stream = new StreamWriter (new FileStream (Cache.Filename ("index_data", cacheKey), FileMode.OpenOrCreate, FileAccess.Write));
                    stream.WriteLine (index.ToString ());
                    stream.Close ();
                    
                    record.SetCustomDataField ("indexData", index);
                }
            } else {
                // cachekey exists - load
                cacheKey = (string)record.GetCustomDataField ("cacheKey");
                
                // Load cachekey if it hasn't been loaded yet
                if (record.HasCustomDataField ("indexData") == false) {
                    StreamReader istream = new StreamReader (new FileStream (Cache.CachedFile ("index_data", cacheKey), FileMode.Open, FileAccess.Read));
                    index = new Tri (istream.ReadToEnd ());
                    istream.Close ();
                    
                    record.SetCustomDataField ("indexData", index);
                }
            }
            
            
//            if (record.HasCustomDataField("cacheKey"))
//            {
//                cacheKey = (string) record.GetCustomDataField("cacheKey");
//                System.Console.WriteLine("Has cacheKey: " + cacheKey);
//
//                Debug.WriteLine(5, "Indexing \"" + record.GetURI() + "\"");
//                index = FileIndexer.Index(record.GetURI());
//                StreamWriter stream = new StreamWriter(new FileStream(Cache.Filename("index_data", cacheKey), FileMode.OpenOrCreate, FileAccess.Write));
//                stream.WriteLine(index.ToString());
//                stream.Close();
//
//                record.AddCustomDataField("indexData", index);
//            }
//            else
//            {
//                System.Console.WriteLine("No cacheKey");
//                System.Console.WriteLine("bibliographer_last_uri: {0}", record.GetField("bibliographer_last_uri"));
//                System.Console.WriteLine("bibliographer_last_md5: {0}", record.GetField("bibliographer_last_md5"));
//
//                if (record.HasField("bibliographer_last_uri") && record.HasField("bibliographer_last_md5"))
//                {
//                    cacheKey = record.GetField("bibliographer_last_uri") + "<" + record.GetField("bibliographer_last_md5") + ">";
//                    record.AddCustomDataField("cacheKey", cacheKey);
//
//                    if (Cache.IsCached("index_data", cacheKey))
//                    {
//                        try
//                        {
//                            StreamReader istream = new StreamReader(new FileStream(Cache.CachedFile("index_data", cacheKey), FileMode.Open, FileAccess.Read));
//                            index = new Tri(istream.ReadToEnd());
//                            istream.Close();
//
//                            record.AddCustomDataField("indexData", index);
//
//                        }
//                        catch (System.Exception e)
//                        {
//                            Debug.WriteLine(0, "Unknown exception while indexing file {0} for record {1}", record.GetURI(), record.GetKey());
//                            Debug.WriteLine(0, e.Message);
//                            Debug.WriteLine(1, e.StackTrace);
//                        }
//                    }
//                    else
//                    {
//                        index = Index(record.GetURI().ToString());
//
//                        record.AddCustomDataField("indexData", index);
//                   }
//                }
//
//            }
        }

        public bool IndexContains (BibtexRecord record, String s)
        {
            if (s == null || s == "")
                return true;
            if (record.HasCustomDataField ("indexData")) {
                object o = record.GetCustomDataField ("indexData");
                if (o == null) {
                    return false;
                } else {
                    Tri index = (Tri)o;
                    return index.IsSubString (s);
                }
            }
            return false;
        }
        
    }
}
