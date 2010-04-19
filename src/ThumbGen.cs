// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using Gdk;
using Gtk;
using Gnome;
using Gnome.Vfs;
using libbibby;

namespace bibliographer
{


    public class ThumbGen
    {

        public static Pixbuf Gen (string uriString)
        {
            // No URI, so just exit
            if (!(uriString == null || uriString == "")) {
                // Thumbnail not cached, generate and then cache :)
                Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (uriString);
                Gnome.Vfs.MimeType mimeType = new Gnome.Vfs.MimeType (uri);
                Gnome.ThumbnailFactory thumbFactory = new Gnome.ThumbnailFactory (Gnome.ThumbnailSize.Normal);

                if (thumbFactory.CanThumbnail (uriString, mimeType.Name, System.DateTime.Now)) {
                    //      System.Console.WriteLine("Generating a thumbnail");
                    return thumbFactory.GenerateThumbnail (uriString, mimeType.Name);
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
                        return new Gdk.Pixbuf (iconPath);
                    } else {
                        // just go blank
                        return null;
                    }
                }
            } else {
                return null;
            }
        }

        public static void Gen (BibtexRecord record)
        {
            Pixbuf smallThumbnail, largeThumbnail;
            string cacheKey;

            if ((record.HasCustomDataField ("smallThumbnail") == false) || (record.HasCustomDataField ("largeThumbnail") == false)) {
                // No thumbnail - so generate it and index

                if (record.HasCustomDataField ("bibliographer_last_uri") && record.HasCustomDataField ("bibliographer_last_md5")) {
                    cacheKey = record.GetCustomDataField ("bibliographer_last_uri") + "<" + record.GetCustomDataField ("bibliographer_last_md5") + ">";
                    record.SetCustomDataField ("cacheKey", cacheKey);

                    largeThumbnail = Gen (record.GetURI ().ToString ());
                    smallThumbnail = ((Pixbuf) largeThumbnail.Clone()).ScaleSimple (20, 20, Gdk.InterpType.Bilinear);

                    largeThumbnail.Save (Cache.Filename("small_thumb",cacheKey), "png");
                    smallThumbnail.Save (Cache.Filename("large_thumb",cacheKey), "png");

                    record.SetCustomDataField ("smallThumbnail", smallThumbnail);
                    record.SetCustomDataField ("largeThumbnail", largeThumbnail);
                }
            } else {
                // thumbnails exist - load
                cacheKey = (string)record.GetCustomDataField ("cacheKey");

                // Load cachekey if it hasn't been loaded yet
                if (record.HasCustomDataField ("smallThumbnail") == false) {
                    try {
                        record.SetCustomDataField ("smallThumbnail", new Gdk.Pixbuf (Cache.CachedFile ("small_thumb", cacheKey)));
                    } catch (Exception) {
                        // probably a corrupt cache file
                        // delete it and try again :-)
                        Cache.RemoveFromCache ("small_thumb", cacheKey);
                    }
                }
                if (record.HasCustomDataField ("largeThumbnail") == false) {
                    try {
                        record.SetCustomDataField ("largeThumbnail", new Gdk.Pixbuf (Cache.CachedFile ("large_thumb", cacheKey)));
                    } catch (Exception) {
                        // probably a corrupt cache file
                        // delete it and try again :-)
                        Cache.RemoveFromCache ("large_thumb", cacheKey);
                    }
                }
            }
        }
    }
}
