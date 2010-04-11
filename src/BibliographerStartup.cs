// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Unix;
using libbibby;

namespace bibliographer
{

    public static class Utilities
    {
        [DllImport("libc")]
        private static extern int Prctl (int option, byte[] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);

        public static void SetProcessName (string name)
        {
            if (Prctl (15,             /* PR_SET_NAME */Encoding.ASCII.GetBytes (name + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
                throw new ApplicationException ("Error setting process name: " + Mono.Unix.Native.Stdlib.GetLastError ());
            }
        }
    }

    public class BibliographerStartup
    {
        public static void Main (string[] args)
        {
            
            System.Reflection.AssemblyTitleAttribute title = (System.Reflection.AssemblyTitleAttribute)Attribute.GetCustomAttribute (System.Reflection.Assembly.GetExecutingAssembly (), typeof(System.Reflection.AssemblyTitleAttribute));
            System.Version version = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version;
            
            Gnome.Program program = new Gnome.Program (title.ToString ().ToLower (), version.Major.ToString () + "." + version.Minor.ToString (), Gnome.Modules.UI, args);
            
            try {
                Utilities.SetProcessName ("bibliographer");
            } catch {
            }
            
            string filename = "";
            
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
                        Debug.SetLevel (System.Convert.ToInt16 (str[1]));
                    }
                } else {
                    if (st != "") {
                        filename = st;
                    }
                }
            }
            
            Gtk.Application.Init (program.AppId, ref args);
            Gnome.Vfs.Vfs.Initialize ();

            System.Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", System.Environment.GetEnvironmentVariable ("HOME") + "/.config/bibliographer/bibtex_records");
            BibtexRecordTypeLibrary.Load ();
            System.Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", System.Environment.GetEnvironmentVariable ("HOME") + "/.config/bibliographer/bibtex_fields");
            BibtexRecordFieldTypeLibrary.Load ();
            
            Config.Initialise ();
            Cache.Initialise ();
            
            BibliographerMainWindow window = new BibliographerMainWindow ();
            
            if (filename != "")
                window.FileOpen (filename);
            
            window.am.indexerThread.Start ();
            window.am.alterationMonitorThread.Start ();
            
            Gtk.Application.Run ();
            
            window.am.indexerThread.Abort ();
            window.am.alterationMonitorThread.Abort ();
            window.am.indexerThread.Join ();
            window.am.alterationMonitorThread.Join ();
            
        }
    }
}
