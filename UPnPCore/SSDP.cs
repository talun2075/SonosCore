﻿/*   
Copyright 2006 - 2010 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Globalization;
using System.Text;
using OpenSource.Utilities;

namespace OSTL.UPnP
{
    /// <summary>
    /// The class that handles SSDP Communication
    /// </summary>
    public sealed class SSDP
    {
        private readonly NetworkInfo NetInfo;

        public class InvalidSearchPacketException : Exception
        {
            public InvalidSearchPacketException(String x) : base(x) { }
        }

        private class SearchStruct
        {
            public string ST;
            public IPEndPoint Source;
            public IPEndPoint Local;
        }

        private readonly Random RND = new();
        private readonly LifeTimeMonitor.LifeTimeHandler LifeTimeHandler;
        private readonly LifeTimeMonitor SearchTimer = new();
        private static readonly Random RandomGenerator = new();

        public delegate void PacketHandler(IPEndPoint source, HTTPMessage Packet);
        public delegate void NotifyHandler(IPEndPoint source, IPEndPoint local, Uri LocationURL, bool IsAlive, String USN, String ST, int MaxAge, HTTPMessage Packet);
        public delegate void SearchHandler(String SearchTarget, IPEndPoint src, IPEndPoint local);
        public delegate void SnifferHandler(IPEndPoint source, IPEndPoint dest, HTTPMessage Packet);

        /// <summary>
        /// Packet Sniffing Event
        /// </summary>
        public event SnifferHandler OnSniffPacket;

        public delegate void RefreshHandler();
        /// <summary>
        /// This is triggered to notify a Device its timeout has elapsed
        /// </summary>
        public event RefreshHandler OnRefresh;

        /// <summary>
        /// This is triggered when another Device on the network has sent a NOTIFY
        /// </summary>
        public event NotifyHandler OnNotify;
        /// <summary>
        /// This is triggered when somebody on the network has issued an M-SEARCH
        /// </summary>
        public event SearchHandler OnSearch;

        public int SSDP_EXPIRATION;
        private readonly Hashtable sessions = new();
        private readonly Hashtable usessions = new();

        private readonly SafeTimer NotifyTimer = new();

        /// <summary>
        /// Constructs a new SSDP Server
        /// </summary>
        /// <param name="Expiration">The number of seconds before a Refresh will be triggered</param>
        public SSDP(int Expiration)
        {
            //InstanceTracker.Add(this);
            LifeTimeHandler = SearchTimerSink;
            SearchTimer.OnExpired += LifeTimeHandler;

            SSDP_EXPIRATION = Expiration;
            if (SSDP_EXPIRATION < 5) SSDP_EXPIRATION = 5;
            int MinVal = (int)(SSDP_EXPIRATION * 0.25 * 1000);
            int MaxVal = (int)(SSDP_EXPIRATION * 0.45 * 1000);

            NotifyTimer.OnElapsed += __NotifyCheck;
            NotifyTimer.Interval = RND.Next(MinVal, MaxVal);
            NotifyTimer.AutoReset = true;
            NotifyTimer.Start();

            NetInfo = new NetworkInfo();
            SetupSessions();
        }

        ~SSDP()
        {
            Dispose();
        }

        public void Dispose()
        {
            sessions.Clear();
            OnNotify = null;
            OnRefresh = null;
            OnSearch = null;
            OnSniffPacket = null;
        }

        private void SetupSessions()
        {
            const int SIO_UDP_CONNRESET = -1744830452;
            byte[] inValue = { 0, 0, 0, 0 };     // == false
            byte[] outValue = { 0, 0, 0, 0 };    // initialize to 0
            IPAddress[] ips = NetInfo.GetLocalAddresses();

            foreach (IPAddress addr in ips)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork || addr.ScopeId != 0)
                {
                    if (sessions.ContainsKey(addr) == false)
                    {
                        try
                        {
                            if (addr.AddressFamily == AddressFamily.InterNetwork)
                            {
                                UdpClient session = new(AddressFamily.InterNetwork);
                                try { session.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); }
                                catch (SocketException ex) 
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                try { session.ExclusiveAddressUse = false; }
                                catch (SocketException ex) 
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                if (!Utils.IsMono()) session.Client.Bind(new IPEndPoint(addr, 1900)); else session.Client.Bind(new IPEndPoint(IPAddress.Any, 1900));
                                session.EnableBroadcast = true;
                                session.JoinMulticastGroup(Utils.UpnpMulticastV4Addr, addr);
                                try { session.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue); }
                                catch (SocketException ex) 
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                session.BeginReceive(OnReceiveSink, new object[] { session, new IPEndPoint(addr, ((IPEndPoint)session.Client.LocalEndPoint).Port) });
                                sessions[addr] = session;

                                UdpClient usession = new(AddressFamily.InterNetwork);
                                usession.Client.Bind(new IPEndPoint(addr, 0));
                                try { usession.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue); }
                                catch (SocketException ex) 
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                usession.BeginReceive(OnReceiveSink, new object[] { usession, new IPEndPoint(addr, ((IPEndPoint)session.Client.LocalEndPoint).Port) });
                                usessions[addr] = usession;
                            }

                            if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                UdpClient session = new(AddressFamily.InterNetworkV6);
                                try { session.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); }
                                catch (SocketException ex) 
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                try { session.ExclusiveAddressUse = false; }
                                catch (SocketException ex)
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                if (!Utils.IsMono()) session.Client.Bind(new IPEndPoint(addr, 1900)); else session.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, 1900));
                                session.EnableBroadcast = true;
                                if (addr.IsIPv6LinkLocal) session.JoinMulticastGroup((int)addr.ScopeId, Utils.UpnpMulticastV6Addr2); else session.JoinMulticastGroup((int)addr.ScopeId, Utils.UpnpMulticastV6Addr1);
                                try { session.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue); }
                                catch (SocketException ex)
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                session.BeginReceive(OnReceiveSink, new object[] { session, new IPEndPoint(addr, ((IPEndPoint)session.Client.LocalEndPoint).Port) });
                                sessions[addr] = session;

                                UdpClient usession = new(AddressFamily.InterNetworkV6);
                                usession.Client.Bind(new IPEndPoint(addr, 0));
                                try { usession.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue); }
                                catch (SocketException ex)
                                {
                                    EventLogger.Log(ex,"SSDP");
                                }
                                usession.BeginReceive(OnReceiveSink, new object[] { usession, new IPEndPoint(addr, ((IPEndPoint)session.Client.LocalEndPoint).Port) });
                                usessions[addr] = usession;
                            }
                        }
                        catch (SocketException ex)
                        {
                            EventLogger.Log(ex,"SSDP");
                        } // Sometimes the bind will thru an exception. In this case, we want to skip that interface and move on.
                    }
                }
            }
        }

        private void OnReceiveSink(IAsyncResult result)
        {
            byte[] buffer;
            IPEndPoint ep = null;
            object[] args = (object[])result.AsyncState;
            UdpClient session = (UdpClient)args[0];
            IPEndPoint local = (IPEndPoint)args[1];

            try
            {
                buffer = session.EndReceive(result, ref ep);
                try
                {
                    HTTPMessage Packet = HTTPMessage.ParseByteArray(buffer, 0, buffer.Length);
                    if (Packet != null)
                    {
                        Packet.LocalEndPoint = local;
                        Packet.RemoteEndPoint = ep;
                        ProcessPacket(Packet, Packet.RemoteEndPoint, local);
                    }
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex,"SSDP");
                }
                session.BeginReceive(OnReceiveSink, args);
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex,"SSDP");
                sessions.Remove(local.Address);
            }
        }

        private void __NotifyCheck()
        {
            OnRefresh?.Invoke();
            int MinVal = (int)(SSDP_EXPIRATION * 0.25 * 1000);
            int MaxVal = (int)(SSDP_EXPIRATION * 0.45 * 1000);
            NotifyTimer.Interval = RND.Next(MinVal, MaxVal);
        }

        /// <summary>
        /// Parses a URL
        /// </summary>
        /// <param name="ServiceURL">The URL to Parse</param>
        /// <param name="WebIP">The IPAddress</param>
        /// <param name="Port">The Port Number</param>
        /// <param name="ServiceName">The Object</param>
        static public void ParseURL(String ServiceURL, out String WebIP, out int Port, out String ServiceName)
        {
            Uri NewUri = new(ServiceURL);
            WebIP = NewUri.Host;
            if (NewUri.HostNameType == UriHostNameType.Dns) WebIP = Dns.GetHostEntry(WebIP).AddressList[0].ToString();
            Port = NewUri.Port;
            ServiceName = HTTPMessage.UnEscapeString(NewUri.PathAndQuery);
        }

        private void ProcessPacket(HTTPMessage msg, IPEndPoint src, IPEndPoint local)
        {
            OnSniffPacket?.Invoke(src, null, msg);

            DText parser = new();
            parser.ATTRMARK = "::";

            bool Alive = false;
            String UDN = msg.GetTag("USN");

            parser[0] = UDN;
            String USN = parser[1];
            USN = USN.Substring(USN.IndexOf(":", StringComparison.Ordinal) + 1);
            String ST = parser[2];
            int MaxAge = 0;

            String NTS = msg.GetTag("NTS").ToUpper();
            if (NTS == "SSDP:ALIVE")
            {
                Alive = true;
                String ma = msg.GetTag("Cache-Control").Trim();
                if (ma != "")
                {
                    parser.ATTRMARK = ",";
                    parser.MULTMARK = "=";
                    parser[0] = ma;
                    for (int i = 1; i <= parser.DCOUNT(); ++i)
                    {
                        if (parser[i, 1].Trim().ToUpper() == "MAX-AGE")
                        {
                            MaxAge = int.Parse(parser[i, 2].Trim());
                            break;
                        }
                    }
                }
            }

            if (msg.Directive == "NOTIFY" && OnNotify != null)
            {
                Uri locuri = null;
                string location = msg.GetTag("Location");
                if (location != null && location.Length > 0) 
                { 
                    try 
                    { 
                        locuri = new Uri(location); 
                    } 
                    catch (Exception ex)
                    {
                        EventLogger.Log(ex,"SSDP");
                    }
                }
                OnNotify(src, msg.LocalEndPoint, locuri, Alive, USN, ST, MaxAge, msg);
            }
            else if (msg.Directive == "M-SEARCH" && OnSearch != null)
            {
                if (ValidateSearchPacket(msg) == false) return;
                int MaxTimer = int.Parse(msg.GetTag("MX"));
                SearchStruct SearchData = new();
                SearchData.ST = msg.GetTag("ST");
                SearchData.Source = src;
                SearchData.Local = local;
                SearchTimer.Add(SearchData, RandomGenerator.Next(0, MaxTimer));
            }
        }

        private void SearchTimerSink(LifeTimeMonitor sender, object obj)
        {
            SearchStruct SS = (SearchStruct)obj;
            OnSearch?.Invoke(SS.ST, SS.Source, SS.Local);
        }

        private bool ValidateSearchPacket(HTTPMessage msg)
        {
            if (msg.GetTag("MAN") != "\"ssdp:discover\"") return false; // { throw (new InvalidSearchPacketException("Invalid MAN")); }
            if (msg.DirectiveObj != "*") return false; // { throw (new InvalidSearchPacketException("Expected * in RequestLine")); }
            if (double.Parse(msg.Version, new CultureInfo("en-US").NumberFormat) < 1.1) return false; // { throw (new InvalidSearchPacketException("Version must be at least 1.1")); }
            if (int.TryParse(msg.GetTag("MX"), out int MX) == false || MX <= 0) return false; // { throw (new InvalidSearchPacketException("MX must be a positive integer")); }
            return true;
        }

        /// <summary>
        /// Multicasts a HTTPMessage
        /// </summary>
        /// <param name="packet">The Packet to Multicast</param>
        /// <param name="netinterface"></param>
        public void BroadcastData(HTTPMessage packet, IPAddress netinterface)
        {
            UdpClient usession = (UdpClient)usessions[netinterface];
            if (usession == null) return;
            byte[] buffer = Encoding.UTF8.GetBytes(packet.StringPacket);
            if (netinterface.AddressFamily == AddressFamily.InterNetwork)
            {
                try
                {
                    usession.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, netinterface.GetAddressBytes());
                    usession.Send(buffer, buffer.Length, Utils.UpnpMulticastV4EndPoint);
                    usession.Send(buffer, buffer.Length, Utils.UpnpMulticastV4EndPoint);
                }
                catch (SocketException ex)
                {
                    EventLogger.Log(ex,"SSDP");
                }
            }
            else if (netinterface.AddressFamily == AddressFamily.InterNetworkV6 && netinterface.ScopeId != 0)
            {
                try
                {
                    usession.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastInterface, BitConverter.GetBytes((int)netinterface.ScopeId));
                    if (netinterface.IsIPv6LinkLocal)
                    {
                        usession.Send(buffer, buffer.Length, Utils.UpnpMulticastV6EndPoint2);
                        usession.Send(buffer, buffer.Length, Utils.UpnpMulticastV6EndPoint2);
                    }
                    else
                    {
                        usession.Send(buffer, buffer.Length, Utils.UpnpMulticastV6EndPoint1);
                        usession.Send(buffer, buffer.Length, Utils.UpnpMulticastV6EndPoint1);
                    }
                }
                catch (SocketException ex)
                {
                    EventLogger.Log(ex,"SSDP");
                }
            }
        }

        /// <summary>
        /// Unicast a HTTPMessage
        /// </summary>
        /// <param name="msg">The Packet to Unicast</param>
        public void UnicastData(HTTPMessage msg)
        {
            UdpClient usession = (UdpClient)usessions[msg.LocalEndPoint.Address];
            if (usession == null) return;
            OnSniffPacket?.Invoke(null, msg.RemoteEndPoint, msg);
            byte[] buffer = msg.RawPacket;
            try
            {
                usession.Send(buffer, buffer.Length, msg.RemoteEndPoint);
            }
            catch (SocketException ex)
            {
                EventLogger.Log(ex,"SSDP");
            }
        }
    }

}
