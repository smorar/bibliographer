// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Text;

namespace bibliographer
{
  public class Tri
  {
    private class TriNode {
      public TriNode()
      {
        hitCount = 0;
        children = new TriNode[26];
        for (int i = 0; i < 26; i++)
          children[i] = null;
      }

      public TriNode(String[] s, ref int pos)
      {
          if (pos >= s.Length)
              return;
        hitCount = int.Parse(s[pos]);
        pos++;
        children = new TriNode[26];
        for (int i = 0; i < 26; i++) {
          if (s[pos] != "")
            children[i] = new TriNode(s, ref pos);
          else {
            children[i] = null;
            pos++;
          }
        }
      }
      
      public void PrintStringsMatched(String s)
      {
        if (hitCount != 0)
          Debug.WriteLine(5, s);
        for (int i = 0; i < 26; i++)
          if (children[i] != null)
            children[i].PrintStringsMatched(s + ((char) ('a' + i)));
      }
           
      public TriNode[] children;
      public int hitCount;
      
      public override String ToString()
      {
        String result = hitCount.ToString();
        for (int i = 0; i < 26; i++) {
          result += ",";
          if (children[i] != null)
            result += children[i].ToString();
        }
        return result;
      }
    }
    
    private TriNode root;
    
    public Tri()
    {
      root = new TriNode();
    }
    
    public Tri(String s)
    {
      int pos = 0;
      root = new TriNode(Decompress(s).Split(','), ref pos);
    }
    
    public void AddString(String s)
    {
      String realS = s.ToLower();
      TriNode curNode = root;
      for (int i = 0; i < realS.Length; i++) {
        int c = realS[i] - 'a';
        if ((c < 0) || (c >= 26))
          continue;
        if (curNode.children[c] == null)
          curNode.children[c] = new TriNode();
        curNode = curNode.children[c];
      }
      if (curNode != root)
        curNode.hitCount++;
    }
    
    public int CountOccurrences(String s)
    {
      String realS = s.ToLower();
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
    
    public bool Contains(String s)
    {
      String realS = s.ToLower();
      TriNode curNode = root;
      for (int i = 0; i < realS.Length; i++) {
        int c = realS[i] - 'a';
        if ((c < 0) || (c >= 26))
          continue;
        if (curNode.children[c] == null)
          return false;
        curNode = curNode.children[c];
      }
      if (curNode.hitCount != 0)
        return true;
      else
        return false;
    }
      
    public bool IsSubString(String s)
    {
      String realS = s.ToLower();
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
    
    public void PrintStringsMatched()
    {
      root.PrintStringsMatched("");
    }
    
    public override String ToString()
    {
      return Compress(root.ToString());
    }
    
    private String Compress(String s)
    {
      String[] toks = s.Split(',');
      StringBuilder result = new StringBuilder();
      int pos = 0;
      while (pos < toks.Length) {
        if (toks[pos] != "") {
          result.Append(Encode(int.Parse(toks[pos])));
          pos++;
        }
        else {
          int startPos = pos;
          pos++;
          while ((pos < toks.Length) && (toks[pos] == ""))
            pos++;
          if ((pos - startPos) > 2) {
            // we can compress this a bit!
            result.Append('*');
            result.Append(Encode(pos - startPos - 1));
          }
          else {
            // no compression, aw phooey
            for (int i = 0; i < (pos - startPos - 1); i++)
              result.Append(',');
          }
        }    
        if (pos < toks.Length)
          result.Append(',');
      }
      return result.ToString();
    }
    
    private String Decompress(String s)
    {
      StringBuilder wsRemoved = new StringBuilder();
      for (int i = 0; i < s.Length; i++)
        switch (s[i]) {
          case ' ':
          case '\n':
          case '\t':
          case '\r':
            break;
          default:
            wsRemoved.Append(s[i]);
            break;
        }
      s = wsRemoved.ToString();
      String[] toks = s.Split(',');
      StringBuilder result = new StringBuilder();
      for (int i = 0; i < toks.Length; i++) {
        if (toks[i] != "") {
          if (toks[i][0] == '*') {
            // expand to commas
            int count = Decode(toks[i].Substring(1));
            for (int j = 0; j < count; j++)
              result.Append(',');
          }
          else
            result.Append(Decode(toks[i]).ToString());
        }
        if (toks.Length > 0 && i < (toks.Length - 1))
          result.Append(',');
      }
      return result.ToString();
    }
    
    private String validChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&()-+";
    
    private String Encode(int x)
    {
      String result = "";
      do {
        result = validChars[x % validChars.Length] + result;
        x = x / validChars.Length;
      } while (x != 0);
      return result;
    }
    
    private int Decode(String s)
    {
      int x = 0;
      for (int i = 0; i < s.Length; i++)
        x = x * validChars.Length + validChars.IndexOf(s[i]);
      return x;
    }
     
  }
}
