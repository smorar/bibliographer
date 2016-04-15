//
//  FileIndexer.cs
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
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using libbibby;

namespace bibliographer
{
    public class FileIndexer
    {
        static StringArrayList GetProcessOutput (String command, String args)
        {
            var result = new StringArrayList ();
            
            var proc = new Process ();
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

        static StringArrayList GetTextualExtractor (object mimeType)
        {
            BibliographerSettings settings;
            string[] extractors;
            StringArrayList extractor;

            extractor = new StringArrayList ();
            settings = new BibliographerSettings ("apps.bibliographer.index");
            extractors = settings.GetStrv ("textual-extractor");

            if (extractors.Length == 0) {
                ArrayList newExtractors;
                newExtractors = new ArrayList();

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Default extractors for Windows systems
                    newExtractors.Add(".pdf:pdftotext:{0} -");
                    newExtractors.Add(".doc:antiword:{0}");
                    newExtractors.Add(".docx:opc_text:{0}");
                }
                else
                {
                    // Default extractors for Gnu/Linux systems
                    newExtractors.Add("application/pdf:pdftotext:{0} -");
                    newExtractors.Add("application/msword:antiword:{0}");
                    newExtractors.Add("application/postscript:pstotext:{0}");
                    newExtractors.Add("text/plain:cat:{0}");
                }

                //TODO: Add default extractors for other systems

                extractors = (string[])newExtractors.ToArray (typeof(string));
                settings.SetStrv ("textual-extractor", extractors);
            } else {
            }

            string[] output;
            
            foreach (string entry in extractors) {
                output = entry.Split (':');
                if (output[0] == mimeType.ToString ()) {
                    extractor.Add (output[1]);
                    extractor.Add (output[2]);
                }
            }
            
            if (extractor.Count > 0)
            {
                Debug.WriteLine(5, "textual extractor determined: " + extractor[0]);
            }
            
            return extractor;
        }

        public static StringArrayList GetTextualData (string uriString)
        {
            // TODO: Cache result, and load cache if cache exists!!
            Uri uri;
            bool uncertain;
            string mimeType;
            byte data;
            ulong data_size;

            data_size = 0;

            uri = new Uri (uriString);
            mimeType = GLib.ContentType.Guess(uri.ToString(), out data, data_size, out uncertain);

            Debug.WriteLine (5, "Indexing a file of MimeType: " + mimeType);
            
            StringArrayList textualData = null;
            StringArrayList extractor;
            
            extractor = GetTextualExtractor (mimeType);

            if (extractor.Count == 2) {
                Debug.WriteLine (5, "Textual extractor is {0}", extractor[0]);
				string extractor_options;
                extractor_options = String.Format (extractor[1], '"' + uri.LocalPath + '"');
                Debug.WriteLine (5, "extractor options are {0}", extractor_options);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Environment.SetEnvironmentVariable("HOME", AppDomain.CurrentDomain.BaseDirectory);
                }
                Debug.WriteLine(5, "Extracting text using: " + extractor[0]);
                textualData = GetProcessOutput (extractor[0], extractor_options);
                Debug.WriteLine(5, "Finished extracting text using: " + extractor[0]);
            }
            
            return textualData;
        }

        public static Tri Index (StringArrayList textualDataArray)
        {
            var index = new Tri ();
            
            if (textualDataArray != null) {
                for (int line = 0; line < textualDataArray.Count; line++) {
                    String data = textualDataArray [line].ToLower ();
                    data = Regex.Replace(data, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", String.Empty);
                    //data = Regex.Replace(data, "[^\\w\\.@-]", String.Empty);
                    data = Regex.Replace (data, "[\\d]", String.Empty);
                    String[] tokens = data.Split (' ');
                    foreach (String token in tokens)
                        index.AddString (token);
                }
            } else
                Debug.WriteLine (5, "Got null back for index data :-(");
            
            return index;
        }

        public static void Index (BibtexRecord record)
        {
            Debug.WriteLine(5, "Indexing " + record.GetKey());
            Tri index;
            string cacheKey, doi;
            Uri uri;
            GLib.IFile file;
            GLib.FileInfo fileInfo;
            ulong sizeRead;
            byte[] contents;
            StreamWriter streamWriter;
            StringArrayList textualDataArray;
            GLib.FileInputStream stream;

            cacheKey = "";

            if ((!record.HasCustomDataField ("cacheKey")) ||
                (Cache.CachedFile ("index_data", (string)record.GetCustomDataField ("cacheKey")).Trim() == ""))
            {
                // No cache exists - so generate it and index
                
                if (record.HasCustomDataField ("bibliographer_last_uri") && record.HasCustomDataField ("bibliographer_last_md5")) {
                    // Information for cacheKey exists - use it!
                    cacheKey = record.GetCustomDataField ("bibliographer_last_uri") + "<" + record.GetCustomDataField ("bibliographer_last_md5") + ">";
                    record.SetCustomDataField ("cacheKey", cacheKey);
                }
                else
                {
                    // Information for cacheKey does not exist - generate it!
                    string uriString = record.GetURI();
                    ulong intSize;

                    uri = new Uri(uriString);

                    try {
                        file = GLib.FileFactory.NewForUri(uri);
                        fileInfo = file.QueryInfo ("*", GLib.FileQueryInfoFlags.NofollowSymlinks, null);
                        intSize = (ulong)fileInfo.Size;
                        stream = file.Read (null);
                        contents = new byte[intSize];
                        if (!stream.ReadAll (contents, intSize, out sizeRead, null)) {
                            Debug.WriteLine (5, "Something weird happened trying to read data for URI \"" + uriString + "\"");
                        }

                        MD5 hasher = MD5.Create ();
                        byte[] newMd5Array = hasher.ComputeHash (contents);
                        string newMd5 = BitConverter.ToString(newMd5Array).Replace("-","").ToLower();

                        record.SetCustomDataField ("bibliographer_last_md5", newMd5);
                        record.SetCustomDataField ("bibliographer_last_uri", uriString);
                        cacheKey = record.GetCustomDataField ("bibliographer_last_uri") + "<" + record.GetCustomDataField ("bibliographer_last_md5") + ">";

                    } catch (Exception e) {
                        Debug.WriteLine (10, e.Message);
                        Debug.WriteLine (1, "\t*** Whoops! Caught an exception!");
                    }

                }

                textualDataArray = GetTextualData (record.GetURI ());

                index = Index (textualDataArray);
                doi = ExtractDOI(textualDataArray);

                streamWriter = new StreamWriter (new FileStream (Cache.Filename ("index_data", cacheKey), FileMode.OpenOrCreate, FileAccess.Write));
                streamWriter.WriteLine (index);
                streamWriter.Close ();

                record.SetCustomDataField ("indexData", index);
                if (doi != null) {
                    Debug.WriteLine(5, "Setting DOI: {0}", doi);
                    record.SetField (BibtexRecord.BibtexFieldName.DOI, doi);
                    Debug.WriteLine (5, "Finished setting DOI field");
                } else {
                    Debug.WriteLine(5, "No DOI record found");
                    // Use MD5 as the BibtexKey for this record
                    record.SetKey ((string)record.GetCustomDataField ("bibliographer_last_md5"));
                }
            } else {
                // cachekey exists - load
                Debug.WriteLine(5, "Loading cached index data for record: {0}", record.GetKey());

                cacheKey = (string)record.GetCustomDataField ("cacheKey");

                // Load cachekey if it hasn't been loaded yet
				if (!record.HasCustomDataField ("indexData")) {

					string cacheFile = Cache.CachedFile ("index_data", cacheKey);

					var istream = new StreamReader (new FileStream (cacheFile, FileMode.Open, FileAccess.Read));
					index = new Tri (istream.ReadToEnd ());
					istream.Close ();

					record.SetCustomDataField ("indexData", index);
				}
            }
        }

        public bool IndexContains (BibtexRecord record, String s)
        {
			if (string.IsNullOrEmpty (s))
				return true;
            if (record.HasCustomDataField ("indexData")) {
                object o = record.GetCustomDataField ("indexData");
                if (o == null) {
                    return false;
                } else {
                    var index = (Tri) o;
                    return index.IsSubString (s);
                }
            }
            return false;
        }

        static string ExtractDOI(StringArrayList stringDataArray)
        {
            Debug.WriteLine(5, "Attempting to extract DOI");
            if (stringDataArray != null) {
                for (int line = 0; line < stringDataArray.Count; line++) {
                    String lineData = stringDataArray [line].ToLower ();
                    if ((lineData.IndexOf ("doi:") >= 0) || (lineData.IndexOf("http://dx.doi.org/") >= 0)) {
                        int idx1;
                        idx1 = 0;
                        if (lineData.IndexOf ("doi:") >= 0)
                            idx1 = lineData.IndexOf ("doi:") + 4;
                        else if (lineData.IndexOf ("http://dx.doi.org/") >= 0)
                            idx1 = lineData.IndexOf ("http://dx.doi.org/") + 18;
                        lineData = lineData.Substring (idx1);
                        lineData = lineData.Trim ();
                        // If there are additional characters on this line, find a space character and chop them off
                        if (lineData.IndexOf (' ') > 0) {
                            int idx2 = lineData.IndexOf (' ');
                            lineData = lineData.Substring (0, lineData.Length - idx2);
                        }
                        Debug.WriteLine (5, "Found doi:{0}", lineData);
                        return lineData;
                    }
                }
            }
            return null;
        }
    }
}
