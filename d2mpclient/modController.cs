// 
// modController.cs
// Created by ilian000 on 2014-06-13
// Licenced under the Apache License, Version 2.0
//

using System.Threading;
using ClientCommon.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace d2mp
{
    static class modController
    {
#if DEV
        private const string modUrlCheck = "http://104.131.255.27:3000/data/mods";
#else
        private const string modUrlCheck = "http://d2modd.in/data/mods";
#endif
        private const string modCDN = "https://s3-us-west-2.amazonaws.com/d2mpclient/";
        public static List<RemoteMod> remoteMods = new List<RemoteMod>();
        public static List<ClientMod> clientMods = new List<ClientMod>();
        public static Queue<RemoteMod> installQueue = new Queue<RemoteMod>();
        private static bool installing = false;

        static modController()
        {
            getLocalMods();
        }

        public static List<ClientMod> getLocalMods()
        {
            clientMods.Clear();
            string[] dirs = Directory.GetDirectories(D2MP.d2mpDir);
            foreach (string modName in dirs.Select(Path.GetFileName))
            {
                D2MP.log.Debug("Found mod: " + modName + " detecting version...");
                string infoPath = Path.Combine(D2MP.d2mpDir, modName + @"\addoninfo.txt");
                string versionFile = "";
                if (File.Exists(infoPath))
                {
                    versionFile = File.ReadAllText(infoPath);
                }

                string version = ReadAddonVersion(versionFile);

                if (!string.IsNullOrEmpty(version))
                {
                    clientMods.Add(new ClientCommon.Data.ClientMod {name = modName, version = version});
                    D2MP.log.Debug(modName + "=" + version);
                }
                else
                {
                    D2MP.log.Error("Can't find version info for mod: " + modName + ", not including");   
                }
            }
            return clientMods;
        }

        public static List<RemoteMod> getRemoteMods()
        {
            string modString = string.Empty;

            using (var wc = new WebClient())
                modString = wc.DownloadString(modUrlCheck);

            JArray msg = JArray.Parse(modString);

            lock (remoteMods)
            {
                remoteMods.Clear();
                foreach (var mod in msg.Where(m => m["playable"].Value<bool>()))
                {
                    remoteMods.Add(new RemoteMod
                    {
                        name = mod["name"].Value<string>(),
                        fullname = mod["fullname"].Value<string>(),
                        version = mod["version"].Value<string>(),
                        author = mod["author"].Value<string>(),
                        url = mod["website"].Value<string>()
                    });
                }
            }

            checkUpdates();
            checkAvailable();

            return remoteMods;
        }

        public static string ReadAddonVersion(string wholeAddonFile)
        {
            Match match = Regex.Match(wholeAddonFile, @"(addonversion)(\s+)(\d+\.)?(\d+\.)?(\d+\.)?(\*|\d+)",
                    RegexOptions.IgnoreCase);

            string version = string.Empty;
            
            if (match.Success)
            {
                version = match.Groups.Cast<Group>()
                    .ToList()
                    .Skip(3)
                    .Aggregate("", (current, part) => current + part.Value);
            }

            return version;
        }

        /// <summary>
        /// Checks for mods with different version on server
        /// </summary>
        /// <returns>Returns a list of mods with different version on server</returns>
        public static List<RemoteMod> checkUpdates()
        {
            foreach (var mod in remoteMods)
            {
                mod.needsUpdate = clientMods.Any(a => a.name == mod.name && a.version != mod.version);
            }
            return remoteMods.FindAll(a => a.needsUpdate);
        }

        /// <summary>
        /// Checks for mods that are not yet installed on the client
        /// </summary>
        /// <returns>Returns a list of mods which are not yet installed on the client</returns>
        public static List<RemoteMod> checkAvailable()
        {
            foreach (var mod in remoteMods)
            {
                mod.needsInstall = clientMods.All(a => a.name != mod.name);
            }
            return remoteMods.FindAll(a => a.needsInstall);
        }

        /// <summary>
        /// Start the install process of all queued mods
        /// </summary>
        public static void InstallQueued()
        {
            if (!installing)
            {
                installing = true;
                ThreadPool.QueueUserWorkItem(InstallProcess);
            }
        }

        private static void InstallProcess(object o)
        {
            RemoteMod currentMod = null;

            while (installQueue.Count > 0 && !D2MP.shutDown)
            {
                Thread.Sleep(1000);

                if (!D2MP.isInstalling && !D2MP.dlRetry)
                {
                    currentMod = installQueue.Dequeue();
                    D2MP.SendRequestMod(currentMod.name);
                    Thread.Sleep(1000);
                }
            }

            installing = false;
        }
    }

    class RemoteMod
    {
        public string name { get; set; }
        public string fullname { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string url { get; set; }
        public bool needsUpdate { get; set; }
        public bool needsInstall { get; set; }

        /// <summary>
        /// Used by Queue.Contains to specify same object
        /// </summary>
        class Comparer : IEqualityComparer<RemoteMod>
        {
            public bool Equals(RemoteMod x, RemoteMod y)
            {
                return x.name == y.name;
            }

            public int GetHashCode(RemoteMod obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
