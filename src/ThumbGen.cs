//
//  ThumbGen.cs
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
using Gdk;
using libbibby;

namespace bibliographer
{


    public static class ThumbGen
    {

        public static Pixbuf Gen (string uriString)
        {
            // No URI, so just exit
            if (!(string.IsNullOrEmpty (uriString))) {
                // Thumbnail not cached, generate and then cache :)
                var uri = new Gnome.Vfs.Uri (uriString);
                var mimeType = new Gnome.Vfs.MimeType (uri);
                var thumbFactory = new Gnome.ThumbnailFactory (Gnome.ThumbnailSize.Normal);

                if (thumbFactory.CanThumbnail (uriString, mimeType.Name, DateTime.Now)) {
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
                        return new Pixbuf (iconPath);
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

            if ((!record.HasCustomDataField ("smallThumbnail")) || (!record.HasCustomDataField ("largeThumbnail"))) {
                // No thumbnail - so generate it and index

                if (record.HasCustomDataField ("bibliographer_last_uri") && record.HasCustomDataField ("bibliographer_last_md5")) {
                    cacheKey = record.GetCustomDataField ("bibliographer_last_uri") + "<" + record.GetCustomDataField ("bibliographer_last_md5") + ">";
                    record.SetCustomDataField ("cacheKey", cacheKey);

                    largeThumbnail = Gen (record.GetURI ());
                    smallThumbnail = ((Pixbuf) largeThumbnail.Clone()).ScaleSimple (20, 20, InterpType.Bilinear);

                    largeThumbnail.Save (Cache.Filename("small_thumb",cacheKey), "png");
                    smallThumbnail.Save (Cache.Filename("large_thumb",cacheKey), "png");

                    record.SetCustomDataField ("smallThumbnail", smallThumbnail);
                    record.SetCustomDataField ("largeThumbnail", largeThumbnail);
                }
            } else {
                // thumbnails exist - load
                cacheKey = (string)record.GetCustomDataField ("cacheKey");

                // Load cachekey if it hasn't been loaded yet
				if (!record.HasCustomDataField ("smallThumbnail")) {
					try {
						record.SetCustomDataField ("smallThumbnail", new Pixbuf (Cache.CachedFile ("small_thumb", cacheKey)));
					} catch (Exception) {
						// probably a corrupt cache file
						// delete it and try again :-)
						Cache.RemoveFromCache ("small_thumb", cacheKey);
					}
				}
				if (!record.HasCustomDataField ("largeThumbnail")) {
					try {
						record.SetCustomDataField ("largeThumbnail", new Pixbuf (Cache.CachedFile ("large_thumb", cacheKey)));
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
