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
        internal static UPnPInternalSmartControlPoint iSCP; //todo: zusammmen führen um zu sehen, was ich auch wirklich brauche. 
        public delegate void DeviceHandler(UPnPSmartControlPoint sender, UPnPDevice device);
        public delegate void ServiceHandler(UPnPSmartControlPoint sender, UPnPService service);
        /// <summary>
        /// Triggered when a Device that passes the filter appears on the network
        /// <para>
        /// Also triggered when a device that contains objects that pass all the filters appears on the network. This only applies if more than one filter is passed.
        /// </para>
        /// </summary>
        public event DeviceHandler OnAddedDevice;
        /// <summary>
        /// Triggered when a Device that passes the filter disappears from the network
        /// </summary>
        public event DeviceHandler OnRemovedDevice;

        /// <summary>
        /// Triggered when a Service that passes the filter appears on the network
        /// </summary>
        public event ServiceHandler OnAddedService;
        /// <summary>
        /// Triggered when a Service that passes the filter disappears from the network
        /// </summary>
        public event ServiceHandler OnRemovedService;
        #endregion
        #region ctor
        public UPnPSmartControlPoint() : this(null, null, null)
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
        /// <param name="OnAddedServiceSink"></param>
        /// <param name="_usnFilter">string, which represent the search criteria, leave empty for NO Filter. Default is RINCON for Sonos Devices.</param>
        public UPnPSmartControlPoint(DeviceHandler OnAddedDeviceSink, ServiceHandler OnAddedServiceSink, string _usnFilter)
        {
            iSCP = new UPnPInternalSmartControlPoint(_usnFilter);
            if (OnAddedDeviceSink != null) { OnAddedDevice += OnAddedDeviceSink; }
            if (OnAddedServiceSink != null) { OnAddedService += OnAddedServiceSink; }

            iSCP.OnAddedDevice += HandleAddedDevice;
            iSCP.OnDeviceExpired += HandleRemovedDevice;
            iSCP.OnRemovedDevice += HandleRemovedDevice;
            iSCP.OnUpdatedDevice += HandleUpdatedDevice;

            IEnumerator cdEN = iSCP.GetCurrentDevices().GetEnumerator();
            if ((OnAddedDeviceSink != null || OnAddedServiceSink != null))
            {
                while (cdEN.MoveNext()) { HandleAddedDevice(null, (UPnPDevice)cdEN.Current); }
            }
        }

        #endregion
        #region public methods
        public static void ForceDisposeDevice(UPnPDevice root)
        {
            while (root.ParentDevice != null)
            {
                root = root.ParentDevice;
            }
            iSCP.SSDPNotifySink(null, null, null, false, root.UniqueDeviceName, "upnp:rootdevice", 0, null);//todo testen ob dies das nicht vorhandene Gerät ist.
        }
        public static void UnicastSearch(IPAddress RemoteAddress)
        {
            iSCP.UnicastSearch(RemoteAddress);
        }
        public void RemoveDevice(UPnPDevice device)
        {
            iSCP.RemoveMe(device);
            iSCP.Rescan();
        }
        #endregion
        #region private methods
        /// <summary>
        /// Forward the OnAddedDevice event to the user.
        /// </summary>
        /// <param name="sender">UPnPInternalSmartControlPoint that sent the event</param>
        /// <param name="device">The UPnPDevice object that was added</param>
        private void HandleAddedDevice(UPnPInternalSmartControlPoint sender, UPnPDevice device)
        {
            if ((OnAddedDevice != null) || (OnAddedService != null))
            {
                OnAddedDevice?.Invoke(this, device);
                return;
            }

        }

        /// <summary>
        /// Forward the OnUpdatedDevice event to the user.
        /// </summary>
        /// <param name="sender">UPnPInternalSmartControlPoint that sent the event</param>
        /// <param name="device">The UPnPDevice object that was updated</param>
        private void HandleUpdatedDevice(UPnPInternalSmartControlPoint sender, UPnPDevice device)
        {
            //todo: empty
        }

        /// <summary>
        /// Forward the OnRemovedDevice event to the user.
        /// </summary>
        /// <param name="sender">UPnPInternalSmartControlPoint that sent the event</param>
        /// <param name="device">The UPnPDevice object that was removed from the network</param>
        private void HandleRemovedDevice(UPnPInternalSmartControlPoint sender, UPnPDevice device)
        {
            if ((OnRemovedDevice != null) || (OnRemovedService != null))
            {
                OnRemovedDevice?.Invoke(this, device);
                return;
            }
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

        public void ForceDeviceAddition(Uri url)
        {
            iSCP.ForceDeviceAddition(url);
        }

        #endregion
    }
}
