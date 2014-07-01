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
#if DEBUG
        private static string server = "ws://localhost:4502/ClientController";
#else
        private static string server = "ws://net1.d2modd.in:4502/ClientController";
#endif
        public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string addonsDir;
        public static string d2mpDir;
        private static string modDir;
        private static ClientCommon.Data.ClientMod activeMod;
        public static bool shutDown = false;
        public static string ourDir;
        private static volatile ProcessIcon icon;
        private static volatile notificationForm notifier;
        private static volatile settingsForm settingsForm = new settingsForm();
        private static volatile bool isInstalling;
        private static bool hasConnected = false;
        private static XSocketClient client;
        private static List<string> steamids;

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

        private static void SetupClient()
        {
            client = new XSocketClient(server, "*");
            client.OnOpen += (sender, args) =>
            {
                notifier.Notify(1, hasConnected ? "Reconnected" : "Connected", hasConnected ? "Connection to the server has been reestablished" : "Client has been connected to the server.");
                //icon.DisplayBubble(hasConnected
                //    ? "Reconnected!"
                //    : "Connected and ready to begin installing mods.");
                hasConnected = true;

                log.Debug("Sending init, version: " + ClientCommon.Version.ClientVersion);
                var init = new Init
                {
                    SteamIDs = steamids.ToArray(),
                    Version = ClientCommon.Version.ClientVersion,
                    Mods = modController.clientMods.ToArray()
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
                        log.Debug("Shutting down due to server request. Client not up to date.");
                        notifier.Notify(2, "Outdated version", "Updating to new version...");
                        updateClient();
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
#if !DEBUG
                        ThreadPool.QueueUserWorkItem(LaunchDota, msg.ToObject<LaunchDota>());
#endif
                        break;
                    case ClientCommon.Methods.ConnectDotaSpectate.Msg:
                        ThreadPool.QueueUserWorkItem(SpectateGame,
                            msg.ToObject<ConnectDotaSpectate>());
                        break;
                    case ClientCommon.Methods.NotifyMessage.Msg:
                        ThreadPool.QueueUserWorkItem(NotifyMessage, msg.ToObject<NotifyMessage>());
                        break;
                    default:
                        log.Error("Command not recognized.");
                        break;
                }
            });

            client.OnClose += (sender, args) =>
            {
                log.Info("Disconnected from the server.");
                HandleClose();
            };
        }

        private static void updateClient()
        {
            var updaterPath = Path.Combine(Directory.GetParent(ourDir).FullName, "updater.exe");
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile("https://s3-us-west-2.amazonaws.com/d2mpclient/StartD2MP.exe", updaterPath);
                Process.Start(updaterPath);
                Application.Exit();
            }
        }

        private static void HandleClose()
        {
            if (hasConnected)
            {
                notifier.Notify(3, "Lost connection", "Attempting to reconnect...");
                //icon.DisplayBubble("Disconnected, attempting to reconnect...");
                hasConnected = false;
            }
            SetupClient();
            Thread.Sleep(10000);
            try
            {
                client.Open();
            }
            catch (Exception)
            {
                HandleClose();
            }
        }

        //Pipe a zip download directly through the decompressor
        private static bool UnzipFromStream(Stream zipStream, string outFolder)
        {
            bool result = false;

            try
            {
                var zipInputStream = new ZipInputStream(zipStream);
                ZipEntry zipEntry = zipInputStream.GetNextEntry();
                while (zipEntry != null)
                {
                    String entryFileName = zipEntry.Name;
                    log.Debug("CRC:" + zipEntry.Crc + " --> " + entryFileName);
                    var buffer = new byte[4096];
                    String fullZipToPath = Path.Combine(outFolder, "temp", entryFileName);
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
            catch (Exception ex)
            {
                log.Error("Error extracted files to temporary folder.", ex);
                notifier.Notify(4, "Mod installation failed", "Error extracted files to temporary folder.");
            }

            try
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(outFolder, "temp"), "*", System.IO.SearchOption.AllDirectories))
                {
                    string destinationPath = Path.Combine(outFolder, file.Substring(outFolder.Length + 6, file.Length - outFolder.Length - 6));
                    if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                    File.Move(file, destinationPath);
                }
                if (Directory.Exists(Path.Combine(outFolder, "temp")))
                    Directory.Delete(Path.Combine(outFolder, "temp"), true);

                result = true;
            }
            catch (Exception ex)
            {
                log.Error("Error moving extracted files from temporary folder.", ex);
                notifier.Notify(4, "Mod installation failed", "Error moving extracted files from temporary folder.");
            }

            return result;
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
            var notifyThread = new Thread(delegate()
            {
                using (notifier = new notificationForm())
                {
                    notifier.Visible = true;
                    Application.Run();
                }
            });
            notifyThread.SetApartmentState(ApartmentState.STA);
            notifyThread.Start();
            ourDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            File.WriteAllText(Path.Combine(ourDir, "version.txt"), ClientCommon.Version.ClientVersion);
            var iconThread = new Thread(delegate()
                                        {
                                            using (icon = new ProcessIcon())
                                            {
                                                icon.Display();
                                                icon.showNotification = () => notifier.Invoke(new MethodInvoker(() => { notifier.Fade(1); notifier.hideTimer.Start(); }));
                                                Application.Run();
                                            }
                                        });

            iconThread.SetApartmentState(ApartmentState.STA);
            iconThread.Start();

            try
            {
                var steam = new SteamFinder();
                if (!steam.checkProtocol())
                {
                    log.Error("Steam protocol not found. Trying to repair...");
                    var steamDir = steam.FindSteam(true, false);
                    var steamservicePath = Path.Combine(steamDir, @"bin\steamservice.exe");
                    if (File.Exists(steamservicePath))
                    {
                        Process process = new Process() { StartInfo = { FileName = steamservicePath, Arguments = "/repair" } };
                        try
                        {
                            process.Start();
                        }
                        catch
                        {
                            log.Fatal("Could not repair protocol. Steamservice couldn't run. Elevation refused?");
                        }
                        process.WaitForExit();
                        Restart();
                    }
                    else
                    {
                        log.Fatal("Could not repair protocol. Steam directory could not be found. Is steam installed? If so, please reinstall steam.");
                    }

                }
                if (!Directory.Exists(Settings.steamDir) || !Directory.Exists(Settings.dotaDir) || !SteamFinder.checkDotaDir(Settings.dotaDir))
                {
                    Settings.steamDir = steam.FindSteam(true);
                    Settings.dotaDir = steam.FindDota(true);
                }

                if (Settings.steamDir == null || Settings.dotaDir == null)
                {
                    log.Fatal("Steam/dota was not found!");
                    return;
                }
                log.Debug("Steam found: " + Settings.steamDir);
                log.Debug("Dota found: " + Settings.dotaDir);

                addonsDir = Path.Combine(Settings.dotaDir, @"dota\addons\");
                d2mpDir = Path.Combine(Settings.dotaDir, @"dota\d2moddin\");
                modDir = Path.Combine(addonsDir, "d2moddin");
                if (!Directory.Exists(addonsDir))
                    Directory.CreateDirectory(addonsDir);
                if (!Directory.Exists(d2mpDir))
                    Directory.CreateDirectory(d2mpDir);
                if (!Directory.Exists(modDir))
                    Directory.CreateDirectory(modDir);

                {
                    modController.getLocalMods();
                }

                // Detect steam account id which was logged in most recently
                string config = File.ReadAllText(Path.Combine(Settings.steamDir, @"config\loginusers.vdf"));
                MatchCollection idMatches = Regex.Matches(config, "\"\\d{17}\"");
                MatchCollection timestampMatches = Regex.Matches(config, "(?m)(?<=\"Timestamp\".{2}).*$");
                Dictionary<int, string> usersDict = new Dictionary<int, string>();
                steamids = new List<string>();
                if (idMatches.Count > 0)
                {
                    foreach (Match match in idMatches)
                    {
                        string steamid = match.Value.Substring(1).Substring(0, match.Value.Length - 2);
                        int index = idMatches.Cast<Match>().TakeWhile(x=> x != match).Count();
                        string timestamp = timestampMatches[index].Value;
                        int iTimestamp = Convert.ToInt32(timestamp.Substring(1).Substring(0, timestamp.Length - 2));
                        log.Debug(String.Format("Steam ID detected: {0} with timestamp: {1}", steamid, iTimestamp));
                        usersDict.Add(iTimestamp, steamid);
                    }
                    string mainId = usersDict.OrderByDescending(x => x.Key).FirstOrDefault().Value;
                    log.Debug("Selecting Steam ID to be sent: " + mainId);
                    steamids.Add(mainId);
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

                try
                {
                    SetupClient();
                    client.Open();
                }
                catch (Exception)
                {
                    notifier.Notify(4, "Server error", "Can't connect to the lobby server!");
                    Thread.Sleep(5000);
                    HandleClose();
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

        public static ClientCommon.Data.ClientMod GetActiveMod()
        {
            try
            {
                string infoPath = Path.Combine(modDir, "modname.txt");
                if (!File.Exists(infoPath)) return null;
                string name = File.ReadAllText(infoPath);
                var mod = JObject.Parse(name).ToObject<ClientMod>();
                log.Debug("Current active mod: " + mod.name);
                return mod;
            }
            catch (Exception ex)
            {
                log.Error("Problem detecting active mod, assuming there is no active mod.", ex);
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

        public static void SetMod(object state)
        {
            activeMod = GetActiveMod();
            var op = state as SetMod;
            if (activeMod != null && Equals(activeMod, op.Mod) && activeMod.version == op.Mod.version) return;
            try
            {
                if (Directory.Exists(modDir))
                    Directory.Delete(modDir, true);
                log.Debug("Setting active mod to " + op.Mod.name + ".");
                FileSystem.CopyDirectory(Path.Combine(d2mpDir, op.Mod.name), modDir);
                File.WriteAllText(Path.Combine(modDir, "modname.txt"),
                    JObject.FromObject(op.Mod).ToString(Formatting.Indented));
                notifier.Notify(1, "Active mod", "The current active mod has been set to " + op.Mod.name + ".");
                refreshMods();
                //icon.DisplayBubble("Set active mod to " + op.Mod.name + "!");
            }
            catch (Exception ex)
            {
                log.Error("Can't set mod " + op.Mod.name + ".", ex);
                notifier.Notify(4, "Active mod", "Unable to set active mod, try closing Dota first.");
                //icon.DisplayBubble("Unable to set active mod, try closing Dota first.");
            }
        }

        private static void ModGameInfo()
        {
            string path = Path.Combine(Settings.dotaDir, @"dota\gameinfo.txt");
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
                        notifier.Notify(2, "Added mod", "Restarting Dota 2 to apply changes...");
                        //icon.DisplayBubble("Restarting Dota 2 for you...");
                        KillDota2();
                        LaunchDota2();
                    }
                }
            }
        }

        private static void UnmodGameInfo()
        {
            string path = Path.Combine(Settings.dotaDir, @"dota\gameinfo.txt");
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

        private static void NotifyMessage(object state)
        {
            var op = state as NotifyMessage;
            MessageBox.Show(op.message.message, op.message.title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (op.message.shutdown)
            {
                shutDown = true;
            }
        }

        public static void DeleteMod(object state)
        {
            var op = state as DeleteMod;
            string targetDir = Path.Combine(d2mpDir, op.Mod.name);
            if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
            log.Debug("Server/user requested that we delete mod " + op.Mod.name + ".");
            var msg = new OnDeletedMod
            {
                Mod = op.Mod
            };
            Send(JObject.FromObject(msg).ToString(Formatting.None));
            var existing = modController.clientMods.FirstOrDefault(m => m.name == op.Mod.name);
            if (existing != null) modController.clientMods.Remove(existing);
            refreshMods();
        }

        public static void InstallMod(object state)
        {
            var op = state as InstallMod;
            if (isInstalling)
            {
                notifier.Notify(3, "Already downloading a mod", "Please try again after a few seconds.");
                //icon.DisplayBubble("Already downloading a mod!");
                return;
            }
            isInstalling = true;
            notifier.Notify(5, "Downloading mod", "Downloading " + op.Mod.name + "...");
            //icon.DisplayBubble("Attempting to download "+op.Mod.name+"...");

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
                    int lastProgress = -1;
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        notifier.reportProgress(e.ProgressPercentage);
                        if (e.ProgressPercentage % 5 == 0 && e.ProgressPercentage > lastProgress)
                        {
                            lastProgress = e.ProgressPercentage;
                            log.Info(String.Format("Downloading mod: {0}% complete.", e.ProgressPercentage));
                        }
                    };
                    wc.DownloadDataCompleted += (sender, e) =>
                    {
                        byte[] buffer = { 0 };
                        try
                        {
                            buffer = e.Result;
                        }
                        catch
                        {
                            notifier.Notify(4, "Error downloading mod", "The connection forcibly closed by the remote host. Please try again.");
                        }
                        notifier.Notify(2, "Extracting mod", "Download completed, extracting files...");
                        Stream s = new MemoryStream(buffer);
                        if (UnzipFromStream(s, targetDir))
                        {
                            refreshMods();
                            log.Info("Mod installed!");
                            notifier.Notify(1, "Mod installed",
                                "The following mod has been installed successfully: " + op.Mod.name);
                            //icon.DisplayBubble("Mod downloaded successfully: " + op.Mod.name + ".");
                            var msg = new OnInstalledMod()
                            {
                                Mod = op.Mod
                            };
                            Send(JObject.FromObject(msg).ToString(Formatting.None));
                            var existing = modController.clientMods.FirstOrDefault(m => m.name == op.Mod.name);
                            if (existing != null) modController.clientMods.Remove(existing);
                            modController.clientMods.Add(op.Mod);
                        }
                        isInstalling = false;
                    };
                    wc.DownloadDataAsync(new Uri(op.url));
                }
            }
            catch (Exception ex)
            {
                isInstalling = false;
                log.Error("Failed to download mod " + op.Mod.name + ".", ex);
                notifier.Notify(4, "Error downloading mod", "Failed to download mod " + op.Mod.name + ".");
                return;
            }
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

        public static void showModManager()
        {
            Form frm = Application.OpenForms["modManager"];
            if (frm != null) frm.Close();
            modManager manager = new modManager();
            manager.Show();
        }

        private static void refreshMods()
        {
            modManager modFrm = (modManager)Application.OpenForms["modManager"];
            if (modFrm != null)
            {
                modFrm.InvokeIfRequired(() => { modFrm.refreshTable(); });
            }

        }

        public static void showPreferences()
        {
            settingsForm.Show();
            settingsForm.Focus();
        }

        public static void showCredits()
        {
            creditsForm frm = new creditsForm();
            frm.Show();
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

        public string FindSteam(bool delCache, bool useProtocol = true)
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

                if (useProtocol)
                {
                    Process.Start("steam://");
                    int tries = 0;
                    while (tries < 20)
                    {
                        Process[] processes = Process.GetProcessesByName("STEAM");
                        if (processes.Length > 0)
                        {
                            string dir = processes[0].MainModule.FileName.Substring(0, processes[0].MainModule.FileName.Length - 9);
                            if (Directory.Exists(dir))
                            {
                                cachedLocation = dir;
                                Settings.steamDir = dir;
                                return cachedLocation;

                            }
                        }
                        else
                        {
                            Thread.Sleep(500);
                            tries++;
                        }
                    }
                }

                return null;
            }
            return cachedLocation;
        }

        public string FindDota(bool delCache, bool useProtocol = true)
        {
            if (!delCache && cachedDotaLocation != null) return cachedDotaLocation;
            string steamDir = FindSteam(false);
            //Get from registry
            RegistryKey regKey = Registry.LocalMachine;
            regKey =
                regKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570");
            if (regKey != null)
            {
                string dir = regKey.GetValue("InstallLocation").ToString();
                if (checkDotaDir(dir))
                {
                    cachedDotaLocation = regKey.GetValue("InstallLocation").ToString();
                    return cachedDotaLocation;
                }
            }

            if (steamDir != null)
            {
                string dir = Path.Combine(steamDir, @"steamapps\common\dota 2 beta\");
                if (checkDotaDir(dir))
                {
                    cachedDotaLocation = dir;
                    return dir;
                }
            }
            if (useProtocol)
            {
                Process.Start("steam://rungameid/570");
                int tries = 0;
                while (tries < 20)
                {
                    Process[] processes = Process.GetProcessesByName("DOTA");
                    if (processes.Length > 0)
                    {
                        string dir = processes[0].MainModule.FileName.Substring(0, processes[0].MainModule.FileName.Length - 8);
                        processes[0].Kill();
                        if (checkDotaDir(dir))
                        {
                            cachedLocation = dir;
                            Settings.dotaDir = dir;
                            return cachedLocation;
                        }

                    }
                    else
                    {
                        Thread.Sleep(500);
                        tries++;
                    }
                }
            }
            return null;
        }
        public static bool checkDotaDir(string path)
        {
            return Directory.Exists(path) && Directory.Exists(Path.Combine(path, "dota")) && File.Exists(Path.Combine(path, "dota/gameinfo.txt"));
        }
        public bool checkProtocol()
        {
            RegistryKey regKey = Registry.ClassesRoot;
            regKey = regKey.OpenSubKey(@"steam");
            if (regKey != null)
            {
                string protocolVal = regKey.GetValue(null).ToString();
                if (protocolVal.Contains("URL:steam protocol"))
                {
                    var commandKey = regKey.OpenSubKey(@"Shell\Open\Command");
                    if (commandKey != null && commandKey.GetValue(null) != null)
                        return true;
                }
            }
            return false;
        }
    }
}