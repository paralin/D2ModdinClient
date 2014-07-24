// 
// Settings.cs
// Created by ilian000 on 2014-06-08
// Licenced under the Apache License, Version 2.0
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace d2mp
{
    static class Settings
    {
        public static string steamDir
        {
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
        public static bool createShortcutAtStartup
        {
            get
            {
                return Properties.Settings.Default.shortcut;
            }
            set
            {
                Properties.Settings.Default.shortcut = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool autoUpdateMods
        {
            get
            {
                return Properties.Settings.Default.autoUpdate;
            }
            set
            {
                Properties.Settings.Default.autoUpdate = value;
                Properties.Settings.Default.Save();
            }
        }
        public static void Reset()
        {
            Properties.Settings.Default.Reset();
        }

    }
    static class ShortcutWriter
    {
        public static void writeDesktopShortcut()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + "D2Moddin Client" + ".url";
            if (!File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + app);
                    writer.WriteLine("IconIndex=0");
                    string icon = app.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon);
                    writer.Flush();
                }
            }
        }
    }
    static class logKeeper
    {
        public static string log = "";
    }
}
