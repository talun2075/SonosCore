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
using System.Collections;
using OpenSource.Utilities;
using System.Net.Http;

namespace OSTL.UPnP
{
    /// <summary>
    /// A helper class to instantiate UPnPDevice(s) from Description XML Uri(s)
    /// </summary>
    public sealed class UPnPDeviceFactory
    {
        public delegate void UPnPDeviceHandler(UPnPDeviceFactory sender, UPnPDevice device, Uri URL);
        public delegate void UPnPDeviceFailedHandler(UPnPDeviceFactory sender, Uri URL, Exception e, string URN);
        /// <summary>
        /// Triggered when the Factory failed to create a device
        /// </summary>
        public event UPnPDeviceFailedHandler OnFailed;
        private event UPnPDeviceFailedHandler OnFailed2;
        /// <summary>
        /// Triggered when the Factory creates a device
        /// </summary>
        public event UPnPDeviceHandler OnDevice;
        //private readonly HttpRequestor httprequestor;
        private UPnPDevice TempDevice;
        private readonly int MaxAge;
        private readonly object CBLock;
        private readonly Hashtable CreateTable;
        private int ServiceNum;
        private readonly IPAddress localaddr;
        private readonly string expected_usn = null;

        /// <summary>
        /// Instantiate a reusable Factory
        /// </summary>
        public UPnPDeviceFactory()
        {
            //InstanceTracker.Add(this);
            CreateTable = Hashtable.Synchronized(new Hashtable());
        }

        /// <summary>
        /// Instantiate a one-time use Factory
        /// </summary>
        /// <param name="DescLocation">XML Description Uri</param>
        /// <param name="MaxSeconds">Device Refresh Cycle</param>
        /// <param name="deviceCB">Success Callback</param>
        /// <param name="failedCB">Failure Callback</param>
        public UPnPDeviceFactory(Uri DescLocation, int MaxSeconds, UPnPDeviceHandler deviceCB, UPnPDeviceFailedHandler failedCB, IPAddress localaddr, string usn)
        {
            //InstanceTracker.Add(this);
            //httprequestor = new HttpRequestor();
            //httprequestor.OnRequestCompleted += Httprequestor_OnRequestCompleted;
            expected_usn = usn;

            CBLock = new object();
            OnDevice += deviceCB;
            OnFailed2 += failedCB;
            //DUrl = DescLocation.ToString();
            MaxAge = MaxSeconds;
            this.localaddr = localaddr;
            //httprequestor.LaunchRequest(DescLocation.ToString(), null, null, null, null);//todo: Neu machen
            HttpManagedRequests(DescLocation.ToString());
        }

        /// <summary>
        /// Create a UPnPDevice from a Uri
        /// </summary>
        /// <param name="DescLocation">XML Description Uri</param>
        /// <param name="MaxSeconds">Device refresh Cycle</param>
        public void CreateDevice(Uri DescLocation, int MaxSeconds, IPAddress localaddr, string usn)
        {
            EventLogger.Log(this, LogType: EventLogEntryType.Information, "Creating Device: " + usn);

            lock (CreateTable)
            {
                // 2TODO: Fix the failed callback
                UPnPDeviceFactory x = new(DescLocation, MaxSeconds, HandleFactory,
                    FactoryFailedSink, localaddr, usn);
                CreateTable[x] = x;
            }
        }

        private void FactoryFailedSink(UPnPDeviceFactory sender, Uri URL, Exception e, string urn)
        {
            lock (CreateTable)
            {
                CreateTable.Remove(sender);
                sender.Shutdown();
            }
            OnFailed?.Invoke(this, URL, e, urn);
        }

        private void HandleFactory(UPnPDeviceFactory Factory, UPnPDevice device, Uri URL)
        {
            lock (CreateTable) { CreateTable.Remove(Factory); }
            Factory.Shutdown();
            OnDevice?.Invoke(this, device, URL);
        }

        /// <summary>
        /// Disable the re-useable factory
        /// </summary>
        public void Shutdown()
        {
        }

        private void FetchServiceDocuments(UPnPDevice device)
        {
            //for (int x = 0; x < device.Services.Length; ++x) httprequestor.LaunchRequest(device.Services[x].SCPDURL, null, null, null, device.Services[x]);
            for (int x = 0; x < device.Services.Length; ++x) HttpManagedRequests(device.Services[x].SCPDURL, device.Services[x]);
            if (device.EmbeddedDevices.Length > 0)
            {
                for (int y = 0; y < device.EmbeddedDevices.Length; ++y) FetchServiceDocuments(device.EmbeddedDevices[y]);
            }
        }

        private int FetchServiceCount(UPnPDevice device)
        {
            int Count = device.Services.Length;
            if (device.EmbeddedDevices.Length > 0)
            {
                for (int x = 0; x < device.EmbeddedDevices.Length; ++x) Count += FetchServiceCount(device.EmbeddedDevices[x]);
            }
            return (Count);
        }
        private readonly HttpClient _httpClient = new();

        private async void HttpManagedRequests(string call, object tag = null)
        {
            Uri urlstate = new(call);
            HttpResponseMessage result;
            string dataValue;
            try
            {
                result = await _httpClient.GetAsync(urlstate);
                dataValue = await result.Content.ReadAsStringAsync();
            }
            catch
            {
                return;//On Error return because no access or not reachable.
            }

            if (tag != null)
            {
                bool IsOK = false;
                lock (CBLock)
                {
                    try
                    {
                        var t = ((UPnPService)tag);
                        t.ParseSCPD(dataValue, 0);
                    }
                    catch (Exception e)
                    {
                        EventLogger.Log(e, "Invalid SCPD XML on device:\r\n   Friendly: " + TempDevice.FriendlyName +
                                                                "\r\n   Service: " + ((UPnPService)tag).ServiceURN +
                                                                "\r\n   URL: " + call +
                                                                "\r\n   XML:\r\n" + dataValue +
                                                                "\r\n");
                        return;
                    }

                    --ServiceNum;
                    if ((ServiceNum == 0) && (OnDevice != null)) IsOK = true;
                }
                if (IsOK)
                {
                    TempDevice.descXmlLocation = urlstate;
                    OnDevice(this, TempDevice, urlstate);
                    TempDevice = null;
                }
                return;
            }

            try
            {
                TempDevice = UPnPDevice.Parse(dataValue, urlstate, localaddr);
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, "UPnP Device Description XML parsing exception: URL=" + call);
                OnFailed2?.Invoke(this, urlstate, new Exception("UPnP Device Description XML parsing exception: URL=" + call), expected_usn);
                if (TempDevice != null) TempDevice = null;
                return;
            }
            if (TempDevice == null)
            {
                //OpenSource.Utilities.EventLogger.Log(this, System.Diagnostics.EventLogEntryType.Error, "Invalid UPnP Device Description: URL=" + url);
                OnFailed2?.Invoke(this, urlstate, new Exception("Invalid UPnP Device Description XML @" + call), expected_usn);
                if (TempDevice != null) TempDevice = null;
                return;
            }
            if (expected_usn != null && TempDevice.UniqueDeviceName != expected_usn)
            {
                EventLogger.Log(this, EventLogEntryType.Error, string.Format("Unique ID mismatch between SSDP packet and device description: {0} / {1} ", TempDevice.UniqueDeviceName, expected_usn));
                OnFailed2?.Invoke(this, urlstate, new Exception(string.Format("Unique ID mismatch between SSDP packet and device description: {0} / {1} ", TempDevice.UniqueDeviceName, expected_usn)), expected_usn);
                if (TempDevice != null) TempDevice = null;
                return;
            }
            TempDevice.LocationURL = call;
            TempDevice.ExpirationTimeout = MaxAge;
            if (TempDevice != null)
            {
                ServiceNum = FetchServiceCount(TempDevice);
                if (ServiceNum == 0)
                {
                    if (OnDevice != null)
                    {
                        OnDevice(this, TempDevice, urlstate);
                        TempDevice = null;
                        return;
                    }
                }
                FetchServiceDocuments(TempDevice);
            }
        }
    }
}
