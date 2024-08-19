using MessageService.Models;
using Npgsql;

namespace MessageService.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;

        public MessageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        //public void AddMessage(Message message)
        //{
        //    using var connection = new NpgsqlConnection(_connectionString);
        //    connection.Open();

        //    using var cmd = new NpgsqlCommand("INSERT INTO messages (Content, Timestamp) VALUES (@content, @timestamp)", connection);
        //    cmd.Parameters.AddWithValue("Content", message.Content);
        //    cmd.Parameters.AddWithValue("Timestamp", message.Timestamp);
        //    message.Id = (int)cmd.ExecuteScalar();
        //    return message;
        //}

        public Message AddMessage(Message message)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                // Вставляем сообщение и возвращаем новый ID
                using var cmd = new NpgsqlCommand("INSERT INTO messages (Content, Timestamp) VALUES (@content, @timestamp) RETURNING Id", connection);
                cmd.Parameters.AddWithValue("Content", message.Content);
                cmd.Parameters.AddWithValue("Timestamp", message.Timestamp);
                message.Id = (int)cmd.ExecuteScalar();
                return message;
            }
        }

        public IEnumerable<Message> GetMessagesByDateRange(DateTime startDate, DateTime endDate)
        {
            var messages = new List<Message>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT Id, Content, Timestamp FROM messages WHERE timestamp BETWEEN @startDate AND @endDate", connection);
            cmd.Parameters.AddWithValue("startDate", startDate);
            cmd.Parameters.AddWithValue("endDate", endDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(new Message
                {
                    Id = reader.GetInt32(0),
                    Content = reader.GetString(1),
                    Timestamp = reader.GetDateTime(2)
                });
            }

            return messages;
        }
    }
}