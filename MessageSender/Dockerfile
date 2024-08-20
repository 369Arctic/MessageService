# Используем официальный образ .NET 8 SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем все файлы в контейнер и восстанавливаем зависимости
COPY . .
RUN dotnet restore

# Сборка приложения
RUN dotnet publish -c Release -o /app

# Используем официальный образ ASP.NET Core для выполнения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Открываем необходимые порты
EXPOSE 5262
EXPOSE 5002

# Устанавливаем переменные среды и запускаем приложение
ENV ASPNETCORE_URLS=http://+:5002;http://+:5262
ENV ASPNETCORE_ENVIRONMENT=Development

# Запуск приложения
ENTRYPOINT ["dotnet", "MessageSender.dll"]
