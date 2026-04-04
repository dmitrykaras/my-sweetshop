FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Путь теперь включает папку my-sweetshop, так как контекст сборки выше на уровень
COPY ["my-sweetshop/src/backend/MySweetShop.Api/my-sweetshop.api.csproj", "my-sweetshop/src/backend/MySweetShop.Api/"]

RUN dotnet restore "my-sweetshop/src/backend/MySweetShop.Api/my-sweetshop.api.csproj"

# Копируем всё остальное (весь репозиторий)
COPY . .

# Переходим в папку проекта внутри контейнера
WORKDIR "/src/my-sweetshop/src/backend/MySweetShop.Api"

RUN dotnet publish "my-sweetshop.api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Создаем пользователя (для Linux контейнеров)
RUN adduser --disabled-password --gecos '' appuser || true
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "my-sweetshop.api.dll"]