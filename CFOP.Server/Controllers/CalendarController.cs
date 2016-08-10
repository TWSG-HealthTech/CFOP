using CFOP.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace CFOP.Server.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        private readonly IConnectionManager _connectionManager;

        public CalendarController(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }
        
        [HttpGet]
        public string Get(string input)
        {
            input = input ?? "some sample string";
            _connectionManager.GetHubContext<GoogleCalendarHub>().Clients.All.CalendarChanged(input);
            return $"Sent '{input}' to CFOP client";
        }
    }
}
