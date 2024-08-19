using MessageSender.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageSender.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(ILogger<MessagesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Send()
        {
            _logger.LogInformation("Загрузка страницы отправки сообщения.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(Message message)
        {
            if (ModelState.IsValid)
            {
                message.Timestamp = DateTime.Now;
                _logger.LogInformation($"Отправка сообщения начата.");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:5001/");
                    try
                    {
                        var response = await client.PostAsJsonAsync("api/Messages/send", message);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Сообщение успешно отправлено на сервер.");
                            TempData["SuccessMessage"] = "Сообщение успешно отправлено.";
                        }
                        else
                        {
                            _logger.LogWarning($"Ошибка при отправке сообщения. Код состояния: {response.StatusCode}.");
                            TempData["ErrorMessage"] = "Ошибка при сохранении сообщения.";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Произошла ошибка при попытке отправить сообщение.");
                        TempData["ErrorMessage"] = "Произошла ошибка при отправке сообщения.";
                    }
                }
            }
            else
            {
                _logger.LogWarning("Модель сообщения недействительна. Возврат на страницу отправки.");
                TempData["ErrorMessage"] = "Модель сообщения недействительна.";
            }

            return RedirectToAction("Send");
        }

    }
}
