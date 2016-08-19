using CFOP.Server.Core.Calendar;
using CFOP.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CFOP.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<GoogleCalendarHub> _calendarHubContext;
        private readonly IClientRepository _clientRepository;

        public HomeController(IHubContext<GoogleCalendarHub> calendarHubContext,
                              IClientRepository clientRepository)
        {
            _calendarHubContext = calendarHubContext;
            _clientRepository = clientRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = _clientRepository.FindAll();

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(string text)
        {
            _calendarHubContext.Clients.All.calendarChanged(text);

            TempData["Message"] = $"Sent '{text}' to all clients";
            return RedirectToAction("Index");
        }
    }
}
