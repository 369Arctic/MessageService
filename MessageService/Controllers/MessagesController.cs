using MessageService.Data;
using MessageService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace MessageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private static readonly List<WebSocket> WebSocketClients = new List<WebSocket>();
        private readonly IMessageRepository _repository;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (!IsMessageValid(message))
                return BadRequest("Сообщение не может превышать 128 символов.");

            try
            {
                _repository.AddMessage(message);
                _logger.LogInformation($"Сообщение сохранено: ID={message.Id}, Content={message.Content}");

                await NotifyWebSocketClients(message);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения.");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/ws")]
        public async Task GetWebSocket()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogWarning("Запрос на WebSocket соединение не был WebSocket запросом.");
                HttpContext.Response.StatusCode = 400;
                return;
            }

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            WebSocketClients.Add(webSocket);
            _logger.LogInformation("WebSocket соединение установлено.");

            await ReceiveAndHandleWebSocketMessages(webSocket);
        }

        [HttpGet("get")]
        public IActionResult GetMessages([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogInformation($"Запрос на получение сообщений за диапазон дат {startDate} - {endDate}.");
            var messages = _repository.GetMessagesByDateRange(startDate, endDate);

            if (!messages.Any())
                _logger.LogInformation("Сообщений за указанный период не найдено");

            return Ok(messages);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("recent")]
        public IActionResult GetRecentMessages()
        {
            _logger.LogInformation("Запрос на получение сообщений за последнюю минуту.");

            var recentMessages = _repository.GetMessagesByDateRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);

            if (!recentMessages.Any())
                _logger.LogInformation("Сообщений за последнюю минуту не найдено");

            return Ok(recentMessages);
        }

        private bool IsMessageValid(Message message)
        {
            if (message.Content.Length <= 128)
                return true;

            _logger.LogWarning($"Сообщение превышает допустимую длину: {message.Content.Length} символов.");
            return false;
        }

        private async Task NotifyWebSocketClients(Message message)
        {
            var messageData = Encoding.UTF8.GetBytes($"{message.Id}: {message.Content} at {DateTime.Now}");
            var disconnectedClients = new List<WebSocket>();

            foreach (var webSocket in WebSocketClients)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(messageData), WebSocketMessageType.Text, true, CancellationToken.None);
                    _logger.LogInformation("Сообщение отправлено по WebSocket.");
                }
                else
                {
                    disconnectedClients.Add(webSocket);
                    _logger.LogWarning("Клиент WebSocket был отключен.");
                }
            }

            // Удаляем отключенных клиентов
            foreach (var client in disconnectedClients)
            {
                WebSocketClients.Remove(client);
            }
        }

        private async Task ReceiveAndHandleWebSocketMessages(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            WebSocketClients.Remove(webSocket);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.LogWarning("WebSocket соединение закрыто.");
        }
    }
}
