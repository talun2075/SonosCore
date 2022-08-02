using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using OSTL.UPnP;
using SonosConst;
using SonosData;
using SonosData.DataClasses;
using SonosData.Props;
using SonosUPnP.Classes;

namespace SonosUPnP.Services.MediaRendererService
{
    public class AVTransport
    {
        #region Klassenvariablen
        private const string ClassName = "AVTransport";
        private UPnPService avTransport;
        private readonly SonosPlayer pl;
        private UPnPDevice mediaRendererService;
        public event EventHandler<SonosPlayer> AVTransport_Changed = delegate { };
        public UPnPStateVariable LastChange { get; private set; }
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        /// <summary>
        /// Liefert den AVTranport.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="mrs"></param>
        public AVTransport(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.SleepTimerRunning, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentCrossFadeMode, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TransportState, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentPlayMode, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.NumberOfTracks, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentTrackNumber, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentSection, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.PlaybackStorageMedium, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AVTransportURI, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AVTransportURIMetaData, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.NextAVTransportURI, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.NextAVTransportURIMetaData, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentTransportActions, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TransportPlaySpeed, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentMediaDuration, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RecordStorageMedium, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.PossiblePlaybackStorageMedia, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.PossibleRecordStorageMedia, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RecordMediumWriteStatus, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentRecordQualityMode, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.PossibleRecordQualityModes, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.EnqueuedTransportURI, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.EnqueuedTransportURIMetaData, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentValidPlayModes, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.MuseSessions, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DirectControlClientID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DirectControlIsSuspended, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DirectControlAccountID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AlarmRunning, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SnoozeRunning, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RestartPending, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.NextTrack, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.CurrentTrack, new DateTime());
        }
        /// <summary>
        /// Liefert den AVTRansport Service zurück. (Dient zum Übermitteln von Befehlen wie Play und Pause) (UPNP)
        /// </summary>
        public UPnPService AVTransportService
        {
            get
            {
                if (avTransport != null)
                    return avTransport;
                if (mediaRendererService == null)
                    if (pl.Device == null)
                    {
                        pl.LoadDevice();
                        if (pl.Device == null)
                            return null;
                    }
                mediaRendererService = pl.Device.EmbeddedDevices.FirstOrDefault(d => d.DeviceURN == "urn:schemas-upnp-org:device:MediaRenderer:1");
                if (mediaRendererService == null)
                    return null;
                avTransport = mediaRendererService.GetService("urn:upnp-org:serviceId:AVTransport");
                return avTransport;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            AVTransportService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                LastChange = service.GetStateVariableObject("LastChange");
                LastChange.OnModified += EventFired_LastChange;
            });
        }

        private void EventFired_LastChange(UPnPStateVariable sender, object NewValue)
        {
            ParseChangeXML(sender.Value.ToString());
        }
        private void ParseChangeXML(string newState)
        {
            XNamespace ns = "urn:schemas-upnp-org:metadata-1-0/AVT/";
            XNamespace nsnext = "urn:schemas-rinconnetworks-com:metadata-1-0/";
            XElement instance;
            try
            {
                var xEvent = XElement.Parse(newState);
                instance = xEvent.Element(ns + "InstanceID");
                // We can receive other types of change events here.
                if (instance == null) return;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("ParseChangeXMLXMLError", ClassName, ex);
                return;
            }
            try
            {
                try
                {
                    //InstanceID
                    var iids = instance.Attribute("val").Value;
                    if (int.TryParse(iids, out int iid))
                    {
                        if (pl.PlayerProperties.InstanceID != iid)
                        {
                            pl.PlayerProperties.InstanceID = iid;
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:Instance", ClassName, ex);
                }
                try
                {
                    //SleepTimerverarbeiten
                    XElement sleepTimerGene = instance.Element(nsnext + "SleepTimerGeneration");
                    if (sleepTimerGene != null)
                    {
                        var stgstring = sleepTimerGene.Attribute("val").Value;
                        var trystate = int.TryParse(stgstring, out int stg);
                        //Hier wurde der SleepTimer geändert
                        if (trystate && (pl.PlayerProperties.SleepTimerRunning == false && stg > 0 || pl.PlayerProperties.SleepTimerRunning && stg <= 0))
                        {
                            pl.PlayerProperties.SleepTimerGeneration = stgstring;
                            pl.PlayerProperties.SleepTimerRunning = stg > 0;
                            if (LastChangeDates[SonosEnums.EventingEnums.SleepTimerRunning].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.SleepTimerRunning] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.SleepTimerRunning, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:SleepTimerGeneration", ClassName, ex);
                }
                if (instance.Element(ns + "TransportState") == null)
                {
                    return;
                }
                try
                {
                    //Fademode
                    XElement currentCrossfadeMode = instance.Element(ns + "CurrentCrossfadeMode");
                    if (currentCrossfadeMode != null)
                    {
                        string t = currentCrossfadeMode.Attribute("val").Value;
                        if (pl.PlayerProperties.CurrentCrossFadeMode == true && t != "1" || pl.PlayerProperties.CurrentCrossFadeMode == false && t == "1" || pl.PlayerProperties.CurrentCrossFadeMode == null)
                        {
                            pl.PlayerProperties.CurrentCrossFadeMode = t == "1";
                            if (LastChangeDates[SonosEnums.EventingEnums.CurrentCrossFadeMode].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.CurrentCrossFadeMode] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.CurrentCrossFadeMode, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:CurrentCrossfadeMode", ClassName, ex);
                }
                try
                {
                    //Transportstate
                    XElement transportStatElement = instance.Element(ns + "TransportState");
                    if (transportStatElement != null)
                    {
                        var ts = transportStatElement.Attribute("val").Value;
                        if (Enum.TryParse(ts, out SonosEnums.TransportState tsenum))
                        {
                            //Transportstate konnte korrekt gepraste werden
                            if (pl.PlayerProperties.TransportState != tsenum)
                            {
                                Debug.WriteLine(pl.Name + " Transportstate wechsel:" + tsenum);
                                pl.PlayerProperties.TransportState = tsenum;
                                if (LastChangeDates[SonosEnums.EventingEnums.TransportState].Ticks == 0)
                                {
                                    LastChangeDates[SonosEnums.EventingEnums.TransportState] = DateTime.Now;
                                }
                                else
                                {
                                    ManuellStateChange(SonosEnums.EventingEnums.TransportState, DateTime.Now);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:Transportstate", ClassName, ex);
                }
                try
                {
                    //Playmode
                    XElement currentPlayModeElement = instance.Element(ns + "CurrentPlayMode");
                    if (currentPlayModeElement != null)
                    {
                        string tcpm = currentPlayModeElement.Attribute("val").Value;
                        if (Enum.TryParse(tcpm, out SonosEnums.PlayModes tcpmenum) && pl.PlayerProperties.CurrentPlayMode != tcpmenum)
                        {
                            pl.PlayerProperties.CurrentPlayMode = tcpmenum;
                            if (LastChangeDates[SonosEnums.EventingEnums.CurrentPlayMode].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.CurrentPlayMode] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.CurrentPlayMode, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:CurrentPlayMode", ClassName, ex);
                }
                try
                {
                    //NumberofTRacks
                    XElement numberOfTracksElement = instance.Element(ns + "NumberOfTracks");
                    if (numberOfTracksElement != null)
                    {
                        if (int.TryParse(numberOfTracksElement.Attribute("val").Value, out int not))
                        {
                            if (pl.PlayerProperties.NumberOfTracks != not)
                            {
                                pl.PlayerProperties.NumberOfTracks = not;
                                if (LastChangeDates[SonosEnums.EventingEnums.NumberOfTracks].Ticks == 0)
                                {
                                    LastChangeDates[SonosEnums.EventingEnums.NumberOfTracks] = DateTime.Now;
                                }
                                else
                                {
                                    ManuellStateChange(SonosEnums.EventingEnums.NumberOfTracks, DateTime.Now);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:NumberofTracks", ClassName, ex);
                }
                try
                {
                    //CurrentSection
                    XElement currentSection = instance.Element(ns + "CurrentSection");
                    if (currentSection != null)
                    {
                        var tctd = currentSection.Attribute("val").Value;
                        if (pl.PlayerProperties.CurrentSection != tctd)
                        {
                            pl.PlayerProperties.CurrentSection = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.CurrentSection].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.CurrentSection] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.CurrentSection, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:CurrentSection", ClassName, ex);
                }
                try
                {
                    //PlaybackStorageMedium
                    XElement playbackStorageMedium = instance.Element(ns + "PlaybackStorageMedium");
                    if (playbackStorageMedium != null)
                    {
                        if (Enum.TryParse(playbackStorageMedium.Attribute("val").Value, out SonosEnums.PlaybackStorageMedium psm) && pl.PlayerProperties.PlaybackStorageMedium != psm)
                        {
                            pl.PlayerProperties.PlaybackStorageMedium = psm;
                            if (LastChangeDates[SonosEnums.EventingEnums.PlaybackStorageMedium].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.PlaybackStorageMedium] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.PlaybackStorageMedium, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:PlaybackStorageMedium", ClassName, ex);
                }
                try
                {
                    //AVTransportURI
                    XElement AVTransportURI = instance.Element(ns + "AVTransportURI");
                    if (AVTransportURI != null)
                    {
                        var tctd = AVTransportURI.Attribute("val").Value;
                        if (pl.PlayerProperties.AVTransportURI != tctd)
                        {
                            pl.PlayerProperties.AVTransportURI = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.AVTransportURI].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.AVTransportURI] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.AVTransportURI, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:AvTransportUri", ClassName, ex);
                }
                try
                {
                    //AVTransportURIMetaData
                    XElement AVTransportURIMetaData = instance.Element(ns + "AVTransportURIMetaData");
                    if (AVTransportURIMetaData != null)
                    {
                        var tctd = AVTransportURIMetaData.Attribute("val").Value;
                        if (pl.PlayerProperties.AVTransportURIMetaData != tctd)
                        {
                            pl.PlayerProperties.AVTransportURIMetaData = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.AVTransportURIMetaData].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.AVTransportURIMetaData] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.AVTransportURIMetaData, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:AvTransportUriMeta", ClassName, ex);
                }
                try
                {
                    //NextAVTransportURI
                    XElement NextAVTransportURI = instance.Element(ns + "NextAVTransportURI");
                    if (NextAVTransportURI != null)
                    {
                        var tctd = NextAVTransportURI.Attribute("val").Value;
                        if (pl.PlayerProperties.NextAVTransportURI != tctd)
                        {
                            pl.PlayerProperties.NextAVTransportURI = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.NextAVTransportURI].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.NextAVTransportURI] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.NextAVTransportURI, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:NextAvTransporturi", ClassName, ex);
                }
                try
                {
                    //NextAVTransportURIMetaData
                    XElement NextAVTransportURIMetaData = instance.Element(ns + "NextAVTransportURIMetaData");
                    if (NextAVTransportURIMetaData != null)
                    {
                        var tctd = NextAVTransportURIMetaData.Attribute("val").Value;
                        if (pl.PlayerProperties.NextAVTransportURIMetaData != tctd)
                        {
                            pl.PlayerProperties.NextAVTransportURIMetaData = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.NextAVTransportURIMetaData].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.NextAVTransportURIMetaData] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.NextAVTransportURIMetaData, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:NextAvTransportUriMeta", ClassName, ex);
                }
                try
                {
                    //CurrentTransportActions
                    XElement CurrentTransportActions = instance.Element(ns + "CurrentTransportActions");
                    if (CurrentTransportActions != null)
                    {
                        var tctd = CurrentTransportActions.Attribute("val").Value;
                        if (tctd.Contains(','))
                        {
                            pl.PlayerProperties.CurrentTransportActions = tctd.Split(',').Select(x => x.Trim()).ToList();
                        }
                        else
                        {
                            if (!pl.PlayerProperties.CurrentTransportActions.Contains(tctd))
                            {
                                pl.PlayerProperties.CurrentTransportActions.Add(tctd);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:CurrentTransportActions", ClassName, ex);
                }
                try
                {
                    //TransportPlaySpeed
                    XElement TransportPlaySpeed = instance.Element(ns + "TransportPlaySpeed");
                    if (TransportPlaySpeed != null)
                    {
                        var tctd = TransportPlaySpeed.Attribute("val").Value;
                        if (pl.PlayerProperties.TransportPlaySpeed != tctd)
                        {
                            pl.PlayerProperties.TransportPlaySpeed = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.TransportPlaySpeed].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.TransportPlaySpeed] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.TransportPlaySpeed, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:TransportPlaySpeed", ClassName, ex);
                }
                try
                {
                    //CurrentMediaDuration
                    XElement CurrentMediaDuration = instance.Element(ns + "CurrentMediaDuration");
                    if (CurrentMediaDuration != null)
                    {
                        var tctd = CurrentMediaDuration.Attribute("val").Value;
                        if (pl.PlayerProperties.CurrentMediaDuration != tctd)
                        {
                            pl.PlayerProperties.CurrentMediaDuration = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.CurrentMediaDuration].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.CurrentMediaDuration] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.CurrentMediaDuration, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:CurrentMediaDuration", ClassName, ex);
                }
                try
                {
                    //RecordStorageMedium
                    XElement RecordStorageMedium = instance.Element(ns + "RecordStorageMedium");
                    if (RecordStorageMedium != null)
                    {
                        var tctd = RecordStorageMedium.Attribute("val").Value;
                        if (pl.PlayerProperties.RecordStorageMedium != tctd)
                        {
                            pl.PlayerProperties.RecordStorageMedium = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.RecordStorageMedium].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.RecordStorageMedium] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.RecordStorageMedium, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:RecordStorageMedium", ClassName, ex);
                }
                try
                {
                    //PossiblePlaybackStorageMedia
                    XElement PossiblePlaybackStorageMedia = instance.Element(ns + "PossiblePlaybackStorageMedia");
                    if (PossiblePlaybackStorageMedia != null)
                    {
                        var tctd = PossiblePlaybackStorageMedia.Attribute("val").Value;
                        if (pl.PlayerProperties.PossiblePlaybackStorageMedia != tctd)
                        {
                            pl.PlayerProperties.PossiblePlaybackStorageMedia = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.PossiblePlaybackStorageMedia].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.PossiblePlaybackStorageMedia] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.PossiblePlaybackStorageMedia, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:PossiblePlaybackStorageMedia", ClassName, ex);
                }
                try
                {
                    //PossibleRecordStorageMedia
                    XElement PossibleRecordStorageMedia = instance.Element(ns + "PossibleRecordStorageMedia");
                    if (PossibleRecordStorageMedia != null)
                    {
                        var tctd = PossibleRecordStorageMedia.Attribute("val").Value;
                        if (pl.PlayerProperties.PossibleRecordStorageMedia != tctd)
                        {
                            pl.PlayerProperties.PossibleRecordStorageMedia = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.PossibleRecordStorageMedia].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.PossibleRecordStorageMedia] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.PossibleRecordStorageMedia, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:PossibleRecordStorageMedia", ClassName, ex);
                }
                try
                {
                    //RecordMediumWriteStatus
                    XElement RecordMediumWriteStatus = instance.Element(ns + "RecordMediumWriteStatus");
                    if (RecordMediumWriteStatus != null)
                    {
                        var tctd = RecordMediumWriteStatus.Attribute("val").Value;
                        if (pl.PlayerProperties.RecordMediumWriteStatus != tctd)
                        {
                            pl.PlayerProperties.RecordMediumWriteStatus = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.RecordMediumWriteStatus].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.RecordMediumWriteStatus] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.RecordMediumWriteStatus, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:RecordMediumWriteStatus", ClassName, ex);
                }
                try
                {
                    //CurrentRecordQualityMode
                    XElement CurrentRecordQualityMode = instance.Element(ns + "CurrentRecordQualityMode");
                    if (CurrentRecordQualityMode != null)
                    {
                        var tctd = CurrentRecordQualityMode.Attribute("val").Value;
                        if (pl.PlayerProperties.CurrentRecordQualityMode != tctd)
                        {
                            pl.PlayerProperties.CurrentRecordQualityMode = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.CurrentRecordQualityMode].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.CurrentRecordQualityMode] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.CurrentRecordQualityMode, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:CurrentRecordQualityMode", ClassName, ex);
                }
                try
                {
                    //PossibleRecordQualityModes
                    XElement PossibleRecordQualityModes = instance.Element(ns + "PossibleRecordQualityModes");
                    if (PossibleRecordQualityModes != null)
                    {
                        var tctd = PossibleRecordQualityModes.Attribute("val").Value;
                        if (pl.PlayerProperties.PossibleRecordQualityModes != tctd)
                        {
                            pl.PlayerProperties.PossibleRecordQualityModes = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.PossibleRecordQualityModes].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.PossibleRecordQualityModes] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.PossibleRecordQualityModes, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:PossibleRecordQualityModes", ClassName, ex);
                }
                try
                {
                    //EnqueuedTransportURI
                    XElement EnqueuedTransportURI = instance.Element(nsnext + "EnqueuedTransportURI");
                    if (EnqueuedTransportURI != null)
                    {
                        var tctd = EnqueuedTransportURI.Attribute("val").Value;
                        if (pl.PlayerProperties.EnqueuedTransportURI != tctd)
                        {
                            pl.PlayerProperties.EnqueuedTransportURI = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.EnqueuedTransportURI].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.EnqueuedTransportURI] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.EnqueuedTransportURI, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:EnqueuedTransportUri", ClassName, ex);
                }
                string enqueuedTransportURIMetaDataValue = String.Empty;
                try
                {
                    //EnqueuedTransportURIMetaData
                    XElement EnqueuedTransportURIMetaData = instance.Element(nsnext + "EnqueuedTransportURIMetaData");
                    if (EnqueuedTransportURIMetaData != null)
                    {
                        enqueuedTransportURIMetaDataValue = EnqueuedTransportURIMetaData.Attribute("val").Value;
                        if (pl.PlayerProperties.EnqueuedTransportURIMetaDataString != enqueuedTransportURIMetaDataValue)
                        {
                            pl.PlayerProperties.EnqueuedTransportURIMetaDataString = enqueuedTransportURIMetaDataValue;
                            pl.PlayerProperties.EnqueuedTransportURIMetaData = SonosItem.ParseSingleItem(enqueuedTransportURIMetaDataValue);
                            if (LastChangeDates[SonosEnums.EventingEnums.EnqueuedTransportURIMetaData].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.EnqueuedTransportURIMetaData] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.EnqueuedTransportURIMetaData, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:EnqueuedTransportUriMetaData MetaData:" + enqueuedTransportURIMetaDataValue, ClassName, ex);
                }
                try
                {
                    //CurrentValidPlayModes
                    XElement CurrentValidPlayModes = instance.Element(nsnext + "CurrentValidPlayModes");
                    if (CurrentValidPlayModes != null)
                    {
                        var tctd = CurrentValidPlayModes.Attribute("val").Value;
                        if (tctd.Contains(','))
                        {
                            pl.PlayerProperties.CurrentValidPlayModes = tctd.Split(',').Select(x => x.Trim()).ToList();
                        }
                        else
                        {
                            if (!pl.PlayerProperties.CurrentValidPlayModes.Contains(tctd))
                            {
                                pl.PlayerProperties.CurrentValidPlayModes.Add(tctd);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:CurrentValidPlayModes", ClassName, ex);
                }
                try
                {
                    //MuseSessions
                    XElement MuseSessions = instance.Element(nsnext + "MuseSessions");
                    if (MuseSessions != null)
                    {
                        var tctd = MuseSessions.Attribute("val").Value;
                        if (pl.PlayerProperties.MuseSessions != tctd)
                        {
                            pl.PlayerProperties.MuseSessions = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.MuseSessions].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.MuseSessions] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.MuseSessions, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:MuseSessions", ClassName, ex);
                }
                try
                {
                    //DirectControlClientID
                    XElement DirectControlClientID = instance.Element(nsnext + "DirectControlClientID");
                    if (DirectControlClientID != null)
                    {
                        var tctd = DirectControlClientID.Attribute("val").Value;
                        if (pl.PlayerProperties.DirectControlClientID != tctd)
                        {
                            pl.PlayerProperties.DirectControlClientID = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.DirectControlClientID].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.DirectControlClientID] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.DirectControlClientID, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:DirectControlClientID", ClassName, ex);
                }
                try
                {
                    //DirectControlIsSuspended
                    XElement DirectControlIsSuspended = instance.Element(nsnext + "DirectControlIsSuspended");
                    if (DirectControlIsSuspended != null)
                    {
                        var tctd = DirectControlIsSuspended.Attribute("val").Value;
                        if (pl.PlayerProperties.DirectControlIsSuspended != tctd)
                        {
                            pl.PlayerProperties.DirectControlIsSuspended = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.DirectControlIsSuspended].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.DirectControlIsSuspended] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.DirectControlIsSuspended, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:DirectControlIsSuspended", ClassName, ex);
                }
                try
                {
                    //DirectControlAccountID
                    XElement DirectControlAccountID = instance.Element(nsnext + "DirectControlAccountID");
                    if (DirectControlAccountID != null)
                    {
                        var tctd = DirectControlAccountID.Attribute("val").Value;
                        if (pl.PlayerProperties.DirectControlAccountID != tctd)
                        {
                            pl.PlayerProperties.DirectControlAccountID = tctd;
                            if (LastChangeDates[SonosEnums.EventingEnums.DirectControlAccountID].Ticks == 0)
                            {
                                LastChangeDates[SonosEnums.EventingEnums.DirectControlAccountID] = DateTime.Now;
                            }
                            else
                            {
                                ManuellStateChange(SonosEnums.EventingEnums.DirectControlAccountID, DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:DirectControlAccountId", ClassName, ex);
                }
                try
                {
                    //AlarmRunning
                    XElement AlarmRunning = instance.Element(nsnext + "AlarmRunning");
                    if (AlarmRunning != null)
                    {
                        var tctd = AlarmRunning.Attribute("val").Value;
                        if (Boolean.TryParse(tctd, out bool ar))
                        {
                            if (pl.PlayerProperties.AlarmRunning != ar)
                            {
                                pl.PlayerProperties.AlarmRunning = ar;
                                if (LastChangeDates[SonosEnums.EventingEnums.AlarmRunning].Ticks == 0)
                                {
                                    LastChangeDates[SonosEnums.EventingEnums.AlarmRunning] = DateTime.Now;
                                }
                                else
                                {
                                    ManuellStateChange(SonosEnums.EventingEnums.AlarmRunning, DateTime.Now);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:AlarmRunnig", ClassName, ex);
                }
                try
                {
                    //SnoozeRunning
                    XElement SnoozeRunning = instance.Element(nsnext + "SnoozeRunning");
                    if (SnoozeRunning != null)
                    {
                        var tctd = SnoozeRunning.Attribute("val").Value;
                        if (Boolean.TryParse(tctd, out bool sr))
                        {
                            if (pl.PlayerProperties.SnoozeRunning != sr)
                            {
                                pl.PlayerProperties.SnoozeRunning = sr;
                                if (LastChangeDates[SonosEnums.EventingEnums.SnoozeRunning].Ticks == 0)
                                {
                                    LastChangeDates[SonosEnums.EventingEnums.SnoozeRunning] = DateTime.Now;
                                }
                                else
                                {
                                    ManuellStateChange(SonosEnums.EventingEnums.SnoozeRunning, DateTime.Now);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:SnoozeRunning", ClassName, ex);
                }
                try
                {
                    //RestartPending
                    XElement RestartPending = instance.Element(nsnext + "RestartPending");
                    if (RestartPending != null)
                    {
                        var tctd = RestartPending.Attribute("val").Value;
                        if (Boolean.TryParse(tctd, out bool rp))
                        {
                            if (pl.PlayerProperties.RestartPending != rp)
                            {
                                pl.PlayerProperties.RestartPending = rp;
                                if (LastChangeDates[SonosEnums.EventingEnums.RestartPending].Ticks == 0)
                                {
                                    LastChangeDates[SonosEnums.EventingEnums.RestartPending] = DateTime.Now;
                                }
                                else
                                {
                                    ManuellStateChange(SonosEnums.EventingEnums.RestartPending, DateTime.Now);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:RestartPending", ClassName, ex);
                }
                try
                {
                    //Wenn NextTrack aktiviert ist, diesen durchlaufen lassen und entsprechend anzeigen
                    XElement nextTrackMetaData = instance.Element(nsnext + "NextTrackMetaData");
                    if (nextTrackMetaData != null)
                    {
                        string b = nextTrackMetaData.Attribute("val").Value;
                        if (pl.PlayerProperties.NextTrack.MetaData != b)
                        {
                            SonosItem tnext = new();
                            if (!String.IsNullOrEmpty(b))
                            {
                                tnext = SonosItem.ParseSingleItem(b);
                            }
                            if (pl.PlayerProperties.NextTrack == null || pl.PlayerProperties.NextTrack.Title != tnext.Title || pl.PlayerProperties.NextTrack.Artist != tnext.Artist)
                            {
                                pl.PlayerProperties.NextTrack = tnext;
                                pl.PlayerProperties.NextTrack.MetaData = b;
                                if (LastChangeDates[SonosEnums.EventingEnums.NextTrack].Ticks == 0)
                                {
                                    LastChangeDates[SonosEnums.EventingEnums.NextTrack] = DateTime.Now;
                                }
                                else
                                {
                                    ManuellStateChange(SonosEnums.EventingEnums.NextTrack, DateTime.Now);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("ParseChangeXML:NextTrackMetaData", ClassName, ex);
                }

            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("ParseChangeXML", ClassName, ex);
            }
            try
            {
                //CurrentTrack
                XElement currentTrackMetaDataElement = instance.Element(ns + "CurrentTrackMetaData");
                string ctmdevalue = String.Empty;
                if (currentTrackMetaDataElement != null)
                {
                    ctmdevalue = currentTrackMetaDataElement.Attribute("val").Value;
                }
                if (!string.IsNullOrEmpty(ctmdevalue) && pl.PlayerProperties.CurrentTrack.MetaData != ctmdevalue)
                {
                    //<r:streamContent></r:streamContent><r:radioShowMd></r:radioShowMd><r:streamInfo>bd:0,sr:0,c:0,l:0,d:0</r:streamInfo>//todo: schauen, was sich dahinter versteckt. Evtl. Streaming ja/nein
                    pl.PlayerProperties.CurrentTrack = SonosItem.ParseSingleItem(ctmdevalue);
                    pl.PlayerProperties.CurrentTrack.MetaData = ctmdevalue;
                    if (LastChangeDates[SonosEnums.EventingEnums.CurrentTrack].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.CurrentTrack] = DateTime.Now;
                    }
                    else
                    {
                        ManuellStateChange(SonosEnums.EventingEnums.CurrentTrack, DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                SonosItem xyz = new() { Artist = "leer" };
                pl.PlayerProperties.CurrentTrack = xyz;
                pl.ServerErrorsAdd("ParseChangeXMLCurrentTrack", ClassName, ex);
            }
            try
            {
                //CurrentTrackNumber
                XElement currentTrackNumberElement = instance.Element(ns + "CurrentTrack");
                if (currentTrackNumberElement != null)
                {
                    var tctn = currentTrackNumberElement.Attribute("val").Value;
                    if (int.TryParse(tctn, out int ctn) && pl.PlayerProperties.CurrentTrackNumber != ctn)
                    {
                        pl.PlayerProperties.CurrentTrackNumber = ctn;
                        if (LastChangeDates[SonosEnums.EventingEnums.CurrentTrackNumber].Ticks == 0)
                        {
                            LastChangeDates[SonosEnums.EventingEnums.CurrentTrackNumber] = DateTime.Now;
                        }
                        else
                        {
                            ManuellStateChange(SonosEnums.EventingEnums.CurrentTrackNumber, DateTime.Now);
                        }
                    }
                }

                ////CurrentTRackDuration
                //XElement currentTrackDurationElement = instance.Element(ns + "CurrentTrackDuration");
                //if (currentTrackDurationElement != null)
                //{
                //    var tctd = pl.PlayerProperties.ParseDuration(currentTrackDurationElement.Attribute("val").Value);
                //    if (pl.PlayerProperties.CurrentTrack.Duration.ToString() != tctd.ToString())
                //    {
                //        pl.PlayerProperties.CurrentTrack.Duration = tctd;
                //    }
                //}
            }
            catch (Exception ex)
            {
                SonosItem xyz = new() { Artist = "leer" };
                pl.PlayerProperties.CurrentTrack = xyz;
                pl.ServerErrorsAdd("CurrentTrackNumber", ClassName, ex);
            }
        }

        #endregion Eventing
        #region Public Methods
        /// <summary>
        /// Fügt mehrere Songs/Playlisten hinzu.
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <param name="numberoftracks">Anzahl der Tracks</param>
        /// <param name="tracks">Tracks</param>
        /// <param name="asNext">true = ersetzten, false = hinzufügen.</param>
        /// <returns>Liefert die Songnummer des hinzugefügten Songs zurück</returns>
        public async Task<QueueData> AddMultipleURIsToQueue(int numberoftracks, SonosItem tracks, bool asNext = false)
        {

            var arguments = new UPnPArgument[13];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("UpdateID", 0u);
            arguments[2] = new UPnPArgument("NumberOfURIs", Convert.ToUInt32(numberoftracks));
            arguments[3] = new UPnPArgument("EnqueuedURIs", tracks.Uri);
            arguments[4] = new UPnPArgument("EnqueuedURIsMetaData", tracks.MetaData);
            arguments[5] = new UPnPArgument("ContainerURI", String.Empty);
            arguments[6] = new UPnPArgument("ContainerMetaData", String.Empty);
            arguments[7] = new UPnPArgument("DesiredFirstTrackNumberEnqueued", 0u);
            arguments[8] = new UPnPArgument("EnqueueAsNext", asNext);
            arguments[9] = new UPnPArgument("FirstTrackNumberEnqueued", null);
            arguments[10] = new UPnPArgument("NumTracksAdded", null);
            arguments[11] = new UPnPArgument("NewQueueLength", null);
            arguments[12] = new UPnPArgument("NewUpdateID", null);
            await Invoke("AddMultipleURIsToQueue", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 12, 100, 10, WaiterTypes.String);
            QueueData queueData = new();
            if (!string.IsNullOrEmpty(arguments[9].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[10].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[11].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[12].DataValue.ToString()))
            {
                if (int.TryParse(arguments[9].DataValue.ToString(), out int ft))
                    queueData.FirstTrackNumberEnqueued = ft;
                if (int.TryParse(arguments[10].DataValue.ToString(), out int nt))
                    queueData.NumTracksAdded = nt;
                if (int.TryParse(arguments[11].DataValue.ToString(), out int nq))
                    queueData.NewQueueLength = nq;
                if (ushort.TryParse(arguments[12].DataValue.ToString(), out ushort nu))
                    queueData.NewUpdateID = nu;
            }
            if (!queueData.IsEmpty)
            {
                ManuellStateChange(SonosEnums.EventingEnums.QueueChanged, DateTime.Now);
            }
            return queueData;
        }
        /// <summary>
        /// Song der Playlist zufügen
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <param name="track">SonosItem mit ausgefüllter URI und MetaData</param>
        /// <param name="asNext">Als Nächstes oder hinten dran stellen</param>
        /// <returns></returns>
        public async Task<QueueData> AddURIToQueue(SonosItem track, bool asNext = false, bool fireQueueChange = true)
        {
            QueueData queueData = new();
            var arguments = new UPnPArgument[8];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("EnqueuedURI", track.Uri);
            arguments[2] = new UPnPArgument("EnqueuedURIMetaData", track.MetaData);
            arguments[3] = new UPnPArgument("DesiredFirstTrackNumberEnqueued", 0u);
            arguments[4] = new UPnPArgument("EnqueueAsNext", asNext);
            arguments[5] = new UPnPArgument("FirstTrackNumberEnqueued", null);
            arguments[6] = new UPnPArgument("NumTracksAdded", null);
            arguments[7] = new UPnPArgument("NewQueueLength", null);
            await Invoke("AddURIToQueue", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 6, 100, 10, WaiterTypes.String);
            pl.PlayerProperties.EnqueuedTransportURI = track.Uri;
            ManuellStateChange(SonosEnums.EventingEnums.EnqueuedTransportURI, DateTime.Now);
            if (arguments[5].DataValue != null && arguments[6].DataValue != null && arguments[7].DataValue != null)
            {
                if (int.TryParse(arguments[5].DataValue.ToString(), out int ft))
                    queueData.FirstTrackNumberEnqueued = ft;
                if (int.TryParse(arguments[6].DataValue.ToString(), out int nt))
                    queueData.NumTracksAdded = nt;
                if (int.TryParse(arguments[7].DataValue.ToString(), out int nq))
                    queueData.NewQueueLength = nq;
            }
            pl.PlayerProperties.QueueChanged += 1;
            if (fireQueueChange)
            {
                ManuellStateChange(SonosEnums.EventingEnums.QueueChanged, DateTime.Now);
            }
            return queueData;
        }

        public async Task<QueueData> AddURIToQueue(SonosItem track, Playlist playlistToUse, bool asNext = false)
        {
            if (playlistToUse != null && playlistToUse.NumberReturned > 0 && playlistToUse.PlayListItems.Count > 0 && playlistToUse.PlayListItems[0].Album != "leer")
            {
                pl.PlayerProperties.Playlist = playlistToUse;
            }
            return await AddURIToQueue(track, asNext, false);
        }

        /// <summary>
        /// Unbekannt
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="ObjectID"></param>
        /// <param name="UpdateID"></param>
        /// <param name="EnqueuedURI"></param>
        /// <param name="EnqueuedURIMetaData"></param>
        /// <param name="AddAtIndex"></param>
        /// <param name="InstanceID"></param>
        /// <returns>Anzahl zugefügter Tracks</returns>
        public async Task<QueueData> AddURIToSavedQueue(string ObjectID, UInt32 UpdateID, string EnqueuedURI, string EnqueuedURIMetaData, UInt32 AddAtIndex = 0, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[9];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("ObjectID", ObjectID);
            arguments[2] = new UPnPArgument("UpdateID", UpdateID);
            arguments[3] = new UPnPArgument("EnqueuedURI", EnqueuedURI);
            arguments[4] = new UPnPArgument("EnqueuedURIMetaData", EnqueuedURIMetaData);
            arguments[5] = new UPnPArgument("AddAtIndex", AddAtIndex);
            arguments[6] = new UPnPArgument("NumTracksAdded", null);
            arguments[7] = new UPnPArgument("NewQueueLength", null);
            arguments[8] = new UPnPArgument("NewUpdateID", null);
            await Invoke("AddURIToSavedQueue", arguments, 100);
            QueueData queueData = new();
            if (!string.IsNullOrEmpty(arguments[5].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[6].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[7].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[7].DataValue.ToString()))
            {
                if (int.TryParse(arguments[5].DataValue.ToString(), out int at))
                    queueData.AddAtIndex = at;
                if (int.TryParse(arguments[6].DataValue.ToString(), out int nt))
                    queueData.NumTracksAdded = nt;
                if (int.TryParse(arguments[7].DataValue.ToString(), out int nq))
                    queueData.NewQueueLength = nq;
                if (ushort.TryParse(arguments[8].DataValue.ToString(), out ushort nu))
                    queueData.NewUpdateID = nu;
            }
            if (!queueData.IsEmpty)
            {
                ManuellStateChange(SonosEnums.EventingEnums.QueueChanged, DateTime.Now);
            }
            return queueData;
        }
        public async Task<Boolean> BackupQueue(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            return await Invoke("BackupQueue", arguments);
        }
        /// <summary>
        /// Player aus vorhandene Zonen entfernen.
        /// </summary>
        public async Task<Boolean> BecomeCoordinatorOfStandaloneGroup()
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("DelegatedGroupCoordinatorID", null);
            arguments[2] = new UPnPArgument("NewGroupID", null);
            var retval = await Invoke("BecomeCoordinatorOfStandaloneGroup", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            if (retval)
            {
                if (pl.PlayerProperties.LocalGroupUUID != pl.UUID)
                {
                    pl.PlayerProperties.LocalGroupUUID = pl.UUID;
                    ManuellStateChange(SonosEnums.EventingEnums.LocalGroupUUID, DateTime.Now);
                }
                if (!pl.PlayerProperties.GroupCoordinatorIsLocal)
                {
                    pl.PlayerProperties.GroupCoordinatorIsLocal = true;
                    ManuellStateChange(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, DateTime.Now);
                }
                if (pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Count > 1)
                {
                    pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Clear();
                    pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Add(pl.UUID);
                    ManuellStateChange(SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup, DateTime.Now);
                }
            }
            return retval;
        }
        public async Task<Boolean> BecomeGroupCoordinator(string CurrentCoordinator, string CurrentGroupID, string OtherMembers, string TransportSettings, string CurrentURI, string CurrentURIMetaData, string SleepTimerState, string AlarmState, string StreamRestartState, string CurrentQueueTrackList, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[11];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentCoordinator", CurrentCoordinator);
            arguments[2] = new UPnPArgument("CurrentGroupID", CurrentGroupID);
            arguments[3] = new UPnPArgument("OtherMembers", OtherMembers);
            arguments[4] = new UPnPArgument("TransportSettings", TransportSettings);
            arguments[5] = new UPnPArgument("CurrentURI", CurrentURI);
            arguments[6] = new UPnPArgument("CurrentURIMetaData", CurrentURIMetaData);
            arguments[7] = new UPnPArgument("SleepTimerState", SleepTimerState);
            arguments[8] = new UPnPArgument("AlarmState", AlarmState);
            arguments[9] = new UPnPArgument("StreamRestartState", StreamRestartState);
            arguments[10] = new UPnPArgument("CurrentQueueTrackList", CurrentQueueTrackList);
            return await Invoke("BecomeGroupCoordinator", arguments);
        }
        public async Task<Boolean> BecomeGroupCoordinatorAndSource(string CurrentCoordinator, string CurrentGroupID, string OtherMembers, string TransportSettings, string CurrentURI, string CurrentURIMetaData, string SleepTimerState, string AlarmState, string StreamRestartState, string CurrentQueueTrackList, string CurrentSourceState, Boolean ResumePlayback, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[13];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentCoordinator", CurrentCoordinator);
            arguments[2] = new UPnPArgument("CurrentGroupID", CurrentGroupID);
            arguments[3] = new UPnPArgument("OtherMembers", OtherMembers);
            arguments[4] = new UPnPArgument("TransportSettings", TransportSettings);
            arguments[5] = new UPnPArgument("CurrentURI", CurrentURI);
            arguments[6] = new UPnPArgument("CurrentURIMetaData", CurrentURIMetaData);
            arguments[7] = new UPnPArgument("SleepTimerState", SleepTimerState);
            arguments[8] = new UPnPArgument("AlarmState", AlarmState);
            arguments[9] = new UPnPArgument("StreamRestartState", StreamRestartState);
            arguments[10] = new UPnPArgument("CurrentQueueTrackList", CurrentQueueTrackList);
            arguments[11] = new UPnPArgument("CurrentSourceState", CurrentSourceState);
            arguments[12] = new UPnPArgument("ResumePlayback", ResumePlayback);
            return await Invoke("BecomeGroupCoordinatorAndSource", arguments);
        }
        public async Task<Boolean> ChangeCoordinator(string CurrentCoordinator, string NewCoordinator, string NewTransportSettings, string CurrentAVTransportURI, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[5];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[0] = new UPnPArgument("CurrentCoordinator", CurrentCoordinator);
            arguments[0] = new UPnPArgument("NewCoordinator", NewCoordinator);
            arguments[0] = new UPnPArgument("NewTransportSettings", NewTransportSettings);
            arguments[0] = new UPnPArgument("CurrentAVTransportURI", CurrentAVTransportURI);
            return await Invoke("ChangeCoordinator", arguments);
        }
        public async Task<Boolean> ChangeTransportSettings(string NewTransportSettings, string CurrentAVTransportURI, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("NewTransportSettings", NewTransportSettings);
            arguments[2] = new UPnPArgument("CurrentAVTransportURI", CurrentAVTransportURI);
            return await Invoke("ChangeTransportSettings", arguments);
        }
        /// <summary>
        /// Setzt den Schlummermodus
        /// </summary>
        /// <param name="sleeptimer">Dauer hh:mm:ss oder String.Empty für Aus</param>
        public async Task<Boolean> ConfigureSleepTimer(string sleeptimer)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("NewSleepTimerDuration", sleeptimer);
            var ret = await Invoke("ConfigureSleepTimer", arguments);
            if (ret)
            {
                if (string.IsNullOrEmpty(sleeptimer))
                {
                    if (pl.PlayerProperties.SleepTimerRunning)
                    {
                        pl.PlayerProperties.SleepTimerRunning = false;
                        ManuellStateChange(SonosEnums.EventingEnums.SleepTimerRunning, DateTime.Now);
                    }
                    if (pl.PlayerProperties.RemainingSleepTimerDuration != SonosConstants.Off)
                    {
                        pl.PlayerProperties.RemainingSleepTimerDuration = SonosConstants.Off;
                        ManuellStateChange(SonosEnums.EventingEnums.RemainingSleepTimerDuration, DateTime.Now);
                    }
                }
                else
                {
                    if (!pl.PlayerProperties.SleepTimerRunning)
                    {
                        pl.PlayerProperties.SleepTimerRunning = true;
                        ManuellStateChange(SonosEnums.EventingEnums.SleepTimerRunning, DateTime.Now);
                    }
                    if (pl.PlayerProperties.RemainingSleepTimerDuration == SonosConstants.Off)
                    {
                        pl.PlayerProperties.RemainingSleepTimerDuration = sleeptimer;
                        ManuellStateChange(SonosEnums.EventingEnums.RemainingSleepTimerDuration, DateTime.Now);
                    }
                }
            }
            return ret;
        }
        public async Task<QueueData> CreateSavedQueue(string Title, string EnqueuedURI, string EnqueuedURIMetaData, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[8];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Title", Title);
            arguments[2] = new UPnPArgument("EnqueuedURI", EnqueuedURI);
            arguments[3] = new UPnPArgument("EnqueuedURIMetaData", EnqueuedURIMetaData);
            arguments[4] = new UPnPArgument("NumTracksAdded", null);
            arguments[5] = new UPnPArgument("NewQueueLength", null);
            arguments[6] = new UPnPArgument("AssignedObjectID", null);
            arguments[7] = new UPnPArgument("NewUpdateID", null);
            await Invoke("CreateSavedQueue", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 7, 100, 10, WaiterTypes.String);
            QueueData queueData = new();
            if (!string.IsNullOrEmpty(arguments[4].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[5].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[6].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[7].DataValue.ToString()))
            {
                if (int.TryParse(arguments[4].DataValue.ToString(), out int nt))
                    queueData.NumTracksAdded = nt;
                if (int.TryParse(arguments[5].DataValue.ToString(), out int nq))
                    queueData.NewQueueLength = nq;
                if (ushort.TryParse(arguments[7].DataValue.ToString(), out ushort nu))
                    queueData.NewUpdateID = nu;
                queueData.AssignedObjectID = arguments[6].DataValue.ToString();
            }
            if (!queueData.IsEmpty)
            {
                ManuellStateChange(SonosEnums.EventingEnums.QueueChanged, DateTime.Now);
            }
            return queueData;
        }
        public async Task<Boolean> DelegateGroupCoordinationTo(string NewCoordinator, Boolean RejoinGroup, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("NewCoordinator", NewCoordinator);
            arguments[2] = new UPnPArgument("RejoinGroup", RejoinGroup);
            return await Invoke("DelegateGroupCoordinationTo", arguments);
        }
        public async Task<Boolean> EndDirectControlSession(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            return await Invoke("EndDirectControlSession", arguments);
        }
        /// <summary>
        /// Liefert den Überblendungs Modus zurück
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean?> GetCrossfadeMode()
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("CrossfadeMode", null);
            await Invoke("GetCrossfadeMode", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 200, 10, WaiterTypes.String);
            if (arguments[1]?.DataValue != null && Boolean.TryParse(arguments[1].DataValue.ToString(), out bool fm))
            {
                if (pl.PlayerProperties.CurrentCrossFadeMode != fm)
                {
                    pl.PlayerProperties.CurrentCrossFadeMode = fm;
                    ManuellStateChange(SonosEnums.EventingEnums.CurrentCrossFadeMode, DateTime.Now);
                }
                return fm;
            }
            return null;
        }
        public async Task<List<String>> GetCurrentTransportActions(UInt32 InstanceID = 0)
        {

            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Actions", null);
            await Invoke("GetCurrentTransportActions", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            pl.PlayerProperties.CurrentTransportActions = arguments[1].DataValue.ToString().Split(',').ToList();
            return pl.PlayerProperties.CurrentTransportActions;
        }
        public async Task<DeviceCapabilities> GetDeviceCapabilities(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("PlayMedia", null);
            arguments[2] = new UPnPArgument("RecMedia", null);
            arguments[3] = new UPnPArgument("RecQualityModes", null);
            await Invoke("GetDeviceCapabilities", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            try
            {
                return new DeviceCapabilities
                {
                    PlayMedia = arguments[1].DataValue.ToString().Split(',').ToList(),
                    RecMedia = arguments[2].DataValue.ToString(),
                    RecQualityModes = arguments[3].DataValue.ToString()
                };


            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("GetDeviceCapabilities", ClassName, ex);
                return new DeviceCapabilities();
            }
        }
        /// <summary>
        /// Liefert die Anzahl der Tracks einer Playlist zurück.
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<MediaInfo> GetMediaInfo()
        {
            var arguments = new UPnPArgument[10];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("NrTracks", 0u);
            arguments[2] = new UPnPArgument("MediaDuration", null);
            arguments[3] = new UPnPArgument("CurrentURI", null);
            arguments[4] = new UPnPArgument("CurrentURIMetaData", null);
            arguments[5] = new UPnPArgument("NextURI", null);
            arguments[6] = new UPnPArgument("NextURIMetaData", null);
            arguments[7] = new UPnPArgument("PlayMedium", null);
            arguments[8] = new UPnPArgument("RecordMedium", null);
            arguments[9] = new UPnPArgument("WriteStatus", null);
            await Invoke("GetMediaInfo", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            MediaInfo mi = new();
            if (arguments[1].DataValue == null || arguments[3].DataValue == null || arguments[4].DataValue == null || arguments[5].DataValue == null || arguments[7].DataValue == null)
                return mi;

            if (int.TryParse(arguments[1].DataValue.ToString(), out int not))
                mi.NumberOfTracks = not;
            mi.NextURI = arguments[5].DataValue.ToString();
            mi.URI = arguments[3].DataValue.ToString();
            mi.URIMetaData = arguments[4].DataValue.ToString();
            mi.PlayMedium = arguments[7].DataValue.ToString();
            if (pl.PlayerProperties.NumberOfTracks != mi.NumberOfTracks)
            {
                pl.PlayerProperties.NumberOfTracks = mi.NumberOfTracks;
                ManuellStateChange(SonosEnums.EventingEnums.NumberOfTracks, DateTime.Now);
            }
            if (pl.PlayerProperties.AVTransportURI != mi.URI)
            {
                pl.PlayerProperties.AVTransportURI = mi.URI;
                ManuellStateChange(SonosEnums.EventingEnums.AVTransportURI, DateTime.Now);
            }
            if (pl.PlayerProperties.NextAVTransportURI != mi.NextURI)
            {
                pl.PlayerProperties.NextAVTransportURI = mi.NextURI;
                ManuellStateChange(SonosEnums.EventingEnums.NextAVTransportURI, DateTime.Now);
            }
            return mi;
        }
        /// <summary>
        /// Liefert den Aktuellen Song
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <returns>PlayerInfo mit aktuellen Song daten</returns>
        public async Task<PlayerInfo> GetPositionInfo()
        {
            var arguments = new UPnPArgument[9];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Track", 0u);
            arguments[2] = new UPnPArgument("TrackDuration", null);
            arguments[3] = new UPnPArgument("TrackMetaData", null);
            arguments[4] = new UPnPArgument("TrackURI", null);
            arguments[5] = new UPnPArgument("RelTime", null);
            arguments[6] = new UPnPArgument("AbsTime", null);
            arguments[7] = new UPnPArgument("RelCount", 0);
            arguments[8] = new UPnPArgument("AbsCount", 0);
            await Invoke("GetPositionInfo", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            try
            {
                PlayerInfo pi = new();
                if (arguments[2].DataValue != null && arguments[1].DataValue != null && arguments[3].DataValue != null && arguments[4].DataValue != null && arguments[5].DataValue != null)
                {
                    if (TimeSpan.TryParse((string)arguments[2].DataValue, out TimeSpan trackDuration))
                        pi.TrackDuration = new SonosTimeSpan(trackDuration);
                    if (TimeSpan.TryParse((string)arguments[5].DataValue, out TimeSpan relTime))
                        pi.RelTime = new SonosTimeSpan(relTime);
                    if (int.TryParse(arguments[1].DataValue.ToString(), out int trackindex))
                        pi.TrackIndex = trackindex;
                    if (arguments[3].DataValue != null)
                        pi.TrackMetaData = arguments[3].DataValue.ToString();
                    if (arguments[4].DataValue != null)
                        pi.TrackURI = arguments[4].DataValue.ToString();
                }
                if (!pi.IsEmpty)
                {
                    //Prüfung auf änderungen.
                    if (pl.PlayerProperties.CurrentTrackNumber != pi.TrackIndex)
                    {
                        pl.PlayerProperties.CurrentTrackNumber = pi.TrackIndex;
                        ManuellStateChange(SonosEnums.EventingEnums.CurrentTrackNumber, DateTime.Now);
                    }
                    if (pl.PlayerProperties.CurrentTrack.Uri != pi.TrackURI && pi.TrackMetaData != SonosConstants.NotImplemented && !string.IsNullOrEmpty(pi.TrackURI) && !string.IsNullOrEmpty(pi.TrackMetaData))
                    {
                        //Song neu machen.
                        var song = SonosItem.ParseSingleItem(pi.TrackMetaData);
                        pl.PlayerProperties.CurrentTrack = song;
                        pl.PlayerProperties.CurrentTrack.FillMP3AndItemFromHDD();
                        pl.PlayerProperties.CurrentTrack.RelTime = pi.RelTime;
                        ManuellStateChange(SonosEnums.EventingEnums.CurrentTrack, DateTime.Now);
                        ManuellStateChange(SonosEnums.EventingEnums.RelTime, DateTime.Now);
                    }
                }
                return pi;
            }
            catch
            {
                return new PlayerInfo();
            }
        }
        /// <summary>
        /// Ermittelt den Schlummermodus
        /// </summary>
        /// <returns></returns>
        public async Task<String> GetRemainingSleepTimerDuration(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("RemainingSleepTimerDuration", null);
            arguments[2] = new UPnPArgument("CurrentSleepTimerGeneration", null);
            await Invoke("GetRemainingSleepTimerDuration", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            if (arguments[1].DataValue == null) return String.Empty;
            var sleeptimer = arguments[1].DataValue.ToString();
            if (string.IsNullOrEmpty(sleeptimer) || sleeptimer == SonosConstants.SleepTimerOffValueFromServer)
            {
                sleeptimer = SonosConstants.Off;
            }
            if (pl.PlayerProperties.RemainingSleepTimerDuration != sleeptimer)
            {
                pl.PlayerProperties.RemainingSleepTimerDuration = sleeptimer;
                ManuellStateChange(SonosEnums.EventingEnums.RemainingSleepTimerDuration, DateTime.Now);
            }
            return sleeptimer;
        }
        public async Task<RunningAlarmProperties> GetRunningAlarmProperties(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("AlarmID", null);
            arguments[2] = new UPnPArgument("GroupID", null);
            arguments[3] = new UPnPArgument("LoggedStartTime", null);
            await Invoke("GetRunningAlarmProperties", arguments, 100);
            if (int.TryParse(arguments[1].DataValue.ToString(), out int alarmid))
            {
                return new RunningAlarmProperties
                {
                    AlarmID = alarmid,
                    GroupID = arguments[2].DataValue.ToString(),
                    LoggedStartTime = arguments[3].DataValue.ToString()
                };
            }
            return new();
        }
        /// <summary>
        /// Liefert zurück, ob ein Gerät Pause macht oder gerade abspielt.
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<SonosEnums.TransportState> GetTransportInfo()
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("CurrentTransportState", "");
            arguments[2] = new UPnPArgument("CurrentTransportStatus", "");
            arguments[3] = new UPnPArgument("CurrentSpeed", "");
            await Invoke("GetTransportInfo", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            if (arguments[1].DataValue == null) return SonosEnums.TransportState.TRANSITIONING;
            if (Enum.TryParse(arguments[1].DataValue.ToString(), out SonosEnums.TransportState ret))
            {
                if (pl.PlayerProperties.TransportState != ret)
                {
                    pl.PlayerProperties.TransportState = ret;
                    ManuellStateChange(SonosEnums.EventingEnums.TransportState, DateTime.Now);
                }
                return pl.PlayerProperties.TransportState;
            }
            return SonosEnums.TransportState.UNKNOWING;
        }
        /// <summary>
        /// Liefert die Wiedergabeart zurück
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<SonosEnums.PlayModes> GetTransportSettings()
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("PlayMode", null);
            arguments[2] = new UPnPArgument("RecQualityMode", null);
            await Invoke("GetTransportSettings", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            if (arguments[1].DataValue == null) return SonosEnums.PlayModes.UNKNOWING;
            if (Enum.TryParse(arguments[1].DataValue.ToString(), out SonosEnums.PlayModes re))
            {
                if (pl.PlayerProperties.CurrentPlayMode != re)
                {
                    pl.PlayerProperties.CurrentPlayMode = re;
                    ManuellStateChange(SonosEnums.EventingEnums.CurrentPlayMode, DateTime.Now);
                }
                return pl.PlayerProperties.CurrentPlayMode;
            }
            else
            {
                return SonosEnums.PlayModes.UNKNOWING;
            }
        }
        /// <summary>
        /// Nächsten Song abspielen
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean> Next()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            if (await Invoke("Next", arguments))
            {
                if ((pl.PlayerProperties.CurrentTrackNumber + 1) <= pl.PlayerProperties.NumberOfTracks)
                {
                    pl.PlayerProperties.CurrentTrackNumber += 1;
                    ManuellStateChange(SonosEnums.EventingEnums.CurrentTrackNumber, DateTime.Now);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<Boolean> NotifyDeletedURI(string DeletedURI, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("DeletedURI", DeletedURI);
            return await Invoke("NotifyDeletedURI", arguments);
        }
        /// <summary>
        /// Paussiert den SonosPlayer
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean> Pause()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            var ret = await Invoke("Pause", arguments);
            if (ret && pl.PlayerProperties.TransportState != SonosEnums.TransportState.PAUSED_PLAYBACK)
            {
                pl.PlayerProperties.TransportState = SonosEnums.TransportState.PAUSED_PLAYBACK;
                ManuellStateChange(SonosEnums.EventingEnums.TransportState, DateTime.Now);
            }
            return ret;
        }
        /// <summary>
        /// Startet die Wiedergabe
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean> Play()
        {

            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Speed", "1");
            var ret = await Invoke("Play", arguments);
            if (ret && pl.PlayerProperties.TransportState != SonosEnums.TransportState.PLAYING)
            {
                pl.PlayerProperties.TransportState = SonosEnums.TransportState.PLAYING;
                ManuellStateChange(SonosEnums.EventingEnums.TransportState, DateTime.Now);
            }
            return ret;
        }
        /// <summary>
        /// Vorherigen Song abspielen
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean> Previous()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            if (await Invoke("Previous", arguments))
            {
                if ((pl.PlayerProperties.CurrentTrackNumber - 1) >= 1)
                {
                    pl.PlayerProperties.CurrentTrackNumber -= 1;
                    ManuellStateChange(SonosEnums.EventingEnums.CurrentTrackNumber, DateTime.Now);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Löscht die Playlist
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean> RemoveAllTracksFromQueue()
        {
            await Pause();
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            if (await Invoke("RemoveAllTracksFromQueue", arguments, 200))
            {
                pl.PlayerProperties.QueueChanged += 1;
                pl.PlayerProperties.Playlist.ResetPlaylist();
                pl.PlayerProperties.CurrentTrackNumber = 0;
                pl.PlayerProperties.NumberOfTracks = 0;
                ManuellStateChange(SonosEnums.EventingEnums.CurrentTrackNumber, DateTime.Now);
                ManuellStateChange(SonosEnums.EventingEnums.QueueChangedEmpty, DateTime.Now);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Entfernt einen song aus der aktuellen Wiedergabeliste
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="songnumber">Nummer des Songs aus der Liste beginned mit 1</param>
        /// <returns></returns>
        public async Task<Boolean> RemoveTrackFromQueue(int songnumber)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("ObjectID", "Q:0/" + songnumber);
            arguments[2] = new UPnPArgument("UpdateID", 0u);
            if (await Invoke("RemoveTrackFromQueue", arguments, 100))
            {
                try
                {
                    if (songnumber > 0 && !pl.PlayerProperties.Playlist.IsEmpty && pl.PlayerProperties.Playlist.PlayListItems.Count <= songnumber)
                    {
                        pl.PlayerProperties.Playlist.PlayListItems.Remove(pl.PlayerProperties.Playlist.PlayListItems[songnumber - 1]);
                    }
                }
                catch
                {
                    //continue
                }
                pl.PlayerProperties.QueueChanged += 1;
                ManuellStateChange(SonosEnums.EventingEnums.QueueChangedNoRefillNeeded, DateTime.Now);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="UpdateID"></param>
        /// <param name="StartingIndex"></param>
        /// <param name="NumberOfTracks"></param>
        /// <returns>New Update ID</returns>
        public async Task<int> RemoveTrackRangeFromQueue(UInt16 StartingIndex = 0, UInt16 NumberOfTracks = 0, UInt16 UpdateID = 0)
        {
            var arguments = new UPnPArgument[5];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("UpdateID", UpdateID);
            arguments[2] = new UPnPArgument("StartingIndex", StartingIndex);
            arguments[3] = new UPnPArgument("NumberOfTracks", NumberOfTracks);
            arguments[4] = new UPnPArgument("NewUpdateID", 0u);
            await Invoke("RemoveTrackRangeFromQueue", arguments, 100);
            int.TryParse(arguments[1].DataValue.ToString(), out int uid);
            return uid;
        }
        /// <summary>
        /// Ändern der Reihenfolge in der aktuellen Playlist
        /// </summary>
        /// <param name="pl">Alte Position</param>
        /// <param name="oldposition">Alte Position</param>
        /// <param name="newposition">Neue Position</param>
        public async Task<Boolean> ReorderTracksInQueue(int oldposition, int newposition, int NumberOfTracks = 1)
        {
            var arguments = new UPnPArgument[5];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("StartingIndex", Convert.ToUInt32(oldposition));
            arguments[2] = new UPnPArgument("NumberOfTracks", Convert.ToUInt32(NumberOfTracks));
            arguments[3] = new UPnPArgument("InsertBefore", Convert.ToUInt32(newposition));
            arguments[4] = new UPnPArgument("UpdateID", 0u);
            var ret = await Invoke("ReorderTracksInQueue", arguments);
            if (ret)
            {
                pl.PlayerProperties.QueueChanged += 1;
                pl.PlayerProperties.EnqueuedTransportURI = String.Empty;
                ManuellStateChange(SonosEnums.EventingEnums.EnqueuedTransportURI, DateTime.Now);
                ManuellStateChange(SonosEnums.EventingEnums.QueueChangeResort, DateTime.Now);

            }
            return ret;
        }
        public async Task<QueueData> ReorderTracksInSavedQueue(string ObjectID, UInt32 UpdateID, string TrackList, string NewPositionList, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[8];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("ObjectID", ObjectID);
            arguments[2] = new UPnPArgument("UpdateID", UpdateID);
            arguments[3] = new UPnPArgument("TrackList", TrackList);
            arguments[4] = new UPnPArgument("NewPositionList", NewPositionList);
            arguments[5] = new UPnPArgument("QueueLengthChange", null);
            arguments[6] = new UPnPArgument("NewQueueLength", null);
            arguments[7] = new UPnPArgument("NewUpdateID", null);
            await Invoke("ReorderTracksInSavedQueue", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 7, 100, 10, WaiterTypes.String);
            QueueData queueData = new();
            if (!string.IsNullOrEmpty(arguments[5].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[6].DataValue.ToString()) &&
                !string.IsNullOrEmpty(arguments[7].DataValue.ToString()))
            {
                if (int.TryParse(arguments[4].DataValue.ToString(), out int qlc))
                    queueData.QueueLengthChange = qlc;
                if (int.TryParse(arguments[6].DataValue.ToString(), out int nq))
                    queueData.NewQueueLength = nq;
                if (ushort.TryParse(arguments[7].DataValue.ToString(), out ushort nu))
                    queueData.NewUpdateID = nu;
                queueData.AssignedObjectID = arguments[6].DataValue.ToString();
            }
            return queueData;
        }
        public async Task<Boolean> RunAlarm(UInt16 AlarmID, string LoggedStartTime, string Duration, string ProgramURI, string ProgramMetaData, SonosEnums.PlayModes PlayMode, UInt16 Volume, Boolean IncludeLinkedZones, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[9];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("AlarmID", AlarmID);
            arguments[2] = new UPnPArgument("LoggedStartTime", LoggedStartTime);
            arguments[3] = new UPnPArgument("Duration", Duration);
            arguments[4] = new UPnPArgument("ProgramURI", ProgramURI);
            arguments[5] = new UPnPArgument("ProgramMetaData", ProgramMetaData);
            arguments[6] = new UPnPArgument("PlayMode", PlayMode.ToString());
            arguments[7] = new UPnPArgument("Volume", Volume);
            arguments[8] = new UPnPArgument("IncludeLinkedZones", IncludeLinkedZones);
            return await Invoke("RunAlarm", arguments);
        }
        /// <summary>
        /// Speichert die aktuelle Wiedergabeliste als SonosPlaylist
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <param name="_title">Titel der Wiedergabeliste</param>
        /// <param name="_id">Optional, falls eine vorhandene Aktualisiert werden soll.</param>
        /// <returns></returns>
        public async Task<Boolean> SaveQueue(string _title, string _id = null)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Title", _title);
            arguments[2] = new UPnPArgument("ObjectID", _id);
            arguments[3] = new UPnPArgument("AssignedObjectID", _id);
            if (await Invoke("SaveQueue", arguments))
            {
                pl.PlayerProperties.QueueChanged += 1;
                ManuellStateChange(SonosEnums.EventingEnums.QueueChangedSaved, DateTime.Now);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Springt zur angegeben Position innerhalb eines Songs / Wiedergabeliste
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="position">String in der Angabe hh:mm:ss bei REL_TIME und Tracknumber bei TRACK_NR dann beginnend bei 1</param>
        /// <param name="unit">Soll in der Zeit REL_TIME oder in der Wiedergabeliste gesprungen werden TRACK_NR</param>
        /// <returns></returns>
        public async Task<Boolean> Seek(string position, SonosEnums.SeekUnit unit = SonosEnums.SeekUnit.REL_TIME)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Unit", unit.ToString());
            arguments[2] = new UPnPArgument("Target", position);
            if (await Invoke("Seek", arguments))
            {
                if (unit == SonosEnums.SeekUnit.TRACK_NR)
                {
                    if (int.TryParse(position, out int pos))
                    {
                        if (pos > 0 && pl.PlayerProperties.CurrentTrackNumber != pos)
                        {
                            pl.PlayerProperties.CurrentTrackNumber = pos;
                            ManuellStateChange(SonosEnums.EventingEnums.CurrentTrackNumber, DateTime.Now);
                        }
                    }
                }
                if (unit == SonosEnums.SeekUnit.REL_TIME && pl.PlayerProperties.TransportState != SonosEnums.TransportState.PLAYING)
                {
                    if (TimeSpan.TryParse(position, out TimeSpan see))
                    {
                        pl.PlayerProperties.CurrentTrack.RelTime = new SonosTimeSpan(see);
                        ManuellStateChange(SonosEnums.EventingEnums.RelTime, DateTime.Now);
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Setzt einen Song als aktuellen Song unabhängig von der Playlist, wird benötigt, wenn kein Song vorhanden ist (Stromlos)
        /// oder man mit Internetradio arbeiten möchte. Beim Replace von der Playlist wird das benötigt.
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="_uri">Wenn man noch keinen Song hatte z.b. nach einen neu Start muß man die akktuelle Queue in form von x-rincon-queue:UUID#0 übergeben</param>
        /// <param name="CurrentURIMetaData">Optional die Metadaten</param>
        public async Task<Boolean> SetAVTransportURI(string _uri, string CurrentURIMetaData = null)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("CurrentURI", _uri);
            arguments[2] = new UPnPArgument("CurrentURIMetaData", CurrentURIMetaData);
            if (await Invoke("SetAVTransportURI", arguments, 150))
            {
                if (pl.PlayerProperties.AVTransportURI != _uri)
                {
                    pl.PlayerProperties.AVTransportURI = _uri;
                    ManuellStateChange(SonosEnums.EventingEnums.AVTransportURI, DateTime.Now);
                }
                if (!string.IsNullOrEmpty(CurrentURIMetaData) && pl.PlayerProperties.AVTransportURIMetaData != CurrentURIMetaData)
                {
                    pl.PlayerProperties.AVTransportURIMetaData = CurrentURIMetaData;
                    ManuellStateChange(SonosEnums.EventingEnums.AVTransportURIMetaData, DateTime.Now);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Setzen des Überblenden
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean> SetCrossfadeMode(Boolean v)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("CrossfadeMode", v);
            var ret = await Invoke("SetCrossfadeMode", arguments);
            if (ret && pl.PlayerProperties.CurrentCrossFadeMode != v)
            {
                pl.PlayerProperties.CurrentCrossFadeMode = v;
                ManuellStateChange(SonosEnums.EventingEnums.CurrentCrossFadeMode, DateTime.Now);
            }
            return ret;
        }
        public async Task<Boolean> SetNextAVTransportURI(string _uri, string CurrentURIMetaData = null)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("NextURI", _uri);
            arguments[2] = new UPnPArgument("NextURIMetaData", CurrentURIMetaData);
            return await Invoke("SetNextAVTransportURI", arguments);
        }
        /// <summary>
        /// Wiedergabemodus definieren
        /// </summary>
        /// <param name="pl">Sonosplayer</param>
        /// <param name="playmode">NORMAL,REPEAT_ALL,SHUFFLE_NOREPEAT,SHUFFLE,REPEAT_ONE,SHUFFLE_REPEAT_ONE</param>
        public async Task<Boolean> SetPlayMode(SonosEnums.PlayModes playmode)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("NewPlayMode", playmode.ToString());
            var ret = await Invoke("SetPlayMode", arguments);
            if (ret && pl.PlayerProperties.CurrentPlayMode != playmode)
            {
                pl.PlayerProperties.CurrentPlayMode = playmode;
                ManuellStateChange(SonosEnums.EventingEnums.CurrentPlayMode, DateTime.Now);
            }
            return ret;
        }
        public async Task<Boolean> SnoozeAlarm(string Duration)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Duration", Duration);
            return await Invoke("SnoozeAlarm", arguments);
        }
        public async Task<Boolean> StartAutoplay(string ProgramURI, string ProgramMetaData, UInt16 Volume, Boolean IncludeLinkedZones, Boolean ResetVolumeAfter, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[6];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("ProgramURI", ProgramURI);
            arguments[2] = new UPnPArgument("ProgramMetaData", ProgramMetaData);
            arguments[3] = new UPnPArgument("Volume", Volume);
            arguments[4] = new UPnPArgument("IncludeLinkedZones", IncludeLinkedZones);
            arguments[5] = new UPnPArgument("ResetVolumeAfter", ResetVolumeAfter);
            return await Invoke("StartAutoplay", arguments);
        }
        /// <summary>
        /// Stoppt die Wiedergabe
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<Boolean> Stop()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            var ret = await Invoke("Stop", arguments);
            if (ret && pl.PlayerProperties.TransportState != SonosEnums.TransportState.STOPPED)
            {
                pl.PlayerProperties.TransportState = SonosEnums.TransportState.STOPPED;
                ManuellStateChange(SonosEnums.EventingEnums.TransportState, DateTime.Now);
            }
            return ret;
        }
        #endregion Public Methods
        #region Private Methods
        /// <summary>
        /// Übergabe an UPNP Service
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="arguments"></param>
        /// <param name="Sleep"></param>
        /// <returns></returns>
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (AVTransportService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " " + ClassName + " ist null"));
                    return false;
                }
                AVTransportService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method, ClassName, ex);
                return false;
            }
        }
        /// <summary>
        /// Dient dazu manuelle Änderungen als Event zu feuern und den LastChange entsprechend zu setzen.
        /// </summary>
        /// <param name="_lastchange"></param>
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime _lastchange)
        {
            try
            {
                if (AVTransport_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                AVTransport_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("AvTRansport_ManuellStateChange", ClassName, ex);
            }
        }
        #endregion Private Methods
    }
}
