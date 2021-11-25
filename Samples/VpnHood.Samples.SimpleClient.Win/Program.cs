using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using VpnHood.Client;
using VpnHood.Client.Device.WinDivert;
using VpnHood.Common;
using VpnHood.Logging;
using VpnHood.Client.Device;
using System.Net;

namespace VpnHood.Samples.SimpleClient.Win
{
    internal class HanzoVpnHoodClient : VpnHoodClient {
        public HanzoVpnHoodClient(IPacketCapture packetCapture, Guid clientId, Token token, ClientOptions options) :
            base(packetCapture, clientId, token, options)
        {
            
        }
    }
    class Program
    {
        static IpRange[] targetRange = new IpRange[] { IpNetwork.Parse("13.107.64.0/18").ToIpRange(), IpNetwork.Parse("52.112.0.0/14").ToIpRange() };

        static string VolumnFormating(float v)
        {
            string result = v.ToString("F00") + "Bytes";
            if (v < 1024) return result;
            if (v >= 1024 && v < 1024 * 1024)
            {
                result = (v/1024.0).ToString("F01") + "KB";
            }else
            {
                if (v >= 1024*1024 && v < 1024 * 1024 * 1024)
                {
                    result = (v / 1024.0 / 1024.0).ToString("F01") + "MB";
                }
                else
                {
                    result = (v / 1024.0 / 1024.0 / 1024.0).ToString("F01") + "GB";
                }
            }
            return result;
        }
        static string SpeedFormating(float v)
        {
            v = v * 8;
            string result = v.ToString("F00") + "bps";
            if (v < 1000)
            {
                return result;
            }
            if (v >= 1000 && v < 1000000)
            {
                result = (v / 1000.0).ToString("F01") + "kbps";
            }
            else
            {
                if (v >= 1000000 && v < 1000000000)
                {
                    result = (v / 1000000.0).ToString("F01") + "mbps";
                }
                else
                {
                    result = (v / 1000000000.0).ToString("F01") + "gbps";
                }
            }
            return result;
        }

        static void PacketCapture_OnPacketReceivedFromInbound(object sender, VpnHood.Client.Device.PacketReceivedEventArgs e)
        {
            foreach (var ipPacket in e.IpPackets)
            {
                if (IpRange.IsInRange(targetRange, ipPacket.DestinationAddress))
                {
                    if (ipPacket.Protocol == PacketDotNet.ProtocolType.Udp)
                    {
                        //Console.WriteLine("HeaderLen: {0} {1} {2} {3} {4}", ipPacket.HeaderLength, ipPacket.HeaderData[1], (ipPacket.HeaderData[2] >> 2) & 0x3f, ipPacket.DestinationAddress.ToString(), ipPacket.Protocol);
                    }
                }

            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello VpnClient!");

            // clientId should be generated for each client

            // accessKey must obtain from the server
            var clientId = Guid.Parse("7BD6C156-EEA3-43D5-90AF-B118FE47ED0A");
            var accessKey = "vh://eyJuYW1lIjpudWxsLCJ2IjoxLCJzaWQiOjEsInRpZCI6IjAyZmUyOGM3LWZlZmItNDQ5YS04NWMzLWUwNzZmNjc1MmY4OSIsInNlYyI6ImY0QUdhRThJcnJZeHdOZjJmYmpZZEE9PSIsImRucyI6InZhLmN5dmFlc2hhcmUubmV0IiwiaXN2ZG5zIjpmYWxzZSwiY2giOiJFSjBYNXdBYWQzVEpKL1NFVGU5S1FEeHF1dUk9IiwiZXAiOlsiODEuNDcuMjAwLjE3Mzo5NDQzIl0sInBiIjpmYWxzZSwidXJsIjpudWxsfQ==";
            
            if (args.Length > 0)
            {
                accessKey = args[0];
            }

            Console.WriteLine("AccessKey: " + accessKey);
            
            var token = Token.FromAccessKey(accessKey);
             
            var packetCapture = new WinDivertPacketCapture();
            // https://techcommunity.microsoft.com/t5/microsoft-teams/ports-needed-for-microsoft-teams/m-p/28417
            //packetCapture.IncludeNetworks = new IpNetwork[] { IpNetwork.Parse("13.107.64.0/18"), IpNetwork.Parse("52.112.0.0/14") };

            var options = new ClientOptions() { };
            options.UseUdpChannel = false;
            //options.DnsServers = new System.Net.IPAddress[] { System.Net.IPAddress.Parse("8.8.8.8") };
            // options.IncludeIpRanges = new IpRange[] { IpNetwork.Parse("13.107.64.0/18").ToIpRange(), IpNetwork.Parse("52.112.0.0/14").ToIpRange() };

            //packetCapture.OnPacketReceivedFromInbound += PacketCapture_OnPacketReceivedFromInbound;
              
            var vpnHoodClient = new VpnHoodClient(packetCapture, clientId, token, options);

            vpnHoodClient.Connect().Wait();
            Console.WriteLine("-VpnHood Client Is Running! Open your browser and browse the Internet! Press Ctrl+C to stop. State={0}", vpnHoodClient.State);
            while (vpnHoodClient.State == ClientState.Disposed)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Connecting");
            }

            while (vpnHoodClient.State == ClientState.Connected)
            {
                Console.WriteLine("SEND: {0} {1} | RECEIVE: {2} {3}", SpeedFormating(vpnHoodClient.SendSpeed), 
                                                                                    VolumnFormating(vpnHoodClient.SentByteCount),
                                                                                    SpeedFormating(vpnHoodClient.ReceiveSpeed), 
                                                                                    VolumnFormating(vpnHoodClient.ReceivedByteCount));
                Thread.Sleep(1000);
            }

            Console.WriteLine("Done");
                
        }
    }
}
