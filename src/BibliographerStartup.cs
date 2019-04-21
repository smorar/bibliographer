//
//  BibliographerStartup.cs
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
using System.Runtime.InteropServices;
using System.Text;
using Gtk;
using libbibby;
using static bibliographer.Debug;

namespace bibliographer
{

    public static class Utilities
    {
        [DllImport("libc")]
        static extern int Prctl (int option, byte[] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);

        public static void SetProcessName (string name)
        {
            if (Prctl (15, Encoding.ASCII.GetBytes (name + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
                throw new ApplicationException ("Error setting process name.");
            }
        }
    }

    public class BibliographerStartup
    {
        public static void Main (string[] args)
        {
			BibliographerMainWindow mainWindow;
            Builder gui;
			string filename;

            try {
                Utilities.SetProcessName ("bibliographer");
            } catch {
				Debug.WriteLine (0, "Cannot set process name");
            }
            
			filename = "";
            
            // Handle startup arguments
            foreach (string arg in args) {
                string st = arg.Trim ();
                
                if (st.IndexOf ("-", StringComparison.CurrentCultureIgnoreCase) == 0 && st.IndexOf ("=", StringComparison.CurrentCultureIgnoreCase) >= 0) {
                    string[] str = st.Split ('=');
                    str[0] = (str[0]).Trim ('-');
                    // Enable debugging
                    if (str[0] == "debug") {
                        if (str[1] == "true") {
                            Debug.Enable (true);
                        }
                    // Modify debug level
                    } else if (str[0] == "debug_level") {
                        Debug.SetLevel (Convert.ToInt16 (str[1]));
                    }
                } else {
                    if (st != "") {
                        filename = st;
                    }
                }
            }
            
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_fields");
            }
            else
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_fields");
            }

            BibtexRecordTypeLibrary.Load ();
            BibtexRecordFieldTypeLibrary.Load ();
            
            Cache.Initialise ();

			Application.Init ();
            try {

                gui = new Builder ();
                System.IO.Stream guiStream = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibliographer.glade");
                try {
                    System.IO.StreamReader reader = new System.IO.StreamReader (guiStream);
                    gui.AddFromString (reader.ReadToEnd ());

                } catch (ArgumentNullException e) {
                    WriteLine (0, "GUI configuration file not found.\n" + e.Message);
                }

                mainWindow = new BibliographerMainWindow (gui);
                mainWindow.am.alterationMonitorThread.Start ();
                mainWindow.am.thumbGenThread.Start ();
                mainWindow.am.indexerThread.Start ();
                mainWindow.am.doiQueryThread.Start ();

                Application.Run ();

                mainWindow.am.FlushQueues();
                mainWindow.am.alterationMonitorThread.Abort ();
                mainWindow.am.alterationMonitorThread.Join ();
                mainWindow.am.indexerThread.Abort ();
                mainWindow.am.indexerThread.Join ();
                mainWindow.am.thumbGenThread.Abort ();
                mainWindow.am.thumbGenThread.Join ();
                mainWindow.am.doiQueryThread.Abort ();
                mainWindow.am.doiQueryThread.Join ();

                mainWindow.sp.splashThread.Abort ();
                mainWindow.sp.splashThread.Join ();

            } catch (NullReferenceException e) {
                WriteLine (0, "Bibliographer window not initialized.");
                WriteLine (0, e.Message);
                WriteLine (0, e.StackTrace);
            }
        }
    }
}
