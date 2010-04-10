// Copyright 2005-2007 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using bibliographer;

namespace bibliographer
{
    public class ParseException : Exception
    {
        private string reason;
    
        public ParseException(string reason)
        {
            this.reason = reason;
        }
    
        public string GetReason()
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
        private Gdk.Pixbuf largeThumbnail = null;
        private Gdk.Pixbuf smallThumbnail = null;
    
        public event System.EventHandler RecordModified;
        public event System.EventHandler FieldAdded;
        public event System.EventHandler FieldDeleted;
        public event System.EventHandler UriAdded;
        public event System.EventHandler UriUpdated;
    
    	private string comment;
        private string recordKey;
        private ArrayList recordFields;
        private string cacheKey = "null data";
    
        public BibtexRecord(string recordType, string recordKey, ArrayList recordFields)
        {
            this.recordType = recordType;
            this.recordKey = recordKey;
            this.recordFields = recordFields;
            this.UriAdded += LookupRecordData;
            this.UriUpdated += LookupRecordData;
        }
    
        public BibtexRecord()
        {
            this.recordType = "";
            this.recordKey = "";
            this.recordFields = new ArrayList();
            this.UriAdded += LookupRecordData;
            this.UriUpdated += LookupRecordData;
        }
    
        public BibtexRecord(StreamReader stream)
        {
            CreateFromStream(stream);
            this.UriAdded += LookupRecordData;
            this.UriUpdated += LookupRecordData;
        }
    
        public BibtexRecord(string s)
        {
            StringReader reader = new StringReader(s);
            CreateFromStream(reader);
            this.UriAdded += LookupRecordData;
            this.UriUpdated += LookupRecordData;
        }
    
        public ArrayList RecordFields
        {
            get
            {
                ArrayList list = new ArrayList();
    			if (recordFields != null)
    	            for (int record = 0; record < recordFields.Count; record++)
    	                if (!((BibtexRecordField) recordFields[record]).fieldName.StartsWith("bibliographer_"))
    	                    list.Add(recordFields[record]);
                return list;
            }
        }
    
        public void SetKey(string key)
        {
            if (this.recordKey != key)
            {
                Debug.WriteLine(5, "Key set: {0}", key);
                this.recordKey = key;
                this.OnRecordModified(new EventArgs());
            }
        }
    
        public string GetKey()
        {
            return this.recordKey;
        }
    
        protected virtual void OnRecordModified(EventArgs e)
        {
            Debug.WriteLine(5, "Record Modified");
            if (RecordModified != null)
                RecordModified(this, e);
        }
    
        protected virtual void OnFieldAdded(EventArgs e)
        {
            Debug.WriteLine(5, "Field Added");
        	this.OnRecordModified(new EventArgs());
            if (FieldAdded != null)
                FieldAdded(this, e);
        }
    
        protected virtual void OnFieldDeleted(EventArgs e)
        {
            Debug.WriteLine(5, "Field Deleted");
        	this.OnRecordModified(new EventArgs());
            if (FieldDeleted != null)
                FieldDeleted(this, e);
        }
    
        protected virtual void OnUriAdded(EventArgs e)
        {
            Debug.WriteLine(5, "Uri Added");
        	this.OnRecordModified(new EventArgs());
            if (UriAdded != null)
                UriAdded(this, e);
        }
    
        protected virtual void OnUriUpdated(EventArgs e)
        {
            Debug.WriteLine(5, "Uri Updated");
        	this.OnRecordModified(new EventArgs());
            if (UriUpdated != null)
                UriUpdated(this, e);
        }
    
        public string RecordType
        {
            get
            {
                return recordType;
            }
            set
            {
                Debug.WriteLine(5, "RecordType changed from {0} to {1}", recordType, value);
                recordType = value;
                this.OnRecordModified(new EventArgs());
            }
        }
    
        public bool HasField(string field)
        {
            // TODO: need a better way of doing all this
            if (field == "Bibtex Key")
                return true;
    		if (recordFields != null)
    	        for (int i = 0; i < recordFields.Count; i++)
    	            if (String.Compare(((BibtexRecordField) recordFields[i]).fieldName, field, true) == 0)
    	                return true;
            return false;
        }
    
        public string GetField(string field)
        {
            // TODO: need a better way of doing all this
            if (field == "Bibtex Key")
                return recordKey;
    		if (recordFields != null)
    	        for (int i = 0; i < recordFields.Count; i++)
    	            if (String.Compare(((BibtexRecordField) recordFields[i]).fieldName, field, true) == 0)
    	                return ((BibtexRecordField) recordFields[i]).fieldValue;
            return null;
        }
    
        public void SetField(string field, string content)
        {
    		if (recordFields != null)
    		{
    	        for (int i = 0; i < recordFields.Count; i++)
    	            if (String.Compare(((BibtexRecordField) recordFields[i]).fieldName, field, true) == 0)
    	            {
    	                // Check if the field has _actually_ changed
    	                if (content != ((BibtexRecordField) recordFields[i]).fieldValue)
    	                {
    	                    Debug.WriteLine(5, "Field: {0} updated with content: {1}", field, content);
    	                    ((BibtexRecordField) recordFields[i]).fieldValue = content;
    	                    if (field == "bibliographer_uri")
    	                    {
    	                        this.OnUriUpdated(new EventArgs());
    	                    }
    	                    else
    	                    {
    		                    this.OnRecordModified(new EventArgs());
    	                    }
    	                }
    	                return;
    	            }
    	        Debug.WriteLine(5, "Field: {0} added with content: {1}", field, content);
    	        recordFields.Add(new BibtexRecordField(field, content));
    	        //this.OnRecordModified(new EventArgs());
    	        this.OnFieldAdded(new EventArgs());
    	        if (field == "bibliographer_uri")
    	        {
    	            this.OnUriAdded(new EventArgs());
    	        }
    		}
        }
    
        public void RemoveField(string field)
        {
    		if (recordFields != null)
    	        for (int i = 0; i < recordFields.Count; i++)
    	            if (String.Compare(((BibtexRecordField) recordFields[i]).fieldName, field, true) == 0)
    	            {
    	                Debug.WriteLine(5, "Field: {0} removed from RecordField", field);
    	                recordFields.RemoveAt(i);
    	                this.OnFieldDeleted(new EventArgs());
    	                //this.OnRecordModified(new EventArgs());
    	                return;
    	            }
            return;
        }
    
        public string GetURI()
        {
            if (!HasField("bibliographer_uri"))
                return null;
            String uriString = GetField("bibliographer_uri").Replace('\n',' ').Trim();
            if (uriString == null || uriString == "")
                return null;
            else
                return uriString;
        }
    
        DateTime lastCheck = DateTime.MinValue;
    
        public bool Altered()
        {
            String uriString = GetURI();
            String indexedUriString = GetField("bibliographer_last_uri");
            if (indexedUriString == null || indexedUriString != uriString) {
                // URI has changed, so make all existing data obsolete
                RemoveField("bibliographer_last_size");
                RemoveField("bibliographer_last_mtime");
                RemoveField("bibliographer_last_md5");
                if (uriString != null)
                {
                    Debug.WriteLine(5, "Setting bibliographer_last_uri to {0}", uriString);
                    SetField("bibliographer_last_uri", uriString);
                }
                lastCheck = DateTime.MinValue; // force a re-check
            }
            if (uriString == null || uriString == "")
                return false;
            Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri(uriString);
            TimeSpan checkInterval;
            switch (uri.Scheme)
            {
            case "http":
                // default update interval for HTTP: 30 mins
                checkInterval = new TimeSpan(0, 30, 0);
                break;
            case "file":
                // default update interval for local files: 5 minutes
                checkInterval = new TimeSpan(0, 0, 30);
                break;
            default:
                // default update interval for anything else: 5 minutes
                checkInterval = new TimeSpan(0, 5, 0);
                break;
            }
            if (DateTime.Now.Subtract(checkInterval).CompareTo(lastCheck) < 0)
                // not enough time has passed for us to check this one
                // FIXME: should probably move this out to the alteration
                // monitor queue
                return false;
            lastCheck = DateTime.Now;
            if (!uri.Exists)
            {
                Debug.WriteLine(5, "URI \"" + uriString + "\" does not seem to exist...");
                return false;
            }
            String size = GetField("bibliographer_last_size");
            if (size == null)
                size = "";
            String mtime = GetField("bibliographer_last_mtime");
            if (mtime == null)
                mtime = "";
            String md5 = GetField("bibliographer_last_md5");
            String newSize = "";
            ulong intSize = 0;
            String newMtime = "";
            try {
                Debug.WriteLine(5, "URI \"" + uriString + "\" has the following characteristics:");
                if (md5 == null)
                    md5 = "";
                else
                    Debug.WriteLine(5, "\t* md5: " + md5);
                Debug.WriteLine(5, "\t* Scheme: " + uri.Scheme);
                Gnome.Vfs.FileInfo info = uri.GetFileInfo();
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.Size) != 0) {
                    Debug.WriteLine(5, "\t* Size: " + info.Size);
                    newSize = info.Size.ToString();
                    intSize = (ulong) info.Size;
                }
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.Ctime) != 0)
                    Debug.WriteLine(5, "\t* ctime: " + info.Ctime);
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.Mtime) != 0) {
                    Debug.WriteLine(5, "\t* mtime: " + info.Mtime);
                    newMtime = info.Mtime.ToString();
                }
                if ((info.ValidFields & Gnome.Vfs.FileInfoFields.MimeType) != 0)
                    Debug.WriteLine(5, "\t* Mime type: " + info.MimeType);
            } catch (Exception e) {
                Debug.WriteLine(10, e.Message);
                Debug.WriteLine(1, "\t*** Whoops! Caught an exception!");
            }
            if ((size != newSize) || (mtime != newMtime) || (md5 == "")) {
                if (size != newSize)
                    SetField("bibliographer_last_size", newSize);
                if (mtime != newMtime)
                    SetField("bibliographer_last_mtime", newMtime);
    
                // something has changed or we don't have a MD5
                // recalculate the MD5
                Debug.WriteLine(5, "\t* Recalculating MD5...");
                Gnome.Vfs.Handle handle = Gnome.Vfs.Sync.Open(uri, Gnome.Vfs.OpenMode.Read);
                ulong sizeRead;
                byte[] contents = new byte[intSize];
                if (Gnome.Vfs.Sync.Read(handle, out contents[0], intSize, out sizeRead) != Gnome.Vfs.Result.Ok) {
                    // read failed
                    Debug.WriteLine(5, "Something weird happened trying to read data for URI \"" + uriString + "\"");
                    return false;
                }
                MD5 hasher = MD5.Create();
                byte[] newMd5Array = hasher.ComputeHash(contents);
                String newMd5 = "";
                for (int i = 0; i < newMd5Array.Length; i++) {
                    switch (newMd5Array[i] & 240) {
                    case 0: newMd5 += "0"; break;
                    case 1: newMd5 += "1"; break;
                    case 2: newMd5 += "2"; break;
                    case 3: newMd5 += "3"; break;
                    case 4: newMd5 += "4"; break;
                    case 5: newMd5 += "5"; break;
                    case 6: newMd5 += "6"; break;
                    case 7: newMd5 += "7"; break;
                    case 8: newMd5 += "8"; break;
                    case 9: newMd5 += "9"; break;
                    case 10: newMd5 += "a"; break;
                    case 11: newMd5 += "b"; break;
                    case 12: newMd5 += "c"; break;
                    case 13: newMd5 += "d"; break;
                    case 14: newMd5 += "e"; break;
                    case 15: newMd5 += "f"; break;
                    }
                    switch (newMd5Array[i] & 15) {
                    case 0: newMd5 += "0"; break;
                    case 1: newMd5 += "1"; break;
                    case 2: newMd5 += "2"; break;
                    case 3: newMd5 += "3"; break;
                    case 4: newMd5 += "4"; break;
                    case 5: newMd5 += "5"; break;
                    case 6: newMd5 += "6"; break;
                    case 7: newMd5 += "7"; break;
                    case 8: newMd5 += "8"; break;
                    case 9: newMd5 += "9"; break;
                    case 10: newMd5 += "a"; break;
                    case 11: newMd5 += "b"; break;
                    case 12: newMd5 += "c"; break;
                    case 13: newMd5 += "d"; break;
                    case 14: newMd5 += "e"; break;
                    case 15: newMd5 += "f"; break;
                    }
                }
                Debug.WriteLine(5, "\t*MD5: " + newMd5);
                if (newMd5 != md5)
                {
                    // definitely something changed
                    SetField("bibliographer_last_md5", newMd5);
                    cacheKey = uriString + "<" + newMd5 + ">";
    
                    this.smallThumbnail = null;
                    string filename;
                    if (Cache.IsCached("small_thumb", cacheKey))
                    {
                        filename = Cache.CachedFile("small_thumb", cacheKey);
                        Debug.WriteLine(5, "Got cached small thumbnail for '{0}' at location '{1}'", cacheKey, filename);
                        this.smallThumbnail = new Gdk.Pixbuf(filename);
                    }
                    else
                    {
                        this.smallThumbnail = this.GenSmallThumbnail();
                        if (this.smallThumbnail != null)
                        {
                            filename = Cache.AddToCache("small_thumb", cacheKey);
                            Debug.WriteLine(5, "Added new small thumb to cache for key '{0}'", cacheKey);
                            this.smallThumbnail.Save(filename, "png");
                        }
                    }
                    // Re-cache small thumbnail to bibtex file
                    //if (this.smallThumbnail != null)
                    //  this.SetField("bibliographer_small_thumbnail", System.Convert.ToBase64String(this.smallThumbnail.SaveToBuffer("png")));
                    //else
                    //  this.SetField("bibliographer_small_thumbnail", "");
                    this.largeThumbnail = null;
                    if (Cache.IsCached("large_thumb", cacheKey))
                    {
                        filename = Cache.CachedFile("large_thumb", cacheKey);
                        this.largeThumbnail = new Gdk.Pixbuf(filename);
                    }
                    else
                    {
                        this.largeThumbnail = this.GenLargeThumbnail();
                        if (this.largeThumbnail != null)
                        {
                            filename = Cache.AddToCache("large_thumb", cacheKey);
                            this.largeThumbnail.Save(filename, "png");
                        }
                    }
                }
                return true;
            }
            if (indexData == null)
            {
                // URI, but null index data. Force a re-index by returning true
                return true;
            }
            return false;
        }
    
        private Tri indexData = null;
    
        public void Index()
        {
            Debug.WriteLine(5, "Indexing \"" + GetURI() + "\"");
            indexData = FileIndexer.Index(GetURI());
            StreamWriter stream = new StreamWriter(new FileStream(Cache.Filename("index_data", cacheKey), FileMode.OpenOrCreate, FileAccess.Write));
            stream.WriteLine(indexData.ToString());
            stream.Close();
        }
    
        public bool IndexContains(String s)
        {
            if (s == null || s == "")
                return true;
            if (indexData == null)
                return false;
            return indexData.IsSubString(s);
        }
    
        private void ConsumeWhitespace(TextReader stream) {
            do {
                switch (stream.Peek()) {
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    stream.Read();
                    break;
                default:
                    // next character isn't whitespace so exit
                    return;
                }
            } while (true);
        }
    	
    	private string ConsumeComment(TextReader stream){
    		ConsumeWhitespace(stream);
    		StringBuilder result = new StringBuilder();
    			
            do {
                int next = stream.Read();
                if (next != '\n')
    			{
                    result.Append((char) next);
                }
                else
    			{
                    break;
    			}
            } while (true);
    		
            ConsumeWhitespace(stream);
            return result.ToString();
    	}
    
        private string ConsumeId(TextReader stream) {
            // munch whitespace before and after identifier
            ConsumeWhitespace(stream);
            StringBuilder result = new StringBuilder();
    
            do {
                int next = stream.Peek();
                if (
                    ((next >= 'a') && (next <= 'z')) ||
                    ((next >= 'A') && (next <= 'Z')) ||
                    ((next >= '0') && (next <= '9')) ||
                    (next == '_') || (next == '-') ||
                    (next == '.')
                ) {
                    result.Append((char) next);
                    stream.Read();
                }
                else
                    break;
            } while (true);
            // TODO: do we allow blank ID's?
            //if (id == "")
            //throw new ParseException("Error parsing id token");
            ConsumeWhitespace(stream);
            return result.ToString();
        }
    
        private string ConsumeUntilFieldEnd(TextReader stream) {
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
    
            ConsumeWhitespace(stream);
    
            StringBuilder result = new StringBuilder();
    
            int bracketCount = 0;
            bool usingQuotes = false;
            bool usingBrackets = false;
            bool precSlash = false;
            bool die = false;
            if (stream.Peek() == '{')
                usingBrackets = true;
            else if (stream.Peek() == '"')
                usingQuotes = true;
            do {
                if (bracketCount == 0) {
                    switch (stream.Peek()) {
                    case ',':
                        stream.Read();
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
                int next = stream.Read();
                switch (next) {
                case -1:
                    throw new ParseException("End-of-file reached before end of record");
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
                result.Append((char) next);
            } while (true);
            string content = result.ToString().Trim();
            if (usingBrackets || usingQuotes)
                content = content.Substring(1, content.Length - 2);
            return content;
        }
    
        private void CreateFromStream(TextReader stream)
        {
            // reads in a BibTeX record from the given
            // stream, throwing an exception if there's
            // any kind of problem
    
    		// tex comments in bibtex files above current record to current record
    		this.comment = "";
    		while (stream.Peek() == '%')
    		{
    			this.comment += ConsumeComment(stream);
    			this.comment += "\n";
    			ConsumeWhitespace(stream);
    		}
    		
            ConsumeWhitespace(stream);
            //if (stream.Peek() == -1)
            //    throw new ParseException("EOF");
    
            // header part of entry
            //if (stream.Read() != '@')
            //    throw new ParseException("BibTeX record does not start with @");
            //recordType = ConsumeId(stream);
            if (stream.Read() == '@')
    		{
    			recordType = ConsumeId(stream);
    
    	        // TODO: scan through record library and print a message if we don't
    	        // know what type of record this is?
    
				if (recordType.ToLower() == "comment")
				{
					// Comment records
					if (stream.Read() != '{')
	    	            throw new ParseException("Expected '{' after record type '" + recordType + "'");
	    	        recordFields = new ArrayList();
					string fieldName = "comment";
					string fieldContent = ConsumeUntilFieldEnd(stream);
					fieldContent = fieldContent.Trim();
    	            while (fieldContent.IndexOf("  ") > 0)
    	                fieldContent = fieldContent.Replace("  ", " ");
    	            while (fieldContent.StartsWith("{") && fieldContent.EndsWith("}"))
    	                fieldContent = fieldContent.Substring(1, fieldContent.Length - 2);
					
					recordFields.Add(new BibtexRecordField(fieldName, fieldContent));
					Debug.WriteLine(0, fieldContent);
					stream.Read();
					ConsumeWhitespace(stream);
				}
				else
				{
	    	        if (stream.Read() != '{')
	    	            throw new ParseException("Expected '{' after record type '" + recordType + "'");
	    	        recordKey = ConsumeId(stream);
    
					// Non-comment records
	    	        if (stream.Read() != ',')
	    	            throw new ParseException("Expected ',' after record key '" + recordKey + "'");
					
    
	    	        // header has been processed, so now let's process the fields
	    	        ConsumeWhitespace(stream);
	    	        recordFields = new ArrayList();
	    	        while (stream.Peek() != '}') {
	    	            if (stream.Peek() == -1)
	    	                throw new ParseException("End-of-file reached before end of record (expected '}')");
	    
	    	            string fieldName = ConsumeId(stream);
	    
	    	            if (stream.Read() != '=')
	    	                throw new ParseException("Expected '=' after field '" + fieldName + "'");
	    	            string fieldContent = ConsumeUntilFieldEnd(stream);
	    	            fieldContent = fieldContent.Trim();
	    	            while (fieldContent.IndexOf("  ") > 0)
	    	                fieldContent = fieldContent.Replace("  ", " ");
	    	            while (fieldContent.StartsWith("{") && fieldContent.EndsWith("}"))
	    	                fieldContent = fieldContent.Substring(1, fieldContent.Length - 2);
	    
	    	            ConsumeWhitespace(stream);
	    
	    	            // TODO: scan through field library and print out a message if
	    	            // we don't know what type of field this is?
	    
	    	            Debug.WriteLine(5, "Parsed field '{0}' with content '{1}'", fieldName, fieldContent);
	    	            recordFields.Add(new BibtexRecordField(fieldName, fieldContent));
	    	        }
	    	        stream.Read();
	    	        ConsumeWhitespace(stream);
	    	        if (stream.Peek() == ',')  // absorb the tailing comma if there is one
	    	            stream.Read();
	    	        if (HasField("bibliographer_last_uri") && HasField("bibliographer_last_md5"))
	    	            cacheKey = GetField("bibliographer_last_uri") + "<" + GetField("bibliographer_last_md5") + ">";
	    	        else
	    	            cacheKey = "null data";
	    	        if (Cache.IsCached("index_data", cacheKey))
	    	        {
						try
						{
		    	            StreamReader istream = new StreamReader(new FileStream(Cache.CachedFile("index_data", cacheKey), FileMode.Open, FileAccess.Read));
		    	            indexData = new Tri(istream.ReadToEnd());
		    	            istream.Close();
						}
						catch (System.Exception e)
						{
							Debug.WriteLine(0, "Unknown exception while indexing file {0} for record {1}", this.GetURI(), this.recordKey);
							Debug.WriteLine(0, e.Message);
							Debug.WriteLine(1, e.StackTrace);
						}
	    	        }
				}
    		}
    		else
    		{
    			RecordType = "";
    			recordKey = "";
    			recordFields = new ArrayList();
    		}
        }
    
        public string ToBibtexString()
        {
            StringBuilder bibtexString = new StringBuilder();
    		
    		if (this.comment != null)
    		{
    			bibtexString.Append(this.comment);
    		}
    		if ((this.recordType != null) && (this.recordType != "") &&
    			    (this.recordFields != null))
    		{
    	        bibtexString.Append('@');
    	        bibtexString.Append(this.recordType);
				if (this.recordType == "comment")
				{
					bibtexString.Append('{');
	    	        IEnumerator iter = this.recordFields.GetEnumerator();
	    	        while(iter.MoveNext())
	    	        {
						BibtexRecordField field = (BibtexRecordField) iter.Current;
						bibtexString.Append(field.fieldValue);
	    	        }
	    	        bibtexString.Append("}\n");
				}
				else
				{
	    	        bibtexString.Append('{');
	    	        // bibtexkey
	    	        bibtexString.Append(this.recordKey);
	    	        bibtexString.Append(",\n");
	    	        IEnumerator iter = this.recordFields.GetEnumerator();
	    	        while(iter.MoveNext())
	    	        {
	    	            bibtexString.Append(((BibtexRecordField)iter.Current).ToBibtexString());
	    	        }
	    	        bibtexString.Append("}\n");
				}
    		}
            return bibtexString.ToString();
        }
    
        // Searches the record for contained text in BibtexSearchField 
        // Text is split into single words by spaces and made case insensitive
        // If the text is found, returns true, else returns false
        public bool SearchRecord(string text, BibtexSearchField sField)
        {
            ArrayList results = new ArrayList();
            
            // pre-process search text
            text = text.ToLower().Trim();
            // split text into tokens
            string[] tokens = text.Split(' ');

            for (int j = 0; j < tokens.Length; j++)
            {
                string textitem = (string) tokens[j];
                bool result = false;
                
                // Checking if the record contains the search string
                for (int i = 0; i < recordFields.Count; i++)
                {
                    BibtexRecordField recordField = (BibtexRecordField) recordFields[i];
                    
                    if (
                        (sField == BibtexSearchField.All) ||
                        ((recordField.fieldName.ToLower() == "author") && (sField == BibtexSearchField.Author)) ||
                        ((recordField.fieldName.ToLower() == "title") && (sField == BibtexSearchField.Title))
                        )
                    {
                        if ((recordField.fieldName.ToLower().IndexOf("bibliographer") < 0) && (recordField.fieldValue != null))
                        {
                            if (recordField.fieldValue.ToLower().IndexOf(textitem) > -1)
                                result = true;
                            else if (sField == BibtexSearchField.All)
                                if (this.IndexContains(textitem))
                                    result = true;
                        }
                    }
                    else if (sField == BibtexSearchField.Article)
                    {
                        if (this.IndexContains(textitem))
                            result = true;
                    }
                }

                if (result)
                    results.Add(true);
                else
                    results.Add(false);
            }
            
            if ((results.Contains(false)) || (results.Count < 1))
                return false;
            else
                return true;
        }
    
        public string GetAuthorsString()
        {
            StringArrayList authors = this.GetAuthors();
            string authorstring = "";
            for (int i = 0; i < authors.Count; i++)
            {
                authorstring = authorstring + authors[i];
                if (i < authors.Count - 1)
                {
                    authorstring = authorstring + " and ";
                }
            }
            return authorstring;
        }
    
        // Method returns a list of the authors in this BibtexRecord for the side bar
        public StringArrayList GetAuthors()
        {
            StringArrayList authors;
            authors = new StringArrayList();
    		if (recordFields != null)
    	        for (int i = 0; i < recordFields.Count; i++)
    	        {
    	            if ((((BibtexRecordField) recordFields[i]).fieldName).ToLower() == "author")
    	            {
    	                string authorString = ((BibtexRecordField) recordFields[i]).fieldValue;
    	                authorString = authorString.Trim();
    	                //System.Console.WriteLine(authorString);
    
    	                string delim = " and ";
    	                int j = 0;
    	                int next;
    	                string author;
    	                while (authorString.IndexOf(delim, j) >= 0)
    	                {
    	                    next = authorString.IndexOf(delim, j);
    	                    author = authorString.Substring(j, next - j).Trim();
    	                    // Check that author is in Surname, Firstname/Initials format, if not - convert it
    	                    if (author.IndexOf(",") < 0)
    	                    {
    	                        if (author.LastIndexOf(' ') >= 0)
    	                        {
    	                            int k = author.LastIndexOf(' ');
    	                            string lastname =  author.Substring(k,author.Length - k);
    	                            string firstname = author.Substring(0,k);
    	                            author = lastname.Trim() + ", " + firstname.Trim();
    	                        }
    	                    }
    	                    authors.Add(author);
    	                    j = next + delim.Length;
    	                }
    	                author = authorString.Substring(j, authorString.Length - j).Trim();
    	                // Check that author is in Surname, Firstname/Initials format, if not - convert it
    	                if (author.IndexOf(",") < 0)
    	                {
    	                    if (author.LastIndexOf(' ')>=0)
    	                    {
    	                        int k = author.LastIndexOf(' ');
    	                        string lastname =  author.Substring(k,author.Length - k);
    	                        string firstname = author.Substring(0,k);
    	                        author = lastname.Trim() + ", " + firstname.Trim();
    	                    }
    	                }
    	                authors.Add(author);
    	            }
    	        }
            //System.Console.WriteLine(authors.ToString());
            return authors;
        }
    
        // Method returns the year of this BibtexRecord for the side bar
        public string GetYear()
        {
            string year = "";
    		if (recordFields != null)
    	        for (int i = 0; i < recordFields.Count; i++)
    	        {
    	            if ((((BibtexRecordField) recordFields[i]).fieldName).ToLower() == "year")
    	            {
    	                year = ((BibtexRecordField) recordFields[i]).fieldValue.Trim();
    	            }
    	        }
            return year;
        }
    
        // Method returns the journal of this BibtexRecord for the side bar
        public string GetJournal()
        {
            string journal = "";
    		if (recordFields != null)
    	        for (int i = 0; i < recordFields.Count; i++)
    	        {
    	            if ((((BibtexRecordField) recordFields[i]).fieldName).ToLower() == "journal")
    	            {
    	                journal = ((BibtexRecordField) recordFields[i]).fieldValue.Trim();
    	            }
    	        }
            return journal;
        }
    
    	public bool HasURI()
    	{
    		if ((this.GetURI() != null) && (this.GetURI() != ""))
    			return true;
    		return false;
    	}
    		
        public Gdk.Pixbuf GetSmallThumbnail()
        {
            Debug.Write(5, "getSmallThumbnail: ");
            string uriString = this.GetURI();
            
            if ((this.smallThumbnail == null) && (this.HasURI()))
            {
                if (Cache.IsCached("small_thumb", cacheKey))
                {
                    try
                    {
                        this.smallThumbnail = new Gdk.Pixbuf(Cache.CachedFile("small_thumb", cacheKey));
                        Debug.WriteLine(5, "Retrieved small thumb for key '{0}'", cacheKey);
                        return this.smallThumbnail;
                    }
                    catch (Exception)
                    {
                        // probably a corrupt cache file
                        // delete it and try again :-)
                        Cache.RemoveFromCache("large_thumb", cacheKey);
                        return this.largeThumbnail;
                    }
                }
                else
                {
                    Debug.WriteLine(5, "not cached... let's go!");
    
                    Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri(uriString);
                    if (!uri.Exists) {
                        // file doesn't exist
                        // FIXME: set an error thumbnail or some such
                        System.Console.WriteLine(uriString);
                        Debug.WriteLine(5, "Non-existent URI");
                        this.smallThumbnail = (new Gdk.Pixbuf(null, "error.png")).ScaleSimple(20, 20, Gdk.InterpType.Bilinear);
                        return this.smallThumbnail;
                    }
                    this.smallThumbnail = this.GenSmallThumbnail();
    
                    if (this.smallThumbnail != null)
                    {
                        string filename = Cache.AddToCache("small_thumb", cacheKey);
                        Debug.WriteLine(5, "Small thumbnail added to cache for key '{0}'", cacheKey);
                        this.smallThumbnail.Save(filename, "png");
                    }
                    else
                        Debug.WriteLine(5, "genSmallThumbnail returned null :-(");
                }
            }
            else if ((this.smallThumbnail == null) && (!this.HasURI()))
            {
                Debug.WriteLine(5, "No URI, generating transparent thumbnail");
                // Generate a transparent pixbuf for 
                Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 20, 20);
                pixbuf.Fill(0);
                pixbuf.AddAlpha(true,0,0,0);
                this.smallThumbnail = pixbuf;
            }
            
            return this.smallThumbnail;
        }
    
        private Gdk.Pixbuf GenSmallThumbnail()
        {
            RunOnMainThread.Run(this, "DoGenSmallThumbnail", null);
            return smallThumbnail;
        }
    
        public void DoGenSmallThumbnail()
        {
            string uriString = this.GetURI();
    		
    		while (Gtk.Application.EventsPending ())
    			Gtk.Application.RunIteration ();
            
            // No URI, so just exit
            if (!(uriString == null || uriString == ""))
            {
                // Thumbnail not cached, generate and then cache :)
                Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri(uriString);
                Gnome.Vfs.MimeType mimeType = new Gnome.Vfs.MimeType(uri);
                Gnome.ThumbnailFactory thumbFactory = new Gnome.ThumbnailFactory(Gnome.ThumbnailSize.Normal);
                if (thumbFactory.CanThumbnail(uriString, mimeType.Name, System.DateTime.Now))
                {
                    //System.Console.WriteLine("Generating a thumbnail");
                    this.smallThumbnail =  thumbFactory.GenerateThumbnail(uriString, mimeType.Name);
                    if (this.smallThumbnail != null)
                    {
                        Debug.WriteLine(5, "Done thumbnail for '{0}'", uriString);
                        this.smallThumbnail = this.smallThumbnail.ScaleSimple(this.smallThumbnail.Width*20/this.smallThumbnail.Height, 20, Gdk.InterpType.Bilinear);
                        string filename;
                        if (Cache.IsCached("small_thumb", cacheKey))
                            filename = Cache.CachedFile("small_thumb", cacheKey);
                        else
                        {
                            filename = Cache.AddToCache("small_thumb", cacheKey);
                            Debug.WriteLine(5, "doGenSmallThumbnail adding thumbnail with key {0} to cache", cacheKey);
                        }
                        this.smallThumbnail.Save(filename, "png");
                    }
                }
                if (this.smallThumbnail == null)
                {
                    // try to get the default icon for the file's mime type
                    Gtk.IconTheme theme = Gtk.IconTheme.Default;
                    Gnome.IconLookupResultFlags result;
                    string iconName = Gnome.Icon.Lookup(
                        theme,
                        null,
                        null,
                        null,
                        new Gnome.Vfs.FileInfo (IntPtr.Zero),
                        mimeType.Name,
                        Gnome.IconLookupFlags.None,
                        out result);
                    Debug.WriteLine(5, "Gnome.Icon.Lookup result: {0}", result);
                    if (iconName == null) {
                        iconName = "gnome-fs-regular";
                    }
                    Debug.WriteLine(5, "IconName is: {0}", iconName);
                    Gtk.IconInfo iconInfo = theme.LookupIcon(iconName, 24, Gtk.IconLookupFlags.UseBuiltin);
                    string iconPath = iconInfo.Filename;
                    if (iconPath != null) {
                        Debug.WriteLine(5, "IconPath: {0}", iconPath);
                        this.smallThumbnail = new Gdk.Pixbuf(iconPath);
                        if (this.smallThumbnail != null)
                            this.smallThumbnail = this.smallThumbnail.ScaleSimple(this.smallThumbnail.Width*20/this.smallThumbnail.Height, 20, Gdk.InterpType.Bilinear);
                    }
                    else {
                        // just go blank
                        this.smallThumbnail = null;
                    }
                }
            }
            else
            {
                smallThumbnail = null;
            }
        }
    
        public Gdk.Pixbuf GetLargeThumbnail()
        {
            string uriString = this.GetURI();
            
            if ((this.largeThumbnail == null) && (this.HasURI()))
            {
                if (Cache.IsCached("large_thumb", cacheKey))
                {
                    try
                    {
                        this.largeThumbnail = new Gdk.Pixbuf(Cache.CachedFile("large_thumb", cacheKey));
                        Debug.WriteLine(5, "Retrieved large thumb for key '{0}'", cacheKey);
                        return this.largeThumbnail;
                    }
                    catch (Exception)
                    {
                        // probably a corrupt cache file
                        // delete it and try again :-)
                        Cache.RemoveFromCache("large_thumb", cacheKey);
                        return this.largeThumbnail;
                    }
                }
                else
                {
                    Debug.WriteLine(5, "not cached... let's go!");
                    
                    Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri(uriString);
                    if (!uri.Exists) {
                        // file doesn't exist
                        // FIXME: set an error thumbnail or some such
                        System.Console.WriteLine(uriString);
                        Debug.WriteLine(5, "Non-existent URI");
                        
                        this.largeThumbnail = (new Gdk.Pixbuf(null, "error.png")).ScaleSimple(96, 128, Gdk.InterpType.Bilinear);
                        return this.largeThumbnail;
                    }
                    this.largeThumbnail = this.GenLargeThumbnail();
                    
                    if (this.largeThumbnail != null)
                    {
                        string filename = Cache.AddToCache("large_thumb", cacheKey);
                        Debug.WriteLine(5, "Large thumbnail added to cache for key '{0}'", cacheKey);
                        this.largeThumbnail.Save(filename, "png");
                    }
                    else
                        Debug.WriteLine(5, "genLargeThumbnail returned null :-(");
                }
            }
            else if ((this.largeThumbnail == null) && (!this.HasURI()))
            {
                Debug.WriteLine(5, "No URI, generating transparent thumbnail");
                // Generate a transparent pixbuf for 
                Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 96, 128);
                pixbuf.Fill(0);
                pixbuf.AddAlpha(true,0,0,0);
                this.largeThumbnail = pixbuf;
            }
            return this.largeThumbnail;
        }
    
        private Gdk.Pixbuf GenLargeThumbnail()
        {
            RunOnMainThread.Run(this, "DoGenLargeThumbnail", null);
            return largeThumbnail;
        }
    
        public void DoGenLargeThumbnail()
        {
            string uriString = this.GetURI();
            
    		while (Gtk.Application.EventsPending ())
    			Gtk.Application.RunIteration ();
    
            // No URI, so just exit
            if (!(uriString == null || uriString == ""))
            {
                // Thumbnail not cached, generate and then cache :)
                Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri(uriString);
                Gnome.Vfs.MimeType mimeType = new Gnome.Vfs.MimeType(uri);
                Gnome.ThumbnailFactory thumbFactory = new Gnome.ThumbnailFactory(Gnome.ThumbnailSize.Normal);
    
                if (thumbFactory.CanThumbnail(uriString, mimeType.Name, System.DateTime.Now))
                {
                    //	    System.Console.WriteLine("Generating a thumbnail");
                    this.largeThumbnail =  thumbFactory.GenerateThumbnail(uriString, mimeType.Name);
    		if (this.largeThumbnail == null)
    			return;
                    string filename;
                    if (Cache.IsCached("large_thumb", cacheKey))
                        filename = Cache.CachedFile("large_thumb", cacheKey);
                    else
                    {
                        filename = Cache.AddToCache("large_thumb", cacheKey);
                        Debug.WriteLine(5, "Adding large thumbnail to cache for key {0}", cacheKey);
                    }
                    this.largeThumbnail.Save(filename, "png");
                }
                else
                {
                    // try to get the default icon for the file's mime type
                    Gtk.IconTheme theme = Gtk.IconTheme.Default;
                    Gnome.IconLookupResultFlags result;
                    String iconName = Gnome.Icon.Lookup(
                        theme,
                        null,
                        null,
                        null,
                        new Gnome.Vfs.FileInfo(),
                        mimeType.Name,
                        Gnome.IconLookupFlags.None,
                        out result);
                    Debug.WriteLine(5, "Gnome.Icon.Lookup result: {0}", result);
                    if (iconName == null) {
                        iconName = "gnome-fs-regular";
                    }
                    Debug.WriteLine(5, "IconName is: {0}", iconName);
                    Gtk.IconInfo iconInfo = theme.LookupIcon(iconName, 48, Gtk.IconLookupFlags.UseBuiltin);
                    string iconPath = iconInfo.Filename;
                    if (iconPath != null) {
                        Debug.WriteLine(5, "IconPath: {0}", iconPath);
                        this.largeThumbnail = new Gdk.Pixbuf(iconPath);
                    }
                    else {
                        // just go blank
                        this.largeThumbnail = null;
                    }
                }
            }
            else
            {
                largeThumbnail = null;
            }
        }
    
        public void LookupRecordData(object o, EventArgs e)
        {
            string URI = this.GetField("bibliographer_uri");
    
            Debug.WriteLine(5, "Uri: {0} added to record", URI);
            // Determine doi number from the uri, and lookup info.
            StringArrayList textualData = FileIndexer.GetTextualData(URI);
            if (textualData != null)
            {
                for (int line = 0; line < textualData.Count; line++) {
                    String data = ((String) textualData[line]).ToLower();
                    if (data.IndexOf("doi:")>0)
                    {
                        int idx1 = data.IndexOf("doi:");
                        data = data.Substring(idx1);
                        data = data.Trim();
                        // If there are additional characters on this line, find a space character and chop them off
                        if (data.IndexOf(' ')>0)
                        {
                            int idx2 = data.IndexOf(' ');
                            data = data.Substring(0, data.Length - idx2);
                        }
                        // Strip out "doi:"
                        data = data.Remove(0,4);
                        Debug.WriteLine(5, "Found doi:{0}", data);
                        this.SetField("bibliographer_doi", data);
    
                        // Start a thread to look up the record's data, so as to not lockup the interface
                        // if the request takes a while, or times out due to no network connectivity.
                        // TODO: Use a threadpool here - we can do some of these simultaneously.
    					RunOnMainThread.Run(this, "LookupData", null);
                        //System.Threading.Thread t = new System.Threading.Thread(this.LookupData);
                        //t.Start();
                    }
                }
            }
        }
    
        private void LookupData()
        {
            // Call this method in a thread, as it will lock up the application until a HttpWebRequest is completed
            string url = "http://www.crossref.org/openurl/?id=doi:"+this.GetField("bibliographer_doi")+"&noredirect=true";
            Debug.WriteLine(5, "Looking up data for {0} from {1}", this.GetField("bibliographer_doi"), url);
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            try {
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                
                try {
                    System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(resStream);
                    reader.MoveToContent();
                    
                    while(reader.Read())
                    {
                        //Debug.WriteLine(2, "{0}: {1}", reader.Name, reader.Value);
                        if (reader.Name.ToLower() == "doi")
                        {
                            if (reader.GetAttribute("type") == "journal_article")
                            {
                                Debug.WriteLine(5, "Setting RecordType: article");
                                this.RecordType = "article";
                            }
                        }
                        if (reader.Name.ToLower() == "journal_title")
                        {
                            reader.Read();
                            if (this.HasField("journal") == false)
                            {
                                Debug.WriteLine(5, "setting journal: {0}", reader.Value);
                                this.SetField("journal", reader.Value);
                            }
                        }
                        if (reader.Name.ToLower() == "contributors")
                        {
                        	Debug.WriteLine(5, "found contributors");
                        	while(reader.Read())
                        	{
                        		reader.Read();
                        		if (reader.Name.ToLower() == "contributor")
                        		{
                        			Debug.WriteLine(5, "found contributor");
                        			string surname = "";
                        			string given_name = "";
                        			
                        			while(reader.Read())
                        			{
                        				int counter = 0;
                        				reader.MoveToContent();
    	                    			if (reader.Name.ToLower() == "given_name" || reader.Name.ToLower() == "surname")
    	                    			{
    	                    				if (reader.Name.ToLower() == "surname")
    	                    				{
    	                    					Debug.WriteLine(5, "found surname");
    	                    					reader.Read();
    	                    					if (surname.Length == 0)
    		                    					surname = reader.Value;
    	                    				}
    	                    				if (reader.Name.ToLower() == "given_name")
    	                    				{
    	                    					Debug.WriteLine(5, "found given_name");
    	                    					reader.Read();
    	                    					if (given_name.Length == 0)
    		                    					given_name = reader.Value;
    	                    				}
    	                    			}
    	                    			else
    			                    		break;
    		                    		counter += 1;
    	                    		}
    	                    		if (this.HasField("author") == false)
    	                    		{
    	                    			if (surname != "")
    	                    			{
    	                    				// sort out case of surname
    	                    				surname = String.Concat(surname.Substring(0,1).ToUpper(), surname.Substring(1).ToLower());
    	                    				if (given_name != "")
    	                    				{
    	                    					// sort out case of firstname
    		                    				given_name = String.Concat(given_name.Substring(0,1).ToUpper(), given_name.Substring(1).ToLower());
    		                    				this.SetField("author", String.Concat(surname, ", ", given_name));
    		                    			}
    		                    			else
    		                    			{
    		                    				this.SetField("author", surname);
    		                    			}
    	                    			}
    	                    		}
    	                    		else
    	                    		{
    	                    			if (surname != "")
    	                    			{
    	                    				if (given_name != "")
    	                    				{
    			                    			this.SetField("author", String.Concat(this.GetField("author"), " and ", surname, ", ", given_name));
    			                    		}
    			                    		else
    			                    		{
    			                    			this.SetField("author", String.Concat(this.GetField("author"), " and ", surname));
    			                    		}
    	                    			}
    	                    		}
                        		}
                        		else
                        			break;
                        	}
                        }
                        if (reader.Name.ToLower() == "volume")
                        {
                            reader.Read();
                            if (this.HasField("volume") == false)
                            {
                                Debug.WriteLine(5, "setting volume: {0}", reader.Value);
                                this.SetField("volume", reader.Value);
                            }
                        }
                        if (reader.Name.ToLower() == "issue")
                        {
                            reader.Read();
                            if (this.HasField("number") == false)
                            {
                                Debug.WriteLine(5, "setting number: {0}", reader.Value);
                                this.SetField("number", reader.Value);
                            }
                        }
                        if (reader.Name.ToLower() == "first_page")
                        {
                            reader.Read();
                            if (this.HasField("pages") == false)
                            {
                                Debug.WriteLine(5, "setting pages: {0}", reader.Value);
                                this.SetField("pages", reader.Value);
                            }
                        }
                        if (reader.Name.ToLower() == "year")
                        {
                            reader.Read();
                            if (this.HasField("year") == false)
                            {
                                Debug.WriteLine(5, "setting year: {0}", reader.Value);
                                this.SetField("year", reader.Value);
                            }
                        }
                        if (reader.Name.ToLower() == "article_title")
                        {
                            reader.Read();
                            if (this.HasField("title") == false)
                            {
                                Debug.WriteLine(5, "setting title: {0}", reader.Value);
                                this.SetField("title", reader.Value);
                            }
                        }
                    }
    /*                if ((this.GetAuthors() != "" or this.GetAuthors() != null) && (this.GetYear() != "" or this.GetYear() != null))
                    {
                    	//TODO: Generate key here
                    	
                    }
     */               
                }
                catch (System.Xml.XmlException e)
                {
                    Debug.WriteLine(2, e.Message);
                }
            }
            catch (System.Net.WebException e)
            {
                Debug.WriteLine(2, e.Message);
            }
        }
        public bool HasURI(string uri)
        {
            if (this.GetField("bibliographer_uri") == uri)
                return true;
            return false;
        }
    }
}
