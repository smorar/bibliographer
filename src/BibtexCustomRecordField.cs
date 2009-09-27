// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

namespace bibliographer
{
	public class BibtexCustomRecordField
	{
		public string fieldName;
		public string fieldValue;
		
		public BibtexCustomRecordField(string fieldName, string fieldValue)
		{
			this.fieldName = fieldName.ToUpper();
			this.fieldValue = fieldValue;
		}
	}
}