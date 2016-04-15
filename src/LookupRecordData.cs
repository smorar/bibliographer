//
//  LookupRecordData.cs
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

using System.Runtime.Serialization;
using System.Net;
using libbibby;
using Newtonsoft.Json;

namespace bibliographer
{

    [DataContract]
    public class jsonDOIWorkMessageAuthor
    {
        [DataMember]
        public string[] affiliation {get; set;}
        [DataMember]
        public string family {get; set;}
        [DataMember]
        public string given {get; set;}
    }

    [DataContract]
    public class jsonDOIWorkMessageDateTime
    {
        [DataMember(Name="date-parts")]
        public int[,] dateParts { get; set;}
        [DataMember]
        public long timestamp { get; set;}

    }

    [DataContract]
    public class jsonDOIWorkMessage
    {
        [DataMember]
        public jsonDOIWorkMessageDateTime indexed { get; set;}
        [DataMember(Name="reference-count")]
        public int referenceCount { get; set;}
        [DataMember]
        public string publisher { get; set;}
        [DataMember]
        public string DOI { get; set;}
        [DataMember]
        public string type { get; set;}
        [DataMember]
        public string page { get; set;}
        [DataMember]
        public string source { get; set;}
        [DataMember]
        public string[] title { get; set;}
        [DataMember]
        public string prefix { get; set;}
        [DataMember]
        public string issue { get; set;}
        [DataMember]
        public string volume { get; set;}
        [DataMember]
        public jsonDOIWorkMessageAuthor[] author { get; set;}
        [DataMember]
        public string member { get; set;}
        [DataMember(Name="container-title")]
        public string[] containerTitle { get; set;}
        [DataMember]
        public jsonDOIWorkMessageDateTime deposited { get; set;}
        [DataMember]
        public double score {get; set;}
        [DataMember]
        public string[] subtitle {get; set;}
        [DataMember]
        public jsonDOIWorkMessageDateTime issued {get; set;}
        [DataMember(Name="alternative-id")]
        public string[] alternativeId {get; set;}
        [DataMember]
        public string URL {get; set;}
        [DataMember]
        public string[] ISSN {get; set;}
        [DataMember]
        public string[] subject {get; set;}
    }

    [DataContract]
    public class jsonDOIWork
    {
        [DataMember]
        public string status { get; set;}
        [DataMember(Name="message-type")]
        public string messageType { get; set;}
        [DataMember(Name="message-version")]
        public string messageVersion { get; set;}
        [DataMember]
        public jsonDOIWorkMessage message { get; set;}
    }

    public class LookupRecordData
    {
        public static void LookupDOIData (BibtexRecord record)
        {
            string doi = record.GetField (BibtexRecord.BibtexFieldName.DOI);
            string url = "http://api.crossref.org/works/" + doi;
            Debug.WriteLine (5, "Looking up data for {0} from {1}", doi, url);
            var request = (HttpWebRequest)WebRequest.Create (url);
            request.Timeout = 30000;
            try {
                System.IO.StreamReader reader;
                string jsonString;
                var response = (HttpWebResponse)request.GetResponse ();
                System.IO.Stream resStream = response.GetResponseStream ();
                reader = new System.IO.StreamReader (resStream);
                jsonString = reader.ReadToEnd ();
                try {
                    jsonDOIWork jsonObj = JsonConvert.DeserializeObject<jsonDOIWork> (jsonString);
                    jsonDOIWorkMessageDateTime date;
                    string authorString, bibtexKeyString;
                    authorString = "";
                    bibtexKeyString = "";
                    foreach (jsonDOIWorkMessageAuthor author in jsonObj.message.author) {
                        if (authorString == "") {
                            authorString = author.family + ", " + author.given;
                            bibtexKeyString = author.family;
                        }
                        else {
                            authorString = authorString + " and " + author.family + ", " + author.given;
                        }
                    }
                    if (jsonObj.message.type == "journal-article") {
                        Debug.WriteLine(5, "Setting field values for article: " + doi);
                        record.RecordType = "article";
                        record.SetField (BibtexRecord.BibtexFieldName.Journal, jsonObj.message.containerTitle [0]);
                        record.SetField (BibtexRecord.BibtexFieldName.Volume, jsonObj.message.volume);
                        record.SetField (BibtexRecord.BibtexFieldName.Number, jsonObj.message.issue);
                        record.SetField (BibtexRecord.BibtexFieldName.Pages, jsonObj.message.page);
                    }
                    record.SetField(BibtexRecord.BibtexFieldName.Author, authorString);
                    record.SetField (BibtexRecord.BibtexFieldName.Title, jsonObj.message.title [0]);
                    date = jsonObj.message.issued;
                    record.SetField (BibtexRecord.BibtexFieldName.Year, date.dateParts [0, 0].ToString ());
                    record.SetField (BibtexRecord.BibtexFieldName.Month, date.dateParts [0, 1].ToString ());

                    if (bibtexKeyString.Length > 0){
                        bibtexKeyString = bibtexKeyString + "_" + date.dateParts [0, 0].ToString () + "_" + (string) record.GetCustomDataField ("bibliographer_last_md5");
                        record.SetKey(bibtexKeyString);
                    }
                }
                catch {
                    Debug.WriteLine (2, "Unhandled exception when parsing JSON string from {0}", doi, url);
                }
            }
            catch (WebException e) {
                Debug.WriteLine (2, e.Message);
            }
            catch {
                Debug.WriteLine (2, "Unhandled exception when looking up {0} from {1}", doi, url);
            }
        }
    }
}
