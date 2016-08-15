using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CFOP.Server.Controllers.DTO;
using CFOP.Server.Hubs;
using CFOP.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace CFOP.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<GoogleCalendarHub> _calendarHubContext;

        public HomeController(IHubContext<GoogleCalendarHub> calendarHubContext)
        {
            _calendarHubContext = calendarHubContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = ConnectedConnections.GetAll()
                .Select(c => new ConnectionDTO {Id = c});

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
