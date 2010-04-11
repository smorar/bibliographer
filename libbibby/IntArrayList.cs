// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

namespace libbibby
{
    public class IntArrayList : System.Collections.CollectionBase
    {
        public int this[int index] {
            get { return ((int)(List[index])); }
            set { List[index] = value; }
        }

        public int Add (int i)
        {
            return List.Add (i);
        }

        public void Insert (int index, int i)
        {
            List.Insert (index, i);
        }

        public void Remove (int i)
        {
            List.Remove (i);
        }

        public bool Contains (int i)
        {
            return List.Contains (i);
        }
        
        // Add other type-safe methods here
        // ...
        // ...
    }
}
