// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Gnome.Vfs;
using libbibby;

namespace bibliographer
{
    public class AlterationMonitor
    {
        public System.Threading.Thread indexerThread, alterationMonitorThread, thumbGenThread;
        private Queue indexerQueue, alterationMonitorQueue, thumbGenQueue;

        public AlterationMonitor ()
        {
            alterationMonitorThread = new System.Threading.Thread (new ThreadStart (AlterationMonitorThread));
            alterationMonitorQueue = new Queue ();

            indexerThread = new System.Threading.Thread (new ThreadStart (IndexerThread));
            indexerQueue = new Queue ();

            thumbGenThread = new System.Threading.Thread (new ThreadStart (ThumbGenThread));
            thumbGenQueue = new Queue ();

        }

        public bool Altered (BibtexRecord record)
        {
            //System.Console.WriteLine("Checking that record is altered: " + record.GetKey());
            DateTime lastCheck = DateTime.MinValue;
            
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
            if (uriString == null || uriString == "")
                return false;
            Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (uriString);
            TimeSpan checkInterval;
            switch (uri.Scheme) {
            case "http":
                // default update interval for HTTP: 30 mins
                checkInterval = new TimeSpan (0, 30, 0);
                break;
            case "file":
                // default update interval for local files: 5 minutes
                checkInterval = new TimeSpan (0, 0, 30);
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
                System.Console.WriteLine("Not enough time has passed to check this record");
                return false;
            }
            lastCheck = DateTime.Now;
            if (!uri.Exists) {
                Debug.WriteLine (5, "URI \"" + uriString + "\" does not seem to exist...");
                return false;
            }
            String size = (string) record.GetCustomDataField ("bibliographer_last_size");
            if (size == null)
                size = "";
            String mtime = (string) record.GetCustomDataField ("bibliographer_last_mtime");
            if (mtime == null)
                mtime = "";
            String md5 = (string) record.GetCustomDataField ("bibliographer_last_md5");
            String newSize = "";
            ulong intSize = 0;
            String newMtime = "";
            try {
                Debug.WriteLine (5, "URI \"" + uriString + "\" has the following characteristics:");
                if (md5 == null)
                    md5 = "";
                else
                    Debug.WriteLine (5, "\t* md5: " + md5);
                Debug.WriteLine (5, "\t* Scheme: " + uri.Scheme);
                Gnome.Vfs.FileInfo info = uri.GetFileInfo ();
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.Size) != 0) {
                    Debug.WriteLine (5, "\t* Size: " + info.Size);
                    newSize = info.Size.ToString ();
                    intSize = (ulong)info.Size;
                }
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.Ctime) != 0)
                    Debug.WriteLine (5, "\t* ctime: " + info.Ctime);
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.Mtime) != 0) {
                    Debug.WriteLine (5, "\t* mtime: " + info.Mtime);
                    newMtime = info.Mtime.ToString ();
                }
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.MimeType) != 0)
                    Debug.WriteLine (5, "\t* Mime type: " + info.MimeType);
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
                Gnome.Vfs.Handle handle = Gnome.Vfs.Sync.Open (uri, Gnome.Vfs.OpenMode.Read);
                ulong sizeRead;
                byte[] contents = new byte[intSize];
                if (Gnome.Vfs.Sync.Read (handle, out contents[0], intSize, out sizeRead) != Gnome.Vfs.Result.Ok) {
                    // read failed
                    Debug.WriteLine (5, "Something weird happened trying to read data for URI \"" + uriString + "\"");
                    return false;
                }

                MD5 hasher = MD5.Create ();
                byte[] newMd5Array = hasher.ComputeHash (contents);
                string newMd5 = BitConverter.ToString(newMd5Array).Replace("-","").ToLower();
                Debug.WriteLine (5, "\t*MD5: " + newMd5);
                if (newMd5 != md5) {
                    // definitely something changed
                    record.SetCustomDataField ("bibliographer_last_md5", newMd5);
                    //cacheKey = uriString + "<" + newMd5 + ">";
                }
                //System.Console.WriteLine(record.GetKey() + " is altered!! - 2");

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
            System.Threading.Monitor.Enter (alterationMonitorQueue);
            alterationMonitorQueue.Clear ();
            System.Threading.Monitor.Exit (alterationMonitorQueue);
            
            System.Threading.Monitor.Enter (indexerQueue);
            indexerQueue.Clear ();
            System.Threading.Monitor.Exit (indexerQueue);

            System.Threading.Monitor.Enter (thumbGenQueue);
            thumbGenQueue.Clear ();
            System.Threading.Monitor.Exit (thumbGenQueue);

        }

        public void SubscribeRecords (BibtexRecords records)
        {
            System.Threading.Monitor.Enter (alterationMonitorQueue);
            foreach (BibtexRecord record in records)
                alterationMonitorQueue.Enqueue (record);
            System.Threading.Monitor.Exit (alterationMonitorQueue);
        }

        public void SubscribeRecord (BibtexRecord record)
        {
            System.Threading.Monitor.Enter (alterationMonitorQueue);
            alterationMonitorQueue.Enqueue (record);
            System.Threading.Monitor.Exit (alterationMonitorQueue);
        }

        public void ThumbGenThread ()
        {
            //System.Console.WriteLine("ThumbGen thread started");
            Debug.WriteLine (5, "ThumbGen thread started");
            try {
                 do{
                    System.Threading.Monitor.Enter (thumbGenQueue);
                    while (thumbGenQueue.Count > 0) {

                        BibtexRecord record = (BibtexRecord)thumbGenQueue.Dequeue ();

                        System.Threading.Monitor.Exit (thumbGenQueue);
                        System.Console.WriteLine("ThumbGen thread loop processing " + record.GetKey());
                        try {
                            ThumbGen.Gen(record);
                        } catch (Exception e) {
                            System.Console.WriteLine ("Unknown exception caught with thumbGen");
                            System.Console.WriteLine (e.Message);
                            System.Console.WriteLine (e.StackTrace);
                        }
                        System.Threading.Monitor.Enter (thumbGenQueue);
                    }
                    System.Threading.Monitor.Exit (thumbGenQueue);
                    System.Threading.Thread.Sleep (100);
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
                    System.Threading.Monitor.Enter (indexerQueue);
                    while (indexerQueue.Count > 0) {
                        
                        BibtexRecord record = (BibtexRecord)indexerQueue.Dequeue ();
                        
                        System.Threading.Monitor.Exit (indexerQueue);
                        System.Console.WriteLine("Indexer thread loop processing " + record.GetKey());
                        try {
                            FileIndexer.Index (record);
                        } catch (Exception e) {
                            System.Console.WriteLine ("Unknown exception caught with indexer");
                            System.Console.WriteLine (e.Message);
                            System.Console.WriteLine (e.StackTrace);
                        }
                        System.Threading.Monitor.Enter (indexerQueue);
                    }
                    System.Threading.Monitor.Exit (indexerQueue);
                    System.Threading.Thread.Sleep (100);
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
                    System.Threading.Monitor.Enter (alterationMonitorQueue);
                    while (alterationMonitorQueue.Count > 0) {
                        
                        BibtexRecord record = (BibtexRecord)alterationMonitorQueue.Dequeue ();
                        
                        System.Threading.Monitor.Exit (alterationMonitorQueue);
                        // FIXME: do the alteration monitoring stuff
                        // FIXME: if continuous monitoring is enabled, then
                        // the entry should be requeued
                        if (Altered (record)) {
                            System.Console.WriteLine("Alteration thread loop processing " + record.GetKey());
                            System.Threading.Monitor.Enter (indexerQueue);
                            // Enqueue record for re-indexing
                            indexerQueue.Enqueue (record);
                            // Enqueue record for regeneration of its thumbnail
                            thumbGenQueue.Enqueue (record);
                            System.Threading.Monitor.Exit (indexerQueue);
                        }
                        
                        System.Threading.Thread.Sleep (100);
                        System.Threading.Monitor.Enter (alterationMonitorQueue);
                        // enqueue record if it has a uri
                        if (record.HasURI())
                            alterationMonitorQueue.Enqueue (record);
                    }
                    System.Threading.Monitor.Exit (alterationMonitorQueue);
                    System.Threading.Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                Debug.WriteLine (5, "Alteration monitor thread terminated");
            }
        }

    }
}
