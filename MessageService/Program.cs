using MessageService.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/MessageService-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Подключение репозитория сообщений
builder.Services.AddSingleton<IMessageRepository>(sp =>
    new MessageRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("https://localhost:5002", "https://localhost:5004")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimpleServer API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy"); // Используем CORS policy
app.UseAuthorization();

app.UseWebSockets(); // Добавление поддержки WebSocket

app.MapControllers();

app.Run();
