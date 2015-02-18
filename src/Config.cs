//
//  Config.cs
//
//  Author:
//       Sameer Morar <smorar@gmail.com>
//       Carl Hultquist <chultquist@gmail.com>
//
//  Copyright (c) 2005-2015 Bibliographer developers
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

namespace bibliographer
{
    public static class Config
    {
        static GConf.Client client;
        static readonly string APP_PATH = "/apps/bibliographer/";

        static string ParseKeyName (string keyName)
        {
            // Replace all spaces in keynames with underscores. spaces are not allowed.
            return keyName.Replace (" ", "_");
        }

        public static void Initialise ()
        {
            if (client == null)
                client = new GConf.Client ();
        }

        public static string GetConfigDir ()
        {
            return System.Environment.GetEnvironmentVariable ("HOME") + "/.config/bibliographer/";
        }

        public static string GetDataDir ()
        {
            return System.Environment.GetEnvironmentVariable ("HOME") + "/.local/share/bibliographer/";
        }

        public static void SetString (string keyName, string keyValue)
        {
            keyName = ParseKeyName (keyName);
            try {
                client.Set (APP_PATH + keyName, keyValue);
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "setting value: {0} to key: {1}", keyValue, keyName);
            }
        }

        public static string GetString (string keyName)
        {
            keyName = ParseKeyName (keyName);
            try {
                return (string)client.Get (APP_PATH + keyName);
            } catch (GConf.NoSuchKeyException) {
                return "";
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "getting value from key: {0}", keyName);
                return "";
            }
        }

        public static void SetInt (string keyName, int keyValue)
        {
            keyName = ParseKeyName (keyName);
            try {
                client.Set (APP_PATH + keyName, keyValue);
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "setting value: {0} to key: {1}", keyValue, keyName);
            }
        }

        public static int GetInt (string keyName)
        {
            keyName = ParseKeyName (keyName);
            try {
                return (int)client.Get (APP_PATH + keyName);
            } catch (GConf.NoSuchKeyException) {
                return 0;
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "getting value from key: {0}", keyName);
                return 0;
            }
        }

        public static void SetBool (string keyName, bool keyValue)
        {
            keyName = ParseKeyName (keyName);
            try {
                client.Set (APP_PATH + keyName, keyValue);
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "setting value: {0} to key: {1}", keyValue, keyName);
            }
            
        }

        public static bool GetBool (string keyName)
        {
            keyName = ParseKeyName (keyName);
            try {
                return (bool)client.Get (APP_PATH + keyName);
            } catch (GConf.NoSuchKeyException) {
                return false;
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "getting value from key: {0}", keyName);
                return false;
            }
        }

        public static bool KeyExists (string keyName)
        {
            keyName = ParseKeyName (keyName);
            try {
                return client.Get (APP_PATH + keyName) != null;
            } catch (GConf.NoSuchKeyException) {
                return false;
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "checking that key {0} exists", keyName);
                return false;
            }
        }

        public static void SetKey (string keyName, object o)
        {
            keyName = ParseKeyName (keyName);
            try {
                client.Set (APP_PATH + keyName, o);
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "setting value: {0} to key: {1}", o.ToString (), keyName);
            }
        }

        public static object GetKey (string keyName)
        {
            keyName = ParseKeyName (keyName);
            try {
                return client.Get (APP_PATH + keyName);
            } catch (GConf.NoSuchKeyException) {
                return null;
            } catch (System.Exception) {
                Debug.WriteLine (1, "Unhandled exception with gconf");
                Debug.WriteLine (1, "getting value from key: {0}", keyName);
                return null;
            }
        }
        
    }
}
