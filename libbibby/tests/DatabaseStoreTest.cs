//
//  DatabaseStore.cs
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
using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using libbibby;

namespace libbibby
{
    [TestFixture]
    public class DatabaseStoreTest
    {
        string testFilename;

        [SetUp]
        public void DataStoreSetUp ()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bibliographer\\bibtex_fields");
                testFilename = Path.GetTempPath() + "\\datastoretest.sqlite";
            }
            else
            {
                Environment.SetEnvironmentVariable("BIBTEX_TYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_records");
                Environment.SetEnvironmentVariable("BIBTEX_FIELDTYPE_LIB", Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibtex_fields");
                testFilename = "/tmp/datastoretest.sqlite";
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
        public void DatabaseStoreConstructor ()
        {
            // Tests to ensure that the database has been created correctly.

            using (var dbConn = new SqliteConnection ("Data Source=" + testFilename + ";Version=3")) {
                dbConn.Open ();
                using (var dbcmd = dbConn.CreateCommand ()) {
                    dbcmd.CommandText = "" +
                        "SELECT name FROM sqlite_master " +
                        "WHERE type='table' " +
                        "ORDER BY name";
                    using (var dbrdr = dbcmd.ExecuteReader()) {
                        dbrdr.Read ();
                        StringAssert.IsMatch ("fieldType", dbrdr.GetString (0), "DatabaseStore contains fieldType table.");
                        dbrdr.Read ();
                        StringAssert.IsMatch ("fields", dbrdr.GetString (0), "DatabaseStore contains fields table.");
                        dbrdr.Read ();
                        StringAssert.IsMatch ("fileRecord", dbrdr.GetString (0), "DatabaseStore contains fileRecord table.");
                        dbrdr.Read ();
                        StringAssert.IsMatch ("recordType", dbrdr.GetString (0), "DatabaseStore contains recordType table.");
                        dbrdr.Read ();
                        StringAssert.IsMatch ("records", dbrdr.GetString (0), "DatabaseStore contains records table.");
                        Assert.False (dbrdr.Read(), "DatabaseStore contains additional tables.");
                    }
                }
                using (var dbcmd = dbConn.CreateCommand ()) {
                    dbcmd.CommandText = "" +
                        "SELECT recordTypeName FROM recordType";
                    using (var dbrdr = dbcmd.ExecuteReader ()) {
                        for (int i = 0; i < BibtexRecordTypeLibrary.Count (); i++) {
                            dbrdr.Read ();
                            StringAssert.IsMatch (BibtexRecordTypeLibrary.GetWithIndex(i).name, dbrdr.GetString (0));
                        }
                    }
                }
                using (var dbcmd = dbConn.CreateCommand ()) {
                    dbcmd.CommandText = "" +
                        "SELECT fieldTypeName FROM fieldType";
                    using (var dbrdr = dbcmd.ExecuteReader ()) {
                        for (int i = 0; i < BibtexRecordFieldTypeLibrary.Count (); i++) {
                            dbrdr.Read ();
                            StringAssert.IsMatch (BibtexRecordFieldTypeLibrary.GetWithIndex(i).name, dbrdr.GetString (0));
                        }
                    }
                }
                dbConn.Close ();
            }

        }

        [Test]
        public void DatabaseNewRecord()
        {
            int result;

            result = DatabaseStoreStatic.NewRecord ();
            Assert.AreEqual (1, result);
            result = DatabaseStoreStatic.NewRecord ("test");
            Assert.AreEqual (2, result);
            Assert.AreEqual ("test", DatabaseStoreStatic.GetKey (result));
            DatabaseStoreStatic.DeleteRecord (1);
            DatabaseStoreStatic.DeleteRecord (2);
        }

        [Test]
        public void DatabaseGetRecords()
        {
            List<int> result;

            result = DatabaseStoreStatic.GetRecords ();
            Assert.AreEqual (0, result.Count);
            DatabaseStoreStatic.NewRecord ();
            result = DatabaseStoreStatic.GetRecords ();
            Assert.AreEqual (1, result.Count);
            DatabaseStoreStatic.DeleteRecord (1);
            result = DatabaseStoreStatic.GetRecords ();
            Assert.AreEqual (0, result.Count);
        }

        [Test]
        public void DatabaseDeleteRecord()
        {
            int record;

            record = DatabaseStoreStatic.NewRecord ("test");
            DatabaseStoreStatic.SetField (record, "author", "first author");
            DatabaseStoreStatic.SetFilename (record, "file://testuri");
            Assert.DoesNotThrow(() => DatabaseStoreStatic.GetKey (record));
            Assert.AreEqual ("test", DatabaseStoreStatic.GetKey (record));
            Assert.DoesNotThrow(() => DatabaseStoreStatic.GetField (record, "author"));
            Assert.AreEqual ("first author", DatabaseStoreStatic.GetField (record, "author"));
            Assert.AreEqual("file://testuri", DatabaseStoreStatic.GetFilename(record));
            DatabaseStoreStatic.DeleteRecord (record);
            Assert.Throws<NoResultException> (() => DatabaseStoreStatic.GetKey (record));
            Assert.AreEqual("", DatabaseStoreStatic.GetFilename (record));
            Assert.AreEqual ("", DatabaseStoreStatic.GetField (record, "author"));
        }

        [Test]
        public void DatabaseSetKey ()
        {
            int record;
            record = DatabaseStoreStatic.NewRecord ();
            DatabaseStoreStatic.SetKey (record, "testing");
            Assert.AreEqual ("testing", DatabaseStoreStatic.GetKey (record));
        }

        [Test]
        public void DatabaseSetField ()
        {
            int record;
            record = DatabaseStoreStatic.NewRecord ();
            DatabaseStoreStatic.SetField (record, "author", "first author");
            Assert.AreEqual ("first author", DatabaseStoreStatic.GetField (record, "author"));
            DatabaseStoreStatic.SetField (record, "author", "first author and second author");
            Assert.AreEqual ("first author and second author", DatabaseStoreStatic.GetField (record, "author"));
            DatabaseStoreStatic.DeleteField(record, "author");
            DatabaseStoreStatic.DeleteRecord(record);
        }

        [Test]
        public void DatabaseGetField()
        {
            int record;
            record = DatabaseStoreStatic.NewRecord ();
            Assert.IsFalse(DatabaseStoreStatic.HasField(record, "author"));
            DatabaseStoreStatic.SetField (record, "author", "first author");
            Assert.IsTrue(DatabaseStoreStatic.HasField(record, "author"));
            DatabaseStoreStatic.DeleteField(record, "author");
            Assert.IsFalse(DatabaseStoreStatic.HasField(record, "author"));
            DatabaseStoreStatic.DeleteRecord(record);
        }

        [Test]
        public void DatabaseGetFieldNames()
        {
            int record;
            record = DatabaseStoreStatic.NewRecord ();
            Assert.IsEmpty (DatabaseStoreStatic.GetFieldNames (record));
            DatabaseStoreStatic.SetField (record, "author", "first author");
            DatabaseStoreStatic.SetField (record, "title", "title");
            Assert.AreEqual (2, DatabaseStoreStatic.GetFieldNames (record).Count);
            Assert.IsTrue (DatabaseStoreStatic.GetFieldNames (record).Contains("author"));
            Assert.IsTrue (DatabaseStoreStatic.GetFieldNames (record).Contains("title"));
            Assert.IsFalse (DatabaseStoreStatic.GetFieldNames (record).Contains("journal"));
            DatabaseStoreStatic.DeleteRecord(record);
            Assert.IsEmpty (DatabaseStoreStatic.GetFieldNames (record));
        }

        [Test]
        public void DatabaseDeleteField()
        {
            int record;
            record = DatabaseStoreStatic.NewRecord ();
            DatabaseStoreStatic.SetField (record, "author", "first author");
            Assert.AreEqual ("first author", DatabaseStoreStatic.GetField (record, "author"));
            DatabaseStoreStatic.DeleteField (record, "author");
            Assert.AreEqual ("", DatabaseStoreStatic.GetField (record, "author"));
            DatabaseStoreStatic.DeleteRecord (record);
            Assert.Throws<NoResultException> (() => DatabaseStoreStatic.GetKey (record));
        }

        [TearDown]
        public void DatabaseStoreTearDown()
        {
            // Clean up after the test
            File.Delete (testFilename);
            Assert.IsFalse (File.Exists (testFilename), string.Format ("File: {0} has been deleted after the tests.", testFilename));
        }

    }
}

