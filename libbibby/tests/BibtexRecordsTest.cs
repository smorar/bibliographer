
using System;
using NUnit.Framework;

namespace libbibby
{


    [TestFixture()]
    public class BibtexRecordsTest
    {

        [Test()]
        public void EmptyRecords ()
        {
            BibtexRecords records = new BibtexRecords();

            Assert.IsInstanceOfType(typeof(BibtexRecords), records);
            Assert.AreEqual(records.Count, 0);
        }

        [Test()]
        public void RecordsCompare()
        {
            BibtexRecords records1 = new BibtexRecords();
            BibtexRecords records2 = new BibtexRecords();

            Assert.AreNotSame(records1, records2, "Two records instances are not the same");

            BibtexRecord record = new BibtexRecord();

            records1.Add(record);
            records2.Add(record);

            Assert.AreNotSame(records1, records2, "Two records instances are not the same when they have the same record stored within them");
        }

        [Test()]
        public void RecordsEvents()
        {

            bool recordAdded, recordDeleted, recordModified, recordsModified, recordURIAdded, recordURIModified;
            int recordAddedCount, recordDeletedCount, recordModifiedCount, recordsModifiedCount, recordURIAddedCount, recordURIModifiedCount;

            recordAdded = false;
            recordDeleted = false;
            recordModified = false;
            recordsModified = false;
            recordURIAdded = false;
            recordURIModified = false;

            recordAddedCount = 0;
            recordDeletedCount = 0;
            recordModifiedCount = 0;
            recordsModifiedCount = 0;
            recordURIAddedCount = 0;
            recordURIModifiedCount = 0;

            BibtexRecords records = new BibtexRecords();
            BibtexRecord record = new BibtexRecord();

            records.RecordAdded += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                recordAdded = true;
                recordAddedCount += 1;
            };

            records.RecordDeleted += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                recordDeleted = true;
                recordDeletedCount += 1;
            };

            records.RecordModified += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                recordModified = true;
                recordModifiedCount += 1;
            };

            records.RecordsModified += delegate(object sender, EventArgs e) {

                recordsModified = true;
                recordsModifiedCount += 1;
            };

            records.RecordURIAdded += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                recordURIAdded = true;
                recordURIAddedCount += 1;
            };

            records.RecordURIModified += delegate(object sender, EventArgs e) {
                Assert.AreSame(sender, record, "Delegate sender is the record");

                recordURIModified = true;
                recordURIModifiedCount += 1;
            };

            records.Add(record);

            Assert.IsTrue(recordAdded, "RecordAdded event is meant to be raised");
            Assert.IsFalse(recordDeleted, "RecordDeleted event is not meant to be raised");
            Assert.IsFalse(recordModified, "RecordModified event is not meant to be raised");
            Assert.IsTrue(recordsModified, "RecordsModified event is meant to be raised");
            Assert.IsFalse(recordURIAdded, "RecordURIAdded event is not meant to be raised");
            Assert.IsFalse(recordURIModified, "RecordURIModified event is not meant to be raised");
            Assert.AreEqual(1, recordAddedCount);
            Assert.AreEqual(0, recordDeletedCount);
            Assert.AreEqual(0, recordModifiedCount);
            Assert.AreEqual(1, recordsModifiedCount);
            Assert.AreEqual(0, recordURIAddedCount);
            Assert.AreEqual(0, recordURIModifiedCount);

            recordAdded = false;
            recordDeleted = false;
            recordModified = false;
            recordsModified = false;
            recordURIAdded = false;
            recordURIModified = false;
            recordAddedCount = 0;
            recordDeletedCount = 0;
            recordModifiedCount = 0;
            recordsModifiedCount = 0;
            recordURIAddedCount = 0;
            recordURIModifiedCount = 0;


            record.SetField("test_field", "test_data");

            Assert.IsFalse(recordAdded, "RecordAdded event is not meant to be raised");
            Assert.IsFalse(recordDeleted, "RecordDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(recordsModified, "RecordsModified event is not meant to be raised");
            Assert.IsFalse(recordURIAdded, "RecordURIAdded event is not meant to be raised");
            Assert.IsFalse(recordURIModified, "RecordURIModified event is not meant to be raised");
            Assert.AreEqual(0, recordAddedCount);
            Assert.AreEqual(0, recordDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, recordsModifiedCount);
            Assert.AreEqual(0, recordURIAddedCount);
            Assert.AreEqual(0, recordURIModifiedCount);

            recordAdded = false;
            recordDeleted = false;
            recordModified = false;
            recordsModified = false;
            recordURIAdded = false;
            recordURIModified = false;
            recordAddedCount = 0;
            recordDeletedCount = 0;
            recordModifiedCount = 0;
            recordsModifiedCount = 0;
            recordURIAddedCount = 0;
            recordURIModifiedCount = 0;


            record.SetField(BibtexRecord.BibtexFieldName.URI ,"file://tmp/test");

            Assert.IsFalse(recordAdded, "RecordAdded event is not meant to be raised");
            Assert.IsFalse(recordDeleted, "RecordDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(recordsModified, "RecordsModified event is not meant to be raised");
            Assert.IsTrue(recordURIAdded, "RecordURIAdded event is meant to be raised");
            Assert.IsFalse(recordURIModified, "RecordURIModified event is not meant to be raised");
            Assert.AreEqual(0, recordAddedCount);
            Assert.AreEqual(0, recordDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, recordsModifiedCount);
            Assert.AreEqual(1, recordURIAddedCount);
            Assert.AreEqual(0, recordURIModifiedCount);

            recordAdded = false;
            recordDeleted = false;
            recordModified = false;
            recordsModified = false;
            recordURIAdded = false;
            recordURIModified = false;
            recordAddedCount = 0;
            recordDeletedCount = 0;
            recordModifiedCount = 0;
            recordsModifiedCount = 0;
            recordURIAddedCount = 0;
            recordURIModifiedCount = 0;


            record.SetField(BibtexRecord.BibtexFieldName.URI ,"file://tmp/test1");

            Assert.IsFalse(recordAdded, "RecordAdded event is not meant to be raised");
            Assert.IsFalse(recordDeleted, "RecordDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(recordsModified, "RecordsModified event is not meant to be raised");
            Assert.IsFalse(recordURIAdded, "RecordURIAdded event is not meant to be raised");
            Assert.IsTrue(recordURIModified, "RecordURIModified event is meant to be raised");
            Assert.AreEqual(0, recordAddedCount);
            Assert.AreEqual(0, recordDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, recordsModifiedCount);
            Assert.AreEqual(0, recordURIAddedCount);
            Assert.AreEqual(1, recordURIModifiedCount);

            recordAdded = false;
            recordDeleted = false;
            recordModified = false;
            recordsModified = false;
            recordURIAdded = false;
            recordURIModified = false;
            recordAddedCount = 0;
            recordDeletedCount = 0;
            recordModifiedCount = 0;
            recordsModifiedCount = 0;
            recordURIAddedCount = 0;
            recordURIModifiedCount = 0;

            records.Remove(record);

            Assert.IsFalse(recordAdded, "RecordAdded event is not meant to be raised");
            Assert.IsTrue(recordDeleted, "RecordDeleted event is not meant to be raised");
            Assert.IsFalse(recordModified, "RecordModified event is meant to be raised");
            Assert.IsTrue(recordsModified, "RecordsModified event is not meant to be raised");
            Assert.IsFalse(recordURIAdded, "RecordURIAdded event is not meant to be raised");
            Assert.IsFalse(recordURIModified, "RecordURIModified event is not meant to be raised");
            Assert.AreEqual(0, recordAddedCount);
            Assert.AreEqual(1, recordDeletedCount);
            Assert.AreEqual(0, recordModifiedCount);
            Assert.AreEqual(1, recordsModifiedCount);
            Assert.AreEqual(0, recordURIAddedCount);
            Assert.AreEqual(0, recordURIModifiedCount);

        }
    }
}
