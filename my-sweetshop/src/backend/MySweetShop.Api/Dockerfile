FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["my-sweetshop/src/backend/MySweetShop.Api/my-sweetshop.api.csproj", "my-sweetshop/src/backend/MySweetShop.Api/"]
RUN dotnet restore "my-sweetshop/src/backend/MySweetShop.Api/my-sweetshop.api.csproj"

# Копируем весь исходный код
COPY . .

# Переходим в папку проекта
WORKDIR "/src/my-sweetshop/src/backend/MySweetShop.Api"

# ВАЖНО: Собираем в конфигурации Debug для возможности отладки
RUN dotnet publish "my-sweetshop.api.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Пользователя создаем, но НЕ переключаемся на него (нужен root для vsdbg)
RUN adduser --disabled-password --gecos '' appuser || true
# USER appuser 

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "my-sweetshop.api.dll"]
RUN echo "FORCE REBUILD NUMBER 2"