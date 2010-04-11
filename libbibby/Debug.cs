// Copyright 2005-2010 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System;

namespace libbibby
{
    public class Debug
    {
        // Debug class
        //
        // Debug levels 0 to 10
        // 0 - essential information
        // 1 - general exceptions
        // 5 - general output about what the program is doing
        // 10 - verbose stuff

        private static int level_ = System.Convert.ToInt16 (System.Environment.GetEnvironmentVariable ("DEBUG_LEVEL"));
        private static bool enabled_ = System.Convert.ToBoolean (System.Environment.GetEnvironmentVariable ("DEBUG"));

        public static void SetLevel (int level)
        {
            level_ = level;
        }

        public static void Enable (bool enabled)
        {
            enabled_ = enabled;
        }

        public static void Write (int level, string format)
        {
            if (level_ >= level && enabled_ == true) {
                System.Console.Write (format);
            }
        }

        public static void WriteLine (int level, string format)
        {
            if (level_ >= level && enabled_ == true) {
                System.Console.WriteLine (format);
            }
        }

        public static void WriteLine (int level, string format, object arg0)
        {
            if (level_ >= level && enabled_ == true) {
                System.Console.WriteLine (format, arg0);
            }
        }
        public static void WriteLine (int level, string format, object arg0, object arg1)
        {
            if (level_ >= level && enabled_ == true) {
                System.Console.WriteLine (format, arg0, arg1);
            }
        }
        public static void WriteLine (int level, string format, object arg0, object arg1, object arg2)
        {
            if (level_ >= level && enabled_ == true) {
                System.Console.WriteLine (format, arg0, arg1, arg2);
            }
        }
        public static void WriteLine (int level, string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (level_ >= level && enabled_ == true) {
                System.Console.WriteLine (format, arg0, arg1, arg2, arg3);
            }
        }
        
    }
}
