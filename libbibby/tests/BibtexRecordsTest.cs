//
//  BibtexRecordsTest.cs
//
//  Author:
//       Sameer Morar <smorar@gmail.com>
//
//  Copyright (c) 2016 Bibliographer developers
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
using System.IO;
using NUnit.Framework;

namespace libbibby
{


    [TestFixture]
    public class BibtexRecordsTest
    {

        string testFilename;

        [SetUp]
        public void BibtexRecordsSetup ()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_fields");
                testFilename = Path.GetTempPath() + "\\bibtexrecordstest.sqlite";
            }
            else
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_fields");
                testFilename = "/tmp/bibtexrecordstest.sqlite";
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
        public void EmptyRecords ()
        {
            var records = new BibtexRecords();

            Assert.IsInstanceOf<BibtexRecords>(records);
            Assert.AreEqual(records.Count, 0);
        }

        [Test]
        public void RecordsCompare()
        {
            var records1 = new BibtexRecords();
            var records2 = new BibtexRecords();

            Assert.AreNotSame(records1, records2, "Two records instances are not the same");

            var record = new BibtexRecord();

            records1.Add(record);
            records2.Add(record);

            Assert.AreNotSame(records1, records2, "Two records instances are not the same when they have the same record stored within them");
        }

        [Test]
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

            var records = new BibtexRecords();
            var record = new BibtexRecord();

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
                Assert.AreNotSame(sender, record, "Delegate sender is not the record");

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


            record.SetURI("file://tmp/test");

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


            record.SetURI("file://tmp/test1");

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

        [TearDown]
        public void BibtexRecordsTearDown()
        {
            // Clean up after the test
            File.Delete (testFilename);
            Assert.IsFalse (File.Exists (testFilename), string.Format ("File: {0} has been deleted after the tests.", testFilename));
        }
    }
}
