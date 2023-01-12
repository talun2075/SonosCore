/*   
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
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using OpenSource.Utilities;

namespace OSTL.UPnP
{
    /// <summary>
    /// The object by which you can leverage UPnPDevices on your network.
    /// </summary>
    public sealed class UPnPControlPoint
    {
        private static int _mx = 5;
        public static int MX
        {
            get { return (_mx); }
            set { if (value > 0) _mx = value; }
        }
        public delegate void SearchHandler(IPEndPoint ResponseFromEndPoint, IPEndPoint ResponseReceivedOnEndPoint, Uri DescriptionLocation, String USN, String SearchTarget, int MaxAge);
        /// <summary>
        /// Triggered when an AsyncSearch result returns
        /// </summary>
        public event SearchHandler OnSearch;

        public delegate void CreateDeviceHandler(UPnPDevice Device, Uri DescriptionURL);
        /// <summary>
        /// Triggered when an Async CreateDevice returns...
        /// <para>
        /// Depracated. Use UPnPDeviceFactory</para>
        /// </summary>
        public event CreateDeviceHandler OnCreateDevice;

        private readonly NetworkInfo NetInfo;
        //private readonly ArrayList SyncData;
        //private DeviceNode SyncDevice;
        private readonly Hashtable CreateTable;
        private readonly SSDP SSDPServer;
        private readonly Hashtable SSDPSessions;
        private readonly LifeTimeMonitor Lifetime;
        private readonly string UsnFilter = "RINCON";

        private struct DeviceNode
        {
            //public UPnPDevice TheDevice;
            //public Uri URL;
        }

        /// <summary>
        /// This event is triggered when other UPnPDevices on your network are alive.
        /// </summary>
        public event SSDP.NotifyHandler OnNotify;

        /// <summary>
        /// Constructs a new Control Point, and waits for your commands and receives events
        /// </summary>
        public UPnPControlPoint(string _usnFilter)
        {
            if (_usnFilter != null)
                UsnFilter = _usnFilter;
            CreateTable = Hashtable.Synchronized(new Hashtable());
            NetInfo = new NetworkInfo(NewInterface);
            SSDPSessions = Hashtable.Synchronized(new Hashtable());
            Lifetime = new LifeTimeMonitor();
            Lifetime.OnExpired += HandleExpired;

            SSDPServer = new SSDP(65535);
            SSDPServer.OnNotify += HandleNotify;
        }

        private void NewInterface(NetworkInfo sender, IPAddress Intfce)
        {
        }

        private void HandleNotify(IPEndPoint source, IPEndPoint local, Uri LocationURL, bool IsAlive, String USN, String ST, int MaxAge, HTTPMessage Packet)
        {
            if (UsnFilter != null && USN.ToUpper().StartsWith(UsnFilter))
                OnNotify?.Invoke(source, local, LocationURL, IsAlive, USN, ST, MaxAge, Packet);
        }

        /// <summary>
        /// Creates a device from a URL. [Depracated] use UPnPDeviceFactory
        /// </summary>
        /// <param name="DescriptionURL"></param>
        /// <param name="LifeTime"></param>
        public void CreateDeviceAsync(Uri DescriptionURL, int LifeTime)
        {
            //2ToDo: Replace the failed callback
            UPnPDeviceFactory fac = new(DescriptionURL, LifeTime, HandleDeviceCreation, null, null, null);
            CreateTable[fac] = fac;
            Lifetime.Add(fac, 30);
        }

        private void HandleDeviceCreation(UPnPDeviceFactory Factory, UPnPDevice device, Uri URL)
        {
            Factory.Shutdown();
            OnCreateDevice?.Invoke(device, URL);
        }

        //private void CreateSyncCallback(UPnPDevice Device, Uri URL)
        //{
        //    SyncDevice.TheDevice = Device;
        //    SyncDevice.URL = URL;
        //}

        private void HandleExpired(LifeTimeMonitor sender, object Obj)
        {
            if (Obj.GetType().FullName == "OSTL.UPnP.UPnPDeviceFactory")
            {
                ((UPnPDeviceFactory)Obj).Shutdown();
                CreateTable.Remove(Obj);
            }
        }

        public void FindDeviceAsync(String SearchTarget)
        {
            FindDeviceAsync(SearchTarget, Utils.UpnpMulticastV4EndPoint);
            FindDeviceAsync(SearchTarget, Utils.UpnpMulticastV6EndPoint1);
            FindDeviceAsync(SearchTarget, Utils.UpnpMulticastV6EndPoint2);
        }

        public void FindDeviceAsync(String SearchTarget, IPAddress RemoteAddress)
        {
            FindDeviceAsync(SearchTarget, Utils.UpnpMulticastV4EndPoint);
        }

        /// <summary>
        /// Searches for a SearchTarget Asynchronously
        /// </summary>
        /// <param name="SearchTarget">The Target</param>
        public void FindDeviceAsync(String SearchTarget, IPEndPoint RemoteEP)
        {
            HTTPMessage request = new();
            request.Directive = "M-SEARCH";
            request.DirectiveObj = "*";
            request.AddTag("ST", SearchTarget);
            request.AddTag("MX", MX.ToString());
            request.AddTag("MAN", "\"ssdp:discover\"");
            if (RemoteEP.AddressFamily == AddressFamily.InterNetwork) request.AddTag("HOST", RemoteEP.ToString()); // "239.255.255.250:1900"
            if (RemoteEP.AddressFamily == AddressFamily.InterNetworkV6) request.AddTag("HOST", string.Format("[{0}]:{1}", RemoteEP.Address.ToString(), RemoteEP.Port)); // "[FF05::C]:1900"
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(request.StringPacket);

            IPAddress[] LocalAddresses = NetInfo.GetLocalAddresses();

            foreach (IPAddress localaddr in LocalAddresses)
            {
                try
                {
                    UdpClient session = (UdpClient)SSDPSessions[localaddr];
                    if (session == null)
                    {
                        session = new UdpClient(new IPEndPoint(localaddr, 0));
                        session.EnableBroadcast = true;
                        session.BeginReceive(OnReceiveSink, session);
                        SSDPSessions[localaddr] = session;
                    }
                    if (RemoteEP.AddressFamily != session.Client.AddressFamily) continue;
                    if ((RemoteEP.AddressFamily == AddressFamily.InterNetworkV6) && ((IPEndPoint)session.Client.LocalEndPoint).Address.IsIPv6LinkLocal && RemoteEP != Utils.UpnpMulticastV6EndPoint2) continue;
                    if ((RemoteEP.AddressFamily == AddressFamily.InterNetworkV6) && ((IPEndPoint)session.Client.LocalEndPoint).Address.IsIPv6LinkLocal == false && RemoteEP != Utils.UpnpMulticastV6EndPoint1) continue;

                    IPEndPoint lep = (IPEndPoint)session.Client.LocalEndPoint;
                    if (session.Client.AddressFamily == AddressFamily.InterNetwork)
                    {
                        session.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, localaddr.GetAddressBytes());
                    }
                    else if (session.Client.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        session.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastInterface, BitConverter.GetBytes((int)localaddr.ScopeId));
                    }

                    session.Send(buffer, buffer.Length, RemoteEP);
                    session.Send(buffer, buffer.Length, RemoteEP);
                }
                catch (Exception ex)
                {
                    EventLogger.Log(this, EventLogEntryType.Error, "CP Failure: " + localaddr.ToString());
                    EventLogger.Log(ex, "UPNPControlPoint");
                }
            }
        }

        public void OnReceiveSink(IAsyncResult ar)
        {
            IPEndPoint ep = null;
            UdpClient client = (UdpClient)ar.AsyncState;
            try
            {
                byte[] buf = client.EndReceive(ar, ref ep);
                if (buf != null)
                {
                    OnReceiveSink2(buf, ep, (IPEndPoint)client.Client.LocalEndPoint);
                    client.BeginReceive(OnReceiveSink, client);
                    return;
                }
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "UPNPControlPoint");
            }
            try
            {
                IPEndPoint local = (IPEndPoint)client.Client.LocalEndPoint;
                SSDPSessions.Remove(local.Address);
            }
            catch (Exception x)
            {
                var k = x;
            }
        }

        private void OnReceiveSink2(byte[] buffer, IPEndPoint remote, IPEndPoint local)
        {
            HTTPMessage msg;

            try
            {
                msg = HTTPMessage.ParseByteArray(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "UPNPControlPoint");
                msg = new HTTPMessage();
                msg.Directive = "---";
                msg.DirectiveObj = "---";
                msg.BodyBuffer = buffer;
            }
            msg.LocalEndPoint = local;
            msg.RemoteEndPoint = remote;

            DText parser = new();

            String Location = msg.GetTag("Location");
            int MaxAge = 0;
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
            ma = msg.GetTag("USN");
            String USN = ma.Substring(ma.IndexOf(":") + 1);
            String ST = msg.GetTag("ST");
            if (USN.IndexOf("::") != -1) USN = USN.Substring(0, USN.IndexOf("::"));
            EventLogger.Log(this, EventLogEntryType.SuccessAudit, msg.RemoteEndPoint.ToString());
            OnSearch?.Invoke(msg.RemoteEndPoint, msg.LocalEndPoint, new Uri(Location), USN, ST, MaxAge);
        }


    }
}
