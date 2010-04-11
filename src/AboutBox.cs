// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using Gtk;
using Gnome;

namespace bibliographer
{
    public class AboutBox : Gtk.AboutDialog
    {
        public AboutBox ()
        {
            
            System.Reflection.AssemblyTitleAttribute title = (System.Reflection.AssemblyTitleAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyTitleAttribute));
            
            System.Reflection.AssemblyCopyrightAttribute copyright = (System.Reflection.AssemblyCopyrightAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyCopyrightAttribute));
            
            System.Reflection.AssemblyDescriptionAttribute description = (System.Reflection.AssemblyDescriptionAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyDescriptionAttribute));
            
            System.Version version = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version;
            
            string[] authors = { "Sameer Morar <smorar@gmail.com>", "Carl Hultquist <chultquist@gmail.com>" };
            
            this.Authors = authors;
            this.Logo = new Gdk.Pixbuf (null, "bibliographer.png");
            this.Copyright = copyright.Copyright;
            this.Comments = description.Description;
            this.ProgramName = title.Title;
            this.Version = version.Major.ToString () + "." + version.Minor.ToString ();
            
        }
    }
}
