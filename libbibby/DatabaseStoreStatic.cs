//
//  DatabaseStoreStatic.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using static libbibby.Debug;

namespace libbibby
{
    public class NoResultException : Exception
    {
        private readonly string reason;

        public NoResultException (string reason)
        {
            this.reason = reason;
        }

        public string GetReason ()
        {
            return reason;
        }
    }

    public static class DatabaseStoreStatic
    {
        private static SqliteConnection SqlConn()
        {
            string filename = Environment.GetEnvironmentVariable ("BIBLIOGRAPHER_DATABASE");
            SqliteConnection dbConn = new SqliteConnection ("Data Source=" + filename + ";Version=3;pooling=true");
            return dbConn;
        }

        public static void Initialize (string filename)
        {
            Environment.SetEnvironmentVariable ("BIBLIOGRAPHER_DATABASE", filename);

            if (!File.Exists (filename)) {
                // Create database and create tables
                SqliteConnection.CreateFile (filename);
                using (SqliteConnection dbConn = SqlConn ()) {
                    dbConn.OpenAsync ();
                    using (SqliteTransaction tr = dbConn.BeginTransaction ()) {
                        using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {
                            dbcmd.Transaction = tr;
                            dbcmd.CommandText = "" +
                            "CREATE TABLE IF NOT EXISTS records (" +
                            "id INTEGER PRIMARY KEY, " +
                            "key TEXT, " +
                            "recordTypeId INTEGER" +
                            ")";
                            dbcmd.ExecuteNonQueryAsync ();
                            dbcmd.CommandText = "" +
                            "CREATE TABLE IF NOT EXISTS fields (" +
                            "id INTEGER PRIMARY KEY, " +
                            "recordId INTEGER, " +
                            "fieldTypeId INTEGER," +
                            "fieldText TEXT" +
                            ")";
                            dbcmd.ExecuteNonQueryAsync ();
                            dbcmd.CommandText = "" +
                            "CREATE UNIQUE INDEX IF NOT EXISTS field_idx ON fields(recordId, fieldTypeId)";
                            dbcmd.ExecuteNonQueryAsync ();
                            dbcmd.CommandText = "" +
                            "CREATE TABLE IF NOT EXISTS recordType (" +
                            "id INTEGER PRIMARY KEY, " +
                            "recordTypeName TEXT NOT NULL UNIQUE" +
                            ")";
                            dbcmd.ExecuteNonQueryAsync ();
                            dbcmd.CommandText = "" +
                            "CREATE TABLE IF NOT EXISTS fieldType (" +
                            "id INTEGER PRIMARY KEY, " +
                            "fieldTypeName TEXT NOT NULL UNIQUE" +
                            ")";
                            dbcmd.ExecuteNonQueryAsync ();
                            dbcmd.CommandText = "" +
                            "CREATE TABLE IF NOT EXISTS fileRecord (" +
                            "id INTEGER PRIMARY KEY, " +
                            "recordId INTEGER UNIQUE, " +
                            "filename TEXT UNIQUE," +
                            "size INTEGER," +
                            "mtime INTEGER," +
                            "md5sum TEXT" +
                            ")";
                            dbcmd.ExecuteNonQueryAsync ();
                            dbcmd.CommandText = "" +
                            "CREATE TABLE IF NOT EXISTS searchData (" +
                            "id INTEGER PRIMARY KEY, " +
                            "recordId INTEGER UNIQUE, " +
                            "data TEXT" +
                            ")";
                            dbcmd.ExecuteNonQueryAsync ();
                        }
                        tr.Commit ();
                    }
                    dbConn.Close ();
                }
            }
            // Update recordType if new types have been implemented
            using (SqliteConnection dbConn = SqlConn ()) {
                dbConn.OpenAsync ();
                using (SqliteTransaction tr = dbConn.BeginTransaction ()) {
                    using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {
                        dbcmd.Transaction = tr;
                        for (int i = 0; i < BibtexRecordTypeLibrary.Count (); i++) {
                            dbcmd.CommandText = "" +
                            "INSERT OR IGNORE INTO recordType (recordTypeName) " +
                            "VALUES ('" + BibtexRecordTypeLibrary.GetWithIndex (i).name.ToLower () + "')";
                            dbcmd.ExecuteNonQueryAsync ();
                        }
                        for (int i = 0; i < BibtexRecordFieldTypeLibrary.Count (); i++) {
                            dbcmd.CommandText = "" +
                            "INSERT OR IGNORE INTO fieldType (fieldTypeName) " +
                            "VALUES ('" + BibtexRecordFieldTypeLibrary.GetWithIndex (i).name.ToLower () + "')";
                            dbcmd.ExecuteNonQueryAsync ();
                        }
                    }
                    tr.Commit ();
                }
                dbConn.Close ();
            }
        }

        private static void VoidQuery (string query)
        {
            try
            {
                using (SqliteConnection dbConn = SqlConn ()) {
                    dbConn.OpenAsync ();
                    using (SqliteTransaction tr = dbConn.BeginTransaction ()) {
                        using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {
                            dbcmd.Transaction = tr;
                            dbcmd.CommandText = string.Format (query);
                            dbcmd.ExecuteNonQueryAsync();
                            tr.Commit ();
                        }
                    }
                    dbConn.Close ();
                }
            } catch (SqliteException e) {
                WriteLine (1, "Sqlite error: \n" + e.Message);
                throw new NoResultException ("Sqlite error: \n" + e.Message);
            }
        }

        private static string ReturnStringQuery (string query)
        {
            string result;

            try
            {
                using (SqliteConnection dbConn = SqlConn ()) {
                    dbConn.OpenAsync ();
                    using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {
                        dbcmd.CommandText = string.Format (query);
                        Task<object> obj = dbcmd.ExecuteScalarAsync ();
                        if (obj.Result == null) {
                            throw new NoResultException (string.Format ("No result returned from query: {0}", query));
                        }

                        result = Convert.ToString (obj.Result);
                    }
                    dbConn.Close ();
                }
                if (result == null) {
                    throw new NoResultException (string.Format ("No result returned from query: {0}", query));
                }
            } catch (SqliteException e) {
                WriteLine (1, "Sqlite error: \n" + e.Message);
                throw new NoResultException ("Sqlite error: \n" + e.Message);
            }
 
            return result;
        }

        private static int ReturnIntQuery (string query)
        {
            int result;
            result = 0;

            try{
                using (SqliteConnection dbConn = SqlConn ()) {
                    dbConn.OpenAsync ();
                    using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {

                        dbcmd.CommandText = string.Format (query);
                        Task<object> obj = dbcmd.ExecuteScalarAsync ();
                        if (obj.Result == DBNull.Value) {
                            throw new NoResultException (string.Format ("No result returned from query: {0}", query));
                        }

                        result = unchecked((int)(long)obj.Result);
                    }
                    dbConn.Close ();
                }
            }
            catch (SqliteException e){
                WriteLine (1, "Sqlite error: \n" + e.Message);
                throw new NoResultException ("Sqlite error: \n" + e.Message);
            }

            return result;
        }

        private static long ReturnLongQuery (string query)
        {
            long result;
            result = 0;
                
            try
            {
                using (SqliteConnection dbConn = SqlConn ()) {
                    dbConn.OpenAsync ();
                    using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {

                        dbcmd.CommandText = string.Format (query);
                        Task<object> obj = dbcmd.ExecuteScalarAsync ();
                        if (obj.Result == DBNull.Value) {
                            throw new NoResultException (string.Format ("No result returned from query: {0}", query));
                        }

                        result = Convert.ToInt64 (obj.Result);
                    }
                    dbConn.Close ();
                }

            } catch (SqliteException e) {
                WriteLine (1, "Sqlite error: \n" + e.Message);
                throw new NoResultException ("Sqlite error: \n" + e.Message);
            }

            return result;
        }

        private static ulong ReturnUlongQuery (string query)
        {
            ulong result;
            result = 0;

            try
            {
                using (SqliteConnection dbConn = SqlConn ()) {
                    dbConn.OpenAsync ();
                    using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {

                        dbcmd.CommandText = string.Format (query);
                        Task<object> obj = dbcmd.ExecuteScalarAsync ();
                        if (obj.Result == DBNull.Value) {
                            throw new NoResultException (string.Format ("No result returned from query: {0}", query));
                        }

                        result = unchecked((ulong)(long)obj.Result);
                    }
                    dbConn.Close ();
                }
            } catch (SqliteException e) {
                WriteLine (1, "Sqlite error: \n" + e.Message);
                throw new NoResultException ("Sqlite error: \n" + e.Message);
            }

            return result;
        }

        public static int NewRecord (string key = null, string recordType = null, int recordTypeId = 0)
        {
            if (recordType != null) {
                recordTypeId = GetRecordTypeId (recordType.ToLower ());
            }
            return ReturnIntQuery (string.Format ("INSERT INTO records VALUES(NULL,'{0}',{1}); SELECT last_insert_rowid() FROM records", key, recordTypeId));
        }

        public static void DeleteRecord (int recordId)
        {
            VoidQuery (string.Format ("DELETE FROM records WHERE id={0}; DELETE FROM fields WHERE recordId={0}; DELETE FROM fileRecord WHERE recordId={0}", recordId));
        }

        public static int GetRecordTypeId (string recordType)
        {
            return ReturnIntQuery (string.Format ("SELECT id FROM recordType WHERE recordTypeName='{0}'", recordType.ToLower ()));
        }

        public static string GetRecordType (int recordId)
        {
            string result;

            try {
                result = ReturnStringQuery (string.Format ("SELECT recordTypeName FROM recordType WHERE id=(SELECT recordTypeId FROM records WHERE id={0})", recordId));
            } catch (NoResultException) {
                result = "";
            }
                
            return result;
        }

        public static void SetRecordType (int recordId, string recordType)
        {
            VoidQuery (string.Format ("UPDATE records SET recordTypeId=(SELECT id FROM recordType WHERE recordTypeName='{0}') WHERE id={1}", recordType.ToLower (), recordId));
        }

        public static void SetKey (int recordId, string key)
        {
            VoidQuery (string.Format ("UPDATE records SET key='{0}' WHERE id={1}", key, recordId));
        }

        public static string GetKey (int recordId)
        {
            return ReturnStringQuery (string.Format ("SELECT key FROM records WHERE id={0}", recordId));
        }

        public static void SetField (int recordId, string field, string fieldText)
        {

            try {
                string currFieldText;

                currFieldText = ReturnStringQuery (string.Format ("SELECT fieldText FROM fields WHERE recordId={0} AND fieldTypeId=(SELECT id FROM fieldType WHERE fieldTypeName='{1}')", recordId, field.ToLower ()));

                if (currFieldText != fieldText) {
                    VoidQuery (string.Format ("UPDATE fields SET fieldText='{0}' WHERE recordId='{1}' AND fieldTypeId=(SELECT id FROM fieldType WHERE fieldTypeName='{2}')", fieldText, recordId, field));
                }
            } catch (NoResultException) {
                VoidQuery (string.Format ("INSERT INTO fields VALUES (NULL, {0}, (SELECT id FROM fieldType WHERE fieldTypeName='{1}'), '{2}')", recordId, field.ToLower (), fieldText));
            }
        }

        public static ArrayList GetFieldNames (int recordId)
        {
            ArrayList result;
            result = new ArrayList ();
            
            using (SqliteConnection dbConn = SqlConn ()) {
                dbConn.OpenAsync ();
                using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {

                    dbcmd.CommandText = string.Format ("SELECT fieldType.fieldTypeName FROM fields INNER JOIN fieldtype ON fields.fieldTypeId=fieldType.id WHERE fields.recordId={0}", recordId);
                    Task<System.Data.Common.DbDataReader> reader = dbcmd.ExecuteReaderAsync ();
                    while (reader.Result.Read ()) {
                        result.Add (reader.Result ["fieldTypeName"]);
                    }
                }
                dbConn.Close ();
            }
            
            return result;
        }

        public static string GetField (int recordId, string field)
        {
            try {
                return ReturnStringQuery (string.Format ("SELECT fieldText FROM fields WHERE recordId={0} AND fieldTypeId=(SELECT id FROM fieldType WHERE fieldTypeName='{1}')", recordId, field.ToLower ()));
            } catch (NoResultException) {
                return "";
            }
        }

        public static void SetFilename (int recordId, string uri)
        {
            WriteLine (5, "SetFilename");
            try {
                string currFileName;
                currFileName = ReturnStringQuery (string.Format("SELECT filename FROM fileRecord WHERE recordId={0}", recordId));
                if (currFileName != uri) {
                    VoidQuery (string.Format("UPDATE fileRecord SET filename='{0}', mtime=NULL, size=NULL, md5sum=NULL WHERE recordId='{1}'", uri, recordId));
                }
            } catch (NoResultException) {
                WriteLine (1, "SetFilename - NoResultException");
                VoidQuery (string.Format("INSERT INTO fileRecord VALUES (NULL, {0}, '{1}', NULL, NULL, NULL)", recordId, uri));
            }
        }

        public static void SetFileAttrs (int recordId, string uri, long size, ulong mtime, string md5sum)
        {
            try {
                string currFileName;
                currFileName = ReturnStringQuery (string.Format("SELECT filename FROM fileRecord WHERE recordId={0}", recordId));
                if (currFileName == uri) {
                    VoidQuery (string.Format("UPDATE fileRecord SET mtime={0}, size={1}, md5sum='{2}' WHERE filename='{3}' AND recordId='{4}'", mtime, size, md5sum, uri, recordId));
                }
            } catch (NoResultException) {
                VoidQuery (string.Format("INSERT INTO fileRecord VALUES (NULL, {0}, {1}, {2}, {3}, {4})", recordId, uri, size, mtime, md5sum));
            }
        }

        public static string GetFilename (int recordId)
        {
            try {
                return ReturnStringQuery (string.Format("SELECT filename FROM fileRecord WHERE recordId={0}", recordId));
            } catch (NoResultException) {
                return "";
            }
        }

        public static long GetFileSize (int recordId)
        {
            try {
                return ReturnLongQuery (string.Format("SELECT size FROM fileRecord WHERE recordId={0}", recordId));
            } catch (NoResultException) {
                return 0;
            }
        }

        public static ulong GetFileMTime (int recordId)
        {
            try {
                return ReturnUlongQuery (string.Format("SELECT mtime FROM fileRecord WHERE recordId={0}", recordId));
            } catch (NoResultException) {
                return 0;
            }
        }

        public static string GetFileMd5sum (int recordId)
        {
            try {
                return ReturnStringQuery (string.Format("SELECT md5sum FROM fileRecord WHERE recordId={0}", recordId));
            } catch (NoResultException) {
                return "";
            }
        }

        public static bool HasField (int recordId, string field)
        {
            try {
                ReturnStringQuery (string.Format ("SELECT fieldText FROM fields WHERE recordId={0} AND fieldTypeId=(SELECT id FROM fieldType WHERE fieldTypeName='{1}')", recordId, field.ToLower ()));
            } catch (NoResultException) {
                return false;
            }
            return true;
        }

        public static void DeleteField (int recordId, string field)
        {
            VoidQuery (string.Format ("DELETE FROM fields WHERE recordId={0} AND fieldTypeId=(SELECT id FROM fieldType WHERE fieldTypeName='{1}')", recordId, field.ToLower ()));
        }

        public static List<int> GetRecords ()
        {
            List<int> recordIds = new List<int> ();
            
            using (SqliteConnection dbConn = SqlConn ()) {
                dbConn.OpenAsync ();
                using (SqliteCommand dbcmd = dbConn.CreateCommand ()) {

                    dbcmd.CommandText = string.Format ("SELECT id FROM records ORDER BY id");
                    Task<System.Data.Common.DbDataReader> reader = dbcmd.ExecuteReaderAsync ();
                    while (reader.Result.Read ()) {
                        recordIds.Add (Convert.ToUInt16 (reader.Result ["id"]));
                    }
                }
                dbConn.Close ();
            }
            
            return recordIds;
        }

        public static void SetSearchData(int recordId, string data)
        {
            try {
                string searchString = ReturnStringQuery (string.Format ("SELECT data FROM searchData WHERE recordId={0}", recordId));
                VoidQuery (string.Format ("UPDATE searchData SET data='{0}' WHERE recordId={1}", data, recordId));
            } catch (NoResultException) {
                VoidQuery (string.Format ("INSERT INTO searchData VALUES (NULL, {0}, '{1}')", recordId, data));
            }
        }

        public static string GetSearchData(int recordId)
        {
            try {
                return ReturnStringQuery (string.Format ("SELECT data FROM searchData WHERE recordId={0}", recordId));
            } catch (NoResultException) {
                return "";
            }

        }
    }
}
