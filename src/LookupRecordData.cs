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
using static bibliographer.Debug;

namespace bibliographer
{

    [DataContract]
    public class JsonDOIWorkMessageAuthor
    {
        [DataMember]
        public string[] Affiliation {get; set;}
        [DataMember]
        public string Family {get; set;}
        [DataMember]
        public string Given {get; set;}
    }

    [DataContract]
    public class JsonDOIWorkMessageDateTime
    {
        [DataMember(Name="date-parts")]
        public int[,] DateParts { get; set;}
        [DataMember]
        public long Timestamp { get; set;}

    }

    [DataContract]
    public class JsonDOIWorkMessage
    {
        [DataMember]
        public JsonDOIWorkMessageDateTime Indexed { get; set;}
        [DataMember(Name="reference-count")]
        public int ReferenceCount { get; set;}
        [DataMember]
        public string Publisher { get; set;}
        [DataMember]
        public string DOI { get; set;}
        [DataMember]
        public string Type { get; set;}
        [DataMember]
        public string Page { get; set;}
        [DataMember]
        public string Source { get; set;}
        [DataMember]
        public string[] Title { get; set;}
        [DataMember]
        public string Prefix { get; set;}
        [DataMember]
        public string Issue { get; set;}
        [DataMember]
        public string Volume { get; set;}
        [DataMember]
        public JsonDOIWorkMessageAuthor[] Author { get; set;}
        [DataMember]
        public string Member { get; set;}
        [DataMember(Name="container-title")]
        public string[] ContainerTitle { get; set;}
        [DataMember]
        public JsonDOIWorkMessageDateTime Deposited { get; set;}
        [DataMember]
        public double Score {get; set;}
        [DataMember]
        public string[] Subtitle {get; set;}
        [DataMember]
        public JsonDOIWorkMessageDateTime Issued {get; set;}
        [DataMember(Name="alternative-id")]
        public string[] AlternativeId {get; set;}
        [DataMember]
        public string URL {get; set;}
        [DataMember]
        public string[] ISSN {get; set;}
        [DataMember]
        public string[] Subject {get; set;}
    }

    [DataContract]
    public class JsonDOIWork
    {
        [DataMember]
        public string Status { get; set;}
        [DataMember(Name="message-type")]
        public string MessageType { get; set;}
        [DataMember(Name="message-version")]
        public string MessageVersion { get; set;}
        [DataMember]
        public JsonDOIWorkMessage Message { get; set;}
    }

    public static class LookupRecordData
    {
        public static void LookupDOIData (BibtexRecord record)
        {
            string doi = record.GetField (BibtexRecord.BibtexFieldName.DOI);
            string url = "http://api.crossref.org/works/" + doi;
            WriteLine (5, "Looking up data for {0} from {1}", doi, url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);
            if (request != null) {
                request.Timeout = 30000;
            }

            try {
                System.IO.StreamReader reader;
                string jsonString;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
                System.IO.Stream resStream = response.GetResponseStream ();
                reader = new System.IO.StreamReader (resStream);
                jsonString = reader.ReadToEnd ();
                try {
                    JsonDOIWork jsonObj = JsonConvert.DeserializeObject<JsonDOIWork> (jsonString);
                    JsonDOIWorkMessageDateTime date;
                    string authorString, bibtexKeyString;
                    authorString = "";
                    bibtexKeyString = "";
                    foreach (JsonDOIWorkMessageAuthor author in jsonObj.Message.Author) {
                        if (authorString == "") {
                            authorString = author.Family + ", " + author.Given;
                            bibtexKeyString = author.Family;
                        }
                        else {
                            authorString = authorString + " and " + author.Family + ", " + author.Given;
                        }
                    }
                    if (jsonObj.Message.Type == "journal-article") {
                        WriteLine (10, "Setting field values for article: " + doi);
                        record.RecordType = "article";
                        record.SetField (BibtexRecord.BibtexFieldName.Journal, jsonObj.Message.ContainerTitle [0]);
                        record.SetField (BibtexRecord.BibtexFieldName.Volume, jsonObj.Message.Volume);
                        record.SetField (BibtexRecord.BibtexFieldName.Number, jsonObj.Message.Issue);
                        record.SetField (BibtexRecord.BibtexFieldName.Pages, jsonObj.Message.Page);
                    }
                    record.SetField(BibtexRecord.BibtexFieldName.Author, authorString);
                    record.SetField (BibtexRecord.BibtexFieldName.Title, jsonObj.Message.Title [0]);
                    date = jsonObj.Message.Issued;
                    record.SetField (BibtexRecord.BibtexFieldName.Year, date.DateParts [0, 0].ToString ());
                    record.SetField (BibtexRecord.BibtexFieldName.Month, date.DateParts [0, 1].ToString ());

                    if (bibtexKeyString.Length > 0){
                        bibtexKeyString = bibtexKeyString + "_" + date.DateParts [0, 0].ToString () + "_" + (string) record.GetCustomDataField ("bibliographer_last_md5");
                        record.SetKey(bibtexKeyString);
                    }
                }
                catch (System.Exception e) {
                    WriteLine (1, "Unhandled exception when parsing JSON string from {0}", doi, url);
                    WriteLine (1, e.Message);
                }
            }
            catch (WebException e) {
                WriteLine (1, e.Message);
            }
            catch (System.Exception e) {
                WriteLine (1, "Unhandled exception when looking up {0} from {1}", doi, url);
                WriteLine (1, e.Message);
            }
        }
    }
}
