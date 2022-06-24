using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSTL.UPnP;
using SonosUPnP.Classes;
using SonosUPnP.DataClasses;

namespace SonosUPnP.Services
{
    public class SystemProperties
    {
        #region Klassenvariablen
        private const string ClassName = "SystemProperties";
        public UPnPStateVariable CustomerID { get; set; }
        public UPnPStateVariable ThirdPartyHash { get; set; }
        public UPnPStateVariable UpdateID { get; set; }
        public UPnPStateVariable UpdateIDX { get; set; }
        public UPnPStateVariable VoiceUpdateID { get; set; }

        private UPnPService systemProperties;
        private readonly SonosPlayer pl;
        public event EventHandler<SonosPlayer> SystemProperties_Changed = delegate { };
        public DateTime LastChangeByEvent { get; private set; }
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        #endregion Klassenvariablen
        #region ctor und Service
        public UPnPService SystemPropertiesService
        {
            get
            {
                if (systemProperties != null)
                    return systemProperties;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    if (pl.Device == null)
                        return null;
                }
                systemProperties = pl.Device.GetService("urn:upnp-org:serviceId:SystemProperties");
                return systemProperties;
            }
        }

        public SystemProperties(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.VoiceUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.UpdateIDX, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.UpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ThirdPartyHash, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CustomerID, new DateTime());
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            if (SystemPropertiesService == null) return;
            SystemPropertiesService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                CustomerID = service.GetStateVariableObject("CustomerID");
                CustomerID.OnModified += EventFired_CustomerID;
                ThirdPartyHash = service.GetStateVariableObject("ThirdPartyHash");
                ThirdPartyHash.OnModified += EventFired_ThirdPartyHash;
                UpdateID = service.GetStateVariableObject("UpdateID");
                UpdateID.OnModified += EventFired_UpdateID;
                UpdateIDX = service.GetStateVariableObject("UpdateIDX");
                UpdateIDX.OnModified += EventFired_UpdateIDX;
                VoiceUpdateID = service.GetStateVariableObject("VoiceUpdateID");
                VoiceUpdateID.OnModified += EventFired_VoiceUpdateID;
            });
        }

        private void EventFired_VoiceUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.SystemProperties_VoiceUpdateID != nv)
            {
                pl.PlayerProperties.SystemProperties_VoiceUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.VoiceUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.VoiceUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.VoiceUpdateID, DateTime.Now);
            }
        }

        private void EventFired_UpdateIDX(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.SystemProperties_UpdateIDX != nv)
            {
                pl.PlayerProperties.SystemProperties_UpdateIDX = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.UpdateIDX].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.UpdateIDX] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.UpdateIDX, DateTime.Now);
            }
        }

        private void EventFired_UpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.SystemProperties_UpdateID != nv)
            {
                pl.PlayerProperties.SystemProperties_UpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.UpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.UpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.UpdateID, DateTime.Now);
            }
        }

        private void EventFired_ThirdPartyHash(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.SystemProperties_ThirdPartyHash != nv)
            {
                pl.PlayerProperties.SystemProperties_ThirdPartyHash = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.ThirdPartyHash].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ThirdPartyHash] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ThirdPartyHash, DateTime.Now);
            }
        }

        private void EventFired_CustomerID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.SystemProperties_CustomerID != nv)
            {
                pl.PlayerProperties.SystemProperties_CustomerID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.CustomerID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.CustomerID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.CustomerID, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        public async Task<Boolean> AddAccountX(UInt16 AccountType, string AccountID, string AccountPassword)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("AccountType", AccountType);
            arguments[1] = new UPnPArgument("AccountID", AccountID);
            arguments[2] = new UPnPArgument("AccountPassword", AccountPassword);
            arguments[3] = new UPnPArgument("AccountUDN", null);
            var retval = await Invoke("AddAccountX", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            return retval;
        }
        public async Task<Boolean> AddOAuthAccountX(UInt16 AccountType, string AccountToken, string AccountKey, string OAuthDeviceID, string AuthorizationCode, string RedirectURI, string UserIdHashCode)
        {
            var arguments = new UPnPArgument[9];
            arguments[0] = new UPnPArgument("AccountType", AccountType);
            arguments[1] = new UPnPArgument("AccountToken", AccountToken);
            arguments[2] = new UPnPArgument("AccountKey", AccountKey);
            arguments[3] = new UPnPArgument("OAuthDeviceID", OAuthDeviceID);
            arguments[4] = new UPnPArgument("AuthorizationCode", AuthorizationCode);
            arguments[5] = new UPnPArgument("RedirectURI", RedirectURI);
            arguments[6] = new UPnPArgument("UserIdHashCode", UserIdHashCode);
            arguments[7] = new UPnPArgument("AccountUDN", null);
            arguments[8] = new UPnPArgument("AccountNickname", null);
            var retval = await Invoke("AddOAuthAccountX", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            return retval;
        }
        public async Task<Boolean> DoPostUpdateTasks()
        {
            return await Invoke("DoPostUpdateTasks", null);
        }
        public async Task<Boolean> EditAccountMd(UInt16 AccountType, string AccountID, string NewAccountMd)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("AccountType", AccountType);
            arguments[1] = new UPnPArgument("AccountID", AccountID);
            arguments[2] = new UPnPArgument("NewAccountMd", NewAccountMd);
            return await Invoke("EditAccountMd", arguments);
        }
        public async Task<Boolean> EnableRDM(Boolean RDMValue)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("RDMValue", RDMValue);
            return await Invoke("EnableRDM", arguments);
        }
        public async Task<Boolean> GetRDM()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("RDMValue", null);
            await Invoke("EnableRDM", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            Boolean.TryParse(arguments[0].DataValue.ToString(), out bool rdm);
            return rdm;
        }
        public async Task<String> GetString(string VariableName)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("VariableName", VariableName);
            arguments[1] = new UPnPArgument("StringValue", null);
            await Invoke("GetString", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            return arguments[1].DataValue.ToString();
        }
        public async Task<String> GetWebCode(UInt16 AccountType)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("AccountType", AccountType);
            arguments[1] = new UPnPArgument("WebCode", null);
            await Invoke("GetWebCode", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 3, WaiterTypes.String);
            return arguments[1].DataValue.ToString();
        }
        public async Task<ProvisionCredentialedTrialAccount> ProvisionCredentialedTrialAccountX(UInt16 AccountType, string AccountID, string AccountPassword)
        {
            var arguments = new UPnPArgument[5];
            arguments[0] = new UPnPArgument("AccountType", AccountType);
            arguments[1] = new UPnPArgument("AccountID", AccountID);
            arguments[2] = new UPnPArgument("AccountPassword", AccountPassword);
            arguments[3] = new UPnPArgument("IsExpired", null);
            arguments[4] = new UPnPArgument("AccountUDN", null);
            await Invoke("ProvisionCredentialedTrialAccountX", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 4, 100, 10, WaiterTypes.String);
            ProvisionCredentialedTrialAccount p = new();
            if (Boolean.TryParse(arguments[3].DataValue.ToString(), out bool isexpired))
                p.IsExpired = isexpired;
            p.AccountUDN = arguments[4].DataValue.ToString();

            return p;
        }
        public async Task<Boolean> RefreshAccountCredentialsX(UInt16 AccountType, UInt16 AccountUID, string AccountToken, string AccountKey)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("AccountType", AccountType);
            arguments[1] = new UPnPArgument("AccountUID", AccountUID);
            arguments[2] = new UPnPArgument("AccountToken", AccountToken);
            arguments[3] = new UPnPArgument("AccountKey", AccountKey);
            return await Invoke("RefreshAccountCredentialsX", arguments);
        }
        public async Task<Boolean> Remove(string VariableName)
        {

            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("VariableName", VariableName);
            return await Invoke("Remove", arguments);
        }
        public async Task<Boolean> RemoveAccount(UInt16 AccountType, string AccountID)
        {

            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("AccountType", AccountType);
            arguments[1] = new UPnPArgument("AccountID", AccountID);
            return await Invoke("RemoveAccount", arguments);
        }
        public async Task<String> ReplaceAccountX(string AccountUDN, string NewAccountID, string NewAccountPassword, string AccountToken, string AccountKey, string OAuthDeviceID)
        {
            var arguments = new UPnPArgument[7];
            arguments[0] = new UPnPArgument("AccountUDN", AccountUDN);
            arguments[1] = new UPnPArgument("NewAccountID", NewAccountID);
            arguments[2] = new UPnPArgument("NewAccountPassword", NewAccountPassword);
            arguments[3] = new UPnPArgument("AccountToken", AccountToken);
            arguments[4] = new UPnPArgument("AccountKey", AccountKey);
            arguments[5] = new UPnPArgument("OAuthDeviceID", OAuthDeviceID);
            arguments[6] = new UPnPArgument("NewAccountUDN", null);
            await Invoke("ReplaceAccountX", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 6, 100, 10, WaiterTypes.String);
            return arguments[6].DataValue.ToString();
        }
        public async Task<Boolean> ResetThirdPartyCredentials()
        {
            return await Invoke("ResetThirdPartyCredentials", null);
        }
        public async Task<Boolean> SetAccountNicknameX(string AccountUDN, string AccountNickname)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("AccountUDN", AccountUDN);
            arguments[1] = new UPnPArgument("AccountNickname", AccountNickname);
            return await Invoke("SetAccountNicknameX", arguments);
        }
        public async Task<Boolean> SetString(string VariableName, string StringValue)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("VariableName", VariableName);
            arguments[1] = new UPnPArgument("StringValue", StringValue);
            return await Invoke("SetString", arguments);
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (SystemPropertiesService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " " + ClassName + " ist null"));
                    return false;
                }
                SystemPropertiesService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method, ClassName, ex);
                return false;
            }
        }
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime _lastchange)
        {
            try
            {
                if (SystemProperties_Changed == null) return;
                LastChangeByEvent = _lastchange;
                LastChangeDates[t] = _lastchange;
                SystemProperties_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("DeviceProperties_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}

