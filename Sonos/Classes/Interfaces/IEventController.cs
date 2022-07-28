using Microsoft.AspNetCore.Mvc;
using Sonos.Classes.Events;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sonos.Classes.Interfaces
{
    public interface IEventController
    {
        Task Broadcast([FromBody] Notification notification);
        Task EventBroadCast(Notification notification);
        IList<RinconLastChangeItem> GetListById(int id);
        Task SubscribeEvents(CancellationToken cancellationToken);
    }
}