// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information
namespace bibliographer
{
public class StringArrayList : System.Collections.CollectionBase
{
  public string this[int index]
  {
    get { return ((string)(List[index])); }
    set { List[index] = value; }
  }

  public int Add(string str)
  {
	if (str == null)
		str = "";
	return List.Add(str);
  }

  public void Insert(int index, string str)
  {
    List.Insert(index, str);
  }

  public void Remove(string str)
  {
    List.Remove(str);
  }

  public bool Contains(string str)
  {
    return List.Contains(str);
  }
  
  public void Sort()
  {
      this.InnerList.Sort();
  }
  
  public override string ToString()
  {
        string output = "[";
        for (int i = 0; i < this.Count; i ++)
        {
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