// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;

namespace bibliographer
{


    public partial class BibtexEntryDialog : Gtk.Dialog
    {

        public BibtexEntryDialog ()
        {
            this.Build ();
        }
        public string GetText ()
        {
            return bibtexData.Buffer.Text;
        }
    }
}
