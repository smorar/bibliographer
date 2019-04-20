//
//  SuperTri.cs
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
using System.Collections.Generic;

namespace bibliographer
{
    public class SuperTriNode
    {
        public SuperTriNode [] children;
        public List<int> hitList = new List<int>();

        public SuperTriNode ()
        {
            children = new SuperTriNode [26];
            for (int i = 0; i < 26; i++)
                children [i] = null;
        }

    }

    public class SuperTri
    {
        SuperTriNode root;

        public SuperTri ()
        {
            root = new SuperTriNode ();
        }

        public void AddTri (Tri t, int id)
        {
            Debug.WriteLine (5, "Adding tri to supertri for record {0}", id);
            NextSuperTriNode (root, t.root, id);
        }

        public void NextSuperTriNode (SuperTriNode superTriNode, TriNode triNode, int id)
        {

            for (int i = 0; i < 26; i++) {
                if (triNode.children [i] != null) {
                    if (superTriNode.children [i] == null) {
                        superTriNode.children [i] = new SuperTriNode ();
                    }
                    if (!superTriNode.children[i].hitList.Contains(id)) {
                        superTriNode.children[i].hitList.Add (id);
                    }
                    NextSuperTriNode (superTriNode.children [i], triNode.children [i], id);
                }
            }
        }

        public List<int> ContainsRecords (string s)
        {
            string realS = s.ToLower ();
            List<int> result = null;
            SuperTriNode curNode = root;
            for (int i = 0; i < realS.Length; i++) {
                int c = realS [i] - 'a';
                if ((c < 0) || (c >= 26)) {
                    continue;
                }
                if (curNode.children [c] == null) {
                    return null;
                }
                curNode = curNode.children [c];
                result = curNode.hitList;
            }
            return result;
        }
    }
}
