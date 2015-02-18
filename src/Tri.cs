//
//  Tri.cs
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
using System.Text;

namespace bibliographer
{
    public class Tri
    {
        class TriNode
        {
            public TriNode ()
            {
                hitCount = 0;
                children = new TriNode[26];
                for (int i = 0; i < 26; i++)
                    children[i] = null;
            }

            public TriNode (String[] s, ref int pos)
            {
                if (pos >= s.Length)
                    return;
                hitCount = int.Parse (s[pos]);
                pos++;
                children = new TriNode[26];
                for (int i = 0; i < 26; i++) {
                    if (s[pos] != "")
                        children[i] = new TriNode (s, ref pos);
                    else {
                        children[i] = null;
                        pos++;
                    }
                }
            }

            public void PrintStringsMatched (String s)
            {
                if (hitCount != 0)
                    Debug.WriteLine (5, s);
                for (int i = 0; i < 26; i++)
                    if (children[i] != null)
                        children[i].PrintStringsMatched (s + ((char)('a' + i)));
            }

            public TriNode[] children;
            public int hitCount;

            public override String ToString ()
            {
                String result = hitCount.ToString ();
                for (int i = 0; i < 26; i++) {
                    result += ",";
                    if (children[i] != null)
                        result += children[i].ToString ();
                }
                return result;
            }
        }

        TriNode root;

        public Tri ()
        {
            root = new TriNode ();
        }

        public Tri (String s)
        {
            int pos = 0;
            root = new TriNode (Decompress (s).Split (','), ref pos);
        }

        public void AddString (String s)
        {
            String realS = s.ToLower ();
            TriNode curNode = root;
            for (int i = 0; i < realS.Length; i++) {
                int c = realS[i] - 'a';
                if ((c < 0) || (c >= 26))
                    continue;
                if (curNode.children[c] == null)
                    curNode.children[c] = new TriNode ();
                curNode = curNode.children[c];
            }
            if (curNode != root)
                curNode.hitCount++;
        }

        public int CountOccurrences (String s)
        {
            String realS = s.ToLower ();
            TriNode curNode = root;
            for (int i = 0; i < realS.Length; i++) {
                int c = realS[i] - 'a';
                if ((c < 0) || (c >= 26))
                    continue;
                if (curNode.children[c] == null)
                    return 0;
                curNode = curNode.children[c];
            }
            return curNode.hitCount;
        }

        public bool Contains (String s)
        {
            String realS = s.ToLower ();
            TriNode curNode = root;
            for (int i = 0; i < realS.Length; i++) {
                int c = realS[i] - 'a';
                if ((c < 0) || (c >= 26))
                    continue;
                if (curNode.children[c] == null)
                    return false;
                curNode = curNode.children[c];
            }
            return curNode.hitCount != 0;
        }

        public bool IsSubString (String s)
        {
            String realS = s.ToLower ();
            TriNode curNode = root;
            for (int i = 0; i < realS.Length; i++) {
                int c = realS[i] - 'a';
                if ((c < 0) || (c >= 26))
                    continue;
                if (curNode.children[c] == null)
                    return false;
                curNode = curNode.children[c];
            }
            return true;
        }

        public void PrintStringsMatched ()
        {
            root.PrintStringsMatched ("");
        }

        public override String ToString ()
        {
            return Compress (root.ToString ());
        }

        String Compress (String s)
        {
            String[] toks = s.Split (',');
            var result = new StringBuilder ();
            int pos = 0;
            while (pos < toks.Length) {
                if (toks[pos] != "") {
                    result.Append (Encode (int.Parse (toks[pos])));
                    pos++;
                } else {
                    int startPos = pos;
                    pos++;
                    while ((pos < toks.Length) && (toks[pos] == ""))
                        pos++;
                    if ((pos - startPos) > 2) {
                        // we can compress this a bit!
                        result.Append ('*');
                        result.Append (Encode (pos - startPos - 1));
                    } else {
                        // no compression, aw phooey
                        for (int i = 0; i < (pos - startPos - 1); i++)
                            result.Append (',');
                    }
                }
                if (pos < toks.Length)
                    result.Append (',');
            }
            return result.ToString ();
        }

        String Decompress (String s)
        {
            var wsRemoved = new StringBuilder ();
            for (int i = 0; i < s.Length; i++)
                switch (s[i]) {
                case ' ':
                case '\n':
                case '\t':
                case '\r':
                    break;
                default:
                    wsRemoved.Append (s[i]);
                    break;
                }
            s = wsRemoved.ToString ();
            String[] toks = s.Split (',');
            var result = new StringBuilder ();
            for (int i = 0; i < toks.Length; i++) {
                if (toks[i] != "") {
                    if (toks[i][0] == '*') {
                        // expand to commas
                        int count = Decode (toks[i].Substring (1));
                        for (int j = 0; j < count; j++)
                            result.Append (',');
                    } else
                        result.Append (Decode (toks[i]).ToString ());
                }
                if (toks.Length > 0 && i < (toks.Length - 1))
                    result.Append (',');
            }
            return result.ToString ();
        }

        readonly String validChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&()-+";

        String Encode (int x)
        {
            String result = "";
            do {
                result = validChars[x % validChars.Length] + result;
                x = x / validChars.Length;
            } while (x != 0);
            return result;
        }

        int Decode (String s)
        {
            int x = 0;
            for (int i = 0; i < s.Length; i++)
                x = x * validChars.Length + validChars.IndexOf (s[i]);
            return x;
        }
        
    }
}
