// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Gnome;
using Mono.Unix;

namespace bibliographer
{

  public static class Utilities
  {
    [DllImport("libc")]
    private static extern int Prctl(int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
        
    public static void SetProcessName(string name)
    {
      if(Prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"), 
	      IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
	throw new ApplicationException("Error setting process name: " + 
	    Mono.Unix.Native.Stdlib.GetLastError());
      }
    }
  }

  public class BibliographerStartup
  {
    public static void Main (string[] args)
    {
        Gnome.Program program = new Gnome.Program("bibliographer", "0.1.1", Gnome.Modules.UI, args);
        
        try{
	        Utilities.SetProcessName("bibliographer");
        } catch{}
        
        string filename = "";
        
        // Handle startup arguments
        foreach (string arg in args)
        {
            string st = arg.Trim();
            
            if (st.IndexOf("-") == 0 && st.IndexOf('=') >= 0 )
            {
                string [] str = st.Split('=');
                str[0] = (str[0]).Trim('-');
                // Enable debugging
                if (str[0] == "debug")
                {
                    if (str[1] == "true")
                    {
                        Debug.Enable(true);
                    }
                }
                // Modify debug level
                else if (str[0] == "debug_level")
                {
                    Debug.SetLevel(System.Convert.ToInt16(str[1]));
                }
            }
            else
            {
                if (st != "")
                {
                    filename = st;
                }
            }
        }
        
        Gtk.Application.Init (program.AppId, ref args);
        Gnome.Vfs.Vfs.Initialize();
        
        BibtexRecordTypeLibrary.Load();
        BibtexRecordFieldTypeLibrary.Load();
        
        Config.Initialise();
        Cache.Initialise();
        BibliographerUI ui;
        ui = new BibliographerUI();
        ui.Init();
        
        if (filename != "")
            ui.FileOpen(filename);
        

        ui.indexerThread.Start();
        ui.alterationMonitorThread.Start();		

        Gtk.Application.Run ();

        ui.indexerThread.Abort();
        ui.alterationMonitorThread.Abort();
        ui.indexerThread.Join();
        ui.alterationMonitorThread.Join();
    }
  }
}
