FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore

# Сборка приложения
RUN dotnet publish -c Release -o /app


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Открываем необходимые порты
EXPOSE 5221
EXPOSE 5003

ENV ASPNETCORE_URLS=http://+:5221;http://+:5003;
ENV ASPNETCORE_ENVIRONMENT=Development

# Запуск приложения
ENTRYPOINT ["dotnet", "MessageRealTimeViewer.dll"]

