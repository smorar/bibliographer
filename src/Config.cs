// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using GConf;
using GLib;

namespace bibliographer
{
    public class Config
    {
        private static GConf.Client client = null;
        private static string APP_PATH = "/apps/bibliographer/";

        private static string ParseKeyName (string keyName)
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
                if (client.Get (APP_PATH + keyName) != null)
                    return true;
                else
                    return false;
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
