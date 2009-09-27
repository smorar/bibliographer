// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System.Text;

namespace bibliographer
{
  public class BibtexRecordField
  {
    public string fieldName;
    public string fieldValue;
    
    public BibtexRecordField(string fieldName, string fieldValue)
    {
      this.fieldName = fieldName;
      this.fieldValue = fieldValue;
    }
    
    public string ToBibtexString()
    {
      StringBuilder fieldString = new StringBuilder();
      
      if (!(this.fieldValue == "" || this.fieldValue == null))
      {
	fieldString.Append('\t');
	fieldString.Append(this.fieldName);
	fieldString.Append(" = {");
	if (this.fieldValue.Length > 4000) {
	  // split the field over several lines to make this file
	  // work happily with Bibtex. The choice of 4000 is fairly
	  // arbitrary; Bibtex's limit is 5000 so we just stay a bit
	  // below the radar :-)
	  int lineCount = this.fieldValue.Length / 4000 + ((this.fieldValue.Length % 4000) == 0 ? 0 : 1);
	  fieldString.Append("\n");
	  for (int i = 0; i < lineCount; i++) {
	    fieldString.Append("\t\t");
	    if ((i + 1) < lineCount)
	      fieldString.Append(this.fieldValue.Substring(i * 4000, 4000));
	    else
	      fieldString.Append(this.fieldValue.Substring(i * 4000));
	    fieldString.Append("\n");
	  }
	  fieldString.Append("\t");
	}
	else
	  fieldString.Append(this.fieldValue);
	fieldString.Append("},\n");
      }
      return fieldString.ToString();
    }
  }
}
