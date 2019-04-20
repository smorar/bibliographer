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
using static bibliographer.Debug;

namespace bibliographer
{
    public static class FileIndexer
    {
        private static Process proc = new Process ();

        private static StringArrayList GetProcessOutput (string command, string args)
        {
            StringArrayList result = new StringArrayList ();

            proc.StartInfo.FileName = command;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            try {
                proc.Start ();

                while (true) {
                    string line = proc.StandardOutput.ReadLine ();
                    if (line != null) {
                        result.Add (line);
                    } else {
                        break;
                    }
                }

                if (proc.HasExited) {
                    if (proc.ExitCode == 0) {
                        return result;
                    }
                    WriteLine (5, "Running of program '{0}' with args '{1}' failed with exit code {2}", command, args, proc.ExitCode);
                    return null;
                }
                WriteLine (5, "Read From File process, '{0}' did not exit, so it was killed", command);
                return result;
            } catch (InvalidOperationException e) {
                WriteLine (1, "Caught InvalidOperationException");
                WriteLine (1, e.Message);
                WriteLine (1, e.StackTrace);
                return null;
            } catch (FileNotFoundException e) {
                WriteLine (1, "Cannot Index file. Application '{0}' not found.", command);
                WriteLine (1, e.Message);
                WriteLine (1, e.StackTrace);
                return null;
                // Why is this exception being thrown under linux???
            } catch (System.ComponentModel.Win32Exception e) {
                WriteLine (1, "Cannot Index file. Application '{0}' not found.", command);
                WriteLine (1, e.Message);
                WriteLine (1, e.StackTrace);
                return null;
            } catch (Exception e) {
                WriteLine (1, "Caught Unhandled Exception");
                WriteLine (1, e.Message);
                WriteLine (1, e.StackTrace);
                return null;
            }
        }

        private static StringArrayList GetTextualExtractor (object mimeType)
        {
            BibliographerSettings settings;
            string [] extractors;
            StringArrayList extractor;

            extractor = new StringArrayList ();
            settings = new BibliographerSettings ("apps.bibliographer.index");
            extractors = settings.GetStrv ("textual-extractor");

            if (extractors.Length == 0) {
                ArrayList newExtractors;
                newExtractors = new ArrayList ();

                if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                    // Default extractors for Windows systems
                    newExtractors.Add (".pdf:pdftotext:{0} -");
                    newExtractors.Add (".doc:antiword:{0}");
                    newExtractors.Add (".docx:opc_text:{0}");
                } else {
                    // Default extractors for Gnu/Linux systems
                    newExtractors.Add ("application/pdf:pdftotext:{0} -");
                    newExtractors.Add ("application/msword:antiword:{0}");
                    newExtractors.Add ("application/postscript:pstotext:{0}");
                    newExtractors.Add ("text/plain:cat:{0}");
                }

                //TODO: Add default extractors for other systems

                extractors = (string [])newExtractors.ToArray (typeof (string));
                settings.SetStrv ("textual-extractor", extractors);
            }
            string [] output;

            foreach (string entry in extractors) {
                output = entry.Split (':');
                if (output [0] == mimeType.ToString ()) {
                    extractor.Add (output [1]);
                    extractor.Add (output [2]);
                }
            }

            if (extractor.Count > 0) {
                WriteLine (5, "textual extractor determined: " + extractor [0]);
            }

            return extractor;
        }

        public static StringArrayList GetTextualData (string uriString)
        {
            // TODO: Cache result, and load cache if cache exists!!
            StringArrayList textualData = null;
            StringArrayList extractor;
            Uri uri;
            string mimeType;
            ulong data_size;

            data_size = 0;

            uri = new Uri (uriString);
            mimeType = GLib.ContentType.Guess (uri.ToString (), out byte data, data_size, out bool uncertain);
            WriteLine (5, "Indexing a file of MimeType: " + mimeType);

            extractor = GetTextualExtractor (mimeType);

            if (extractor.Count == 2) {
                WriteLine (10, "Textual extractor is {0}", extractor [0]);
                string extractor_options;
                extractor_options = string.Format (extractor [1], '"' + uri.LocalPath + '"');
                WriteLine (10, "extractor options are {0}", extractor_options);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                    Environment.SetEnvironmentVariable ("HOME", AppDomain.CurrentDomain.BaseDirectory);
                }
                WriteLine (10, "Extracting text using: " + extractor [0]);
                textualData = GetProcessOutput (extractor [0], extractor_options);
                WriteLine (5, "Finished extracting text using: " + extractor [0]);
            }

            return textualData;
        }

        public static Tri Index (StringArrayList textualDataArray)
        {
            Tri index = new Tri ();

            if (textualDataArray != null) {
                for (int line = 0; line < textualDataArray.Count; line++) {
                    string data = textualDataArray [line].ToLower ();
                    data = Regex.Replace (data, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", string.Empty);
                    //data = Regex.Replace(data, "[^\\w\\.@-]", String.Empty);
                    data = Regex.Replace (data, "[\\d]", string.Empty);
                    string [] tokens = data.Split (' ');
                    foreach (string token in tokens) {
                        index.AddString (token);
                    }
                }
            } else {
                WriteLine (5, "Got null back for index data :-(");
            }

            return index;
        }

        public static void Index (BibtexRecord record)
        {
            WriteLine (5, "Indexing " + record.GetKey ());
            Tri index;
            string doi;
            StringArrayList textualDataArray;
            textualDataArray = GetTextualData (record.GetURI ());

            index = Index (textualDataArray);
            DatabaseStoreStatic.SetSearchData (record.DbId (), index.ToString ());

            doi = ExtractDOI (textualDataArray);

            record.SetCustomDataField ("indexData", index);
            if (doi != null) {
                WriteLine (10, "Setting DOI: {0}", doi);
                record.SetField (BibtexRecord.BibtexFieldName.DOI, doi);
                WriteLine (10, "Finished setting DOI field");
            } else {
                WriteLine (10, "No DOI record found");
                // Use MD5 as the BibtexKey for this record
                record.SetKey ((string)record.GetCustomDataField ("bibliographer_last_md5"));
            }
        }

        private static string ExtractDOI (StringArrayList stringDataArray)
        {
            WriteLine (5, "Attempting to extract DOI");
            if (stringDataArray != null) {
                for (int line = 0; line < stringDataArray.Count; line++) {
                    string lineData = stringDataArray [line].ToLower ();
                    if (lineData.IndexOf ("doi", StringComparison.CurrentCultureIgnoreCase) >= 0) {
                        int idx1;
                        idx1 = 0;
                        if (lineData.IndexOf ("doi:", StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            idx1 = lineData.IndexOf ("doi:", StringComparison.CurrentCultureIgnoreCase) + 4;
                            if (lineData.IndexOf ("doi:", idx1, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                                idx1 = lineData.IndexOf ("doi:", idx1, StringComparison.CurrentCultureIgnoreCase) + 4;
                            }
                        } else if (lineData.IndexOf ("doi.org/", StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            idx1 = lineData.IndexOf ("doi.org/", StringComparison.CurrentCultureIgnoreCase) + 8;
                        }

                        lineData = lineData.Substring (idx1);
                        lineData = lineData.Trim ();
                        // If there are additional characters on this line, find a space character and chop them off
                        if (lineData.IndexOf (' ') > 0) {
                            int idx2 = lineData.IndexOf (' ');
                            lineData = lineData.Substring (0, lineData.Length - idx2);
                        }
                        if (lineData.IndexOf (',') > 0) {
                            int idx3 = lineData.IndexOf (',');
                            lineData = lineData.Substring (0, lineData.Length - idx3);
                        }
                        if ((lineData.Length > 3) && lineData.Contains ("/")) {
                            WriteLine (10, "Found doi:{0}", lineData);
                            return lineData;
                        }
                    }
                }
            }
            return null;
        }
    }
}
