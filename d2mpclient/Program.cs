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
// <summary>Entry point for the d2moddin plugin.</summary>
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net.Config;

namespace d2mp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //check to see if we are already running
            string pid = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), D2MP.PIDFile);
            //delete the pid to shut down the other instance
            if (File.Exists(pid)) File.Delete(pid);

            if (CheckSettings())
            {
                MessageBox.Show("Your user.config file was corrupted and needed to be deleted.\nAll your settings have been reset to default.\nNo further action is required.",
                    "D2Moddin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            XmlConfigurator.Configure();
            D2MP.main();
        }

        static bool CheckSettings()
        {
            try
            {
                Properties.Settings.Default.Reload();
                return false;
            }
            catch (ConfigurationErrorsException ex)
            {
                string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;

                File.Delete(filename);
                CheckSettings();
                return true;
            }
        }
    }
}
