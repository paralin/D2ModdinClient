// 
// modController.cs
// Created by ilian000 on 2014-06-13
// Licenced under the Apache License, Version 2.0
//
            
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
        private const string modUrlCheck = "http://d2modd.in/data/mods";
        private const string modCDN = "https://s3-us-west-2.amazonaws.com/d2mpclient/";
        private static List<RemoteMod> remoteMods = new List<RemoteMod>();
        public static List<ClientCommon.Data.ClientMod> clientMods = new List<ClientCommon.Data.ClientMod>();

        public static List<ClientCommon.Data.ClientMod> getLocalMods()
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
                Match match = Regex.Match(versionFile, @"(addonversion)(\s+)(\d+\.)?(\d+\.)?(\d+\.)?(\*|\d+)",
                    RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string version = match.Groups.Cast<Group>()
                        .ToList()
                        .Skip(3)
                        .Aggregate("", (current, part) => current + part.Value);
                    D2MP.log.Debug(modName + "=" + version);
                    clientMods.Add(new ClientCommon.Data.ClientMod { name = modName, version = version });
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
            JArray msg;
            using (var wc = new WebClient())
            {
                msg = JArray.Parse(wc.DownloadString(modUrlCheck));
                remoteMods.Clear();
                foreach (var mod in msg)
                {
                    remoteMods.Add(new RemoteMod { name = mod["name"].Value<string>(), fullname = mod["fullname"].Value<string>() , version = mod["version"].Value<string>(), author = mod["author"].Value<string>() });
                }
            }
            checkUpdates();
            checkAvailable();
            return remoteMods;
        }

        /// <summary>
        /// Checks for mods with different version on server
        /// </summary>
        /// <returns>Returns a list of mods with different version on server</returns>
        public static List<RemoteMod> checkUpdates()
        {
            var results =
                from rMod in remoteMods
                where clientMods.Any(cMod => cMod.name == rMod.name && cMod.version != rMod.version)
                select rMod;
            List<RemoteMod> updateMods = results.ToList();
            remoteMods.ForEach(rMod =>{
                if (updateMods.Any(uMod => rMod.name == uMod.name)) { rMod.needsUpdate = true; } else { rMod.needsUpdate = false; }
            });
   
            return updateMods;
        }

        /// <summary>
        /// Checks for mods that are not yet installed on the client
        /// </summary>
        /// <returns>Returns a list of mods which are not yet installed on the client</returns>
        public static List<RemoteMod> checkAvailable()
        {
            var results =
              from rMod in remoteMods
              where clientMods.All(cMod => cMod.name != rMod.name)
              select rMod;
            List<RemoteMod> availableMods = results.ToList();
            remoteMods.ForEach(rMod =>
            {
                rMod.needsInstall = availableMods.Any(iMod => rMod.name == iMod.name);
            });
            return availableMods;
        }
    }

    class RemoteMod
    {
        public string name { get; set;}
        public string fullname { get; set; }
        public string version { get; set;}
        public string author { get; set; }
        public string url { get; set; }
        public bool needsUpdate { get; set; }
        public bool needsInstall { get; set; }
    }
}
