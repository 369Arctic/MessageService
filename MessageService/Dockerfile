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
EXPOSE 5129
EXPOSE 5001

# Устанавливаем переменные среды и запускаем приложение
ENV ASPNETCORE_URLS=http://+:5001;http://+:5129
ENV ASPNETCORE_ENVIRONMENT=Development

# Запуск приложения
ENTRYPOINT ["dotnet", "MessageService.dll"]


#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#WORKDIR /app
#EXPOSE 80
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /src
#COPY ["MessageService/MessageService.csproj", "MessageService/"]
#RUN dotnet restore "MessageService/MessageService.csproj"
#COPY . .
#WORKDIR "/src/MessageService"
#RUN dotnet build "MessageService.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "MessageService.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "MessageService.dll"]
