// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace libbibby
{
    public class ParseException : Exception
    {
        private string reason;

        public ParseException (string reason)
        {
            this.reason = reason;
        }

        public string GetReason ()
        {
            return reason;
        }
    }

    public enum BibtexSearchField
    {
        All,
        Author,
        Title,
        Article
    }

    public class BibtexRecord
    {
        private string recordType;

        public event System.EventHandler RecordModified;
        public event System.EventHandler FieldAdded;
        public event System.EventHandler FieldDeleted;
        public event System.EventHandler UriAdded;
        public event System.EventHandler UriUpdated;

        private string comment;
        private string recordKey;
        private ArrayList recordFields;

        // Custom Data - for associating data with the record
        // NB!! This data is not, and should not be serialised
        private BibtexCustomDataFields customData = new BibtexCustomDataFields ();

        public BibtexRecord (string recordType, string recordKey, ArrayList recordFields)
        {
            this.recordType = recordType;
            this.recordKey = recordKey;
            this.recordFields = recordFields;
        }

        public BibtexRecord ()
        {
            this.recordType = "";
            this.recordKey = "";
            this.recordFields = new ArrayList ();
        }

        public BibtexRecord (StreamReader stream)
        {
            CreateFromStream (stream);
        }

        public BibtexRecord (string s)
        {
            StringReader reader = new StringReader (s);
            CreateFromStream (reader);
        }

        public ArrayList RecordFields {
            get {
                ArrayList list = new ArrayList ();
                if (recordFields != null)
                    for (int record = 0; record < recordFields.Count; record++)
                        if (!((BibtexRecordField)recordFields[record]).fieldName.StartsWith ("bibliographer_"))
                            list.Add (recordFields[record]);
                return list;
            }
        }

        public void SetKey (string key)
        {
            if (this.recordKey != key) {
                Debug.WriteLine (5, "Key set: {0}", key);
                this.recordKey = key;
                //System.Console.WriteLine ("RecordModified event emitted: SetKey");
                this.OnRecordModified (new EventArgs ());
            }
        }

        public string GetKey ()
        {
            return this.recordKey;
        }

        protected virtual void OnRecordModified (EventArgs e)
        {
            Debug.WriteLine (5, "Record Modified");
            if (RecordModified != null)
                RecordModified (this, e);
        }

        protected virtual void OnFieldAdded (EventArgs e)
        {
            Debug.WriteLine (5, "Field Added");
            //System.Console.WriteLine("RecordModified event emitted: OnFieldAdded");
            //this.OnRecordModified(new EventArgs());
            if (FieldAdded != null)
                FieldAdded (this, e);
        }

        protected virtual void OnFieldDeleted (EventArgs e)
        {
            Debug.WriteLine (5, "Field Deleted");
            //System.Console.WriteLine("RecordModified event emitted: OnFieldDeleted");
            //this.OnRecordModified(new EventArgs());
            if (FieldDeleted != null)
                FieldDeleted (this, e);
        }

        protected virtual void OnUriAdded (EventArgs e)
        {
            Debug.WriteLine (5, "Uri Added");
            //System.Console.WriteLine("RecordModified event emitted: OnUriAdded");
            //this.OnRecordModified(new EventArgs());
            if (UriAdded != null)
                UriAdded (this, e);
        }

        protected virtual void OnUriUpdated (EventArgs e)
        {
            Debug.WriteLine (1, "Uri Updated");
            //this.OnRecordModified(new EventArgs());
            if (UriUpdated != null)
                UriUpdated (this, e);
        }

        public string RecordType {
            get { return recordType; }
            set {
                Debug.WriteLine (5, "RecordType changed from {0} to {1}", recordType, value);
                recordType = value;
                //System.Console.WriteLine ("RecordModified event emitted: set RecordType");
                this.OnRecordModified (new EventArgs ());
            }
        }

        public bool HasField (string field)
        {
            // TODO: need a better way of doing all this
            if (field == "Bibtex Key")
                return true;
            if (recordFields != null)
                for (int i = 0; i < recordFields.Count; i++)
                    if (String.Compare (((BibtexRecordField)recordFields[i]).fieldName, field, true) == 0)
                        return true;
            return false;
        }

        public string GetField (string field)
        {
            // TODO: need a better way of doing all this
            if (field == "Bibtex Key")
                return recordKey;
            if (recordFields != null)
                for (int i = 0; i < recordFields.Count; i++)
                    if (String.Compare (((BibtexRecordField)recordFields[i]).fieldName, field, true) == 0)
                        return ((BibtexRecordField)recordFields[i]).fieldValue;
            return null;
        }

        public void SetField (string field, string content)
        {
            if (recordFields != null) {
                for (int i = 0; i < recordFields.Count; i++) {
                    if (String.Compare (((BibtexRecordField)recordFields[i]).fieldName, field, true) == 0) {
                        // Check if the field has _actually_ changed
                        if (content != ((BibtexRecordField)recordFields[i]).fieldValue) {
                            Debug.WriteLine (5, "Field: {0} updated with content: {1}", field, content);
                            ((BibtexRecordField)recordFields[i]).fieldValue = content;
                            if (field == "bibliographer_uri") {
                                //System.Console.WriteLine ("Uri updated event emitted: SetField {0}", field);
                                this.OnUriUpdated (new EventArgs ());
                            }
                            //System.Console.WriteLine ("Record modified event emitted: SetField {0}", field);
                            this.OnRecordModified (new EventArgs ());
                        }
                        return;
                    }
                }
                
                // Field doesn't exist, so add it
                Debug.WriteLine (5, "Field: {0} added with content: {1}", field, content);
                
                recordFields.Add (new BibtexRecordField (field, content));
                //System.Console.WriteLine ("OnFieldAdded event emitted: SetField {0}", field);
                this.OnFieldAdded (new EventArgs ());
                //System.Console.WriteLine ("Record modified event emitted: SetField {0}", field);
                this.OnRecordModified (new EventArgs ());
                
                if (field == "bibliographer_uri") {
                    //System.Console.WriteLine ("OnUriAdded event emitted: SetField {0}", field);
                    this.OnUriAdded (new EventArgs ());
                }
            }
        }

        public void RemoveField (string field)
        {
            if (recordFields != null)
                for (int i = 0; i < recordFields.Count; i++)
                    if (String.Compare (((BibtexRecordField)recordFields[i]).fieldName, field, true) == 0) {
                        Debug.WriteLine (5, "Field: {0} removed from RecordField", field);
                        recordFields.RemoveAt (i);
                        //System.Console.WriteLine ("OnFieldDeleted event emitted: RemoveField");
                        this.OnFieldDeleted (new EventArgs ());
                        //System.Console.WriteLine ("OnRecordModified event emitted: RemoveField");
                        this.OnRecordModified(new EventArgs());
                        return;
                    }
            return;
        }

        public string GetURI ()
        {
            if (!HasField ("bibliographer_uri"))
                return null;
            String uriString = GetField ("bibliographer_uri").Replace ('\n', ' ').Trim ();
            if (uriString == null || uriString == "")
                return null;
            else
                return uriString;
        }

        private void ConsumeWhitespace (TextReader stream)
        {
            do {
                switch (stream.Peek ()) {
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    stream.Read ();
                    break;
                default:
                    // next character isn't whitespace so exit
                    return;
                }
            } while (true);
        }

        private string ConsumeComment (TextReader stream)
        {
            ConsumeWhitespace (stream);
            StringBuilder result = new StringBuilder ();
            
            do {
                int next = stream.Read ();
                if (next != '\n') {
                    result.Append ((char)next);
                } else {
                    break;
                }
            } while (true);
            
            ConsumeWhitespace (stream);
            return result.ToString ();
        }

        private string ConsumeId (TextReader stream)
        {
            // munch whitespace before and after identifier
            ConsumeWhitespace (stream);
            StringBuilder result = new StringBuilder ();
            
            do {
                int next = stream.Peek ();
                if (((next >= 'a') && (next <= 'z')) || ((next >= 'A') && (next <= 'Z')) || ((next >= '0') && (next <= '9')) || (next == '_') || (next == '-') || (next == '.')) {
                    result.Append ((char)next);
                    stream.Read ();
                } else
                    break;
            } while (true);
            // TODO: do we allow blank ID's?
            //if (id == "")
            //throw new ParseException("Error parsing id token");
            ConsumeWhitespace (stream);
            return result.ToString ();
        }

        private string ConsumeUntilFieldEnd (TextReader stream)
        {
            // consumes data from the stream until
            // a comma is hit, maintaining a bracket
            // count and only terminating when a comma
            // is reached after the final bracket has
            // been closed. Alternatively, we also
            // bail if we hit a closing bracket when
            // our bracket count is 0, since the comma
            // is optional on the last field
            
            // UPDATED to also keep a double-quote count,
            // and to check for preceding \'s
            
            ConsumeWhitespace (stream);
            
            StringBuilder result = new StringBuilder ();
            
            int bracketCount = 0;
            bool usingQuotes = false;
            bool usingBrackets = false;
            bool precSlash = false;
            bool die = false;
            if (stream.Peek () == '{')
                usingBrackets = true; else if (stream.Peek () == '"')
                usingQuotes = true;
            do {
                if (bracketCount == 0) {
                    switch (stream.Peek ()) {
                    case ',':
                        stream.Read ();
                        die = true;
                        break;
                    case '}':
                        die = true;
                        break;
                    default:
                        break;
                    }
                    if (die)
                        break;
                }
                bool nextSlash = false;
                int next = stream.Read ();
                switch (next) {
                case -1:
                    throw new ParseException ("End-of-file reached before end of record");
                case '{':
                    if (usingBrackets && !precSlash)
                        bracketCount++;
                    break;
                case '}':
                    if (usingBrackets && !precSlash)
                        bracketCount--;
                    break;
                case '"':
                    if (usingQuotes && !precSlash)
                        bracketCount = (bracketCount == 0) ? 1 : 0;
                    break;
                case '\\':
                    if (!precSlash)
                        nextSlash = true;
                    break;
                case '\n':
                case '\r':
                    next = ' ';
                    break;
                default:
                    break;
                }
                precSlash = nextSlash;
                result.Append ((char)next);
            } while (true);
            string content = result.ToString ().Trim ();
            if (usingBrackets || usingQuotes)
                content = content.Substring (1, content.Length - 2);
            return content;
        }

        private void CreateFromStream (TextReader stream)
        {
            // reads in a BibTeX record from the given
            // stream, throwing an exception if there's
            // any kind of problem
            
            // tex comments in bibtex files above current record to current record
            this.comment = "";
            while (stream.Peek () == '%') {
                this.comment += ConsumeComment (stream);
                this.comment += "\n";
                ConsumeWhitespace (stream);
            }
            
            while (stream.Peek () == '#') {
                this.comment += ConsumeComment (stream);
                this.comment += "\n";
                ConsumeWhitespace (stream);
            }
            
            ConsumeWhitespace (stream);
            //if (stream.Peek() == -1)
            //    throw new ParseException("EOF");
            
            // header part of entry
            //if (stream.Read() != '@')
            //    throw new ParseException("BibTeX record does not start with @");
            //recordType = ConsumeId(stream);
            if (stream.Read () == '@') {
                recordType = ConsumeId (stream);
                
                // TODO: scan through record library and print a message if we don't
                // know what type of record this is?
                
                if (recordType.ToLower () == "comment") {
                    // Comment records
                    if (stream.Read () != '{')
                        throw new ParseException ("Expected '{' after record type '" + recordType + "'");
                    recordFields = new ArrayList ();
                    string fieldName = "comment";
                    string fieldContent = ConsumeUntilFieldEnd (stream);
                    fieldContent = fieldContent.Trim ();
                    while (fieldContent.IndexOf ("  ") > 0)
                        fieldContent = fieldContent.Replace ("  ", " ");
                    while (fieldContent.StartsWith ("{") && fieldContent.EndsWith ("}"))
                        fieldContent = fieldContent.Substring (1, fieldContent.Length - 2);
                    
                    recordFields.Add (new BibtexRecordField (fieldName, fieldContent));
                    stream.Read ();
                    ConsumeWhitespace (stream);
                } else {
                    if (stream.Read () != '{')
                        throw new ParseException ("Expected '{' after record type '" + recordType + "'");
                    recordKey = ConsumeId (stream);
                    
                    // Non-comment records
                    if (stream.Read () != ',')
                        throw new ParseException ("Expected ',' after record key '" + recordKey + "'");
                    
                    // header has been processed, so now let's process the fields
                    ConsumeWhitespace (stream);
                    recordFields = new ArrayList ();
                    while (stream.Peek () != '}') {
                        if (stream.Peek () == -1)
                            throw new ParseException ("End-of-file reached before end of record (expected '}')");
                        
                        string fieldName = ConsumeId (stream);
                        
                        if (stream.Read () != '=')
                            throw new ParseException ("Expected '=' after field '" + fieldName + "'");
                        string fieldContent = ConsumeUntilFieldEnd (stream);
                        fieldContent = fieldContent.Trim ();
                        while (fieldContent.IndexOf ("  ") > 0)
                            fieldContent = fieldContent.Replace ("  ", " ");
                        while (fieldContent.StartsWith ("{") && fieldContent.EndsWith ("}"))
                            fieldContent = fieldContent.Substring (1, fieldContent.Length - 2);
                        
                        ConsumeWhitespace (stream);
                        
                        // TODO: scan through field library and print out a message if
                        // we don't know what type of field this is?
                        
                        Debug.WriteLine (5, "Parsed field '{0}' with content '{1}'", fieldName, fieldContent);
                        recordFields.Add (new BibtexRecordField (fieldName, fieldContent));
                    }
                    stream.Read ();
                    ConsumeWhitespace (stream);
                    if (stream.Peek () == ',')
                        // absorb the tailing comma if there is one
                        stream.Read ();
                }
            } else {
                RecordType = "";
                recordKey = "";
                recordFields = new ArrayList ();
            }
        }

        public string ToBibtexString ()
        {
            StringBuilder bibtexString = new StringBuilder ();
            
            if (this.comment != null) {
                bibtexString.Append (this.comment);
            }
            if ((this.recordType != null) && (this.recordType != "") && (this.recordFields != null)) {
                bibtexString.Append ('@');
                bibtexString.Append (this.recordType);
                if (this.recordType == "comment") {
                    bibtexString.Append ('{');
                    IEnumerator iter = this.recordFields.GetEnumerator ();
                    while (iter.MoveNext ()) {
                        BibtexRecordField field = (BibtexRecordField)iter.Current;
                        bibtexString.Append (field.fieldValue);
                    }
                    bibtexString.Append ("}\n");
                } else {
                    bibtexString.Append ('{');
                    // bibtexkey
                    bibtexString.Append (this.recordKey);
                    bibtexString.Append (",\n");
                    IEnumerator iter = this.recordFields.GetEnumerator ();
                    while (iter.MoveNext ()) {
                        bibtexString.Append (((BibtexRecordField)iter.Current).ToBibtexString ());
                    }
                    bibtexString.Append ("}\n");
                }
            }
            return bibtexString.ToString ();
        }

        // Searches the record for contained text in BibtexSearchField 
        // Text is split into single words by spaces and made case insensitive
        // If the text is found, returns true, else returns false
        public bool SearchRecord (string text, BibtexSearchField sField)
        {
            ArrayList results = new ArrayList ();
            
            // pre-process search text
            text = text.ToLower ().Trim ();
            // split text into tokens
            string[] tokens = text.Split (' ');
            
            for (int j = 0; j < tokens.Length; j++) {
                string textitem = (string)tokens[j];
                bool result = false;
                
                // Checking if the record contains the search string
                for (int i = 0; i < recordFields.Count; i++) {
                    BibtexRecordField recordField = (BibtexRecordField)recordFields[i];
                    
                    if ((sField == BibtexSearchField.All) || ((recordField.fieldName.ToLower () == "author") && (sField == BibtexSearchField.Author)) || ((recordField.fieldName.ToLower () == "title") && (sField == BibtexSearchField.Title))) {
                        if ((recordField.fieldName.ToLower ().IndexOf ("bibliographer") < 0) && (recordField.fieldValue != null)) {
                            if (recordField.fieldValue.ToLower ().IndexOf (textitem) > -1)
                                result = true;
                        }
                    }
                }
                
                if (result)
                    results.Add (true);
                else
                    results.Add (false);
            }
            
            if ((results.Contains (false)) || (results.Count < 1))
                return false;
            else
                return true;
        }

        public string GetAuthorsString ()
        {
            StringArrayList authors = this.GetAuthors ();
            string authorstring = "";
            for (int i = 0; i < authors.Count; i++) {
                authorstring = authorstring + authors[i];
                if (i < authors.Count - 1) {
                    authorstring = authorstring + " and ";
                }
            }
            return authorstring;
        }

        // Method returns a list of the authors in this BibtexRecord for the side bar
        public StringArrayList GetAuthors ()
        {
            StringArrayList authors;
            authors = new StringArrayList ();
            if (recordFields != null)
                for (int i = 0; i < recordFields.Count; i++) {
                    if ((((BibtexRecordField)recordFields[i]).fieldName).ToLower () == "author") {
                        string authorString = ((BibtexRecordField)recordFields[i]).fieldValue;
                        authorString = authorString.Trim ();
                        //System.Console.WriteLine(authorString);
                        
                        string delim = " and ";
                        int j = 0;
                        int next;
                        string author;
                        while (authorString.IndexOf (delim, j) >= 0) {
                            next = authorString.IndexOf (delim, j);
                            author = authorString.Substring (j, next - j).Trim ();
                            // Check that author is in Surname, Firstname/Initials format, if not - convert it
                            if (author.IndexOf (",") < 0) {
                                if (author.LastIndexOf (' ') >= 0) {
                                    int k = author.LastIndexOf (' ');
                                    string lastname = author.Substring (k, author.Length - k);
                                    string firstname = author.Substring (0, k);
                                    author = lastname.Trim () + ", " + firstname.Trim ();
                                }
                            }
                            authors.Add (author);
                            j = next + delim.Length;
                        }
                        author = authorString.Substring (j, authorString.Length - j).Trim ();
                        // Check that author is in Surname, Firstname/Initials format, if not - convert it
                        if (author.IndexOf (",") < 0) {
                            if (author.LastIndexOf (' ') >= 0) {
                                int k = author.LastIndexOf (' ');
                                string lastname = author.Substring (k, author.Length - k);
                                string firstname = author.Substring (0, k);
                                author = lastname.Trim () + ", " + firstname.Trim ();
                            }
                        }
                        authors.Add (author);
                    }
                }
            //System.Console.WriteLine(authors.ToString());
            return authors;
        }

        // Method returns the year of this BibtexRecord for the side bar
        public string GetYear ()
        {
            string year = "";
            if (recordFields != null)
                for (int i = 0; i < recordFields.Count; i++) {
                    if ((((BibtexRecordField)recordFields[i]).fieldName).ToLower () == "year") {
                        year = ((BibtexRecordField)recordFields[i]).fieldValue.Trim ();
                    }
                }
            return year;
        }

        // Method returns the journal of this BibtexRecord for the side bar
        public string GetJournal ()
        {
            string journal = "";
            if (recordFields != null)
                for (int i = 0; i < recordFields.Count; i++) {
                    if ((((BibtexRecordField)recordFields[i]).fieldName).ToLower () == "journal") {
                        journal = ((BibtexRecordField)recordFields[i]).fieldValue.Trim ();
                    }
                }
            return journal;
        }

        public bool HasURI ()
        {
            if ((this.GetURI () != null) && (this.GetURI () != ""))
                return true;
            return false;
        }


        public bool HasURI (string uri)
        {
            if (this.GetField ("bibliographer_uri") == uri)
                return true;
            return false;
        }

        public bool HasCustomDataField (string field)
        {
            if (customData.HasField (field))
                return true;
            return false;
        }

        public object GetCustomDataField (string field)
        {
            if (customData.HasField (field)) {
                return customData.GetField (field);
            } else
                return null;
        }

        public void SetCustomDataField (string field, object data)
        {
            bool present = false;
            foreach (BibtexCustomData customDataField in customData) {
                if (customDataField.GetFieldName () == field) {
                    customDataField.SetData (data);
                    present = true;
                }
            }
            if (present == false)
                customData.Add (new BibtexCustomData (field, data));
        }
    }
}