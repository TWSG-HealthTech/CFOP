using CFOP.Server.Core.Calendar;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace CFOP.Server.Hubs
{
    [HubName("calendarHub")]
    public class GoogleCalendarHub : Hub
    {
        private readonly IClientRepository _clientRepository;

        public GoogleCalendarHub(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public void Connect()
        {
            _clientRepository.Add(new Client(Context.ConnectionId));
        }

        public void Disconnect()
        {
            _clientRepository.DeleteBy(Context.ConnectionId);
        }
    }
}
