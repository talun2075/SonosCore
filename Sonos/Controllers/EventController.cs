#nullable enable
using Sonos.Classes.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SonosUPnP.DataClasses;
using Sonos.Classes;
using System.Collections.Generic;
using System.Linq;
using SonosUPNPCore.Enums;
using SonosUPnP;

namespace Sonos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private static readonly IMessageRepository _messageRepository = new MessageRepository();
        private static int EventID = 0;
        private static readonly Dictionary<int, RinconLastChangeItem> ListEvents = new();
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new () { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Produce SSE
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Produces("text/event-stream")]
        [HttpGet]
        public async Task SubscribeEvents(CancellationToken cancellationToken)
        {
            SetServerSentEventHeaders();
            // On connect, welcome message ;)
            var data = new { Message = "connected!" };
            var jsonConnection = JsonSerializer.Serialize(data, _jsonSerializerOptions);
            await Response.WriteAsync($"event:connection\n", cancellationToken);
            await Response.WriteAsync($"data: {jsonConnection}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            async void OnNotification(object? sender, NotificationArgs eventArgs)
            {
                try
                {
                    // idea: https://stackoverflow.com/a/58565850/80527
                    var json = await PrepareData(eventArgs);
                    //await Response.WriteAsync($"id:{eventArgs.Notification.Aurora.SerialNo}\n", cancellationToken);
                    //await Response.WriteAsync("retry: 10000\n", cancellationToken);
                    await Response.WriteAsync($"event:sonos\n", cancellationToken);
                    await Response.WriteAsync($"data:{json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    var k = ex.Message;
                }
            }
            _messageRepository.NotificationEvent += OnNotification;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Spin until something break or stop...
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                _messageRepository.NotificationEvent -= OnNotification;
            }
        }
        private async static Task<string> PrepareData(NotificationArgs eventArgs)
        {
            try
            {
                if (eventArgs.Notification.Player != null)
                    return await PrepareDataForPlayer(eventArgs);

                return PrepareDataForDiscovery(eventArgs);
            }
            catch
            {
                //ignore
                return String.Empty;
            }
        }
        public static Task EventBroadCast(Notification notification)
        {
            _messageRepository.Broadcast(notification);

            return Task.CompletedTask;
        }

        private async static Task<string> PrepareDataForPlayer(NotificationArgs eventArgs)
        {
            try
            {
                var pl = eventArgs.Notification.Player;
                var eventchange = eventArgs.Notification.EventType;

                var t = new RinconLastChangeItem
                {
                    UUID = pl.UUID,
                    LastChange = pl.LastChange,
                    TypeEnum = eventchange
                };
                try
                {
                    switch (eventchange)
                    {
                        case SonosEnums.EventingEnums.GroupVolumeChangeable:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.GroupRenderingControl_GroupVolumeChangeable.ToString());
                            break;
                        case SonosEnums.EventingEnums.GroupVolume:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.GroupRenderingControl_GroupVolume.ToString());
                            break;
                        case SonosEnums.EventingEnums.GroupMute:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.GroupRenderingControl_GroupMute.ToString());
                            break;
                        case SonosEnums.EventingEnums.Volume:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.Volume.ToString());
                            break;
                        case SonosEnums.EventingEnums.Mute:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.Mute.ToString());
                            break;
                        case SonosEnums.EventingEnums.QueueChanged:
                        case SonosEnums.EventingEnums.QueueChangedNoRefillNeeded:
                            t.TypeEnum = SonosEnums.EventingEnums.QueueChanged;
                            t.ChangedValues.Add(SonosEnums.EventingEnums.QueueChanged.ToString(), pl.PlayerProperties.QueueChanged.ToString());
                            t.ChangedValues.Add("Override", (eventchange == SonosEnums.EventingEnums.QueueChanged).ToString());
                            break;
                        case SonosEnums.EventingEnums.QueueChangedEmpty:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.Playlist.NumberReturned.ToString());
                            break;
                        case SonosEnums.EventingEnums.QueueChangedSaved:
                            t.ChangedValues.Add(eventchange.ToString(), "LoadPlaylists");
                            break;
                        case SonosEnums.EventingEnums.SleepTimerRunning:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.SleepTimerRunning.ToString());
                            break;
                        case SonosEnums.EventingEnums.CurrentCrossFadeMode:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.CurrentCrossFadeMode.ToString());
                            break;
                        case SonosEnums.EventingEnums.TransportState:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.TransportState.ToString());
                            break;
                        case SonosEnums.EventingEnums.CurrentPlayMode:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.CurrentPlayMode.ToString());
                            break;
                        case SonosEnums.EventingEnums.NumberOfTracks:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.NumberOfTracks.ToString());
                            break;
                        case SonosEnums.EventingEnums.CurrentTrackNumber:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.CurrentTrackNumber.ToString());
                            break;
                        case SonosEnums.EventingEnums.NextTrack:
                            await SonosItemHelper.UpdateItemToHashPath(pl.PlayerProperties.NextTrack);
                            t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(pl.PlayerProperties.NextTrack, _jsonSerializerOptions));
                            break;
                        case SonosEnums.EventingEnums.CurrentTrack:
                            await SonosItemHelper.UpdateItemToHashPath(pl.PlayerProperties.CurrentTrack);
                            t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(pl.PlayerProperties.CurrentTrack, _jsonSerializerOptions));
                            break;
                        case SonosEnums.EventingEnums.LineInConnected:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.AudioInput_LineInConnected.ToString());
                            break;
                        case SonosEnums.EventingEnums.AudioInputName:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.AudioInput_Name.ToString());
                            break;
                        case SonosEnums.EventingEnums.ZoneName:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.DeviceProperties_ZoneName.ToString());
                            break;
                        case SonosEnums.EventingEnums.LocalGroupUUID:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.LocalGroupUUID.ToString());
                            break;
                        case SonosEnums.EventingEnums.GroupCoordinatorIsLocal:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.GroupCoordinatorIsLocal.ToString());
                            break;
                        case SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup:
                            t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup, _jsonSerializerOptions));
                            t.ChangedValues.Add("Counter", pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Count.ToString());
                            break;
                        case SonosEnums.EventingEnums.ZoneGroupName:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.ZoneGroupTopology_ZoneGroupName.ToString());
                            break;
                        case SonosEnums.EventingEnums.ZoneGroupID:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.ZoneGroupTopology_ZoneGroupID.ToString());
                            break;
                        case SonosEnums.EventingEnums.IsIdle:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.DeviceProperties_IsIdle.ToString());
                            break;
                        case SonosEnums.EventingEnums.LastChangedPlayState:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.DeviceProperties_LastChangedPlayState);
                            break;
                        case SonosEnums.EventingEnums.AVTransportURI:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.AVTransportURI);
                            break;
                        case SonosEnums.EventingEnums.EnqueuedTransportURI:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.EnqueuedTransportURI);
                            break;
                        case SonosEnums.EventingEnums.EnqueuedTransportURIMetaData:
                            t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(pl.PlayerProperties.EnqueuedTransportURIMetaData, _jsonSerializerOptions) );
                            break;
                        case SonosEnums.EventingEnums.RelTime:
                            t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(pl.PlayerProperties.CurrentTrack.RelTime, _jsonSerializerOptions));
                            //t.ChangedValues.Add(SonosEnums.EventingEnums.CurrentTrackUri.ToString(), pl.PlayerProperties.CurrentTrack.Uri);
                            t.ChangedValues.Add(SonosEnums.EventingEnums.CurrentTrackNumber.ToString(), pl.PlayerProperties.CurrentTrackNumber.ToString());
                            break;
                        case SonosEnums.EventingEnums.ThirdPartyMediaServersX:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.ZoneGroupTopology_ThirdPartyMediaServersX.ToString());
                            break;
                        case SonosEnums.EventingEnums.RemainingSleepTimerDuration:
                            t.ChangedValues.Add(eventchange.ToString(), pl.PlayerProperties.RemainingSleepTimerDuration);
                            break;
                        case SonosEnums.EventingEnums.RatingFilter:
                            t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(pl.RatingFilter, _jsonSerializerOptions));
                            break;
                        case SonosEnums.EventingEnums.QueueChangeResort:
                            t.ChangedValues.Add(eventchange.ToString(), "");
                            t.ChangedValues.Add(SonosEnums.EventingEnums.EnqueuedTransportURI.ToString(), pl.PlayerProperties.EnqueuedTransportURI);
                            break;
                        default:
                            t.ChangedValues.Add(eventchange.ToString(), "Unbekannter Wert");
                            break;
                    }
                    EventID++;
                    t.ChangedValues.Add("EventID", EventID.ToString());
                    ListEvents.Add(EventID, t);
                }
                catch (Exception ex)
                {
                    SonosHelper.Logger.ServerErrorsAdd("EventPlayerChange:Switch:eventchange:"+ eventchange.ToString()+":", ex, "EventController");
                    throw;
                }
                return JsonSerializer.Serialize(t, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                
                SonosHelper.Logger.ServerErrorsAdd("EventPlayerChange Eventenum:" + eventArgs.Notification.EventType.ToString(), ex, "EventController");
                return "Fehler beim eventing";
            }

            
        }
        private static string PrepareDataForDiscovery(NotificationArgs eventArgs)
        {

            var sd = eventArgs.Notification.Discovery;
            var eventchange = eventArgs.Notification.EventType;
            RinconLastChangeItem t = new ()
            {
                UUID = "Discovery",
                LastChange = DateTime.Now,
                TypeEnum = eventchange
            };
            switch (eventchange)
            {
                case SonosEnums.EventingEnums.AlarmListVersion:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.AlarmListVersion[SoftwareGeneration.ZG1].ToString());
                    break;
                case SonosEnums.EventingEnums.DailyIndexRefreshTime:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.DailyIndexRefreshTime[SoftwareGeneration.ZG1]);
                    break;
                case SonosEnums.EventingEnums.DateFormat:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.DateFormat[SoftwareGeneration.ZG1]);
                    break;
                case SonosEnums.EventingEnums.TimeFormat:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.TimeFormat[SoftwareGeneration.ZG1]);
                    break;
                case SonosEnums.EventingEnums.TimeGeneration:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.TimeGeneration[SoftwareGeneration.ZG1].ToString());
                    break;
                case SonosEnums.EventingEnums.TimeServer:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.TimeServer[SoftwareGeneration.ZG1]);
                    break;
                case SonosEnums.EventingEnums.TimeZone:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.TimeZone[SoftwareGeneration.ZG1]);
                    break;
                case SonosEnums.EventingEnums.ShareListUpdateID:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.ShareListUpdateID.ToString());
                    break;
                case SonosEnums.EventingEnums.SavedQueuesUpdateID:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.SavedQueuesUpdateID.ToString());
                    break;
                case SonosEnums.EventingEnums.FavoritesUpdateID:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.FavoritesUpdateID.ToString());
                    break;
                case SonosEnums.EventingEnums.ShareIndexInProgress:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.ShareIndexInProgress.ToString());
                    break;
                case SonosEnums.EventingEnums.ShareIndexLastError:
                    t.ChangedValues.Add(eventchange.ToString(), sd.ZoneProperties.ShareIndexLastError[SoftwareGeneration.ZG1]);
                    break;
                case SonosEnums.EventingEnums.ReloadNeeded:
                    t.ChangedValues.Add(eventchange.ToString(), "true");
                    break;
                default:
                    t.ChangedValues.Add(eventchange.ToString(), "Unbekannter Wert aus Discovery Übertragen");
                    break;

            }
            return JsonSerializer.Serialize(t, _jsonSerializerOptions);
        }

        private void SetServerSentEventHeaders()
        {
            Response.StatusCode = 200;
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
        }
        [HttpPost("broadcast")]
        public Task Broadcast([FromBody] Notification notification)
        {
            _messageRepository.Broadcast(notification);

            return Task.CompletedTask;
        }

        [HttpGet("GetListById/{id}")]
        public IList<RinconLastChangeItem> GetListById(int id)
        {
            lock (ListEvents)
            {
                return ListEvents.Where(x => x.Key > id).Select(x => x.Value).ToList();
            }
        }

    }
}
