//
//  Alteration.cs
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
using System.Collections;
using System.Security.Cryptography;
using libbibby;

namespace bibliographer
{
    public class AlterationMonitor
    {
		public Thread indexerThread;
		public Thread alterationMonitorThread;
		public Thread thumbGenThread;
        Queue indexerQueue, alterationMonitorQueue, thumbGenQueue;
        protected DateTime lastCheck;

        public AlterationMonitor ()
        {
            alterationMonitorThread = new Thread (new ThreadStart (AlterationMonitorThread));
            alterationMonitorQueue = new Queue ();

            indexerThread = new Thread (new ThreadStart (IndexerThread));
            indexerQueue = new Queue ();

            thumbGenThread = new Thread (new ThreadStart (ThumbGenThread));
            thumbGenQueue = new Queue ();

        }

        public bool Altered (BibtexRecord record)
        {
            String uriString = record.GetURI ();
            String indexedUriString = (string) record.GetCustomDataField ("bibliographer_last_uri");
            
            if (indexedUriString == null || indexedUriString != uriString || indexedUriString == "") {
                // URI has changed, so make all existing data obsolete
                record.RemoveCustomDataField ("bibliographer_last_size");
                record.RemoveCustomDataField ("bibliographer_last_mtime");
                record.RemoveCustomDataField ("bibliographer_last_md5");
                if (uriString != null) {
                    Debug.WriteLine (5, "Setting bibliographer_last_uri to {0}", uriString);
                    
                    record.SetCustomDataField ("bibliographer_last_uri", uriString);
                }
                lastCheck = DateTime.MinValue;
                // force a re-check
            }
			if (string.IsNullOrEmpty (uriString))
				return false;
            Uri uri;
            TimeSpan checkInterval;

            uri = new Uri(uriString);

            //TODO: specify checking frequencies in settings
            switch (uri.Scheme) {
            case "http":
                // default update interval for HTTP: 30 mins
                checkInterval = new TimeSpan (0, 30, 0);
                break;
            case "file":
                // default update interval for local files: 1 minute
                checkInterval = new TimeSpan (0, 1, 0);
                break;
            default:
                // default update interval for anything else: 5 minutes
                checkInterval = new TimeSpan (0, 5, 0);
                break;
            }
            //checkInterval = new TimeSpan (0, 0, 1);
            if (DateTime.Now.Subtract (checkInterval).CompareTo (lastCheck) < 0)
            {
                // not enough time has passed for us to check this one
                // FIXME: should probably move this out to the alteration
                // monitor queue
                Debug.WriteLine (10, "Not enough time has passed to check this record");
                return false;
            }
            if (!uri.IsFile) {
                Debug.WriteLine (5, "URI \"" + uriString + "\" does not seem to exist...");
                return false;
            }
            // initiating a check - recording time
            lastCheck = DateTime.Now;

            String size = (string)record.GetCustomDataField ("bibliographer_last_size") ?? "";
            String mtime = (string) record.GetCustomDataField ("bibliographer_last_mtime");
			if (mtime == null) {
				mtime = "";
			}
            String md5 = (string) record.GetCustomDataField ("bibliographer_last_md5");
            String newSize = "";

            long intSize;
            GLib.IFile file;
            GLib.FileInfo fileInfo;

            intSize = 0;

            String newMtime = "";
            try {
                Debug.WriteLine (5, "URI \"" + uriString + "\" has the following characteristics:");
                if (md5 == null)
                    md5 = "";
                else
                    Debug.WriteLine (5, "\t* md5: " + md5);
                Debug.WriteLine (5, "\t* Scheme: " + uri.Scheme);

                file = GLib.FileFactory.NewForUri(uri);

                fileInfo = file.QueryInfo ("*", GLib.FileQueryInfoFlags.NofollowSymlinks, null);
                if (fileInfo.Size != 0) {
                    Debug.WriteLine (5, "\t* Size: " + fileInfo.Size);
                    newSize = fileInfo.Size.ToString();
                    intSize = fileInfo.Size;
                }
                if ((fileInfo.GetAttributeULong("time::changed")) != 0)
                    Debug.WriteLine (5, "\t* ctime: " + fileInfo.GetAttributeULong("time::changed"));
                if ((fileInfo.GetAttributeULong("time::modified")) != 0) {
                    Debug.WriteLine (5, "\t* mtime: " + fileInfo.GetAttributeULong("time::modified"));
                    newMtime = fileInfo.GetAttributeULong("time::modified").ToString ();
                }
                bool uncertain;
                string result;
                byte data;
                ulong data_size;

                data_size = 0;

                result = GLib.ContentType.Guess(uri.ToString(), out data, data_size, out uncertain);
                if (result != "" || result != null)
                    Debug.WriteLine (5, "\t* Mime type: " + result);
                
            } catch (Exception e) {
                Debug.WriteLine (10, e.Message);
                Debug.WriteLine (1, "\t*** Whoops! Caught an exception!");
            }
            if ((size != newSize) || (mtime != newMtime) || (md5 == "")) {
                if (size != newSize)
                    record.SetCustomDataField ("bibliographer_last_size", newSize);
                if (mtime != newMtime)
                    record.SetCustomDataField ("bibliographer_last_mtime", newMtime);
                
                // something has changed or we don't have a MD5
                // recalculate the MD5
                Debug.WriteLine (5, "\t* Recalculating MD5...");

                GLib.IFile uriFile;
                GLib.FileInputStream stream;
                uriFile = GLib.FileFactory.NewForUri (uri);
                stream = uriFile.Read (null);

                ulong sizeRead;
                byte[] contents;
                contents = new byte[intSize];
                
                if (!stream.ReadAll (contents, (ulong)intSize, out sizeRead, null)) {
                    Debug.WriteLine (5, "Something weird happened trying to read data for URI \"" + uriString + "\"");
                }

                MD5 hasher = MD5.Create ();
                byte[] newMd5Array = hasher.ComputeHash (contents);
                string newMd5 = BitConverter.ToString(newMd5Array).Replace("-","").ToLower();
                Debug.WriteLine (5, "\t*MD5: " + newMd5);
                if (newMd5 != md5) {
                    // definitely something changed
                    record.SetCustomDataField ("bibliographer_last_md5", newMd5);
                }
                //Console.WriteLine(record.GetKey() + " is altered!! - 2");

                if (record.HasCustomDataField("indexData"))
                    record.RemoveCustomDataField("indexData");
                if (record.HasCustomDataField("largeThumbnail"))
                    record.RemoveCustomDataField("largeThumbnail");
                if (record.HasCustomDataField("smallThumbnail"))
                    record.RemoveCustomDataField("smallThumbnail");
                
                return true;
            }
			// TODO:
			// This is not the correct place for this... Instead, an item with a URI, 
			// but without indexData should only attempt to be re-indexed on opening the file again.
			
            //if (record.GetCustomDataField ("indexData") == null) {
                // URI, but null index data. Force a re-index by returning true
                //System.Console.WriteLine(record.GetKey() + " is altered!! - 1");
            //    return true;
            //}
            return false;
        }

        public void FlushQueues ()
        {
            Monitor.Enter (alterationMonitorQueue);
            alterationMonitorQueue.Clear ();
            Monitor.Exit (alterationMonitorQueue);
            
            Monitor.Enter (indexerQueue);
            indexerQueue.Clear ();
            Monitor.Exit (indexerQueue);

            Monitor.Enter (thumbGenQueue);
            thumbGenQueue.Clear ();
            Monitor.Exit (thumbGenQueue);

        }

        public void SubscribeRecords (BibtexRecords records)
        {
            Monitor.Enter (alterationMonitorQueue);
            foreach (BibtexRecord record in records)
                alterationMonitorQueue.Enqueue (record);
            Monitor.Exit (alterationMonitorQueue);
        }

        public void SubscribeRecord (BibtexRecord record)
        {
            Monitor.Enter (alterationMonitorQueue);
            alterationMonitorQueue.Enqueue (record);
            Monitor.Exit (alterationMonitorQueue);
        }

        public void ThumbGenThread ()
        {
            //System.Console.WriteLine("ThumbGen thread started");
            Debug.WriteLine (5, "ThumbGen thread started");
            try {
                 do{
                    Monitor.Enter (thumbGenQueue);
                    while (thumbGenQueue.Count > 0) {

                        var record = (BibtexRecord)thumbGenQueue.Dequeue ();

                        Monitor.Exit (thumbGenQueue);
                        //System.Console.WriteLine("ThumbGen thread loop processing " + record.GetKey());
                        try {
                            //ThumbGen.Gen(record);
                            if(!ThumbGen.getThumbnail(record))
                            {
                                Console.WriteLine("Thumbnail does not exist");
                            }

                        } catch (Exception e) {
                            Console.WriteLine ("Unknown exception caught with thumbGen");
                            Console.WriteLine (e.Message);
                            Console.WriteLine (e.StackTrace);
                        }
                        Monitor.Enter (thumbGenQueue);
                    }
                    Monitor.Exit (thumbGenQueue);
                    Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                Debug.WriteLine (5, "ThumbGen thread terminated");
            }
        }

        public void IndexerThread ()
        {
            Debug.WriteLine (5, "Indexer thread started");
            try {
                do {
                    Monitor.Enter (indexerQueue);
                    while (indexerQueue.Count > 0) {
                        
                        var record = (BibtexRecord)indexerQueue.Dequeue ();
                        
                        Monitor.Exit (indexerQueue);
                        //System.Console.WriteLine("Indexer thread loop processing " + record.GetKey());
                        try {
                            FileIndexer.Index (record);
                        } catch (Exception e) {
                            Console.WriteLine ("Unknown exception caught with indexer");
                            Console.WriteLine (e.Message);
                            Console.WriteLine (e.StackTrace);
                        }
                        Monitor.Enter (indexerQueue);
                    }
                    Monitor.Exit (indexerQueue);
                    Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                Debug.WriteLine (5, "Indexer thread terminated");
            }
        }

        public void AlterationMonitorThread ()
        {
            Debug.WriteLine (5, "Alteration monitor thread started");
            try {
                do {
                    Monitor.Enter (alterationMonitorQueue);
                    while (alterationMonitorQueue.Count > 0) {
                        
                        var record = (BibtexRecord)alterationMonitorQueue.Dequeue ();
                        
                        Monitor.Exit (alterationMonitorQueue);
                        // FIXME: do the alteration monitoring stuff
                        // FIXME: if continuous monitoring is enabled, then
                        // the entry should be requeued
                        if (Altered (record)) {
                            //Console.WriteLine("Alteration thread loop processing " + record.GetKey());
                            // Enqueue record for re-indexing
                            Monitor.Enter (indexerQueue);
                            indexerQueue.Enqueue (record);
                            Monitor.Exit (indexerQueue);
                            // Enqueue record for regeneration of its thumbnail
                            Monitor.Enter (thumbGenQueue);
                            thumbGenQueue.Enqueue (record);
                            Monitor.Exit (thumbGenQueue);
                        } else if (
                                   (record.HasURI()) &&
                                   ((record.GetKey() == "")   || (record.GetKey() == null))   &&
                                   ((record.RecordType == "") || (record.RecordType == null)) &&
                                   (!record.HasCustomDataField ("largeThumbnail"))      &&
							       (!record.HasCustomDataField ("indexData"))
                                   )
                        {
                            //System.Console.WriteLine("Alteration thread loop processing " + record.GetKey());
                            // Enqueue record for re-indexing
                            Monitor.Enter (indexerQueue);
                            indexerQueue.Enqueue (record);
                            Monitor.Exit (indexerQueue);
                            // Enqueue record for regeneration of its thumbnail
                            Monitor.Enter (thumbGenQueue);
                            thumbGenQueue.Enqueue (record);
                            Monitor.Exit (thumbGenQueue);
                        }
                        Thread.Sleep (100);
                        Monitor.Enter (alterationMonitorQueue);
                        // enqueue record if it has a uri
                        if (record.HasURI())
                            alterationMonitorQueue.Enqueue (record);
                    }
                    Monitor.Exit (alterationMonitorQueue);
                    Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                Debug.WriteLine (5, "Alteration monitor thread terminated");
            }
        }

    }
}
