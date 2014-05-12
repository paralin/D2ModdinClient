// <copyright file="D2MP.cs">
// Copyright (c) 2014 All Right Reserved
//
// This source is subject to the License.
// Please see the License.txt file for more information.
// All other rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Christian Stewart</author>
// <email>kidovate@gmail.com</email>
// <date>2014-05-10</date>
// <summary>Core D2Moddin client functions.</summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using WebSocketSharp;

namespace d2mp
{
    public class D2MP
    {
        private static string server = "ws://d2modd.in:3005/";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static WebSocket ws;
        private static string addonsDir;
        private static string d2mpDir;
        private static string modDir;
        private static string dotaDir;
        private static string activeMod;
        public static bool shutDown = false;
        public static string ourDir;
        private static string[] modNames = null;
        private static volatile ProcessIcon icon;

        static void LaunchDota2()
        {
            Process.Start("explorer.exe", "steam://run/570");
            log.Debug("Launched Dota 2.");
        }

        static bool Dota2Running()
        {
            Process[] localByName = Process.GetProcessesByName("dota");
            return localByName.Length > 0;
        }

        static void KillDota2()
        {
            Process[] localByName = Process.GetProcessesByName("dota");
            foreach (Process p in localByName)
            {
                p.Kill();
                log.Debug("Killed Dota 2.");
            }

        }

        //Pipe a zip download directly through the decompressor
        static void UnzipFromStream(Stream zipStream, string outFolder)
        {
            ZipInputStream zipInputStream = new ZipInputStream(zipStream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;
                log.Debug(" --> " + entryFileName);
                byte[] buffer = new byte[4096];
                String fullZipToPath = Path.Combine(outFolder, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                    Thread.Sleep(30);
                }

                if (Path.GetFileName(fullZipToPath) != String.Empty)
                {
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                    }
                }
                zipEntry = zipInputStream.GetNextEntry();
            }
        }

        static void UninstallD2MP()
        {
            var installdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
            info.Arguments = "/C timeout 3 & Del /s /f /q " + installdir + " & exit";
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            Process.Start(info);
        }

        public static void main()
        {
            log.Debug("D2MP starting...");

            ourDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var iconThread = new Thread(delegate()
            {
                using (icon = new ProcessIcon())
                {
                    icon.Display();
                    Application.Run();
                }
            });

            iconThread.SetApartmentState(ApartmentState.STA);
            iconThread.Start();

            try
            {
                var steam = new SteamFinder();
                var steamDir = steam.FindSteam(true);
                dotaDir = steam.FindDota(true);
                if (steamDir == null || dotaDir == null)
                {
                    log.Fatal("Steam/dota was not found!");
                    MessageBox.Show("Steam/Dota was not found. Please make sure you have installed it properly or try contacting us at http://d2modd.in/contact");
                    Uninstall();
                }
                else
                {
                    log.Debug("Steam found: " + steamDir);
                    log.Debug("Dota found: " + dotaDir);
                }

                addonsDir = Path.Combine(dotaDir, "dota/addons/");
                d2mpDir = Path.Combine(dotaDir, "dota/d2moddin/");
                modDir = Path.Combine(addonsDir, "d2moddin");
                if (!Directory.Exists(addonsDir))
                    Directory.CreateDirectory(addonsDir);
                if (!Directory.Exists(d2mpDir))
                    Directory.CreateDirectory(d2mpDir);
                if (!Directory.Exists(modDir))
                    Directory.CreateDirectory(modDir);
                
                {
                    var dirs = Directory.GetDirectories(d2mpDir);
                    modNames = new string[dirs.Length];
                    int i = 0;
                    foreach (var dir in dirs)
                    {
                        var modName = Path.GetFileName(dir);
                        log.Debug("Found mod: " + modName + " detecting version...");
                        var infoPath = Path.Combine(d2mpDir, modName + "/addoninfo.txt");
                        string versionFile = "";
                        if (File.Exists(infoPath))
                        {
                            versionFile = File.ReadAllText(infoPath);
                        }
                        var match = Regex.Match(versionFile, @"(addonversion)(\s+)(\d+\.)?(\d+\.)?(\d+\.)?(\*|\d+)",
                            RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            string version = match.Groups.Cast<Group>()
                                .ToList()
                                .Skip(3)
                                .Aggregate("", (current, part) => current + part.Value);
                            log.Debug(modName + "=" + version);
                            modNames[i] = modName + "=" + version;
                        }
                        else
                        {
                            log.Error("Can't find version info for mod: " + modName + ", not including");
                            modNames[i] = modName + "=?";
                        }
                        i++;
                    }
                }

                //Detect user
                var config = File.ReadAllText(Path.Combine(steamDir, @"config\config.vdf"));
                var matches = Regex.Matches(config, "\"\\d{17}\"");
                string steamid;
                List<string> steamids = new List<string>();
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        steamid = match.Value.Substring(1).Substring(0, match.Value.Length - 2);
                        log.Debug("Steam ID detected: " + steamid);
                        steamids.Add(steamid);
                    }
                }
                else
                {
                    log.Fatal("Could not detect steam ID.");
                    return;
                }

                //Modify gameinfo.txt
                ModGameInfo();

                log.Debug("Starting shutdown file watcher...");
                string pathToShutdownFile = Path.Combine(ourDir, "d2mp.pid");
                File.WriteAllText(pathToShutdownFile, "Delete this file to shutdown D2MP.");

                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = ourDir;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName;
                watcher.Filter = "d2mp.pid";
                watcher.Deleted += (sender, args) =>
                                   {
                                       shutDown = true;
                                   };
                watcher.EnableRaisingEvents = true;

                shutDown = false;
                int tryCount = 0;
                while (tryCount < 30 && !shutDown)
                {
                    using (ws = new WebSocket(server))
                    {
                        ws.OnMessage += (sender, e) =>
                                        {
                                            log.Debug("server: " + e.Data);
                                            if (e.Data == "invalidid")
                                            {
                                                log.Debug("Invalid ID!");
                                                shutDown = true;
                                                return;
                                            }

                                            if (e.Data == "close")
                                            {
                                                log.Debug("Shutting down due to server request.");
                                                shutDown = true;
                                                return;
                                            }

                                            if (e.Data == "uninstall")
                                            {
                                                log.Debug("Uninstalling due to server request...");
                                                Uninstall();
                                                shutDown = true;
                                                return;
                                            }

                                            var msgParts = e.Data.Split(':');
                                            switch (msgParts[0])
                                            {
                                                case "installmod":
                                                    ThreadPool.QueueUserWorkItem(InstallMod, msgParts);
                                                    break;
                                                case "setmod":
                                                    ThreadPool.QueueUserWorkItem(SetMod, msgParts);
                                                    break;
                                                case "launchdota":
                                                    ThreadPool.QueueUserWorkItem(LaunchDota, msgParts);
                                                    break;
                                                default:
                                                    log.Error("Command not recognized: " + msgParts[0]);
                                                    break;
                                            }
                                        };

                        ws.OnOpen += (sender, e) => log.Debug("Connected");
                        ws.OnClose += (sender, args) => log.Debug("Disconnected");
                        ws.Connect();
                        tryCount++;
                        if (!ws.IsAlive)
                        {
                            if (tryCount == 1)
                            {
                                icon.DisplayBubble("Disconnected, attempting to reconnect...");
                            }
                            log.Debug("Can't connect to server, tries: " + tryCount);
                            Thread.Sleep(500);
                            continue;
                        }

                        if (tryCount > 1)
                        {
                            icon.DisplayBubble("Reconnected!");
                        }
                        else
                        {
                            icon.DisplayBubble("Connected and ready to begin installing mods.");
                        }

                        try
                        {
                            var ver =
                                File.ReadAllText(
                                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                        "version.txt"));
                            log.Debug("sending version: " + ver);
                            ws.Send("init:" + String.Join(",", steamids.ToArray(), 0, steamids.Count) + ":" + ver +
                                    ":" + String.Join(",", modNames));
                        }
                        catch (Exception ex)
                        {
                            log.Debug("Can't detect ID from version.txt, : " + ex);
                            return;
                        }

                        tryCount = 0;
                        while (ws.IsAlive && !shutDown)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                log.Fatal("Overall error in the program: " + ex);
            }
            UnmodGameInfo();
            Application.Exit();
        }

        static string GetActiveMod()
        {
            var infoPath = Path.Combine(modDir, "modname.txt");
            if (!File.Exists(infoPath)) return null;
            var name = File.ReadAllText(infoPath);
            log.Debug("Current active mod: " + name);
            return name;
        }

        private static void LaunchDota(object state)
        {
          if(!Dota2Running()) LaunchDota2();
        }

        private static void SetMod(object state)
        {
            activeMod = GetActiveMod();
            var msgParts = (string[]) state;
            var mod = msgParts[1];
            if (activeMod == null || activeMod != mod)
            {
                if(Directory.Exists(modDir))
                    Directory.Delete(modDir, true);
                log.Debug("Setting active mod to "+mod+".");
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(Path.Combine(d2mpDir, mod.Split('=')[0]), modDir);
                File.WriteAllText(Path.Combine(modDir, "modname.txt"), mod);
                icon.DisplayBubble("Set active mod to "+mod+"!");
            }
        }

        private static void ModGameInfo()
        {
            var path = Path.Combine(dotaDir, "dota/gameinfo.txt");
            if (File.Exists(path))
            {
                log.Debug("Checking if patch needed on " + path + "...");
                Regex reg = new Regex(@"(Game)(\s+)(platform)(\s+)?(\r\n?|\n)(\s+)(})");
                var text = File.ReadAllText(path);
                var match = reg.Match(text);
                if (match.Success)
                {
                    text = reg.Replace(text, "Game				platform\n			Game				|gameinfo_path|addons\\d2moddin\n		}");
                    File.WriteAllText(path, text);
                    log.Debug("Patched file to add d2moddin search path.");
                    if (Dota2Running())
                    {
                        icon.DisplayBubble("Restarting Dota 2 for you...");
                        KillDota2();
                        LaunchDota2();
                    }
                }
            }
        }

        private static void UnmodGameInfo()
        {
            var path = Path.Combine(dotaDir, "dota/gameinfo.txt");
            if (File.Exists(path))
            {
                log.Debug("Checking if unpatch needed on " + path + "...");
                Regex reg = new Regex(@"(\s+)(Game)(\s+)(\|gameinfo_path\|addons\\d2moddin)(\r\n?|\n)");
                var text = File.ReadAllText(path);
                var match = reg.Match(text);
                if (match.Success)
                {
                    text = reg.Replace(text, "\n");
                    File.WriteAllText(path, text);
                    log.Debug("Patched file to remove d2moddin search path.");
                }
            }
        }
   
        private static void InstallMod(object state)
        {
            var msgParts = (string[])state;
            var modp = msgParts[1].Split('=');
            var modname = modp[0];
            var url = "http:" + msgParts[3];
            log.Info("Server requested that we install mod " + modname + " from download " + url);

            icon.DisplayBubble("Downloading mod " + modname + "...");

            //delete if already exists
            var targetDir = Path.Combine(d2mpDir, modname);
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);
            //Make the dir again
            Directory.CreateDirectory(targetDir);
            //Stream the ZIP to the folder
            WebClient client = new WebClient();
            UnzipFromStream(client.OpenRead(url), targetDir);
            log.Info("Mod installed!");
            icon.DisplayBubble("Mod downloaded successfully: "+modname+".");
            ws.Send("installedMod:" + modname);
            for(int i=0; i<modNames.Length; i++)
            {
                string[] parts = modNames[i].Split('=');
                if (parts[0] == modname)
                {
                    modNames[i] = msgParts[1];
                    return;
                }
            }
            var newArr = new string[modNames.Length + 1];
            for (int i = 0; i < modNames.Length; i++)
            {
                newArr[i] = modNames[i];
            }
            newArr[modNames.Length] = msgParts[1];
            modNames = newArr;
        }

        public static void DeleteMods()
        {
            if(Directory.Exists(modDir))Directory.Delete(modDir, true);
            if (Directory.Exists(d2mpDir)) Directory.Delete(d2mpDir, true);
            log.Debug("Deleted all mod files.");
        }

        public static void Uninstall()
        {
            DeleteMods();
            UninstallD2MP();
            shutDown = true;
        }

        public static void Restart()
        {
            Process proc = new Process();
            proc.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            shutDown = true;
        }

        public static void ShowModList()
        {
            string message = "You currently have the following detected mods installed:\n\n"+String.Join(", ", modNames);
            MessageBox.Show(message, "Installed Mods");
        }
    }

    public class SteamFinder
    {
        private string cachedLocation = "";
        private string cachedDotaLocation = "";
        private static string[] knownLocations = new string[] { @"C:\Steam\", @"C:\Program Files (x86)\Steam\", @"C:\Program Files\Steam\" };

        public SteamFinder()
        {
        }

        bool ContainsSteam(string dir)
        {
            return Directory.Exists(dir) && File.Exists(Path.Combine(dir, "Steam.exe"));
        }

        public string FindSteam(bool delCache)
        {
            if (delCache) cachedLocation = "";
            if (delCache || cachedLocation == "")
            {
                foreach (var loc in knownLocations)
                {
                    if (ContainsSteam(loc))
                    {
                        cachedLocation = loc;
                        return loc;
                    }
                }

                //Get from registry?
                RegistryKey regKey = Registry.CurrentUser;
                regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

                if (regKey != null)
                {
                    cachedLocation = regKey.GetValue("SteamPath").ToString();
                    return cachedLocation;
                }

                //Search using file search? Eh... Return null.
                return null;
            }
            else
            {
                return cachedLocation;
            }
        }

        public string FindDota(bool delCache)
        {
            if (!delCache && cachedDotaLocation != null) return cachedDotaLocation;
            var steamDir = FindSteam(false);
            if (steamDir != null)
            {
                var dir = Path.Combine(steamDir, "steamapps/common/dota 2 beta/");
                if (Directory.Exists(dir))
                {
                    cachedDotaLocation = dir;
                    return dir;
                }
            }

            //Get from registry
            RegistryKey regKey = Registry.LocalMachine;
            regKey =
                regKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570");
            if (regKey != null)
            {
                cachedDotaLocation = regKey.GetValue("InstallLocation").ToString();
                return cachedDotaLocation;
            }

            return null;
        }
    }
}
