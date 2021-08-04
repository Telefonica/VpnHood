﻿using System;
using System.Net;

namespace VpnHood.Server
{
    public class ServerInfo
    {
        public Version Version { get; set; }
        public Version EnvironmentVersion { get; set; }
        public string OsVersion { get; set; }
        public IPAddress LocalIp { get; set; }
        public IPAddress PublicIp { get; set; }
        public long TotalMemory { get; set; }
        public long FreeMemory { get; set; }
        public string MachineName { get; set; }
    }
}