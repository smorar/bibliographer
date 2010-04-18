
using System;
using NUnit.Framework;

namespace libbibby
{
    [TestFixture()]
    public class BibtexRecordTest
    {

        [Test()]
        public void EmptyRecord ()
        {
            BibtexRecord record = new BibtexRecord();

            Assert.IsInstanceOfType(typeof(libbibby.BibtexRecord), record);

            Assert.AreEqual(record.RecordType, "", "Empty record's RecordType is an empty string");
            Assert.AreEqual(record.GetKey(),"", "Empty record's Key is an empty string");
            Assert.AreEqual(record.GetAuthors(), new StringArrayList(), "Empty record's Authors array is an empty StringArrayList");
            Assert.AreEqual(record.GetAuthorsString(), "", "Empty record's Authors string is an empty string");
            Assert.AreEqual(record.GetJournal(), "", "Empty record's Journal is an empty string");
            Assert.AreEqual(record.GetYear(), "", "Empty record's Year is an empty string");
            Assert.IsNull(record.GetURI(), "Empty record's URI is null");
            Assert.IsFalse(record.HasURI(), "Empty record does not have an URI");
        }

        [Test()]
        public void RecordCompare ()
        {
            BibtexRecord record1 = new BibtexRecord();
            BibtexRecord record2 = new BibtexRecord();

            Assert.AreNotSame(record1, record2, "Two record instances are not the same");

            record1.SetKey("testKey");
            record2.SetKey("testKey");

            Assert.AreNotSame(record1, record2, "Two record instances are not the same when they have the same key");
        }

        [Test()]
        public void RecordFieldEvents ()
        {
            bool fieldAddedRaised, fieldDeletedRaised, recordModified, uriAdded, uriUpdated;

            fieldAddedRaised = false;
            fieldDeletedRaised = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;

            BibtexRecord record = new BibtexRecord();

            record.FieldAdded += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                fieldAddedRaised = true;
            };

            record.FieldDeleted += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                fieldDeletedRaised = true;
            };

            record.RecordModified += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                recordModified = true;
            };

            record.UriAdded += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                uriAdded = true;
            };

            record.UriUpdated += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                uriUpdated = true;
            };

            record.SetField("test_field", "test_data");

            Assert.IsTrue(fieldAddedRaised, "FieldAdded event is raised");
            Assert.IsTrue(recordModified, "RecordModified event is raised");
            Assert.IsFalse(fieldDeletedRaised, "FieldDeleted event is not raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not raised");

            fieldAddedRaised = false;
            fieldDeletedRaised = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;

            record.RemoveField("test_field");

            Assert.IsTrue(fieldDeletedRaised, "FieldDeleted event is raised");
            Assert.IsTrue(recordModified, "RecordModified event is raised");
            Assert.IsFalse(fieldAddedRaised, "FieldAdded event is not raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not raised");

            fieldAddedRaised = false;
            fieldDeletedRaised = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;

            // Adding URI
            record.SetField(BibtexRecord.BibtexFieldName.URI, "file:///tmp/test");

            Assert.IsTrue(fieldAddedRaised, "FieldAdded event is raised");
            Assert.IsTrue(recordModified, "RecordModified event is raised");
            Assert.IsFalse(fieldDeletedRaised, "FieldDeleted event is not raised");
            Assert.IsTrue(uriAdded, "URIAdded event is raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not raised");

            fieldAddedRaised = false;
            fieldDeletedRaised = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;

            // Setting URI without a change
            record.SetField(BibtexRecord.BibtexFieldName.URI, "file:///tmp/test");

            Assert.IsFalse(fieldAddedRaised, "FieldAdded event is not raised");
            Assert.IsFalse(recordModified, "RecordModified event is not raised");
            Assert.IsFalse(fieldDeletedRaised, "FieldDeleted event is not raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not raised");

            fieldAddedRaised = false;
            fieldDeletedRaised = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;

            // Updating URI
            record.SetField(BibtexRecord.BibtexFieldName.URI, "file:///tmp/test1");

            Assert.IsFalse(fieldAddedRaised, "FieldAdded event is not raised");
            Assert.IsTrue(recordModified, "RecordModified event is raised");
            Assert.IsFalse(fieldDeletedRaised, "FieldDeleted event is not raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not raised");
            Assert.IsTrue(uriUpdated, "URIUpdated event is raised");

        }
    }
}
