using System.Collections.Generic;
using CFOP.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace CFOP.Server.Hubs
{
    [HubName("calendarHub")]
    public class GoogleCalendarHub : Hub
    {
        public void Connect()
        {
            ConnectedConnections.Add(Context.ConnectionId);
        }

        public void Disconnect()
        {
            ConnectedConnections.Remove(Context.ConnectionId);
        }
    }
}
