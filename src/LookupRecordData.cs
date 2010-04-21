// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using libbibby;

namespace bibliographer
{
    public class LookupRecordData
    {
        public LookupRecordData ()
        {
        }

        public void LookupDOI (object o, EventArgs e)
        {
            // TODO: refactor this in terms of a doi record
            // should be a static function
            BibtexRecord record = (BibtexRecord)o;
            
            Debug.WriteLine (1, "Uri updated");
            string URI = record.GetField (BibtexRecord.BibtexFieldName.URI);
            
            Debug.WriteLine (5, "Uri: {0} added to record", URI);
            // Determine doi number from the uri, and lookup info.
            StringArrayList textualData = FileIndexer.GetTextualData (URI);
            if (textualData != null) {
                for (int line = 0; line < textualData.Count; line++) {
                    String data = ((String)textualData[line]).ToLower ();
                    if (data.IndexOf ("doi:") > 0) {
                        int idx1 = data.IndexOf ("doi:");
                        data = data.Substring (idx1);
                        data = data.Trim ();
                        // If there are additional characters on this line, find a space character and chop them off
                        if (data.IndexOf (' ') > 0) {
                            int idx2 = data.IndexOf (' ');
                            data = data.Substring (0, data.Length - idx2);
                        }
                        // Strip out "doi:"
                        data = data.Remove (0, 4);
                        Debug.WriteLine (5, "Found doi:{0}", data);
                        record.SetCustomDataField ("bibliographer_doi", data);
                        
                        // Start a thread to look up the record's data, so as to not lockup the interface
                        // if the request takes a while, or times out due to no network connectivity.
                        // TODO: Use a threadpool here - we can do some of these simultaneously.
                        //RunOnMainThread.Run (this, "LookupData", null);
                    }
                }
            }
        }

//        private void LookupData ()
//        {
//            // Call this method in a thread, as it will lock up the application until a HttpWebRequest is completed
//            string url = "http://www.crossref.org/openurl/?id=doi:"+this.GetField("bibliographer_doi")+"&noredirect=true";
//            Debug.WriteLine(5, "Looking up data for {0} from {1}", this.GetField("bibliographer_doi"), url);
//            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
//            try {
//                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
//                Stream resStream = response.GetResponseStream();
//                
//                try {
//                    System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(resStream);
//                    reader.MoveToContent();
//                    
//                    while(reader.Read())
//                    {
//                        //Debug.WriteLine(2, "{0}: {1}", reader.Name, reader.Value);
//                        if (reader.Name.ToLower() == "doi")
//                        {
//                            if (reader.GetAttribute("type") == "journal_article")
//                            {
//                                Debug.WriteLine(5, "Setting RecordType: article");
//                                this.RecordType = "article";
//                            }
//                        }
//                        if (reader.Name.ToLower() == "journal_title")
//                        {
//                            reader.Read();
//                            if (this.HasField("journal") == false)
//                            {
//                                Debug.WriteLine(5, "setting journal: {0}", reader.Value);
//                                this.SetField("journal", reader.Value);
//                            }
//                        }
//                        if (reader.Name.ToLower() == "contributors")
//                        {
//                          Debug.WriteLine(5, "found contributors");
//                          while(reader.Read())
//                          {
//                              reader.Read();
//                              if (reader.Name.ToLower() == "contributor")
//                              {
//                                  Debug.WriteLine(5, "found contributor");
//                                  string surname = "";
//                                  string given_name = "";
//                                  
//                                  while(reader.Read())
//                                  {
//                                      int counter = 0;
//                                      reader.MoveToContent();
//                                      if (reader.Name.ToLower() == "given_name" || reader.Name.ToLower() == "surname")
//                                      {
//                                          if (reader.Name.ToLower() == "surname")
//                                          {
//                                              Debug.WriteLine(5, "found surname");
//                                              reader.Read();
//                                              if (surname.Length == 0)
//                                                  surname = reader.Value;
//                                          }
//                                          if (reader.Name.ToLower() == "given_name")
//                                          {
//                                              Debug.WriteLine(5, "found given_name");
//                                              reader.Read();
//                                              if (given_name.Length == 0)
//                                                  given_name = reader.Value;
//                                          }
//                                      }
//                                      else
//                                          break;
//                                      counter += 1;
//                                  }
//                                  if (this.HasField("author") == false)
//                                  {
//                                      if (surname != "")
//                                      {
//                                          // sort out case of surname
//                                          surname = String.Concat(surname.Substring(0,1).ToUpper(), surname.Substring(1).ToLower());
//                                          if (given_name != "")
//                                          {
//                                              // sort out case of firstname
//                                              given_name = String.Concat(given_name.Substring(0,1).ToUpper(), given_name.Substring(1).ToLower());
//                                              this.SetField("author", String.Concat(surname, ", ", given_name));
//                                          }
//                                          else
//                                          {
//                                              this.SetField("author", surname);
//                                          }
//                                      }
//                                  }
//                                  else
//                                  {
//                                      if (surname != "")
//                                      {
//                                          if (given_name != "")
//                                          {
//                                              this.SetField("author", String.Concat(this.GetField("author"), " and ", surname, ", ", given_name));
//                                          }
//                                          else
//                                          {
//                                              this.SetField("author", String.Concat(this.GetField("author"), " and ", surname));
//                                          }
//                                      }
//                                  }
//                              }
//                              else
//                                  break;
//                          }
//                        }
//                        if (reader.Name.ToLower() == "volume")
//                        {
//                            reader.Read();
//                            if (this.HasField("volume") == false)
//                            {
//                                Debug.WriteLine(5, "setting volume: {0}", reader.Value);
//                                this.SetField("volume", reader.Value);
//                            }
//                        }
//                        if (reader.Name.ToLower() == "issue")
//                        {
//                            reader.Read();
//                            if (this.HasField("number") == false)
//                            {
//                                Debug.WriteLine(5, "setting number: {0}", reader.Value);
//                                this.SetField("number", reader.Value);
//                            }
//                        }
//                        if (reader.Name.ToLower() == "first_page")
//                        {
//                            reader.Read();
//                            if (this.HasField("pages") == false)
//                            {
//                                Debug.WriteLine(5, "setting pages: {0}", reader.Value);
//                                this.SetField("pages", reader.Value);
//                            }
//                        }
//                        if (reader.Name.ToLower() == "year")
//                        {
//                            reader.Read();
//                            if (this.HasField("year") == false)
//                            {
//                                Debug.WriteLine(5, "setting year: {0}", reader.Value);
//                                this.SetField("year", reader.Value);
//                            }
//                        }
//                        if (reader.Name.ToLower() == "article_title")
//                        {
//                            reader.Read();
//                            if (this.HasField("title") == false)
//                            {
//                                Debug.WriteLine(5, "setting title: {0}", reader.Value);
//                                this.SetField("title", reader.Value);
//                            }
//                        }
//                    }
//    /*                if ((this.GetAuthors() != "" or this.GetAuthors() != null) && (this.GetYear() != "" or this.GetYear() != null))
//                    {
//                      //TODO: Generate key here
//                      
//                    }
//     */               
//                }
//                catch (System.Xml.XmlException e)
//                {
//                    Debug.WriteLine(2, e.Message);
//                }
//            }
//            catch (System.Net.WebException e)
//            {
//                Debug.WriteLine(2, e.Message);
//            }
//        }
    }

    public class RunOnMainThread
    {
        private object methodClass;
        private string methodName;
        private object[] arguments;

        public RunOnMainThread (object methodClass, string methodName, object[] arguments)
        {
            this.methodClass = methodClass;
            this.methodName = methodName;
            this.arguments = arguments;
            GLib.Idle.Add (new GLib.IdleHandler (Go));
        }

        public static void Run (object methodClass, string methodName, object[] arguments)
        {
            new RunOnMainThread (methodClass, methodName, arguments);
        }

        private bool Go ()
        {
            methodClass.GetType ().InvokeMember (methodName, System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod, null, methodClass, arguments);
            return false;
        }
    }
}
