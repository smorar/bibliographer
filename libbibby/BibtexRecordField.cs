//
//  BibtexRecordField.cs
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

using System.Text;

namespace libbibby
{
    public class BibtexRecordField
    {
        public string fieldName;
        public string fieldValue;

        public BibtexRecordField (string fieldName, string fieldValue)
        {
            this.fieldName = fieldName;
            this.fieldValue = fieldValue;
        }

        public string ToBibtexString ()
        {
            StringBuilder fieldString = new StringBuilder ();
            
            if (!(this.fieldValue == "" || this.fieldValue == null)) {
                fieldString.Append ('\t');
                fieldString.Append (this.fieldName);
                fieldString.Append (" = {");
                if (this.fieldValue.Length > 4000) {
                    // split the field over several lines to make this file
                    // work happily with Bibtex. The choice of 4000 is fairly
                    // arbitrary; Bibtex's limit is 5000 so we just stay a bit
                    // below the radar :-)
                    int lineCount = this.fieldValue.Length / 4000 + ((this.fieldValue.Length % 4000) == 0 ? 0 : 1);
                    fieldString.Append ("\n");
                    for (int i = 0; i < lineCount; i++) {
                        fieldString.Append ("\t\t");
                        if ((i + 1) < lineCount)
                            fieldString.Append (this.fieldValue.Substring (i * 4000, 4000));
                        else
                            fieldString.Append (this.fieldValue.Substring (i * 4000));
                        fieldString.Append ("\n");
                    }
                    fieldString.Append ("\t");
                } else
                    fieldString.Append (this.fieldValue);
                fieldString.Append ("},\n");
            }
            return fieldString.ToString ();
        }
    }
}
