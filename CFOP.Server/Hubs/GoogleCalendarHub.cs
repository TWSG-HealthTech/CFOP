using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace CFOP.Server.Hubs
{
    [HubName("calendarHub")]
    public class GoogleCalendarHub : Hub
    {
    }
}
