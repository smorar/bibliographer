//
//  StringArrayList.cs
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

namespace libbibby
{
    public class StringArrayList : System.Collections.CollectionBase
    {
        public string this[int index] {
            get { return ((string)(List[index])); }
            set { List[index] = value; }
        }

        public int Add (string str)
        {
            if (str == null)
                str = "";
            return List.Add (str);
        }

        public void Insert (int index, string str)
        {
            List.Insert (index, str);
        }

        public void Remove (string str)
        {
            List.Remove (str);
        }

        public bool Contains (string str)
        {
            return List.Contains (str);
        }

        public void Sort ()
        {
            this.InnerList.Sort ();
        }

        public override string ToString ()
        {
            string output = "[";
            for (int i = 0; i < this.Count; i++) {
                if (i == 0)
                    output = output + List[i];
                else
                    output = output + "," + List[i];
            }
            output = output + "]";
            return output;
        }
        
        // Add other type-safe methods here
        // ...
        // ...
    }
}
