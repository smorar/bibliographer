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
        public System.Threading.Thread indexerThread, alterationMonitorThread;
        private Queue indexerQueue, alterationMonitorQueue;

        public AlterationMonitor ()
        {
            indexerThread = new System.Threading.Thread (new ThreadStart (IndexerThread));
            indexerQueue = new Queue ();
            
            alterationMonitorThread = new System.Threading.Thread (new ThreadStart (AlterationMonitorThread));
            alterationMonitorQueue = new Queue ();
        }

        public bool Altered (BibtexRecord record)
        {
            //System.Console.WriteLine("Checking that record is altered: " + record.GetKey());
            DateTime lastCheck = DateTime.MinValue;
            
            string cacheKey;
            
            String uriString = record.GetURI ();
            String indexedUriString = record.GetField ("bibliographer_last_uri");
            
            if (indexedUriString == null || indexedUriString != uriString || indexedUriString == "") {
                // URI has changed, so make all existing data obsolete
                record.RemoveField ("bibliographer_last_size");
                record.RemoveField ("bibliographer_last_mtime");
                record.RemoveField ("bibliographer_last_md5");
                if (uriString != null) {
                    Debug.WriteLine (5, "Setting bibliographer_last_uri to {0}", uriString);
                    
                    record.SetField ("bibliographer_last_uri", uriString);
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
            if (DateTime.Now.Subtract (checkInterval).CompareTo (lastCheck) < 0)
                // not enough time has passed for us to check this one
                // FIXME: should probably move this out to the alteration
                // monitor queue
                return false;
            lastCheck = DateTime.Now;
            if (!uri.Exists) {
                Debug.WriteLine (5, "URI \"" + uriString + "\" does not seem to exist...");
                return false;
            }
            String size = record.GetField ("bibliographer_last_size");
            if (size == null)
                size = "";
            String mtime = record.GetField ("bibliographer_last_mtime");
            if (mtime == null)
                mtime = "";
            String md5 = record.GetField ("bibliographer_last_md5");
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
                    record.SetField ("bibliographer_last_size", newSize);
                if (mtime != newMtime)
                    record.SetField ("bibliographer_last_mtime", newMtime);
                
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
                String newMd5 = "";
                for (int i = 0; i < newMd5Array.Length; i++) {
                    switch (newMd5Array[i] & 240) {
                    case 0:
                        newMd5 += "0";
                        break;
                    case 1:
                        newMd5 += "1";
                        break;
                    case 2:
                        newMd5 += "2";
                        break;
                    case 3:
                        newMd5 += "3";
                        break;
                    case 4:
                        newMd5 += "4";
                        break;
                    case 5:
                        newMd5 += "5";
                        break;
                    case 6:
                        newMd5 += "6";
                        break;
                    case 7:
                        newMd5 += "7";
                        break;
                    case 8:
                        newMd5 += "8";
                        break;
                    case 9:
                        newMd5 += "9";
                        break;
                    case 10:
                        newMd5 += "a";
                        break;
                    case 11:
                        newMd5 += "b";
                        break;
                    case 12:
                        newMd5 += "c";
                        break;
                    case 13:
                        newMd5 += "d";
                        break;
                    case 14:
                        newMd5 += "e";
                        break;
                    case 15:
                        newMd5 += "f";
                        break;
                    }
                    switch (newMd5Array[i] & 15) {
                    case 0:
                        newMd5 += "0";
                        break;
                    case 1:
                        newMd5 += "1";
                        break;
                    case 2:
                        newMd5 += "2";
                        break;
                    case 3:
                        newMd5 += "3";
                        break;
                    case 4:
                        newMd5 += "4";
                        break;
                    case 5:
                        newMd5 += "5";
                        break;
                    case 6:
                        newMd5 += "6";
                        break;
                    case 7:
                        newMd5 += "7";
                        break;
                    case 8:
                        newMd5 += "8";
                        break;
                    case 9:
                        newMd5 += "9";
                        break;
                    case 10:
                        newMd5 += "a";
                        break;
                    case 11:
                        newMd5 += "b";
                        break;
                    case 12:
                        newMd5 += "c";
                        break;
                    case 13:
                        newMd5 += "d";
                        break;
                    case 14:
                        newMd5 += "e";
                        break;
                    case 15:
                        newMd5 += "f";
                        break;
                    }
                }
                Debug.WriteLine (5, "\t*MD5: " + newMd5);
                if (newMd5 != md5) {
                    // definitely something changed
                    record.SetField ("bibliographer_last_md5", newMd5);
                    cacheKey = uriString + "<" + newMd5 + ">";
                    
                    record.SetCustomDataField ("smallThumbnail", null);
                    string filename;
                    if (Cache.IsCached ("small_thumb", cacheKey)) {
                        filename = Cache.CachedFile ("small_thumb", cacheKey);
                        Debug.WriteLine (5, "Got cached small thumbnail for '{0}' at location '{1}'", cacheKey, filename);
                        record.SetCustomDataField ("smallThumbnail", new Gdk.Pixbuf (filename));
                    } else {
                        record.SetCustomDataField ("smallThumbnail", this.GenSmallThumbnail (record));
                        if (record.GetCustomDataField ("smallThumbnail") != null) {
                            filename = Cache.AddToCache ("small_thumb", cacheKey);
                            Debug.WriteLine (5, "Added new small thumb to cache for key '{0}'", cacheKey);
                            ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).Save (filename, "png");
                        }
                    }
                    if (Cache.IsCached ("large_thumb", cacheKey)) {
                        filename = Cache.CachedFile ("large_thumb", cacheKey);
                        record.SetCustomDataField ("largeThumbnail", new Gdk.Pixbuf (filename));
                    } else {
                        record.SetCustomDataField ("largeThumbnail", this.GenLargeThumbnail (record));
                        if (record.GetCustomDataField ("largeThumbnail") != null) {
                            filename = Cache.AddToCache ("large_thumb", cacheKey);
                            ((Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail")).Save (filename, "png");
                        }
                    }
                }
                //System.Console.WriteLine(record.GetKey() + " is altered!! - 2");
                return true;
            }
            if (record.GetCustomDataField ("indexData") == null) {
                // URI, but null index data. Force a re-index by returning true
                //System.Console.WriteLine(record.GetKey() + " is altered!! - 1");
                return true;
            }
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

        public void IndexerThread ()
        {
            Debug.WriteLine (5, "Indexer thread started");
            try {
                do {
                    System.Threading.Monitor.Enter (indexerQueue);
                    while (indexerQueue.Count > 0) {
                        
                        BibtexRecord record = (BibtexRecord)indexerQueue.Dequeue ();
                        
                        System.Threading.Monitor.Exit (indexerQueue);
                        //System.Console.WriteLine("Indexer thread loop processing " + record.GetKey());
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
                            //System.Console.WriteLine("Alteration thread loop processing " + record.GetKey());
                            System.Threading.Monitor.Enter (indexerQueue);
                            indexerQueue.Enqueue (record);
                            System.Threading.Monitor.Exit (indexerQueue);
                        }
                        
                        System.Threading.Thread.Sleep (100);
                        System.Threading.Monitor.Enter (alterationMonitorQueue);
                        alterationMonitorQueue.Enqueue (record);
                    }
                    System.Threading.Monitor.Exit (alterationMonitorQueue);
                    System.Threading.Thread.Sleep (100);
                } while (true);
            } catch (ThreadAbortException) {
                Debug.WriteLine (5, "Alteration monitor thread terminated");
            }
        }

        public Gdk.Pixbuf GetSmallThumbnail (BibtexRecord record)
        {
            Debug.Write (5, "getSmallThumbnail: ");
            string cacheKey;
            string uriString = record.GetURI ();
            
            if (((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail") == null) && (record.HasURI ())) {
                cacheKey = (string)record.GetCustomDataField ("cacheKey");
                if (Cache.IsCached ("small_thumb", cacheKey)) {
                    try {
                        record.SetCustomDataField ("smallThumbnail", new Gdk.Pixbuf (Cache.CachedFile ("small_thumb", cacheKey)));
                        Debug.WriteLine (5, "Retrieved small thumb for key '{0}'", cacheKey);
                        return (Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail");
                    } catch (Exception) {
                        // probably a corrupt cache file
                        // delete it and try again :-)
                        Cache.RemoveFromCache ("large_thumb", cacheKey);
                        return (Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail");
                    }
                } else {
                    Debug.WriteLine (5, "not cached... let's go!");
                    
                    Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (uriString);
                    if (!uri.Exists) {
                        // file doesn't exist
                        // FIXME: set an error thumbnail or some such
                        System.Console.WriteLine (uriString);
                        Debug.WriteLine (5, "Non-existent URI");
                        record.SetCustomDataField ("smallThumbnail", (new Gdk.Pixbuf (null, "error.png")).ScaleSimple (20, 20, Gdk.InterpType.Bilinear));
                        return (Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail");
                    }
                    record.SetCustomDataField ("smallThumbnail", this.GenSmallThumbnail (record));
                    
                    if ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail") != null) {
                        string filename = Cache.AddToCache ("small_thumb", cacheKey);
                        Debug.WriteLine (5, "Small thumbnail added to cache for key '{0}'", cacheKey);
                        ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).Save (filename, "png");
                    } else
                        Debug.WriteLine (5, "genSmallThumbnail returned null :-(");
                }
            } else if (((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail") == null) && (!record.HasURI ())) {
                Debug.WriteLine (5, "No URI, generating transparent thumbnail");
                // Generate a transparent pixbuf for 
                Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, true, 8, 20, 20);
                pixbuf.Fill (0);
                pixbuf.AddAlpha (true, 0, 0, 0);
                record.SetCustomDataField ("smallThumbnail", pixbuf);
            }
            
            return (Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail");
        }

        private Gdk.Pixbuf GenSmallThumbnail (BibtexRecord record)
        {
            object[] oArr = new object[1] { record };
            
            try {
                this.GetType ().InvokeMember ("DoGenSmallThumbnail", System.Reflection.BindingFlags.InvokeMethod, null, this, oArr);
                return (Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail");
            } catch (Exception e) {
                System.Console.WriteLine ("Exception caught generating thumbnail\n" + e.Message);
                System.Console.WriteLine (e.StackTrace);
            }
            return null;
        }

        public void DoGenSmallThumbnail (BibtexRecord record)
        {
            
            string cacheKey = (string)record.GetCustomDataField ("cacheKey");
            string uriString = record.GetURI ();
            
            //while (Gtk.Application.EventsPending ())
            //    Gtk.Application.RunIteration ();
            
            // No URI, so just exit
            if (!(uriString == null || uriString == "")) {
                // Thumbnail not cached, generate and then cache :)
                Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (uriString);
                Gnome.Vfs.MimeType mimeType = new Gnome.Vfs.MimeType (uri);
                Gnome.ThumbnailFactory thumbFactory = new Gnome.ThumbnailFactory (Gnome.ThumbnailSize.Normal);
                if (thumbFactory.CanThumbnail (uriString, mimeType.Name, System.DateTime.Now)) {
                    //System.Console.WriteLine("Generating a thumbnail");
                    record.SetCustomDataField ("smallThumbnail", thumbFactory.GenerateThumbnail (uriString, mimeType.Name));
                    if ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail") != null) {
                        Debug.WriteLine (5, "Done thumbnail for '{0}'", uriString);
                        record.SetCustomDataField ("smallThumbnail", ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).ScaleSimple (((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).Width * 20 / ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).Height, 20, Gdk.InterpType.Bilinear));
                        string filename;
                        if (Cache.IsCached ("small_thumb", cacheKey))
                            filename = Cache.CachedFile ("small_thumb", cacheKey);
                        else {
                            filename = Cache.AddToCache ("small_thumb", cacheKey);
                            Debug.WriteLine (5, "doGenSmallThumbnail adding thumbnail with key {0} to cache", cacheKey);
                        }
                        ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).Save (filename, "png");
                    }
                }
                if ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail") == null) {
                    // try to get the default icon for the file's mime type
                    Gtk.IconTheme theme = Gtk.IconTheme.Default;
                    Gnome.IconLookupResultFlags result;
                    string iconName = Gnome.Icon.Lookup (theme, null, null, null, new Gnome.Vfs.FileInfo (IntPtr.Zero), mimeType.Name, Gnome.IconLookupFlags.None, out result);
                    Debug.WriteLine (5, "Gnome.Icon.Lookup result: {0}", result);
                    if (iconName == null) {
                        iconName = "gnome-fs-regular";
                    }
                    Debug.WriteLine (5, "IconName is: {0}", iconName);
                    Gtk.IconInfo iconInfo = theme.LookupIcon (iconName, 24, Gtk.IconLookupFlags.UseBuiltin);
                    string iconPath = iconInfo.Filename;
                    if (iconPath != null) {
                        Debug.WriteLine (5, "IconPath: {0}", iconPath);
                        record.SetCustomDataField ("smallThumbnail", new Gdk.Pixbuf (iconPath));
                        if ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail") != null)
                            record.SetCustomDataField ("smallThumbnail", ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).ScaleSimple (((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).Width * 20 / ((Gdk.Pixbuf)record.GetCustomDataField ("smallThumbnail")).Height, 20, Gdk.InterpType.Bilinear));
                    } else {
                        // just go blank
                        record.SetCustomDataField ("smallThumbnail", null);
                    }
                }
            } else {
                record.SetCustomDataField ("smallThumbnail", null);
            }
        }

        public Gdk.Pixbuf GetLargeThumbnail (BibtexRecord record)
        {
            string cacheKey;
            string uriString = record.GetURI ();
            
            if ((record.GetCustomDataField ("largeThumbnail") == null) && (record.HasURI ())) {
                cacheKey = (string)record.GetCustomDataField ("cacheKey");
                
                if (Cache.IsCached ("large_thumb", cacheKey)) {
                    try {
                        record.SetCustomDataField ("largeThumbnail", new Gdk.Pixbuf (Cache.CachedFile ("large_thumb", cacheKey)));
                        Debug.WriteLine (5, "Retrieved large thumb for key '{0}'", cacheKey);
                        return (Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail");
                    } catch (Exception) {
                        // probably a corrupt cache file
                        // delete it and try again :-)
                        Cache.RemoveFromCache ("large_thumb", cacheKey);
                        return (Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail");
                    }
                } else {
                    Debug.WriteLine (5, "not cached... let's go!");
                    
                    Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (uriString);
                    if (!uri.Exists) {
                        // file doesn't exist
                        // FIXME: set an error thumbnail or some such
                        System.Console.WriteLine (uriString);
                        Debug.WriteLine (5, "Non-existent URI");
                        
                        record.SetCustomDataField ("largeThumbnail", (new Gdk.Pixbuf (null, "error.png")).ScaleSimple (96, 128, Gdk.InterpType.Bilinear));
                        return (Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail");
                    }
                    record.SetCustomDataField ("largeThumbnail", this.GenLargeThumbnail (record));
                    
                    if (record.GetCustomDataField ("largeThumbnail") != null) {
                        string filename = Cache.AddToCache ("large_thumb", cacheKey);
                        Debug.WriteLine (5, "Large thumbnail added to cache for key '{0}'", cacheKey);
                        ((Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail")).Save (filename, "png");
                    } else
                        Debug.WriteLine (5, "genLargeThumbnail returned null :-(");
                }
            } else if ((record.GetCustomDataField ("largeThumbnail") == null) && (!record.HasURI ())) {
                Debug.WriteLine (5, "No URI, generating transparent thumbnail");
                // Generate a transparent pixbuf for 
                Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, true, 8, 96, 128);
                pixbuf.Fill (0);
                pixbuf.AddAlpha (true, 0, 0, 0);
                record.SetCustomDataField ("largeThumbnail", pixbuf);
            }
            return (Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail");
        }

        private Gdk.Pixbuf GenLargeThumbnail (BibtexRecord record)
        {
            object[] oArr = new object[1] { record };
            
            Console.WriteLine ("Executing DoGenLargeThumbnail");
            try {
                this.GetType ().InvokeMember ("DoGenLargeThumbnail", System.Reflection.BindingFlags.InvokeMethod, null, this, oArr);
                return (Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail");
            } catch (Exception e) {
                System.Console.WriteLine ("Exception caught generating thumbnail\n" + e.Message);
                System.Console.WriteLine (e.StackTrace);
            }
            return null;
        }

        public void DoGenLargeThumbnail (BibtexRecord record)
        {
            string cacheKey = (string)record.GetCustomDataField ("cacheKey");
            
            string uriString = record.GetURI ();
            System.Console.WriteLine ("DoGenLargeThumbnail for " + uriString);
            
            while (Gtk.Application.EventsPending ())
                Gtk.Application.RunIteration ();
            
            // No URI, so just exit
            if (!(uriString == null || uriString == "")) {
                // Thumbnail not cached, generate and then cache :)
                Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (uriString);
                Gnome.Vfs.MimeType mimeType = new Gnome.Vfs.MimeType (uri);
                Gnome.ThumbnailFactory thumbFactory = new Gnome.ThumbnailFactory (Gnome.ThumbnailSize.Normal);
                
                if (thumbFactory.CanThumbnail (uriString, mimeType.Name, System.DateTime.Now)) {
                    //      System.Console.WriteLine("Generating a thumbnail");
                    record.SetCustomDataField ("largeThumbnail", thumbFactory.GenerateThumbnail (uriString, mimeType.Name));
                    if (record.GetCustomDataField ("largeThumbnail") == null)
                        return;
                    string filename;
                    if (Cache.IsCached ("large_thumb", cacheKey))
                        filename = Cache.CachedFile ("large_thumb", cacheKey);
                    else {
                        filename = Cache.AddToCache ("large_thumb", cacheKey);
                        Debug.WriteLine (5, "Adding large thumbnail to cache for key {0}", cacheKey);
                    }
                    ((Gdk.Pixbuf)record.GetCustomDataField ("largeThumbnail")).Save (filename, "png");
                } else {
                    // try to get the default icon for the file's mime type
                    Gtk.IconTheme theme = Gtk.IconTheme.Default;
                    Gnome.IconLookupResultFlags result;
                    String iconName = Gnome.Icon.Lookup (theme, null, null, null, new Gnome.Vfs.FileInfo (), mimeType.Name, Gnome.IconLookupFlags.None, out result);
                    Debug.WriteLine (5, "Gnome.Icon.Lookup result: {0}", result);
                    if (iconName == null) {
                        iconName = "gnome-fs-regular";
                    }
                    Debug.WriteLine (5, "IconName is: {0}", iconName);
                    Gtk.IconInfo iconInfo = theme.LookupIcon (iconName, 48, Gtk.IconLookupFlags.UseBuiltin);
                    string iconPath = iconInfo.Filename;
                    if (iconPath != null) {
                        Debug.WriteLine (5, "IconPath: {0}", iconPath);
                        record.SetCustomDataField ("largeThumbnail", new Gdk.Pixbuf (iconPath));
                    } else {
                        // just go blank
                        record.SetCustomDataField ("largeThumbnail", null);
                    }
                }
            } else {
                record.SetCustomDataField ("largeThumbnail", null);
            }
        }
    }
}
