// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
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
            // TODO: Cache result, and load cache if cache exists!!
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

        public static Tri Index (StringArrayList textualDataArray)
        {
            Tri index = new Tri ();
            
            if (textualDataArray != null) {
                //System.Console.WriteLine("Converted textual data is as follows:\n---\n");
                for (int line = 0; line < textualDataArray.Count; line++) {
                    //while (Gtk.Application.EventsPending ())
                    //    Gtk.Application.RunIteration ();
                    String data = ((String)textualDataArray[line]).ToLower ();
                    data = Regex.Replace (data, "[^\\w\\.@-]", " ");
                    data = Regex.Replace (data, "[\\d]", " ");
                    //System.Console.WriteLine(data);
                    String[] tokens = data.Split (' ');
                    foreach (String token in tokens)
                        index.AddString (token);
                }
                //System.Console.WriteLine("\n---");
            } else
                Debug.WriteLine (5, "Got null back for index data :-(");
            
            return index;
        }

        public static void Index (BibtexRecord record)
        {
            //System.Console.WriteLine("Indexing " + record.GetKey());
            Tri index;
            string cacheKey = "";
            
            if ((record.HasCustomDataField ("cacheKey") == false) ||
                (Cache.CachedFile ("index_data", (string)record.GetCustomDataField ("cacheKey")).Trim() == ""))
            {
                // No cache exists - so generate it and index
                
                if (record.HasCustomDataField ("bibliographer_last_uri") && record.HasCustomDataField ("bibliographer_last_md5")) {
                    cacheKey = record.GetCustomDataField ("bibliographer_last_uri") + "<" + record.GetCustomDataField ("bibliographer_last_md5") + ">";
                    record.SetCustomDataField ("cacheKey", cacheKey);
                }
                else
                {
                    string uriString = record.GetURI();
                    ulong intSize = 0;
                    Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri(uriString);

                    try {
                        Gnome.Vfs.FileInfo info = uri.GetFileInfo ();
                        if ((info.ValidFields & Gnome.Vfs.FileInfoFields.Size) != 0) {
                            intSize = (ulong)info.Size;
                        }
                    } catch (Exception e) {
                        Debug.WriteLine (10, e.Message);
                        Debug.WriteLine (1, "\t*** Whoops! Caught an exception!");
                    }

                    Gnome.Vfs.Handle handle = Gnome.Vfs.Sync.Open (uriString, Gnome.Vfs.OpenMode.Read);
                    ulong sizeRead;
                    byte[] contents = new byte[intSize];
                    if (Gnome.Vfs.Sync.Read (handle, out contents[0], intSize, out sizeRead) != Gnome.Vfs.Result.Ok) {
                        // read failed
                        Debug.WriteLine (5, "Something weird happened trying to read data for URI \"" + uriString + "\"");
                    }
                    MD5 hasher = MD5.Create ();
                    byte[] newMd5Array = hasher.ComputeHash (contents);
                    string newMd5 = BitConverter.ToString(newMd5Array).Replace("-","").ToLower();

                    record.SetCustomDataField ("bibliographer_last_md5", newMd5);
                    record.SetCustomDataField ("bibliographer_last_uri", uriString);
                    cacheKey = record.GetCustomDataField ("bibliographer_last_uri") + "<" + record.GetCustomDataField ("bibliographer_last_md5") + ">";
                }

                StringArrayList textualDataArray = GetTextualData (record.GetURI().ToString());

                index = Index (textualDataArray);
                string doi = ExtractDOI(textualDataArray);


                StreamWriter stream = new StreamWriter (new FileStream (Cache.Filename ("index_data", cacheKey), FileMode.OpenOrCreate, FileAccess.Write));
                stream.WriteLine (index.ToString ());
                stream.Close ();

                record.SetCustomDataField ("indexData", index);
                if (doi != null)
                {
                    //System.Console.WriteLine("Setting DOI: {0}", doi);
                    record.SetField(BibtexRecord.BibtexFieldName.DOI, doi);
                }
            } else {
                // cachekey exists - load
                //System.Console.WriteLine("Loading cached index data for record: {0}", record.GetKey());

                cacheKey = (string)record.GetCustomDataField ("cacheKey");

                // Load cachekey if it hasn't been loaded yet
                if (record.HasCustomDataField ("indexData") == false) {

                    string cacheFile = Cache.CachedFile ("index_data", cacheKey);
                    //System.Console.WriteLine(cacheFile);

                    StreamReader istream = new StreamReader (new FileStream (cacheFile , FileMode.Open, FileAccess.Read));
                    index = new Tri (istream.ReadToEnd ());
                    istream.Close ();

                    record.SetCustomDataField ("indexData", index);
                }
            }
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
                    Tri index = (Tri) o;
                    return index.IsSubString (s);
                }
            }
            return false;
        }

        private static string ExtractDOI(StringArrayList stringDataArray)
        {
            //System.Console.WriteLine ("Attempting to extract DOI");
            if (stringDataArray != null) {
                for (int line = 0; line < stringDataArray.Count; line++) {
                    String lineData = ((String)stringDataArray[line]).ToLower ();
                    if (lineData.IndexOf ("doi:") > 0) {
                        int idx1 = lineData.IndexOf ("doi:");
                        lineData = lineData.Substring (idx1);
                        lineData = lineData.Trim ();
                        // If there are additional characters on this line, find a space character and chop them off
                        if (lineData.IndexOf (' ') > 0) {
                            int idx2 = lineData.IndexOf (' ');
                            lineData = lineData.Substring (0, lineData.Length - idx2);
                        }
                        // Strip out "doi:"
                        lineData = lineData.Remove (0, 4);
                        Debug.WriteLine (5, "Found doi:{0}", lineData);
                        return lineData;
                    }
                }
            }
            return null;
        }
    }
}
