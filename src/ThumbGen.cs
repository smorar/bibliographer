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
        public static bool getThumbnail(BibtexRecord record)
        {
            Pixbuf smallThumbnail, largeThumbnail;
            Uri uri;
            GLib.IFile file;
            GLib.FileInfo fileInfo;
            string pixbufPath;

            if ((!record.HasCustomDataField("smallThumbnail")) || (!record.HasCustomDataField("largeThumbnail")))
            {
                // No thumbnail - so generate it and index

                if (record.HasCustomDataField("bibliographer_last_uri") && record.HasCustomDataField("bibliographer_last_md5"))
                {
                    // Check if Nautilus has generated a thumbnail for this file
                    uri = new Uri(record.GetURI());
                    file = GLib.FileFactory.NewForUri(uri);

                    fileInfo = file.QueryInfo("*", GLib.FileQueryInfoFlags.None, null);
                    pixbufPath = fileInfo.GetAttributeByteString("thumbnail::path");

                    if (pixbufPath != "" && pixbufPath != null)
                    {
                        largeThumbnail = new Pixbuf(pixbufPath);
                        smallThumbnail = ((Pixbuf)largeThumbnail.Clone()).ScaleSimple(20, 20, InterpType.Bilinear);

                        record.SetCustomDataField("smallThumbnail", smallThumbnail);
                        record.SetCustomDataField("largeThumbnail", largeThumbnail);

                        // Thumbnails exist and have now been stored in the BibtexRecord instance
                        return true;
                    }
                }
            }
            else
            {
                // Thumbnails exist and are stored in the BibtexRecord instance
                return true;
            }

            // Catch all other conditions. No existing thumbnail.
            return false;
        }
    }
}
