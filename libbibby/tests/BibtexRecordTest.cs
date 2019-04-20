
using System;
using System.IO;
using System.Collections;
using NUnit.Framework;

namespace libbibby
{
    [TestFixture]
    public class BibtexRecordTest
    {
        string testFilename;

        [SetUp]
        public void BibtexRecordSetup ()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_fields");
                testFilename = Path.GetTempPath() + "\\bibtexrecordtest.sqlite";
            }
            else
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_fields");
                testFilename = "/tmp/bibtexrecordtest.sqlite";
            }
            BibtexRecordTypeLibrary.Load ();
            BibtexRecordFieldTypeLibrary.Load ();

            // Delete testfilename if it exists so that we can check correct creation
            if (File.Exists (testFilename)) {
                File.Delete (testFilename);
            }
            Assert.IsFalse(File.Exists(testFilename),"File: "+testFilename+" exists prior to databaseStore constructor.");

            DatabaseStoreStatic.Initialize (testFilename);
            Assert.IsTrue (File.Exists (testFilename), "File: "+testFilename+" has been created by the databaseStore constructor.");
        }

        [Test]
        public void EmptyRecord ()
        {
            var record = new BibtexRecord();

            Assert.IsInstanceOf<BibtexRecord>(record);

            Assert.AreEqual(record.RecordType, "", "Empty record's RecordType is an empty string");
            Assert.AreEqual(record.GetKey(),"", "Empty record's Key is an empty string");
            Assert.AreEqual(record.GetAuthors(), new StringArrayList(), "Empty record's Authors array is an empty StringArrayList");
            Assert.AreEqual(record.GetAuthorsString(), "", "Empty record's Authors string is an empty string");
            Assert.AreEqual(record.GetJournal(), "", "Empty record's Journal is an empty string");
            Assert.AreEqual(record.GetYear(), "", "Empty record's Year is an empty string");
            Assert.IsNull(record.GetURI(), "Empty record's URI is null");
            Assert.IsFalse(record.HasURI(), "Empty record does not have an URI");
        }

        [Test]
        public void RecordCompare ()
        {
            var record1 = new BibtexRecord();
            var record2 = new BibtexRecord();

            Assert.AreNotSame(record1, record2, "Two record instances are not the same");

            record1.SetKey("testKey");
            record2.SetKey("testKey");

            Assert.AreNotSame(record1, record2, "Two record instances are not the same when they have the same key");
        }

        [Test]
        public void RecordEvents ()
        {
            bool fieldAdded, fieldDeleted, recordModified, uriAdded, uriUpdated, doiAdded, doiUpdated;
            int fieldAddedCount, fieldDeletedCount, recordModifiedCount, uriAddedCount, uriUpdatedCount, doiAddedCount, doiUpdatedCount;

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            var record = new BibtexRecord();

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

            record.DoiAdded += delegate(object sender, EventArgs e) {
                Assert.AreSame (sender, record, "Delegate sender is the record");

                doiAdded = true;
                doiAddedCount += 1;
            };

            record.DoiUpdated += delegate(object sender, EventArgs e) {
                Assert.AreSame (sender, record, "Delegate sender is the record");

                doiUpdated = true;
                doiUpdatedCount += 1;
            };

            record.SetField("test_field", "test_data");

            Assert.IsTrue(fieldAdded, "FieldAdded event is meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.IsFalse(doiAdded, "DOIAdded event is not meant to be raised");
            Assert.IsFalse(doiUpdated, "DOIUpdated event is not meant to be raised");
            Assert.AreEqual(1, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);
            Assert.AreEqual(0, doiAddedCount);
            Assert.AreEqual(0, doiUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            record.RemoveField("test_field");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsTrue(fieldDeleted, "FieldDeleted event is meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.IsFalse(doiAdded, "DOIAdded event is not meant to be raised");
            Assert.IsFalse(doiUpdated, "DOIUpdated event is not meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(1, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);
            Assert.AreEqual(0, doiAddedCount);
            Assert.AreEqual(0, doiUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            // Adding URI
            record.SetURI( "file://tmp/test");

            Assert.IsFalse(fieldAdded, "FieldAdded event is meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsTrue(uriAdded, "URIAdded event is meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.IsFalse(doiAdded, "DOIAdded event is meant to be raised");
            Assert.IsFalse(doiUpdated, "DOIUpdated event is not meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(1, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);
            Assert.AreEqual(0, doiAddedCount);
            Assert.AreEqual(0, doiUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            // Setting URI without a change
            record.SetURI("file://tmp/test");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsFalse(recordModified, "RecordModified event is not meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.IsFalse(doiAdded, "DOIAdded event is not meant to be raised");
            Assert.IsFalse(doiUpdated, "DOIUpdated event is not meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(0, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);
            Assert.AreEqual(0, doiAddedCount);
            Assert.AreEqual(0, doiUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            // Updating URI
            record.SetURI("file:///tmp/test1");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsTrue(uriUpdated, "URIUpdated event is meant to be raised");
            Assert.IsFalse(doiAdded, "DOIAdded event is not meant to be raised");
            Assert.IsFalse(doiUpdated, "DOIUpdated event is meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(1, uriUpdatedCount);
            Assert.AreEqual(0, doiAddedCount);
            Assert.AreEqual(0, doiUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            record.SetField ("doi", "testdoi");

            Assert.IsTrue(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is not meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.IsTrue(doiAdded, "DOIAdded event is meant to be raised");
            Assert.IsFalse(doiUpdated, "DOIUpdated event is not meant to be raised");
            Assert.AreEqual(1, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);
            Assert.AreEqual(1, doiAddedCount);
            Assert.AreEqual(0, doiUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            // re-setting doi without change
            record.SetField ("doi", "testdoi");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsFalse(recordModified, "RecordModified event is not meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.IsFalse(doiAdded, "DOIAdded event is not meant to be raised");
            Assert.IsFalse(doiUpdated, "DOIUpdated event is not meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(0, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);
            Assert.AreEqual(0, doiAddedCount);
            Assert.AreEqual(0, doiUpdatedCount);

            fieldAdded = false;
            fieldDeleted = false;
            recordModified = false;
            uriAdded = false;
            uriUpdated = false;
            doiAdded = false;
            doiUpdated = false;
            fieldAddedCount = 0;
            fieldDeletedCount = 0;
            recordModifiedCount = 0;
            uriAddedCount = 0;
            uriUpdatedCount = 0;
            doiAddedCount = 0;
            doiUpdatedCount = 0;

            // updating doi to new value
            record.SetField ("doi", "testdoiwithchange");

            Assert.IsFalse(fieldAdded, "FieldAdded event is not meant to be raised");
            Assert.IsFalse(fieldDeleted, "FieldDeleted event is not meant to be raised");
            Assert.IsTrue(recordModified, "RecordModified event is not meant to be raised");
            Assert.IsFalse(uriAdded, "URIAdded event is not meant to be raised");
            Assert.IsFalse(uriUpdated, "URIUpdated event is not meant to be raised");
            Assert.IsFalse(doiAdded, "DOIAdded event is not meant to be raised");
            Assert.IsTrue(doiUpdated, "DOIUpdated event is not meant to be raised");
            Assert.AreEqual(0, fieldAddedCount);
            Assert.AreEqual(0, fieldDeletedCount);
            Assert.AreEqual(1, recordModifiedCount);
            Assert.AreEqual(0, uriAddedCount);
            Assert.AreEqual(0, uriUpdatedCount);
            Assert.AreEqual(0, doiAddedCount);
            Assert.AreEqual(1, doiUpdatedCount);

        }

        [Test]
        public void BibtexRecordGetJournal()
        {
            var record = new BibtexRecord();
            Assert.AreEqual ("", record.GetJournal ());
            record.SetField ("journal", "test journal");
            Assert.AreEqual ("test journal", record.GetJournal ());
        }

        [Test]
        public void BibtexRecordGetYear()
        {
            var record = new BibtexRecord();
            Assert.AreEqual ("", record.GetYear ());
            record.SetField ("year", "2016");
            Assert.AreEqual ("2016", record.GetYear ());
        }

        [Test]
        public void BibtexRecordGetDOI()
        {
            var record = new BibtexRecord();
            Assert.AreEqual ("", record.GetDOI ());
            Assert.AreEqual (false, record.HasDOI ());
            record.SetField ("doi", "testdoi");
            Assert.AreEqual ("testdoi", record.GetDOI ());
            Assert.AreEqual (true, record.HasDOI ());
        }


        [TearDown]
        public void BibtexRecordTearDown()
        {
            // Clean up after the test
            File.Delete (testFilename);
            Assert.IsFalse (File.Exists (testFilename), string.Format ("File: {0} has been deleted after the tests.", testFilename));
        }
    }
}
