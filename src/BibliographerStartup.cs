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
using libbibby;

namespace bibliographer
{

    public static class Utilities
    {
        [DllImport("libc")]
        static extern int Prctl (int option, byte[] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);

        public static void SetProcessName (string name)
        {
            if (Prctl (15,             /* PR_SET_NAME */Encoding.ASCII.GetBytes (name + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
                throw new ApplicationException ("Error setting process name.");
            }
        }
    }

    public class BibliographerStartup
    {
        public static void Main (string[] args)
        {
			BibliographerMainWindow window;

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
                
                if (st.IndexOf ("-") == 0 && st.IndexOf ('=') >= 0) {
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

			Gtk.Application.Init ();
            try {
                window = new BibliographerMainWindow ();
                if (filename != "")
                    window.FileOpen (filename);

                window.am.thumbGenThread.Start ();
                window.am.indexerThread.Start ();
                window.am.alterationMonitorThread.Start ();
                window.am.doiQueryThread.Start ();

                Gtk.Application.Run ();

                window.am.alterationMonitorThread.Abort ();
                window.am.alterationMonitorThread.Join ();
                window.am.indexerThread.Abort ();
                window.am.indexerThread.Join ();
                window.am.thumbGenThread.Abort ();
                window.am.thumbGenThread.Join ();
                window.am.doiQueryThread.Abort ();
                window.am.doiQueryThread.Join ();

            } catch (NullReferenceException e) {
                Console.WriteLine ("Bibliographer window not initialized.\n" + e.Message);
            } catch {
                Console.WriteLine ("Unhandled exception in main thread.");
            }
        }
    }
}
