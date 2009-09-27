// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using Gtk;
using Glade;
using Gnome;

namespace bibliographer
{
	public class AboutBox : Gtk.AboutDialog
	{
		public AboutBox()
		{
			System.Reflection.AssemblyName asm = 
				System.Reflection.Assembly.GetEntryAssembly().GetName();
			
			string name = asm.Name[0].ToString().ToUpper() + asm.Name.Substring(1);
			string version = asm.Version.Major + "." + asm.Version.Minor;
			string[] authors = {"Sameer Morar <smorar@gmail.com>", "Carl Hultquist <chultquist@gmail.com>"};
		    
		    this.Authors = authors;
		    this.Logo = new Gdk.Pixbuf(null, "bibliographer.png");
		    this.Copyright = "Copyright ©2005-2007";
		    this.Comments = "A Bibtex editor";
		    this.Name = name;
		    this.Version = version;

		}
	}
}