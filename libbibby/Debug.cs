//
//  Debug.cs
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

using System;

namespace libbibby
{
    public static class Debug
    {
        // Debug class
        //
        // Debug levels 0 to 10
        // 0 - essential information
        // 1 - general exceptions
        // 5 - general output about what the program is doing
        // 10 - verbose stuff

        private static int level_ = Convert.ToInt16 (Environment.GetEnvironmentVariable ("DEBUG_LEVEL"));
        private static bool enabled_ = Convert.ToBoolean (Environment.GetEnvironmentVariable ("DEBUG"));

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
                Console.Write (format);
            }
        }

        public static void WriteLine (int level, string format)
        {
            if (level_ >= level && enabled_ == true) {
                Console.WriteLine (format);
            }
        }

        public static void WriteLine (int level, string format, object arg0)
        {
            if (level_ >= level && enabled_ == true) {
                Console.WriteLine (format, arg0);
            }
        }
        public static void WriteLine (int level, string format, object arg0, object arg1)
        {
            if (level_ >= level && enabled_ == true) {
                Console.WriteLine (format, arg0, arg1);
            }
        }
        public static void WriteLine (int level, string format, object arg0, object arg1, object arg2)
        {
            if (level_ >= level && enabled_ == true) {
                Console.WriteLine (format, arg0, arg1, arg2);
            }
        }
        public static void WriteLine (int level, string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (level_ >= level && enabled_ == true) {
                Console.WriteLine (format, arg0, arg1, arg2, arg3);
            }
        }
        
    }
}
