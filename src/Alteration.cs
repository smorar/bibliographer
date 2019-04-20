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
using System.IO;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;
using libbibby;
using static bibliographer.Debug;
using static System.Threading.Monitor;

namespace bibliographer
{
    public class AlterationMonitor
    {
        public Thread indexerThread, alterationMonitorThread, thumbGenThread, doiQueryThread;
        protected Queue indexerQueue, alterationMonitorQueue, thumbGenQueue, doiQueryQueue;
        protected DateTime lastCheck;

        public AlterationMonitor ()
        {
            alterationMonitorThread = new Thread (new ThreadStart (AlterationMonitorThread));
            alterationMonitorQueue = new Queue ();

            indexerThread = new Thread (new ThreadStart (IndexerThread));
            indexerQueue = new Queue ();

            thumbGenThread = new Thread (new ThreadStart (ThumbGenThread));
            thumbGenQueue = new Queue ();

            doiQueryThread = new Thread (new ThreadStart (DoiQueryThread));
            doiQueryQueue = new Queue ();

        }

        public bool Altered (BibtexRecord record)
        {
            string uriString = record.GetURI ();

            if (record.GetFileSize () == 0 || record.GetFileMTime () == 0 || record.GetFileMD5Sum () == "") {
                lastCheck = DateTime.MinValue;
            }
            if (string.IsNullOrEmpty (uriString)) {
                return false;
            }

            Uri uri;
            TimeSpan checkInterval;

            uri = new Uri (uriString);

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
            if (DateTime.Now.Subtract (checkInterval).CompareTo (lastCheck) < 0) {
                WriteLine (10, "Not enough time has passed to check this record");
                return false;
            }
            if (!uri.IsFile) {
                WriteLine (10, "URI \"" + uriString + "\" does not seem to exist...");
                return false;
            }
            // initiating a check - recording time
            lastCheck = DateTime.Now;

            long size = record.GetFileSize ();
            ulong mtime = record.GetFileMTime ();
            string md5 = record.GetFileMD5Sum ();
            long newSize = 0;

            long intSize;
            GLib.IFile file;
            GLib.FileInfo fileInfo;

            intSize = 0;

            ulong newMtime = 0;
            try {
                WriteLine (10, "URI \"" + uriString + "\" has the following characteristics:");
                if (md5 == null) {
                    md5 = "";
                } else {
                    WriteLine (10, "\t* md5: " + md5);
                }

                WriteLine (10, "\t* Scheme: " + uri.Scheme);

                file = GLib.FileFactory.NewForUri (uri);

                fileInfo = file.QueryInfo ("*", GLib.FileQueryInfoFlags.NofollowSymlinks, null);
                if (fileInfo.Size != 0) {
                    WriteLine (10, "\t* Size: " + fileInfo.Size);
                    newSize = fileInfo.Size;
                    intSize = fileInfo.Size;
                }
                if (fileInfo.GetAttributeULong ("time::changed") != 0) {
                    WriteLine (10, "\t* ctime: " + fileInfo.GetAttributeULong ("time::changed"));
                }

                if (fileInfo.GetAttributeULong ("time::modified") != 0) {
                    WriteLine (10, "\t* mtime: " + fileInfo.GetAttributeULong ("time::modified"));
                    newMtime = fileInfo.GetAttributeULong ("time::modified");
                }
                fileInfo.Dispose ();

                string result;
                ulong data_size;

                data_size = 0;

                result = GLib.ContentType.Guess (uri.ToString (), out byte data, data_size, out bool uncertain);
                if (!string.IsNullOrEmpty (result)) {
                    WriteLine (10, "\t* Mime type: " + result);
                }
            } catch (Exception e) {
                WriteLine (1, e.Message);
                WriteLine (1, "\t*** Whoops! Caught an exception!");
            }
            if ((size != newSize) || (mtime != newMtime) || (md5 == "")) {
                WriteLine (10, "\t* Recalculating MD5...");

                string newMd5;

                using (MD5 md5er = MD5.Create ()) {
                    using (Stream strea = File.OpenRead (uri.LocalPath)) {
                        byte [] hash = md5er.ComputeHash (strea);
                        newMd5 = BitConverter.ToString (hash).Replace ("-", "").ToLowerInvariant ();
                    }
                }

                if (size != newSize || mtime != newMtime || newMd5 != md5) {
                    record.SetFileAttrs (uriString, newSize, newMtime, newMd5);
                }

                if (record.HasCustomDataField ("indexData")) {
                    record.RemoveCustomDataField ("indexData");
                }

                if (record.HasCustomDataField ("largeThumbnail")) {
                    record.RemoveCustomDataField ("largeThumbnail");
                }

                if (record.HasCustomDataField ("smallThumbnail")) {
                    record.RemoveCustomDataField ("smallThumbnail");
                }

                return true;
            }
            // TODO:
            // This is not the correct place for this... Instead, an item with a URI, 
            // but without indexData should only attempt to be re-indexed on opening the file again.

            //if (record.GetCustomDataField ("indexData") == null) {
            // URI, but null index data. Force a re-index by returning true
            //    return true;
            //}
            return false;
        }

        public void FlushQueues ()
        {
            Enter (alterationMonitorQueue);
            alterationMonitorQueue.Clear ();
            Exit (alterationMonitorQueue);

            Enter (indexerQueue);
            indexerQueue.Clear ();
            Exit (indexerQueue);

            Enter (thumbGenQueue);
            thumbGenQueue.Clear ();
            Exit (thumbGenQueue);

            Enter (doiQueryQueue);
            doiQueryQueue.Clear ();
            Exit (doiQueryQueue);
        }

        public void SubscribeAlteredRecords (BibtexRecords records)
        {
            Enter (alterationMonitorQueue);
            foreach (BibtexRecord record in records) {
                if (!alterationMonitorQueue.Contains (records)) {
                    alterationMonitorQueue.Enqueue (record);
                }
            }
            Exit (alterationMonitorQueue);
        }

        public void SubscribeAlteredRecord (BibtexRecord record)
        {
            Enter (alterationMonitorQueue);
            if (!alterationMonitorQueue.Contains (record)) {
                alterationMonitorQueue.Enqueue (record);
            }
            Exit (alterationMonitorQueue);
        }

        public void SubscribeRecordForDOILookup (BibtexRecord record)
        {
            Enter (doiQueryQueue);
            if (!doiQueryQueue.Contains (record)) {
                doiQueryQueue.Enqueue (record);
            }
            Exit (doiQueryQueue);
        }

        public void DoiQueryThread ()
        {
            WriteLine (5, "DoiQuery thread started");
            try {
                do {
                    Enter (doiQueryQueue);
                    while (doiQueryQueue.Count > 0) {
                        BibtexRecord record = (BibtexRecord)doiQueryQueue.Dequeue ();
                        Exit (doiQueryQueue);

                        if (record != null) {
                            try {
                                if (record.HasDOI ()) {
                                    LookupRecordData.LookupDOIData (record);
                                }
                            } catch (Exception e) {
                                WriteLine (1, "Unknown exception caught with doiQuery");
                                WriteLine (1, e.Message);
                                WriteLine (1, e.StackTrace);
                            }
                        }
                        Enter (doiQueryQueue);
                    }
                    Exit (doiQueryQueue);
                    Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                WriteLine (5, "DoiQuery thread terminated");
            }
        }

        public void ThumbGenThread ()
        {
            WriteLine (5, "ThumbGen thread started");
            try {
                do {
                    Enter (thumbGenQueue);
                    while (thumbGenQueue.Count > 0) {

                        BibtexRecord record = (BibtexRecord)thumbGenQueue.Dequeue ();
                        Exit (thumbGenQueue);
                        try {
                            WriteLine (10, "ThumbGenThread: Check if record has a thumbnail - " + record);
                            if (!ThumbGen.getThumbnail (record)) {
                                WriteLine (10, "Thumbnail does not exist");
                            }

                        } catch (Exception e) {
                            WriteLine (1, "Unknown exception caught with thumbGen");
                            WriteLine (1, e.Message);
                            WriteLine (1, e.StackTrace);
                        }
                        Enter (thumbGenQueue);
                    }
                    Exit (thumbGenQueue);
                    Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                WriteLine (5, "ThumbGen thread terminated");
            }
        }

        public void IndexerThread ()
        {
            WriteLine (5, "Indexer thread started");
            try {
                do {
                    Enter (indexerQueue);
                    while (indexerQueue.Count > 0) {

                        BibtexRecord record = (BibtexRecord)indexerQueue.Dequeue ();
                        Exit (indexerQueue);
                        try {
                            WriteLine (10, "IndexerQueue: Indexing record - " + record.GetURI ());
                            FileIndexer.Index (record);
                        } catch (Exception e) {
                            WriteLine (1, "Unknown exception caught with indexer");
                            WriteLine (1, e.Message);
                            WriteLine (1, e.StackTrace);
                        }
                        Enter (indexerQueue);
                    }
                    Exit (indexerQueue);
                    Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                WriteLine (5, "Indexer thread terminated");
            }
        }

        public void AlterationMonitorThread ()
        {
            WriteLine (5, "Alteration monitor thread started");
            try {
                do {
                    Enter (alterationMonitorQueue);
                    while (alterationMonitorQueue.Count > 0) {

                        BibtexRecord record = (BibtexRecord)alterationMonitorQueue.Dequeue ();
                        Exit (alterationMonitorQueue);
                        // FIXME: do the alteration monitoring stuff
                        // FIXME: if continuous monitoring is enabled, then
                        // the entry should be requeued
                        if (Altered (record)) {
                            WriteLine (10, "AlterationThread: processing - " + record.GetURI ());
                            Enter (indexerQueue);
                            if (!indexerQueue.Contains (record)) {
                                indexerQueue.Enqueue (record);
                            }

                            Exit (indexerQueue);
                            Enter (thumbGenQueue);
                            if (!thumbGenQueue.Contains (record)) {
                                thumbGenQueue.Enqueue (record);
                            }

                            Exit (thumbGenQueue);
                        } else if (
                                   record.HasURI () &&
                                   string.IsNullOrEmpty (record.GetKey ()) &&
                                   string.IsNullOrEmpty (record.RecordType) &&
                                   !record.HasCustomDataField ("largeThumbnail") &&
                                   !record.HasCustomDataField ("indexData")
                                   ) {
                            Enter (indexerQueue);
                            if (!indexerQueue.Contains (record)) {
                                indexerQueue.Enqueue (record);
                            }

                            Exit (indexerQueue);
                            Enter (thumbGenQueue);
                            if (!thumbGenQueue.Contains (record)) {
                                thumbGenQueue.Enqueue (record);
                            }

                            Exit (thumbGenQueue);
                        }
                        Enter (alterationMonitorQueue);
                        // enqueue record if it has a uri
                        if (record.HasURI () && !alterationMonitorQueue.Contains (record)) {
                            alterationMonitorQueue.Enqueue (record);
                        }

                        Thread.Sleep (100);
                    }
                    Exit (alterationMonitorQueue);
                    Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                WriteLine (1, "Alteration monitor thread terminated");
            }
        }
    }
}
