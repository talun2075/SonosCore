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
using System.Net;
using System.Collections;
using OpenSource.Utilities;

namespace OSTL.UPnP
{
    /// <summary>
    /// Enables the user to quickly & efficiently find and keep track of UPnP devices on the
    /// network. This class is internaly optimized to keep network traffic to a minimum while
    /// serving many users at once. This class will keep track of devices coming and going and
    /// devices that change IP address or change subnet. The class will also gather all the
    /// relevent UPnP device information prior to informing the user about the device.
    /// </summary>
    public sealed class UPnPSmartControlPoint //todo: auch über DI machen
    {
        #region Vars
        public delegate void DeviceHandler(UPnPSmartControlPoint sender, UPnPDevice device);

        private readonly UPnPControlPoint genericControlPoint;
        private readonly Hashtable deviceTable = new();
        private readonly object deviceTableLock = new();
        private readonly ArrayList activeDeviceList = ArrayList.Synchronized(new ArrayList());
        private readonly string UsnFilter = "RINCON";
        private readonly LifeTimeMonitor deviceLifeTimeClock = new();
        private readonly LifeTimeMonitor deviceUpdateClock = new();
        private readonly UPnPDeviceFactory deviceFactory = new();

        private struct DeviceInfo
        {
            public UPnPDevice Device;
            public DateTime NotifyTime;
            public string UDN;

            public Uri BaseURL;
            public int MaxAge;
            public IPEndPoint LocalEP;

            public Uri PendingBaseURL;
            public int PendingMaxAge;
            public IPEndPoint PendingLocalEP;
            public IPEndPoint PendingSourceEP;
        }

        #endregion
        #region Event Delegate 
        private readonly WeakEvent OnDeviceExpiredEvent = new();
        private readonly WeakEvent OnAddedDeviceEvent = new();
        private readonly WeakEvent OnRemovedDeviceEvent = new();

        public event DeviceHandler OnDeviceExpired
        {
            add { OnDeviceExpiredEvent.Register(value); }
            remove { OnDeviceExpiredEvent.UnRegister(value); }
        }
        public event DeviceHandler OnAddedDevice
        {
            add { OnAddedDeviceEvent.Register(value); }
            remove { OnAddedDeviceEvent.UnRegister(value); }
        }
        public event DeviceHandler OnRemovedDevice
        {
            add { OnRemovedDeviceEvent.Register(value); }
            remove { OnRemovedDeviceEvent.UnRegister(value); }
        }

        #endregion Event Delegate
        #region ctor
        public UPnPSmartControlPoint() : this(null, null)
        {

        }
        /// <summary>
        /// Keep track of all UPnP devices on the network. The user can expect the OnAddedDeviceSink or OnAddedServiceSink
        /// delegate to immidiatly be called for each device that is already known.
        /// <para>
        /// if multiple filters are supplied, the results will be that of the parent device which satisfies all the search criteria. 
        /// </para>
        /// </summary>
        /// <param name="OnAddedDeviceSink"></param>
        /// <param name="_usnFilter">string, which represent the search criteria, leave empty for NO Filter. Default is RINCON for Sonos Devices.</param>
        public UPnPSmartControlPoint(DeviceHandler OnAddedDeviceSink, string _usnFilter)
        {
            if (OnAddedDeviceSink != null) { OnAddedDevice += OnAddedDeviceSink; }


            if (_usnFilter != null)
                UsnFilter = _usnFilter;
            deviceFactory.OnDevice += DeviceFactoryCreationSink;
            deviceFactory.OnFailed += DeviceFactoryFailedSink;
            deviceLifeTimeClock.OnExpired += DeviceLifeTimeClockSink;
            deviceUpdateClock.OnExpired += DeviceUpdateClockSink;

            // Launch a search for all devices and start populating the
            // internal smart control point device list.
            genericControlPoint = new UPnPControlPoint(UsnFilter);
            genericControlPoint.OnSearch += UPnPControlPointSearchSink;
            genericControlPoint.OnNotify += SSDPNotifySink;

            genericControlPoint.FindDeviceAsync("upnp:rootdevice");
        }
        #endregion
        #region public methods
        public UPnPDevice[] GetCurrentDevices()
        {
            return (UPnPDevice[])activeDeviceList.ToArray(typeof(UPnPDevice));//todo: das evtl. in Discovery nutzen. 
        }
        public void ForceDeviceAddition(Uri url)
        {
            DeviceInfo deviceInfo = new();
            deviceInfo.Device = null;
            deviceInfo.UDN = "FORCEDDEVICE";
            deviceInfo.NotifyTime = DateTime.Now;
            deviceInfo.BaseURL = url;
            deviceInfo.MaxAge = 1800;
            deviceInfo.LocalEP = null;
            deviceTable["FORCEDDEVICE"] = deviceInfo;
            deviceFactory.CreateDevice(deviceInfo.BaseURL, deviceInfo.MaxAge, null, null);
        }
        /// <summary>
        /// To Remove Device for example is not reachable
        /// </summary>
        /// <param name="root"></param>
        public void ForceDisposeDevice(UPnPDevice root)
        {
            while (root.ParentDevice != null)
            {
                root = root.ParentDevice;
            }
            SSDPNotifySink(null, null, null, false, root.UniqueDeviceName, "upnp:rootdevice", 0, null);
        }
        #endregion
        #region private methods
        /// <summary>
        /// Triggered when a SSDP search result is received
        /// </summary>
        private void UPnPControlPointSearchSink(IPEndPoint source, IPEndPoint local, Uri LocationURL, String USN, String SearchTarget, int MaxAge)
        {
            if (UsnFilter != null && !USN.ToUpper().StartsWith(UsnFilter)) return;

            // A bit like getting a SSDP notification, but we don't do automatic
            // source change in this case. The only valid scenario of a search
            // result is device creation.
            lock (deviceTableLock)
            {
                if (deviceTable.ContainsKey(USN) == false)
                {
                    // Never saw this device before
                    DeviceInfo deviceInfo = new();
                    deviceInfo.Device = null;
                    deviceInfo.UDN = USN;
                    deviceInfo.NotifyTime = DateTime.Now;
                    deviceInfo.BaseURL = LocationURL;
                    deviceInfo.MaxAge = MaxAge;
                    deviceInfo.LocalEP = local;
                    deviceTable[USN] = deviceInfo;
                    deviceFactory.CreateDevice(deviceInfo.BaseURL, deviceInfo.MaxAge, local.Address, USN);
                }
                else
                {
                    DeviceInfo deviceInfo = (DeviceInfo)deviceTable[USN];
                    if (deviceInfo.Device != null) // If the device is in creation mode, do nothing
                    {
                        if (deviceInfo.BaseURL.Equals(LocationURL))
                        {
                            // Cancel a possible source change
                            deviceUpdateClock.Remove(deviceInfo);
                            deviceInfo.PendingBaseURL = null;
                            deviceInfo.PendingMaxAge = 0;
                            deviceInfo.PendingLocalEP = null;
                            deviceInfo.PendingSourceEP = null;
                            // Then simply update the lifetime
                            deviceInfo.NotifyTime = DateTime.Now;
                            deviceTable[USN] = deviceInfo;
                            deviceLifeTimeClock.Add(deviceInfo.UDN, MaxAge);
                        }
                        else
                        {
                            // Wow, same device, different source - Check timing
                            if (deviceInfo.NotifyTime.AddSeconds(10).Ticks < DateTime.Now.Ticks)
                            {
                                // This is a possible source change. Wait for 3 seconds and make the switch.
                                deviceInfo.PendingBaseURL = LocationURL;
                                deviceInfo.PendingMaxAge = MaxAge;
                                deviceInfo.PendingLocalEP = local;
                                deviceInfo.PendingSourceEP = source;
                                deviceUpdateClock.Add(deviceInfo.UDN, 3);
                            }
                        }
                    }
                }
            }
        }

        private UPnPDevice UnprotectedRemoveMe(string UDN)
        {
            DeviceInfo deviceInfo;
            UPnPDevice removedDevice = null;

            if (deviceTable.ContainsKey(UDN))
            {
                deviceInfo = (DeviceInfo)deviceTable[UDN];
                removedDevice = deviceInfo.Device;
                deviceTable.Remove(UDN);
                deviceLifeTimeClock.Remove(deviceInfo.UDN);
                deviceUpdateClock.Remove(deviceInfo);
                activeDeviceList.Remove(removedDevice);
            }

            return (removedDevice);
        }
        /// <summary>
        /// Triggered when a SSDP notification is received
        /// </summary>
        private void SSDPNotifySink(IPEndPoint source, IPEndPoint local, Uri LocationURL, bool IsAlive, String USN, String SearchTarget, int MaxAge, HTTPMessage Packet)
        {
            UPnPDevice removedDevice = null;
            // Simple ignore everything that is not root
            if (SearchTarget != "upnp:rootdevice" || (UsnFilter !=null && !USN.ToUpper().StartsWith(UsnFilter))) return;

            if (IsAlive == false)
            {
                // The easy part first... we got a SSDP BYE message
                // Remove the device completely no matter what state it is in
                // right now. Also clear all clocks.
                lock (deviceTableLock)
                {
                    removedDevice = UnprotectedRemoveMe(USN);
                }
                if (removedDevice != null)
                {
                    removedDevice.Removed();
                    OnRemovedDeviceEvent.Fire(this, removedDevice);
                }
            }
            else
            {
                lock (deviceTableLock)
                {
                    // Ok, This device is annoncing itself.
                    if (deviceTable.ContainsKey(USN) == false)
                    {
                        // Never saw this device before
                        DeviceInfo deviceInfo = new();
                        deviceInfo.Device = null;
                        deviceInfo.UDN = USN;
                        deviceInfo.NotifyTime = DateTime.Now;
                        deviceInfo.BaseURL = LocationURL;
                        deviceInfo.MaxAge = MaxAge;
                        deviceInfo.LocalEP = local;
                        deviceTable[USN] = deviceInfo;
                        deviceFactory.CreateDevice(deviceInfo.BaseURL, deviceInfo.MaxAge, local.Address, USN);
                    }
                    else
                    {
                        // We already know about this device, lets check it out

                        DeviceInfo deviceInfo = (DeviceInfo)deviceTable[USN];
                        if (deviceInfo.Device != null) // If the device is in creation mode, do nothing
                        {
                            if (deviceInfo.BaseURL.Equals(LocationURL))
                            {
                                // Cancel a possible source change
                                deviceUpdateClock.Remove(deviceInfo);
                                deviceInfo.PendingBaseURL = null;
                                deviceInfo.PendingMaxAge = 0;
                                deviceInfo.PendingLocalEP = null;
                                deviceInfo.PendingSourceEP = null;
                                // Then simply update the lifetime
                                deviceInfo.NotifyTime = DateTime.Now;
                                deviceTable[USN] = deviceInfo;
                                deviceLifeTimeClock.Add(deviceInfo.UDN, MaxAge);
                            }
                            else
                            {
                                // Wow, same device, different source - Check timing
                                if (deviceInfo.NotifyTime.AddSeconds(10).Ticks < DateTime.Now.Ticks)
                                {
                                    // This is a possible source change. Wait for 3 seconds and make the switch.
                                    deviceInfo.PendingBaseURL = LocationURL;
                                    deviceInfo.PendingMaxAge = MaxAge;
                                    deviceInfo.PendingLocalEP = local;
                                    deviceInfo.PendingSourceEP = source;
                                    deviceTable[USN] = deviceInfo;
                                    deviceUpdateClock.Add(deviceInfo.UDN, 3);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Triggered when a device lifetime has expired
        /// </summary>
        /// <param name="Obj"></param>
        private void DeviceLifeTimeClockSink(LifeTimeMonitor sender, object obj)
        {
            DeviceInfo deviceInfo;
            lock (deviceTableLock)
            {
                if (deviceTable.ContainsKey(obj) == false)
                {
                    return;
                }
                deviceInfo = (DeviceInfo)deviceTable[obj];
                deviceTable.Remove(obj);
                deviceUpdateClock.Remove(obj);	// Cancel any source change
                if (activeDeviceList.Contains(deviceInfo.Device))
                {
                    activeDeviceList.Remove(deviceInfo.Device);
                }
                else
                {
                    deviceInfo.Device = null;	// Don't warn user about this, user does not know about device
                }
            }
            //if (deviceInfo.Device != null)
            //{
            //    deviceInfo.Device.Removed();
            //}//double?
            if (deviceInfo.Device != null)
            {
                deviceInfo.Device.Removed();
                OnDeviceExpiredEvent.Fire(this, deviceInfo.Device);
            }
        }

        /// <summary>
        /// Triggered when a device must be updated to a new source IP address
        /// </summary>
        /// <param name="Obj"></param>
        private void DeviceUpdateClockSink(LifeTimeMonitor sender, object obj)
        {
            // Make the source switch
            lock (deviceTableLock)
            {
                if (deviceTable.ContainsKey(obj) == false)
                {
                    return;
                }
                DeviceInfo deviceInfo = (DeviceInfo)deviceTable[obj];
                if (deviceInfo.PendingBaseURL == null)
                {
                    return;	// Cancel the switch
                }

                deviceInfo.BaseURL = deviceInfo.PendingBaseURL;
                deviceInfo.MaxAge = deviceInfo.PendingMaxAge;
                deviceInfo.LocalEP = deviceInfo.PendingLocalEP;
                deviceInfo.NotifyTime = DateTime.Now;
                deviceInfo.Device.UpdateDevice(deviceInfo.BaseURL, deviceInfo.LocalEP.Address);
                deviceTable[obj] = deviceInfo;

                deviceLifeTimeClock.Add(deviceInfo.UDN, deviceInfo.MaxAge);
            }
            //if (OnUpdatedDevice != null) OnUpdatedDevice(this,deviceInfo.Device);
        }

        private void DeviceFactoryFailedSink(UPnPDeviceFactory sender, Uri URL, Exception e, string urn)
        {
            lock (deviceTableLock)
            {
                if (deviceTable.ContainsKey(urn)) deviceTable.Remove(urn);
            }
        }

        /// <summary>
        /// Triggered when a new UPnP device is created.
        /// </summary>
        private void DeviceFactoryCreationSink(UPnPDeviceFactory sender, UPnPDevice device, Uri locationURL)
        {
            // Hardening
            if (deviceTable.Contains(device.UniqueDeviceName) == false && deviceTable.Contains("FORCEDDEVICE") == false)
            {
                EventLogger.Log(this, EventLogEntryType.Error, "UPnPDevice[" + device.FriendlyName + "]@" + device.LocationURL + " advertised UDN[" + device.UniqueDeviceName + "] in xml but not in SSDP");
                return;
            }

            lock (deviceTableLock)
            {
                DeviceInfo deviceInfo;
                if (deviceTable.Contains(device.UniqueDeviceName) == false)
                {
                    // This must be the forced device
                    deviceInfo = (DeviceInfo)deviceTable["FORCEDDEVICE"];
                    deviceTable.Remove("FORCEDDEVICE");
                    deviceTable[device.UniqueDeviceName] = deviceInfo;
                }

                // Hardening - Creating a device we have should never happen.
                if (((DeviceInfo)deviceTable[device.UniqueDeviceName]).Device != null)
                {
                    EventLogger.Log(this, EventLogEntryType.Error, "Unexpected UPnP Device Creation: " + device.FriendlyName + "@" + device.LocationURL);
                    return;
                }

                // Lets update out state and notify the user.
                deviceInfo = (DeviceInfo)deviceTable[device.UniqueDeviceName];
                deviceInfo.Device = device;
                deviceTable[device.UniqueDeviceName] = deviceInfo;
                deviceLifeTimeClock.Add(device.UniqueDeviceName, device.ExpirationTimeout);
                activeDeviceList.Add(device);
            }
            OnAddedDeviceEvent.Fire(this, device);
        }

        private bool CheckDeviceAgainstFilter(string filter, double Version, UPnPDevice device, out object[] MatchingObject)
        {
            ArrayList TempList = new();
            // No devices to filter.
            if (device == null)
            {
                MatchingObject = Array.Empty<object>();
                return false;
            }

            // Filter is null, all devices will show up.
            if ((filter == "upnp:rootdevice") &&
                device.Root)
            {
                MatchingObject = new Object[] { device };
                return true;
            }

            if (device.Root == false)
            {
                bool TempBool;

                foreach (UPnPDevice edevice in device.EmbeddedDevices)
                {
                    TempBool = CheckDeviceAgainstFilter(filter, Version, edevice, out object[] TempObj);
                    if (TempBool)
                    {
                        foreach (Object t in TempObj)
                        {
                            TempList.Add(t);
                        }
                    }
                }
            }
            else
            {
                foreach (UPnPDevice dv in device.EmbeddedDevices)
                {
                    CheckDeviceAgainstFilter(filter, Version, dv, out object[] m);
                    foreach (object mm in m)
                    {
                        TempList.Add(mm);
                    }
                }
            }

            if ((device.UniqueDeviceName == filter) ||
                (device.DeviceURN_Prefix == filter && double.Parse(device.Version) >= Version))
            {
                TempList.Add(device);
            }
            else
            {
                // Check Services
                for (int x = 0; x < device.Services.Length; ++x)
                {
                    if ((device.Services[x].ServiceID == filter) ||
                       (device.Services[x].ServiceURN_Prefix == filter && double.Parse(device.Services[x].Version) >= Version))
                    {
                        TempList.Add(device.Services[x]);
                    }
                }
            }

            if (TempList.Count == 0)
            {
                MatchingObject = Array.Empty<object>();
                return (false);
            }
            MatchingObject = (object[])TempList.ToArray(typeof(object));
            return (true);
        }
        #endregion
    }
}
