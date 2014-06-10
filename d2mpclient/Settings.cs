using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace d2mp
{
    static class Settings
    {
        public static string steamDir {
            get
            {
                return Properties.Settings.Default.steamDir;
            }
            set
            {
                Properties.Settings.Default.steamDir = value;
                Properties.Settings.Default.Save();
            }
        }
        public static string dotaDir
        {
            get
            {
                return Properties.Settings.Default.dotaDir;
            }
            set
            {
                Properties.Settings.Default.dotaDir = value;
                Properties.Settings.Default.Save();
            }
        }
        public static void Reset()
        {
            Properties.Settings.Default.Reset();
        }

    }
    static class logKeeper
    {
        public static string log = "";
    }
}
