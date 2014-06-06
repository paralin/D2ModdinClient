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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ClientCommon.Data;
using ClientCommon.Methods;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XSockets.Client40;
using XSockets.Client40.Common.Event.Arguments;

namespace d2mp
{
    public class D2MP
    {
        private static string server = "ws://ddp2.d2modd.in:4502/ClientController";
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string addonsDir;
        private static string d2mpDir;
        private static string modDir;
        private static string dotaDir;
        private static ClientCommon.Data.ClientMod activeMod;
        public static bool shutDown = false;
        public static string ourDir;
        private static List<ClientCommon.Data.ClientMod> mods = new List<ClientCommon.Data.ClientMod>();
        private static volatile ProcessIcon icon;
        private static volatile bool isInstalling;
        private static bool hasConnected = true;
        private static XSocketClient client;

        private static void SteamCommand(string command)
        {
            Process.Start("explorer.exe", "steam://" + command);
        }

        private static void LaunchDota2()
        {
            SteamCommand("run/570");
            log.Debug("Launched Dota 2.");
        }

        private static bool Dota2Running()
        {
            Process[] localByName = Process.GetProcessesByName("dota");
            return localByName.Length > 0;
        }

        private static void KillDota2()
        {
            Process[] localByName = Process.GetProcessesByName("dota");
            foreach (Process p in localByName)
            {
                p.Kill();
                log.Debug("Killed Dota 2.");
            }
        }

        //Pipe a zip download directly through the decompressor
        private static void UnzipFromStream(Stream zipStream, string outFolder)
        {
            var zipInputStream = new ZipInputStream(zipStream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;
                log.Debug(" --> " + entryFileName);
                var buffer = new byte[4096];
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

        static void Send(string json)
        {
            client.Send(new TextArgs(json, "data"));
        }

        private static void UninstallD2MP()
        {
            string installdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var info = new ProcessStartInfo("cmd.exe");
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
            File.WriteAllText(Path.Combine(ourDir, "version.txt"), ClientCommon.Version.ClientVersion);
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
                string steamDir = steam.FindSteam(true);
                dotaDir = steam.FindDota(true);
                if (steamDir == null || dotaDir == null)
                {
                    log.Fatal("Steam/dota was not found!");
                    return;
                }
                log.Debug("Steam found: " + steamDir);
                log.Debug("Dota found: " + dotaDir);

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
                    string[] dirs = Directory.GetDirectories(d2mpDir);
                    int i = 0;
                    foreach (string dir in dirs)
                    {
                        string modName = Path.GetFileName(dir);
                        log.Debug("Found mod: " + modName + " detecting version...");
                        string infoPath = Path.Combine(d2mpDir, modName + "/addoninfo.txt");
                        string versionFile = "";
                        if (File.Exists(infoPath))
                        {
                            versionFile = File.ReadAllText(infoPath);
                        }
                        Match match = Regex.Match(versionFile, @"(addonversion)(\s+)(\d+\.)?(\d+\.)?(\d+\.)?(\*|\d+)",
                            RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            string version = match.Groups.Cast<Group>()
                                .ToList()
                                .Skip(3)
                                .Aggregate("", (current, part) => current + part.Value);
                            log.Debug(modName + "=" + version);
                            mods.Add(new ClientMod {name = modName, version = version});
                        }
                        else
                        {
                            log.Error("Can't find version info for mod: " + modName + ", not including");
                        }
                        i++;
                    }
                }

                //Detect user
                string config = File.ReadAllText(Path.Combine(steamDir, @"config\config.vdf"));
                MatchCollection matches = Regex.Matches(config, "\"\\d{17}\"");
                string steamid;
                var steamids = new List<string>();
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

                var watcher = new FileSystemWatcher();
                watcher.Path = ourDir;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName;
                watcher.Filter = "d2mp.pid";
                watcher.Deleted += (sender, args) => { shutDown = true; };
                watcher.EnableRaisingEvents = true;

                client = new XSocketClient(server, "*");
                bool hasConnected = false;
                client.OnOpen += (sender, args) =>
                {
                    icon.DisplayBubble(hasConnected
                        ? "Reconnected!"
                        : "Connected and ready to begin installing mods.");
                    hasConnected = true;

                    log.Debug("Sending init, version: " + ClientCommon.Version.ClientVersion);
                    var init = new Init
                    {
                        SteamIDs = steamids.ToArray(),
                        Version = ClientCommon.Version.ClientVersion,
                        Mods = mods.ToArray()
                    };
                    var json = JObject.FromObject(init).ToString(Formatting.None);
                    Send(json);
                };

                client.Bind("commands", e =>
                {
                    log.Debug("server: " + e.data);
                    JObject msg = JObject.Parse(e.data);
                    switch (msg["msg"].Value<string>())
                    {
                        case Shutdown.Msg:
                            log.Debug("Shutting down due to server request.");
                            shutDown = true;
                            return;
                        case ClientCommon.Methods.Uninstall.Msg:
                            log.Debug("Uninstalling due to server request...");
                            Uninstall();
                            shutDown = true;
                            return;
                        case ClientCommon.Methods.InstallMod.Msg:
                            ThreadPool.QueueUserWorkItem(InstallMod, msg.ToObject<InstallMod>());
                            break;
                        case ClientCommon.Methods.DeleteMod.Msg:
                            ThreadPool.QueueUserWorkItem(DeleteMod, msg.ToObject<DeleteMod>());
                            break;
                        case ClientCommon.Methods.SetMod.Msg:
                            ThreadPool.QueueUserWorkItem(SetMod, msg.ToObject<SetMod>());
                            break;
                        case ClientCommon.Methods.ConnectDota.Msg:
                            ThreadPool.QueueUserWorkItem(ConnectDota, msg.ToObject<ConnectDota>());
                            break;
                        case ClientCommon.Methods.LaunchDota.Msg:
                            ThreadPool.QueueUserWorkItem(LaunchDota, msg.ToObject<LaunchDota>());
                            break;
                        case ClientCommon.Methods.ConnectDotaSpectate.Msg:
                            ThreadPool.QueueUserWorkItem(SpectateGame,
                                msg.ToObject<ConnectDotaSpectate>());
                            break;
                        default:
                            log.Error("Command not recognized.");
                            break;
                    }
                });
                client.OnClose += (sender, args) =>
                                  {
                                      if (hasConnected)
                                      {
                                          icon.DisplayBubble("Disconnected, attempting to reconnect...");
                                          hasConnected = false;
                                      }
                                      try
                                      {
                                          client.Open();
                                      }
                                      catch (Exception ex)
                                      {
                                          icon.DisplayBubble("Can't connect to the lobby server!");
                                      }
                                  };
                try
                {
                    client.Open();
                }
                catch (Exception ex)
                {
                    icon.DisplayBubble("Can't connect to the lobby server!");
                }
                while (!shutDown)
                {
                    Thread.Sleep(100);
                }
                client.Close();
            }
            catch (Exception ex)
            {
                log.Fatal("Overall error in the program: " + ex);
            }
            //UnmodGameInfo();
            shutDown = true;
            Application.Exit();
        }

        private static ClientCommon.Data.ClientMod GetActiveMod()
        {
            string infoPath = Path.Combine(modDir, "modname.txt");
            if (!File.Exists(infoPath)) return null;
            string name = File.ReadAllText(infoPath);
            try
            {
                var mod = JObject.Parse(name).ToObject<ClientMod>();
                log.Debug("Current active mod: " + mod.name);
                return mod;
            }
            catch(Newtonsoft.Json.JsonReaderException e)
            {
                File.Delete(infoPath);
                //TODO: Restart launcher
                return null;
            }
        }

        private static void SpectateGame(object state)
        {
            var op = state as ConnectDotaSpectate;
            if (Dota2Running()) KillDota2();
            //AFAIK this doesn't work
            SteamCommand("rungameid/570//+connect_hltv " + op.ip);
        }

        private static void LaunchDota(object state)
        {
            if (!Dota2Running()) LaunchDota2();
        }

        private static void SetMod(object state)
        {
            activeMod = GetActiveMod();
            var op = state as SetMod;
            if (Equals(activeMod, op.Mod)) return;
            if (Directory.Exists(modDir))
                Directory.Delete(modDir, true);
            log.Debug("Setting active mod to " + op.Mod.name + ".");
            FileSystem.CopyDirectory(Path.Combine(d2mpDir, op.Mod.name), modDir);
            File.WriteAllText(Path.Combine(modDir, "modname.txt"), JObject.FromObject(op.Mod).ToString(Formatting.Indented));
            icon.DisplayBubble("Set active mod to " + op.Mod.name + "!");
        }

        private static void ModGameInfo()
        {
            string path = Path.Combine(dotaDir, "dota/gameinfo.txt");
            if (File.Exists(path))
            {
                log.Debug("Checking if patch needed on " + path + "...");
                var reg = new Regex(@"(Game)(\s+)(platform)(\s+)?(\r\n?|\n)(\s+)(})");
                string text = File.ReadAllText(path);
                Match match = reg.Match(text);
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
            string path = Path.Combine(dotaDir, "dota/gameinfo.txt");
            if (File.Exists(path))
            {
                log.Debug("Checking if unpatch needed on " + path + "...");
                var reg = new Regex(@"(\s+)(Game)(\s+)(\|gameinfo_path\|addons\\d2moddin)(\r\n?|\n)");
                string text = File.ReadAllText(path);
                Match match = reg.Match(text);
                if (match.Success)
                {
                    text = reg.Replace(text, "\n");
                    File.WriteAllText(path, text);
                    log.Debug("Patched file to remove d2moddin search path.");
                }
            }
        }

        private static void ConnectDota(object state)
        {
            var op = state as ConnectDota;
            SteamCommand("connect/" + op.ip);
            log.Debug("Told Steam to connect to " + op.ip + ".");
        }

        private static void DeleteMod(object state)
        {
            var op = state as DeleteMod;
            string targetDir = Path.Combine(d2mpDir, op.Mod.name);
            if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
            log.Debug("Server requested that we delete mod " + op.Mod.name + ".");
        }

        private static void InstallMod(object state)
        {
            var op = state as InstallMod; 
            if (isInstalling)
            {
                icon.DisplayBubble("Already downloading a mod!");
                return;
            }
            isInstalling = true;
            icon.DisplayBubble("Attempting to download "+op.Mod.name+"...");

            log.Info("Server requested that we install mod " + op.Mod.name + " from download " + op.url);

            //delete if already exists
            string targetDir = Path.Combine(d2mpDir, op.Mod.name);
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);
            //Make the dir again
            Directory.CreateDirectory(targetDir);
            //Stream the ZIP to the folder
            try
            {
                using (var wc = new WebClient())
                {
                    UnzipFromStream(wc.OpenRead(op.url), targetDir);
                }
            }
            catch (Exception ex)
            {
                isInstalling = false;
                icon.DisplayBubble("Failed to download mod " + op.Mod.name + ".");
                return;
            }
            log.Info("Mod installed!");
            icon.DisplayBubble("Mod downloaded successfully: " + op.Mod.name + ".");
            var msg = new OnInstalledMod()
                                 {
                                     Mod = op.Mod
                                 };
            Send(JObject.FromObject(msg).ToString(Formatting.None));
            var existing = mods.FirstOrDefault(m => m.name == op.Mod.name);
            if (existing != null) mods.Remove(existing);
            mods.Add(op.Mod);
            isInstalling = false;
        }

        public static void DeleteMods()
        {
            if (Directory.Exists(modDir)) Directory.Delete(modDir, true);
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
            var proc = new Process();
            proc.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            shutDown = true;
        }

        public static void ShowModList()
        {
            string message = "You currently have the following detected mods installed:\n\n";
            foreach (var mod in mods)
            {
                message += mod.name + "@" + mod.version;
            }
            MessageBox.Show(message, "Installed Mods");
        }
    }

    public class SteamFinder
    {
        private static readonly string[] knownLocations =
        {
            @"C:\Steam\", @"C:\Program Files (x86)\Steam\", @"C:\Program Files\Steam\"
        };

        private string cachedDotaLocation = "";
        private string cachedLocation = "";

        private bool ContainsSteam(string dir)
        {
            return Directory.Exists(dir) && File.Exists(Path.Combine(dir, "Steam.exe"));
        }

        public string FindSteam(bool delCache)
        {
            if (delCache) cachedLocation = "";
            if (delCache || cachedLocation == "")
            {
                foreach (string loc in knownLocations)
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
            return cachedLocation;
        }

        public string FindDota(bool delCache)
        {
            if (!delCache && cachedDotaLocation != null) return cachedDotaLocation;
            string steamDir = FindSteam(false);
            //Get from registry
            RegistryKey regKey = Registry.LocalMachine;
            regKey =
                regKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570");
            if (regKey != null)
            {
                cachedDotaLocation = regKey.GetValue("InstallLocation").ToString();
                return cachedDotaLocation;
            }

            if (steamDir != null)
            {
                string dir = Path.Combine(steamDir, "steamapps/common/dota 2 beta/");
                if (Directory.Exists(dir))
                {
                    cachedDotaLocation = dir;
                    return dir;
                }
            }

            return null;
        }
    }
}
