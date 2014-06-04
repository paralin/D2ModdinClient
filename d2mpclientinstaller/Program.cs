// <copyright file="Program.cs">
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
// <summary>Core D2Moddin manager launcher.</summary>
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace D2MPClientInstaller
{
    static class Program
    {
        private static bool doLog = false;
        private static string installdir;
        static void Log(string text)
        {
            if (doLog)
                File.AppendAllText("d2mpinstaller.log", text + "\n");
        }

        static void DeleteOurselves(string path)
        {
            ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
            info.Arguments = "/C timeout 3 & Del \"" + path+"\"";
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            Process.Start(info);
        }

        static void UnzipFromStream(Stream zipStream, string outFolder)
        {
            ZipInputStream zipInputStream = new ZipInputStream(zipStream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;
                // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                // Optionally match entrynames against a selection list here to skip as desired.
                // The unpacked length is available in the zipEntry.Size property.

                byte[] buffer = new byte[4096];     // 4K is optimum

                // Manipulate the output filename here as desired.
                String fullZipToPath = Path.Combine(outFolder, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);

                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                // of the file, but does not waste memory.
                // The "using" will close the stream even if an exception occurs.
                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                }
                zipEntry = zipInputStream.GetNextEntry();
            }
        }

        static void LaunchD2MP(string path)
        {
            var info = new ProcessStartInfo(path);
            info.WorkingDirectory = Path.GetDirectoryName(path);
            Process.Start(info);
        }

        static void ShutdownD2MP()
        {
            var exepath = Path.Combine(installdir, "d2mp.exe");
            var pidpath = Path.Combine(installdir, "d2mp.pid");
            if (Process.GetProcessesByName("d2mp").Length != 0)
            {
                if (File.Exists(pidpath))
                {
                    File.Delete(pidpath);
                    int wait = 0;
                    while (wait < 30 && Process.GetProcessesByName("d2mp").Length != 0)
                    {
                        Thread.Sleep(1000);
                        wait++;
                    }
                }
                var remaining = Process.GetProcessesByName("d2mp");
                foreach (var remain in remaining)
                {
                    remain.Kill();
                    remain.WaitForExit();
                }
            }
        }

        static void UninstallD2MP(string installdir)
        {
            ShutdownD2MP();
            //Delete all files 
            string[] filePaths = Directory.GetFiles(installdir);
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }
        }

        static void ShowError(string message)
        {
            MessageBox.Show(message, "D2Moddin");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log("Finding install directories...");
            installdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "D2MP");
            var verpath = Path.Combine(installdir, "version.txt");
            Log("Temporary dir: " + installdir);
            Log("Verpath: " + verpath);
            if (!Directory.Exists(installdir))
                Directory.CreateDirectory(installdir);
            string infos;
            Log("Checking for client version...");
            using (WebClient client = new WebClient())
            {
                try
                {
                    infos = client.DownloadString("http://ddp2.d2modd.in/clientver");
                }
                catch (Exception e)
                {
                    ShowError("Failed to download the latest client version information. Check your internet connection!");
                    return;
                }
            }

            Log("Client info: \n"+infos);
            var info = infos.Split('|');
            Log("Version string: " + String.Join(",", info));
            var versplit = info[0].Split(':');
            if (versplit[0] == "version" && versplit[1] != "disabled")
            {
                //check for existing installed file
                if (!File.Exists(verpath) || File.ReadAllText(verpath) != versplit[1])
                {
                    Log("Uninstalling old version..");
                    try
                    {
                        UninstallD2MP(installdir);
                    }
                    catch (Exception ex)
                    {
                        Log("Problem uninstalling: " + ex);
                    }
                    Log("Downloading/unzipping new version...");
                    try
                    {
                        using(WebClient client = new WebClient())
                            UnzipFromStream(client.OpenRead(info[1]), installdir);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.ToString());
                        ShowError("Problem downloading new D2Moddin launcher: " + ex);
                    }
                }
            }
            else
            {
                try
                {
                    UninstallD2MP(installdir);
                }
                catch (Exception ex)
                {
                    Log("Problem uninstalling D2MP: " + ex);
                }
            }

            Log("Launching D2MP...");
            ShutdownD2MP();
            LaunchD2MP(Path.Combine(installdir, "d2mp.exe"));
            //delete ourselves
            Log("Deleting ourselves...");
            try
            {
                DeleteOurselves(Assembly.GetExecutingAssembly().Location);
            }
            catch (Exception ex)
            {
                Log("Problem deleting ourselves: " + ex);
            }
            return;
        }
    }
}
