// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information


namespace libbibby
{

    public class BibtexCustomData
    {
        private string fieldName;
        private object fieldData;

        public BibtexCustomData (string fieldName, object fieldValue)
        {
            this.fieldName = fieldName;
            this.fieldData = fieldValue;
        }

        public string GetFieldName ()
        {
            return this.fieldName;
        }

        public object GetData ()
        {
            return this.fieldData;
        }

        public void SetData (object data)
        {
            this.fieldData = data;
        }
    }
}
