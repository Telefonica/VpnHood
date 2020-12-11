﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using VpnHood.Common;

namespace VpnHood.App.Launcher
{
    class Program
    {
        static int Main(string[] args)
        {
            var moduleFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var jsonFilePath = Path.Combine(moduleFolder, "publish.json");

            // read run.json
            var json = File.ReadAllText(jsonFilePath);
            var launcherInfo = JsonSerializer.Deserialize<PublishInfo>(json);
            var launchPath = Path.Combine(moduleFolder, launcherInfo.LaunchPath);

            // create processStartInfo
            ProcessStartInfo processStartInfo = new() { FileName = "dotnet" };
            processStartInfo.ArgumentList.Add(launchPath);
            if (args != null)
            {
                foreach (var arg in args)
                    if (arg != "/delaystart" && arg != "/nowait")
                        processStartInfo.ArgumentList.Add(arg);
            }

            // delayStart
            bool delay = !args.Contains("/delaystart");
            if (delay)
                Thread.Sleep(2000);

            // Start process
            var process = Process.Start(processStartInfo);

            // wait for any error or early exit to share the console properly
            // exit this process to allow later updates
            bool wait = !args.Contains("/nowait");
            process.WaitForExit(wait ? -1 : 5000);
            return process.HasExited ? process.ExitCode : 0;
        }
    }
}
