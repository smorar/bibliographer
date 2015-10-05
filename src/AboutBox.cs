//
//  AboutBox.cs
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

namespace bibliographer
{
    public class AboutBox : Gtk.AboutDialog
    {
        public AboutBox ()
        {
            var title = (System.Reflection.AssemblyTitleAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyTitleAttribute));
            
            var copyright = (System.Reflection.AssemblyCopyrightAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyCopyrightAttribute));
            
            var description = (System.Reflection.AssemblyDescriptionAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyDescriptionAttribute));
            
            Version version = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version;
            
            string[] authors = { "Sameer Morar <smorar@gmail.com>", "Carl Hultquist <chultquist@gmail.com>" };
            
            Authors = authors;
            Logo = new Gdk.Pixbuf (null, "bibliographer.png");
            Copyright = copyright.Copyright;
            Comments = description.Description;
            ProgramName = title.Title;
            Version = version.Major + "." + version.Minor;
            
        }
    }
}
