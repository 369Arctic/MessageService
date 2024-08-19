using MessageService.Models;

namespace MessageService.Data
{
    public interface IMessageRepository
    {
        Message AddMessage(Message message);
        IEnumerable<Message> GetMessagesByDateRange(DateTime startDate, DateTime endDate);
    }
}
