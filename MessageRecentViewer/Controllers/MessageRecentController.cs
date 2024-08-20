using MessageRecentViewer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MessageRecentViewer.Controllers
{
    public class MessageRecentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MessageRecentController> _logger;

        public MessageRecentController(HttpClient httpClient, ILogger<MessageRecentController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var messages = await GetRecentMessagesFromApi();
                return View(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сообщений за последнюю минуту.");
                TempData["ErrorMessage"] = "Ошибка при получении последних сообщений";
                return StatusCode(500, "Ошибка при получении сообщений.");
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRecentMessages()
        {
            try
            {
                var messages = await GetRecentMessagesFromApi();
                return Json(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении последних сообщений.");
                TempData["ErrorMessage"] = "Ошибка при получении последних сообщений";
                return Json(new { error = "Ошибка при получении последних сообщений." });
            }
        }

        private async Task<List<Message>> GetRecentMessagesFromApi()
        {
            _logger.LogInformation("Запрос на получение сообщений за последнюю минуту.");

            //var response = await _httpClient.GetAsync("https://localhost:5001/api/messages/recent");
            var response = await _httpClient.GetAsync("http://messageservice:5001/api/messages/recent");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var messages = JsonSerializer.Deserialize<List<Message>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!messages.Any())
            {
                _logger.LogInformation("Сообщений за последнюю минуту не найдено.");
                TempData["SuccessMessage"] = "Сообщений за последнюю минуту не найдено";
            }
            else
            {
                TempData["SuccessMessage"] = "Сообщения успешно получены.";
                _logger.LogInformation("Сообщения успешно получены.");

            }
            return messages;
        }
    }
}
