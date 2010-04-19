
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
        public void RecordEvents ()
        {
            bool fieldAdded, fieldDeleted, recordModified, uriAdded, uriUpdated;
            int fieldAddedCount, fieldDeletedCount, recordModifiedCount, uriAddedCount, uriUpdatedCount;

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;

            BibtexRecord record = new BibtexRecord();

            record.FieldAdded += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                fieldAdded = true;
                fieldAddedCount += 1;
            };

            record.FieldDeleted += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                fieldDeleted = true;
                fieldDeletedCount += 1;
            };

            record.RecordModified += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                recordModified = true;
                recordModifiedCount += 1;
            };

            record.UriAdded += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                uriAdded = true;
                uriAddedCount += 1;
            };

            record.UriUpdated += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                uriUpdated = true;
                uriUpdatedCount += 1;
            };

            record.SetField("test_field", "test_data");

            Assert.IsTrue(fieldAdded, "FieldAdded event is meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.AreEqual(1, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;

            record.RemoveField("test_field");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsTrue(fieldDeleted, "FieldDeleted event is meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(1, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;

            // Adding URI
            record.SetField(BibtexRecord.BibtexFieldName.URI, "file:///tmp/test");

            Assert.IsTrue(fieldAdded, "FieldAdded event is meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsTrue(uriAdded, "URIAdded event is meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.AreEqual(1, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(1, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;

            // Setting URI without a change
            record.SetField(BibtexRecord.BibtexFieldName.URI, "file:///tmp/test");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsFalse(recordModified, "RecordModified event is not meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(0, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;

            // Updating URI
            record.SetField(BibtexRecord.BibtexFieldName.URI, "file:///tmp/test1");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsTrue(uriUpdated, "URIUpdated event is meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(1, uriUpdatedCount);

        }
    }
}
