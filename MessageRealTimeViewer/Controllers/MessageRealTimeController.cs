using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace MessageRealTimeViewer.Controllers
{
    public class MessageRealTimeController : Controller
    {
        private readonly ILogger<MessageRealTimeController> _logger;

        public MessageRealTimeController(ILogger<MessageRealTimeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
