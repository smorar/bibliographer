//
//  BibliographerSplashScreen.cs
//
//  Author:
//       Sameer Morar <smorar@gmail.com>
//
//  Copyright (c) 2019 
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
using System.Threading;
using System.Collections;
using static System.Threading.Monitor;
using Gtk;
using static bibliographer.Debug;

namespace bibliographer
{
    public class BibliographerSplashScreen
    {
        public Thread splashThread;
        protected Queue splashQueue;

        public BibliographerSplashScreen ()
        {
            splashThread = new Thread (new ThreadStart (SplashThreadStart));
            splashQueue = new Queue ();

        }

        public void SubscribeSplashMessage (string message)
        {
            Enter (splashQueue);
            if (!splashQueue.Contains(message)) {
                splashQueue.Enqueue (message);
            }
            Exit (splashQueue);
        }

        public void SplashThreadStart ()
        {
            Window splashScreen = new Window (WindowType.Toplevel);
            HBox hbox = new HBox ();
            VBox vbox = new VBox ();
            Image splashImage = new Image ();
            Label statusLabel = new Label ();
            Label splashTitle = new Label ();

            splashScreen.Modal = true;
            splashScreen.KeepAbove = true;
            splashScreen.SkipPagerHint = true;
            splashScreen.SkipTaskbarHint = true;
            splashScreen.SetDefaultSize (400, 200);
            splashScreen.SetPosition (WindowPosition.Center);
            splashScreen.Decorated = false;
            hbox.Expand = true;
            vbox.Expand = true;
            splashImage.Expand = true;
            splashTitle.Expand = true;
            statusLabel.Expand = false;
            statusLabel.Justify = Justification.Center;

            splashScreen.Add (vbox);
            vbox.Add (hbox);
            hbox.Add (splashImage);
            hbox.Add (splashTitle);
            vbox.Add (statusLabel);
            splashImage.Pixbuf = new Gdk.Pixbuf (System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("bibliographer.png"));
            Version version = System.Reflection.Assembly.GetExecutingAssembly ().GetName().Version;
            splashTitle.Markup = String.Format("<big><b>Bibliographer</b></big>\n\nVersion: {0}.{1}", version.Major, version.Minor);
            splashTitle.Justify = Justification.Center;
            splashScreen.ShowAll ();

            do {
                WriteLine (1, "Splashscreen loop");
                Enter (splashQueue);
                if (splashQueue.Count>0) {
                    string message = (string)splashQueue.Dequeue ();
                    statusLabel.Text = message;
                    if (message == "loaded") {
                        break;
                    }
                }
                Exit (splashQueue);
                Thread.Sleep (1000);
            } while (true);

            Enter (splashQueue);
            splashQueue.Clear ();
            Exit (splashQueue);
            splashScreen.Destroy ();
        }

    }
}
