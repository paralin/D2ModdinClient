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
using System.ComponentModel;
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
        public const string PIDFile = "d2mp.pid";
#if DEV||DEBUG
        private static string server = "ws://172.250.79.95:4502/ClientController";
#else
        private static string server = "ws://net1.d2modd.in:4502/ClientController";
#endif
        private const string installerURL = "https://s3-us-west-2.amazonaws.com/d2mpclient/D2MPLauncher.exe";
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
        public static volatile bool isInstalling;
        private static bool hasConnected = false;
        private static XSocketClient client;
        private static List<string> steamids = new List<string>();

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

        private static void Wait(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                if( shutDown ) break;
                
                Thread.Sleep(1000);
            }
        }

        private static void SendInit()
        {
            log.Debug("Sending init, version: " + ClientCommon.Version.ClientVersion);
            var init = new Init
            {
                SteamIDs = steamids.ToArray(),
                Version = ClientCommon.Version.ClientVersion,
                Mods = modController.clientMods.ToArray()
            };
            var json = JObject.FromObject(init).ToString(Formatting.None);
            Send(json);
            ThreadPool.QueueUserWorkItem(new WaitCallback(a => AutoUpdateMods(false)));
        }

        private static void SetupClient()
        {
            client = new XSocketClient(server, "*");
            client.OnOpen += (sender, args) =>
            {
                notifier.Notify(NotificationType.Success, hasConnected ? "Reconnected" : "Connected", hasConnected ? "Connection to the server has been reestablished" : "Client has been connected to the server.");
                hasConnected = true;
                SendInit();
            };

            client.Bind("commands", e =>
            {
                log.Debug("server: " + e.data);
                JObject msg = JObject.Parse(e.data);
                switch (msg["msg"].Value<string>())
                {
                    case Shutdown.Msg:
                        log.Debug("Shutting down due to server request. Client not up to date.");
                        notifier.Notify(NotificationType.Info, "Outdated version", "Updating to new version...");
                        updateClient();
                        shutDown = true;
                        return;
                    case ClientCommon.Methods.Uninstall.Msg:
                        log.Debug("Uninstalling due to server request...");
                        Uninstall();
                        shutDown = true;
                        return;
                    case ClientCommon.Methods.InstallMod.Msg:
                        SendPing(ClientCommon.Methods.InstallMod.Msg);
                        ThreadPool.QueueUserWorkItem(InstallMod, msg.ToObject<InstallMod>());
                        break;
                    case ClientCommon.Methods.DeleteMod.Msg:
                        ThreadPool.QueueUserWorkItem(DeleteMod, msg.ToObject<DeleteMod>());
                        break;
                    case ClientCommon.Methods.SetMod.Msg:
                        SendPing(ClientCommon.Methods.SetMod.Msg);
                        ThreadPool.QueueUserWorkItem(SetMod, msg.ToObject<SetMod>());
                        break;
                    case ClientCommon.Methods.ConnectDota.Msg:
                        SendPing(ClientCommon.Methods.ConnectDota.Msg);
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
                    case ClientCommon.Methods.UpdateMods.Msg:
                        ThreadPool.QueueUserWorkItem(UpdateMods, msg.ToObject<UpdateMods>());
                        break;
                    default:
                        log.Error("Command not recognized.");
                        break;
                }
            });

            client.OnError += (sender, args) => log.Error(string.Format("Controller [{0}] sent us error [{1}] on event [{2}].", args.controller, args.data, args.@event));

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
                wc.DownloadFile(installerURL, updaterPath);
                Process.Start(updaterPath);
                Application.Exit();
            }
        }

        private static void HandleClose()
        {
            if (shutDown) return;

            if (hasConnected)
            {
                notifier.Notify(NotificationType.Warning, "Lost connection", "Attempting to reconnect...");
                hasConnected = false;
            }

            while (!shutDown)
            {
                SetupClient();
                try
                {
                    client.Open();
                    break;
                }
                catch
                {
                    Wait(10);
                }
            }
        }

        /// <summary>
        /// Pipe a zip download directly through the decompressor
        /// </summary>
        private static bool UnzipWithTemp(InstallMod op, Stream zipStream, string outFolder)
        {
            try
            {
                string tempFolder = Path.Combine(outFolder, "temp");
                UnZip.unzipFromStream(zipStream, tempFolder);
            }
            catch (Exception ex)
            {
                log.Error("Error extracting files. Downloaded archive is possibly corrupt." , ex);
                if (op != null)
                    AskTryAgain(op, "Error Extracting Files", "Downloaded archive is possibly corrupt");
                return false;
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

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Error moving extracted files from temporary folder.", ex);
                if (op != null)
                    AskTryAgain(op, "Error Installing Mod", "Error moving extracted files from temporary folder");
            }

            return false;
        }

        public static void SendPing(string msg)
        {
            log.DebugFormat("Sending ACK for command [{0}]", msg);
            client.Ping(new byte[] {1});
        }

        public static void SendRequestMod(string mod)
        {
            log.DebugFormat("Sending mod request: {0}", mod);
            var req = new RequestMod()
            {
                Mod = new ClientMod() { name = mod }
            };
            var json = JObject.FromObject(req).ToString(Formatting.None);
            Send(json);
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
            info.UseShellExecute = false;
            Process.Start(info);
        }

        private static void AutoUpdateMods(bool force)
        {
            if (force || Settings.autoUpdateMods)
            {
                try
                {
                    List<RemoteMod> lstRemote = modController.getRemoteMods();

                    //find all local mods that aren't in the remote list
                    modController.clientMods.FindAll(a => a.name != "checker" && lstRemote.All(b => b.name != a.name))
                        .ForEach((mod) => DeleteMod(new DeleteMod() {Mod = mod}));

                    lstRemote.FindAll(a => a.needsUpdate)
                        .ForEach(mod => modController.installQueue.Enqueue(mod));

                    if (modController.installQueue.Count > 0)
                        modController.InstallQueued();
                }
                catch (Exception ex)
                {
                    log.Error("Could not auto update mods.", ex);
                }
            }
        }

        public static void main()
        {
            log.Debug("D2MP starting...");

            Application.ThreadException += (sender, args) => log.Error("Unhandled thread exception.", args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => log.Error("Unhandled domain exception.", (Exception) args.ExceptionObject);

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

            if (Settings.createShortcutAtStartup)
                ShortcutWriter.writeDesktopShortcut();

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
                else
                {
                    var contents = Directory.GetDirectories(addonsDir);
                    foreach (var dir in contents)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Can't delete addon dir, "+dir, ex);
                        }
                    }
                }
                if (!Directory.Exists(d2mpDir))
                    Directory.CreateDirectory(d2mpDir);
                if (!Directory.Exists(modDir))
                    Directory.CreateDirectory(modDir);

                modController.getLocalMods();

                Dictionary<int, string> usersDict = steam.FindUsers();
                if (usersDict.Count > 0)
                {
                    steamids.AddRange(usersDict.OrderByDescending(x => x.Key).Select(m=>m.Value));
                }
                else
                {
                    log.Fatal("Could not detect steam ID.");
                    return;
                }

                //Modify gameinfo.txt
                ModGameInfo();

                log.Debug("Starting shutdown file watcher...");
                string pathToShutdownFile = Path.Combine(ourDir, PIDFile);
                File.WriteAllText(pathToShutdownFile, "Delete this file to shutdown D2MP.");

                var watcher = new FileSystemWatcher();
                watcher.Path = ourDir;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName;
                watcher.Filter = PIDFile;
                watcher.Deleted += (sender, args) => { shutDown = true; };
                watcher.EnableRaisingEvents = true;

                try
                {
                    SetupClient();
                    client.Open();
                }
                catch (Exception)
                {
                    notifier.Notify(NotificationType.Error, "Server error", "Can't connect to the lobby server!");
                    Wait(5);
                    HandleClose();
                }
                while (!shutDown)
                {
                    Thread.Sleep(100);
                }

                if (client != null && client.IsConnected) client.Close();
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

        public static void UpdateMods(object state)
        {
            AutoUpdateMods(true);
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
                notifier.Notify(NotificationType.Success, "Active mod", "The current active mod has been set to " + op.Mod.name + ".");
                refreshMods();
                //icon.DisplayBubble("Set active mod to " + op.Mod.name + "!");
            }
            catch (Exception ex)
            {
                log.Error("Can't set mod " + op.Mod.name + ".", ex);
                notifier.Notify(NotificationType.Error, "Active mod", "Unable to set active mod, try closing Dota first.");
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
                        notifier.Notify(NotificationType.Info, "Added mod", "Restarting Dota 2 to apply changes...");
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
        /// <summary>
        /// Used to check if we already tried to redownload the mod.
        /// </summary>
        public static bool dlRetry;
        public static void InstallMod(object state)
        {
            var op = state as InstallMod;
            if (isInstalling)
            {
                notifier.Notify(NotificationType.Warning, "Already downloading a mod", "Please try again after a few seconds.");
                return;
            }
            isInstalling = true;
            notifier.Notify(NotificationType.Progress, "Downloading mod", "Downloading " + op.Mod.name + "...");

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
                        catch(Exception ex)
                        {
                            log.Error("Error downloading mod", ex);
                            AskTryAgain(op, "Error Downloading Mod", "The connection was forcibly closed by the remote host");
                            return;
                        }
                        notifier.Notify(NotificationType.Info, "Extracting mod", "Download completed, extracting files...");
                        Stream s = new MemoryStream(buffer);
                        if (UnzipWithTemp(op, s, targetDir))
                        {
                            refreshMods();
                            log.Info("Mod installed!");
                            notifier.Notify(NotificationType.Success, "Mod installed",
                                "The following mod has been installed successfully: " + op.Mod.name);
                            var msg = new OnInstalledMod()
                            {
                                Mod = op.Mod
                            };
                            Send(JObject.FromObject(msg).ToString(Formatting.None));
                            var existing = modController.clientMods.FirstOrDefault(m => m.name == op.Mod.name);
                            if (existing != null) modController.clientMods.Remove(existing);
                            modController.clientMods.Add(op.Mod);
                            isInstalling = false;
                            dlRetry = false;
                        }
                    };
                    wc.DownloadDataAsync(new Uri(op.url));
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to download mod " + op.Mod.name + ".", ex);
                AskTryAgain(op, "Error Downloading Mod", "Error downloading mod " + op.Mod.name);
            }
        }

        private static void AskTryAgain(InstallMod op, string title, string msg)
        {
            isInstalling = false;
            dlRetry = true;
            notifier.NotifyTryAgain(title, msg,
                () => InstallMod(op), //try again
                () => //cancel
                {
                    dlRetry = false;
                    log.Info("User don't want to try again");
                },
                () => //download manually
                {
                    dlRetry = false;
                    log.Info("Downloading mod manually");
                    Process.Start(op.url);
                });
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
                modFrm.InvokeIfRequired(modFrm.refreshTable);
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

        internal static void manualInstallMod()
        {
            if (isInstalling)
            {
                notifier.Notify(NotificationType.Warning, "Already downloading a mod", "Please try again after a few seconds.");
                return;
            }
            isInstalling = true;

            using (var dlg = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "zip",
                Filter = "Zip Files|*.zip",
                FilterIndex = 1,
                Multiselect = false,
                Title = "Choose the mod zip to install"
            })
            {
                //user pressed ok
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    //open the file in a stream
                    using (var fileStream = dlg.OpenFile())
                    {
                        ZipFile zip = new ZipFile(fileStream);

                        //check integrity
                        if (zip.TestArchive(true))
                        {
                            //look for the map file. It contains the mod name
                            ZipEntry map = zip.Cast<ZipEntry>().FirstOrDefault(a => a.Name.ToLower().EndsWith(".bsp"));

                            if (map != null)
                            {
                                //look for the version file
                                int entry = zip.FindEntry("addoninfo.txt", true);
                                if (entry >= 0)
                                {
                                    string allText = string.Empty;

                                    using (var infoStream = new StreamReader(zip.GetInputStream(entry)))
                                        allText = infoStream.ReadToEnd();

                                    string version = modController.ReadAddonVersion(allText);

                                    if (!string.IsNullOrEmpty(version))
                                    {
                                        Version v = new Version(version);
                                        string name = Path.GetFileNameWithoutExtension(map.Name).ToLower();

                                        //check if this same mod is already installed and if it needs an update
                                        if (modController.clientMods.Any(
                                            a => a.name.ToLower().Equals(name) && new Version(a.version) >= v))
                                        {
                                            MessageBox.Show("The mod you are trying to install is already installed or outdated.", "Mod Manual Install",
                                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                        else
                                        {
                                            string targetDir = Path.Combine(d2mpDir, name);
                                            if (Directory.Exists(targetDir))
                                                Directory.Delete(targetDir, true);
                                            //Make the dir again
                                            Directory.CreateDirectory(targetDir);

                                            if (UnzipWithTemp(null, fileStream, targetDir))
                                            {
                                                refreshMods();
                                                log.Info("Mod manually installed!");
                                                notifier.Notify(NotificationType.Success, "Mod installed", "The following mod has been installed successfully: " + name);

                                                var mod = new ClientMod() {name = name, version = v.ToString()};
                                                var msg = new OnInstalledMod() {Mod = mod};

                                                Send(JObject.FromObject(msg).ToString(Formatting.None));

                                                var existing = modController.clientMods.FirstOrDefault(m => m.name == mod.name);
                                                if (existing != null) modController.clientMods.Remove(existing);
                                                
                                                modController.clientMods.Add(mod);
                                            }
                                            else
                                            {
                                                MessageBox.Show("The mod could not be installed. Read the log file for details.", "Mod Manual Install",
                                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Could not read the mod version from the zip file.", "Mod Manual Install",
                                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("No mod info was found in the zip file.", "Mod Manual Install",
                                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("No mod map was found in the zip file.", "Mod Manual Install",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("The zip file you selected seems to be invalid.", "Mod Manual Install",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
            }

            isInstalling = false;
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
                try
                {
                    regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

                    if (regKey != null)
                    {
                        cachedLocation = regKey.GetValue("SteamPath").ToString();
                        return cachedLocation;
                    }
                }
                catch (Exception ex)
                {
                    D2MP.log.Error("Error trying to read dota path through registry", ex);
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
                            try
                            {
                                string dir = processes[0].MainModule.FileName.Substring(0,
                                    processes[0].MainModule.FileName.Length - 9);
                                if (Directory.Exists(dir))
                                {
                                    cachedLocation = dir;

                                    return cachedLocation;
                                }
                            }
                            catch (Exception ex)
                            {
                                D2MP.log.Error("Error trying to read steam path through process", ex);
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
            try
            {
                regKey = regKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570");
                if (regKey != null)
                {
                    string dir = regKey.GetValue("InstallLocation").ToString();
                    if (checkDotaDir(dir))
                    {
                        cachedDotaLocation = dir;
                        return cachedDotaLocation;
                    }
                }
            }
            catch (Exception ex)
            {
                D2MP.log.Error("Error trying to read dota path through registry", ex);
            }

            if (steamDir != null)
            {
                string dir = Path.Combine(steamDir, @"steamapps\common\dota 2 beta\");
                if (checkDotaDir(dir))
                {
                    cachedDotaLocation = dir;
                    return cachedDotaLocation;
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
                        try
                        {
                            string dir = processes[0].MainModule.FileName.Substring(0, processes[0].MainModule.FileName.Length - 8);
                            processes[0].Kill();
                            if (checkDotaDir(dir))
                            {
                                cachedLocation = dir;

                                return cachedLocation;
                            }

                        }
                        catch (Exception ex)
                        {
                            D2MP.log.Error("Error trying to read dota path through process", ex);
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

        public Dictionary<int, string> FindUsers()
        {
            Dictionary<int, string> usersDict = new Dictionary<int, string>();
            string steamDir = FindSteam(false);

            // Detect steam account id which was logged in most recently
            string config = File.ReadAllText(Path.Combine(steamDir, @"config\loginusers.vdf"));

            MatchCollection idMatches = Regex.Matches(config, "\"\\d{17}\"");
            MatchCollection timestampMatches = Regex.Matches(config, "(?m)(?<=\"Timestamp\".{2}).*$", RegexOptions.IgnoreCase);
            
            if (idMatches.Count > 0)
            {
                for (int i = 0; i < idMatches.Count; i++)
                {
                    try
                    {
                        string steamid = idMatches[i].Value.Trim(' ', '"');
                        string timestamp = timestampMatches[i].Value.Trim(' ', '"');
                        int iTimestamp = Convert.ToInt32(timestamp);
                        D2MP.log.Debug(String.Format("Steam ID detected: {0} with timestamp: {1}", steamid, iTimestamp));
                        usersDict.Add(iTimestamp, steamid);
                    }
                    catch (Exception ex)
                    {
                        D2MP.log.Error("Error finding user", ex);
                    }
                }
            }

            return usersDict;
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