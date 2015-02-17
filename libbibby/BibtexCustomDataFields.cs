// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System.Collections;

namespace libbibby
{
    public class BibtexCustomDataFields : ArrayList
    {
        public BibtexCustomDataFields ()
        {
        }

        public bool HasField (string field)
        {
            if (this.Count > 0) {
                foreach (BibtexCustomData customDataField in this) {
                    if (customDataField.GetFieldName () == field)
                        return true;
                }
            }
            return false;
        }

        public object GetField (string field)
        {
            if (this.Count > 0) {
                foreach (BibtexCustomData customDataField in this) {
                    if (customDataField.GetFieldName () == field)
                        return customDataField.GetData ();
                }
            }
            return null;
        }
    }
}
