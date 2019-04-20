//
//  BibtexRecord.cs
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
using System.Collections;
using System.IO;
using System.Text;
using static System.Threading.Monitor;
using static libbibby.Debug;
using static libbibby.DatabaseStoreStatic;

namespace libbibby
{
    public class ParseException : Exception
    {
        private readonly string reason;

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
        public static class BibtexFieldName
        {
            public static string Author => "author";
            public static string Title => "title";
            public static string DOI => "doi";
            public static string Journal => "journal";
            public static string Volume => "volume";
            public static string Number => "number";
            public static string Pages => "pages";
            public static string Year => "year";
            public static string Month => "month";

        }

        public event EventHandler RecordModified;
        public event EventHandler FieldAdded;
        public event EventHandler FieldDeleted;
        public event EventHandler UriAdded;
        public event EventHandler UriUpdated;
        public event EventHandler DoiAdded;
        public event EventHandler DoiUpdated;

        private string comment;
        public ArrayList recordFields;

        // Custom Data - for associating data with the record
        // NB!! This data is not, and should not be serialised
        private readonly BibtexCustomDataFields customData = new BibtexCustomDataFields ();

        internal int RecordId {
            get;
            set;
        }

        public BibtexRecord (int databaseId)
        {
            RecordId = databaseId;
            SetCustomDataField ("indexData", DatabaseStoreStatic.GetSearchData (databaseId));
        }

        public BibtexRecord ()
        {
            Enter (this);
            RecordId = NewRecord ();
            Exit (this);
        }

        public int DbId ()
        {
            return RecordId;
        }

        public BibtexRecord (StreamReader stream)
        {
            RecordId = NewRecord ();

            CreateFromStream (stream);
        }

        public BibtexRecord (string s)
        {
            RecordId = NewRecord ();

            StringReader reader = new StringReader (s);
            CreateFromStream (reader);
        }

        public void SetKey (string key)
        {
            if (DatabaseStoreStatic.GetKey (RecordId) != key) {
                Enter (this);
                WriteLine (5, "Key set: {0}", key);
                DatabaseStoreStatic.SetKey (RecordId, key);
                Exit (this);

                OnRecordModified (new EventArgs ());
            }
        }

        public string GetKey ()
        {
            return DatabaseStoreStatic.GetKey (RecordId);
        }

        protected virtual void OnRecordModified (EventArgs e)
        {
            WriteLine (5, "Record Modified");
            RecordModified?.Invoke (this, e);
        }

        protected virtual void OnFieldAdded (EventArgs e)
        {
            WriteLine (5, "Field Added");
            FieldAdded?.Invoke (this, e);
        }

        protected virtual void OnFieldDeleted (EventArgs e)
        {
            WriteLine (5, "Field Deleted");
            FieldDeleted?.Invoke (this, e);
        }

        protected virtual void OnUriAdded (EventArgs e)
        {
            WriteLine (5, "Uri Added");
            UriAdded?.Invoke (this, e);
        }

        protected virtual void OnUriUpdated (EventArgs e)
        {
            WriteLine (5, "Uri Updated");
            UriUpdated?.Invoke (this, e);
        }

        protected virtual void OnDoiAdded (EventArgs e)
        {
            WriteLine (5, "Doi Added");
            DoiAdded?.Invoke (this, e);
        }

        protected virtual void OnDoiUpdated (EventArgs e)
        {
            WriteLine (5, "Doi Updated");
            DoiUpdated?.Invoke (this, e);
        }

        public string RecordType {
            get => GetRecordType (RecordId);
            set {
                Enter (this);
                SetRecordType (RecordId, value);
                Exit (this);

                OnRecordModified (new EventArgs ());
            }
        }

        public bool HasField (string field)
        {
            // TODO: need a better way of doing all this
            return field == "Bibtex Key" || DatabaseStoreStatic.HasField (RecordId, field);
        }

        public string GetField (string field)
        {
            // TODO: need a better way of doing all this
            if (field == "Bibtex Key") {
                return DatabaseStoreStatic.GetKey (RecordId);
            }

            try {
                return DatabaseStoreStatic.GetField (RecordId, field);
            } catch (NoResultException) {
                return "";
            }
        }

        public void SetField (string field, string content)
        {
            WriteLine (5, "SetField: " + field);
            string oldField = GetField (field);

            if (GetField (field) != content) {
                Enter (this);
                WriteLine (5, "Field: {0} updated with content: {1}", field, content);
                DatabaseStoreStatic.SetField (RecordId, field, content);
                Exit (this);

                if (oldField == "") {
                    OnFieldAdded (new EventArgs ());
                }

                OnRecordModified (new EventArgs ());

                if (field == BibtexFieldName.DOI) {
                    if (oldField != content && oldField != "") {
                        OnDoiUpdated (new EventArgs ());
                        return;
                    }
                    if (oldField == "" && oldField != content) {
                        OnDoiAdded (new EventArgs ());
                        return;
                    }
                }
            }
        }

        public void RemoveField (string field)
        {
            Enter (this);
            WriteLine (5, "Field: {0} removed from RecordField", field);
            DeleteField (RecordId, field);
            Exit (this);

            OnFieldDeleted (new EventArgs ());
            OnRecordModified (new EventArgs ());
            return;
        }

        public string GetURI ()
        {
            string result = GetFilename (RecordId);
            return result == "" ? null : result;
        }

        public void SetURI (string uri)
        {
            string OldURI = GetURI ();

            if (OldURI == null && uri != "") {
                SetFilename (RecordId, uri);
                OnUriAdded (new EventArgs ());
                OnRecordModified (new EventArgs ());
            }
            if (OldURI != uri && OldURI != null) {
                SetFilename (RecordId, uri);
                OnUriUpdated (new EventArgs ());
                OnRecordModified (new EventArgs ());
            }
        }

        public void SetFileAttrs (string uri, long size = 0, ulong mtime = 0, string md5sum = null)
        {
            DatabaseStoreStatic.SetFileAttrs (RecordId, uri, size, mtime, md5sum);
        }

        public string GetDOI ()
        {
            if (!HasField (BibtexFieldName.DOI)) {
                return "";
            }

            string doiString = GetField (BibtexFieldName.DOI);
            return string.IsNullOrEmpty (doiString) ? "" : doiString;
        }

        public ulong GetFileMTime ()
        {
            return DatabaseStoreStatic.GetFileMTime (RecordId);
        }

        public long GetFileSize ()
        {
            return DatabaseStoreStatic.GetFileSize (RecordId);
        }

        public string GetFileMD5Sum ()
        {
            return GetFileMd5sum (RecordId);
        }

        private static void ConsumeWhitespace (TextReader stream)
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

        private static string ConsumeComment (TextReader stream)
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

        private static string ConsumeId (TextReader stream)
        {
            // munch whitespace before and after identifier
            ConsumeWhitespace (stream);
            StringBuilder result = new StringBuilder ();

            do {
                int next = stream.Peek ();
                if (((next >= 'a') && (next <= 'z')) ||
                    ((next >= 'A') && (next <= 'Z')) ||
                    ((next >= '0') && (next <= '9')) ||
                    (next == '_') ||
                    (next == '-') ||
                    (next == '.')) {
                    result.Append ((char)next);
                    stream.Read ();
                } else {
                    break;
                }
            } while (true);
            // TODO: do we allow blank ID's?
            //if (id == "")
            //throw new ParseException("Error parsing id token");
            ConsumeWhitespace (stream);
            return result.ToString ();
        }

        private static string ConsumeUntilFieldEnd (TextReader stream)
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
            if (stream.Peek () == '{') {
                usingBrackets = true;
            } else {
                usingQuotes |= stream.Peek () == '"';
            }

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
                    }
                    if (die) {
                        break;
                    }
                }
                bool nextSlash = false;
                int next = stream.Read ();
                switch (next) {
                case -1:
                    throw new ParseException ("End-of-file reached before end of record");
                case '{':
                    if (usingBrackets && !precSlash) {
                        bracketCount++;
                    }

                    break;
                case '}':
                    if (usingBrackets && !precSlash) {
                        bracketCount--;
                    }

                    break;
                case '"':
                    if (usingQuotes && !precSlash) {
                        bracketCount = (bracketCount == 0) ? 1 : 0;
                    }

                    break;
                case '\\':
                    nextSlash |= !precSlash;
                    break;
                case '\n':
                case '\r':
                    next = ' ';
                    break;
                }
                precSlash = nextSlash;
                result.Append ((char)next);
            } while (true);
            string content = result.ToString ().Trim ();
            if (usingBrackets || usingQuotes) {
                content = content.Substring (1, content.Length - 2);
            }

            return content;
        }

        private void CreateFromStream (TextReader stream)
        {
            // reads in a BibTeX record from the given
            // stream, throwing an exception if there's
            // any kind of problem

            // tex comments in bibtex files above current record to current record
            comment = "";
            while (stream.Peek () == '%') {
                comment += ConsumeComment (stream);
                comment += "\n";
                ConsumeWhitespace (stream);
            }

            while (stream.Peek () == '#') {
                comment += ConsumeComment (stream);
                comment += "\n";
                ConsumeWhitespace (stream);
            }

            ConsumeWhitespace (stream);
            //if (stream.Peek() == -1)
            //    throw new ParseException("EOF");

            // header part of entry
            //if (stream.Read() != '@')
            //    throw new ParseException("BibTeX record does not start with @");
            if (stream.Read () == '@') {
                RecordType = ConsumeId (stream);


                // TODO: scan through record library and print a message if we don't
                // know what type of record this is?

                if (RecordType.ToLower () == "comment") {
                    // Comment records
                    if (stream.Read () != '{') {
                        throw new ParseException ("Expected '{' after record type '" + RecordType + "'");
                    }
                    //string fieldName = "comment";
                    string fieldContent = ConsumeUntilFieldEnd (stream);
                    fieldContent = fieldContent.Trim ();
                    while (fieldContent.IndexOf ("  ", StringComparison.CurrentCultureIgnoreCase) > 0) {
                        fieldContent = fieldContent.Replace ("  ", " ");
                    }

                    while (fieldContent.StartsWith ("{", StringComparison.CurrentCultureIgnoreCase) && fieldContent.EndsWith ("}", StringComparison.CurrentCultureIgnoreCase)) {
                        fieldContent = fieldContent.Substring (1, fieldContent.Length - 2);
                    }

                    DatabaseStoreStatic.SetKey (RecordId, fieldContent);
                    stream.Read ();
                    ConsumeWhitespace (stream);
                } else {
                    if (stream.Read () != '{') {
                        throw new ParseException ("Expected '{' after record type '" + RecordType + "'");
                    }

                    DatabaseStoreStatic.SetKey (RecordId, ConsumeId (stream));

                    // Non-comment records
                    if (stream.Read () != ',') {
                        throw new ParseException ("Expected ',' after record key '" + DatabaseStoreStatic.GetKey (RecordId) + "'");
                    }

                    // header has been processed, so now let's process the fields
                    ConsumeWhitespace (stream);
                    while (stream.Peek () != '}') {
                        if (stream.Peek () == -1) {
                            throw new ParseException ("End-of-file reached before end of record (expected '}')");
                        }

                        string fieldName = ConsumeId (stream);

                        if (stream.Read () != '=') {
                            throw new ParseException ("Expected '=' after field '" + fieldName + "'");
                        }

                        string fieldContent = ConsumeUntilFieldEnd (stream);
                        fieldContent = fieldContent.Trim ();
                        while (fieldContent.IndexOf ("  ", StringComparison.CurrentCultureIgnoreCase) > 0) {
                            fieldContent = fieldContent.Replace ("  ", " ");
                        }

                        while (fieldContent.StartsWith ("{", StringComparison.CurrentCultureIgnoreCase) && fieldContent.EndsWith ("}", StringComparison.CurrentCultureIgnoreCase)) {
                            fieldContent = fieldContent.Substring (1, fieldContent.Length - 2);
                        }

                        ConsumeWhitespace (stream);
                        WriteLine (5, "Parsed field '{0}' with content '{1}'", fieldName, fieldContent);
                        DatabaseStoreStatic.SetField (RecordId, fieldName, fieldContent);
                    }
                    stream.Read ();
                    ConsumeWhitespace (stream);
                    if (stream.Peek () == ',') {
                        // absorb the tailing comma if there is one
                        stream.Read ();
                    }
                }
            } else {
                RecordType = "";
                DatabaseStoreStatic.SetKey (RecordId, "");
            }
        }

        public string ToBibtexString ()
        {
            StringBuilder bibtexString = new StringBuilder ();

            if (comment != null) {
                bibtexString.Append (comment);
            }
            //if (!string.IsNullOrEmpty (RecordType) && (recordFields != null)) {
            if (!string.IsNullOrEmpty (RecordType)) {
                bibtexString.Append ('@');
                bibtexString.Append (RecordType);
                if (RecordType == "comment") {
                    bibtexString.Append ("{" + DatabaseStoreStatic.GetKey (RecordId) + "}\n");
                } else {
                    bibtexString.Append ('{');
                    // bibtexkey
                    bibtexString.Append (DatabaseStoreStatic.GetKey (RecordId));
                    bibtexString.Append (",\n");
                    //TODO: Generate bibtex field info from databasestore
                    //IEnumerator iter = recordFields.GetEnumerator ();
                    //while (iter.MoveNext ()) {
                    //    bibtexString.Append (((BibtexRecordField)iter.Current).ToBibtexString ());
                    //}
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

            if (RecordType == "comment") {
                return false;
            }
            // pre-process search text
            text = text.ToLower ().Trim ();
            // split text into tokens
            string [] tokens = text.Split (' ');

            foreach (string textitem in tokens) {
                bool result = false;
                // Checking if the record contains the search string
                foreach (object field in GetFieldNames (RecordId)) {
                    if ((sField == BibtexSearchField.All) ||
                        (((string)field == "author") && (sField == BibtexSearchField.Author)) ||
                        (((string)field == "title") && (sField == BibtexSearchField.Title))) {
                        result |= DatabaseStoreStatic.GetField (RecordId, (string)field).IndexOf (textitem, StringComparison.CurrentCultureIgnoreCase) > -1;
                    }
                }
                if (result) {
                    results.Add (true);
                } else {
                    results.Add (false);
                }
            }

            return !results.Contains (false) && (results.Count >= 1);
        }

        public string GetAuthorsString ()
        {
            StringArrayList authors = GetAuthors ();
            string authorstring = "";
            for (int i = 0; i < authors.Count; i++) {
                authorstring = authorstring + authors [i];
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
            string authorString = DatabaseStoreStatic.GetField (RecordId, BibtexFieldName.Author);
            authorString = authorString.Trim ();

            const string delim = " and ";
            int j = 0;
            int next;
            string author;
            while (authorString.IndexOf (delim, j, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                next = authorString.IndexOf (delim, j, StringComparison.CurrentCultureIgnoreCase);
                author = authorString.Substring (j, next - j).Trim ();
                // Check that author is in Surname, Firstname/Initials format, if not - convert it
                if (author.IndexOf (",", StringComparison.CurrentCultureIgnoreCase) < 0) {
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
            if (author.IndexOf (",", StringComparison.CurrentCultureIgnoreCase) < 0) {
                if (author.LastIndexOf (' ') >= 0) {
                    int k = author.LastIndexOf (' ');
                    string lastname = author.Substring (k, author.Length - k);
                    string firstname = author.Substring (0, k);
                    author = lastname.Trim () + ", " + firstname.Trim ();
                }
            }
            if (author != "") {
                authors.Add (author);
            }

            return authors;
        }

        // Method returns the year of this BibtexRecord for the side bar
        public string GetYear ()
        {
            try {
                return DatabaseStoreStatic.GetField (RecordId, BibtexFieldName.Year);
            } catch (NoResultException) {
                return "";
            }
        }

        // Method returns the journal of this BibtexRecord for the side bar
        public string GetJournal ()
        {
            try {
                return DatabaseStoreStatic.GetField (RecordId, BibtexFieldName.Journal);
            } catch (NoResultException) {
                return "";
            }
        }

        public bool HasDOI ()
        {
            return !string.IsNullOrEmpty (GetDOI ());
        }

        public bool HasDOI (string doi)
        {
            return DatabaseStoreStatic.GetField (RecordId, BibtexFieldName.DOI) == doi;
        }

        public bool HasURI ()
        {
            return !string.IsNullOrEmpty (GetFilename (RecordId));
        }

        public bool HasURI (string uri)
        {
            return GetFilename (RecordId) == uri;
        }

        public bool HasCustomDataField (string field)
        {
            return customData.HasField (field);
        }

        public object GetCustomDataField (string field)
        {
            return customData.HasField (field) ? customData.GetField (field) : null;
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
            if (!present) {
                Enter (this);

                customData.Add (new BibtexCustomData (field, data));
                Exit (this);
            }
        }

        public void RemoveCustomDataField (string field)
        {
            BibtexCustomData removeField = null;

            foreach (BibtexCustomData customDataField in customData) {
                if (customDataField.GetFieldName () == field) {
                    removeField = customDataField;
                }
            }
            if (removeField != null) {
                Enter (this);

                customData.Remove (removeField);
                Exit (this);
            }
        }

    }
}
