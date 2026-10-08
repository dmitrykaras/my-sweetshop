# **🍬 My Sweetshop (Моя кондитерская)**
Кроссплатформенное мобильное приложение на **.NET MAUI** (Android / iOS) с собственным серверным API на **ASP.NET Core** и системой скидок по QR-кодам.
## **🚀 Основной функционал**
* **Авторизация и сессии:** Безопасная аутентификация пользователей с сохранением сессий (JWT Tokens & Refresh Tokens) и интеграцией с API. 
* **Главная страница:** QR-код системы лояльности и скидок, интерактивное местонахождение кондитерских магазинов, заказ доставки, а также карточки с быстрыми ссылками на социальные сети. 
* **Каталог товаров:** Интерактивная витрина сладостей с динамическим обновлением данных (Pull-to-Refresh). 
* **Поддержка:** Удобный экран с прямыми ссылками для оперативного получения техподдержки по любым вопросам. 
* **Профиль:** Отображение скидочных баллов, детальная информация о профиле с возможностью редактирования личных данных и смены Email, страница избранных товаров и ссылки на полезные ресурсы.
## **🌐 API & Swagger Documentation**
Для ознакомления с работой серверной части и тестирования REST API эндпоинтов развернут интерактивный Swagger UI:

👉 **[Открыть Swagger UI](https://karas-sweetshopots.duckdns.org/swagger/index.html)**

## **📱 Интерфейс приложения**
<details>
<summary>🔐 Авторизация и вход (нажмите, чтобы развернуть)</summary>

<br>

<table width="100%">
  <tr>
    <td width="25%" align="center"><b>Старт</b><br/><img src="/screenshots/AuthStartPage_image.png" width="100%"/></td>
    <td width="25%" align="center"><b>Ввод Email</b><br/><img src="/screenshots/AuthStartPage_iamge2.png" width="100%"/></td>
    <td width="25%" align="center"><b>Ввод кода</b><br/><img src="/screenshots/AuthStartPage_image3.png" width="100%"/></td>
    <td width="25%" align="center"><b>Профиль</b><br/><img src="/screenshots/AuthStartPage_image4.png" width="100%"/></td>
  </tr>
</table>

</details>

<details>
<summary>🍰 Главная и каталог товаров</summary>

<br>

<table width="100%">
  <tr>
    <td width="25%" align="center"><b>Главный экран</b><br/><img src="/screenshots/HomePage_image.png" width="100%"/></td>
    <td width="25%" align="center"><b>Каталог</b><br/><img src="/screenshots/CatalogPage_image3.png" width="100%"/></td>
    <td width="25%" align="center"><b>Детали товара</b><br/><img src="/screenshots/ProductPage_image.png" width="100%"/></td>
    <td width="33%" align="center"><b>Сплеш-скрин</b><br/><img src="/screenshots/SplashScreen_image.png" width="100%"/></td>
  </tr>
</table>

</details>

<details>
<summary>🏪 Поддержка и избранное</summary>

<br>

<table width="100%">
  <tr>
    <td width="25%" align="center"><b>Экран поддержки</b><br/><img src="/screenshots/ContactPage_image.png" width="100%"/></td>
    <td width="25%" align="center"><b>Избранное</b><br/><img src="/screenshots/FavoritePage_image.png" width="100%"/></td>
    <td width="25%" align="center"><b>Избранное</b><br/><img src="/screenshots/FavoritePage_image2.png" width="100%"/></td>
  </tr>
</table>

</details>

<details>
<summary>👤 Профиль и редактирование</summary>

<br>

<table width="100%">
  <tr>
    <td width="33%" align="center"><b>Профиль</b><br/><img src="/screenshots/ProfilePage_image.png" width="100%"/></td>
    <td width="33%" align="center"><b>Редактирование</b><br/><img src="/screenshots/EditProfilePage_image.png" width="100%"/></td>
    <td width="33%" align="center"><b>Смена Email</b><br/><img src="/screenshots/AuthStartPage_image3.png" width="100%"/></td>
  </tr>
</table>

</details>

## **🛠 Технологический стек**
### **Client (Mobile app)**
- **.NET MAUI** (C# 14/ XAML) — кроссплатформенный UI.
- **MVVM Pattern** — для разделения логики и представления.
- **HttpClient / System.Text.Json** — для работы с API и десериализации DTO.
### **Backend & Database**
- **.NET Web API (ASP.NET Core, C# 13)** — архитектура серверной части.
- **Entity Framework Core** — ORM для работы с данными.
- **PostgreSQL** — хранение основных данных и логов/метаданных.
- **Docker** — контейнеризация бэкенда для быстрой развертки.

## **🧪 Тестирование и CI**

[![CI](https://github.com/dmitrykaras/my-sweetshop/actions/workflows/ci.yml/badge.svg)](https://github.com/dmitrykaras/my-sweetshop/actions/workflows/ci.yml)

Серверная часть покрыта **50 интеграционными тестами** (`MySweetShop.IntegrationTests`, .NET 9, xUnit).
Тесты проверяют реальные HTTP-сценарии API, включая негативные кейсы (400 / 401 / 404):

| Область | Что проверяется |
|---|---|
| **Авторизация** (`/auth`) | запрос кода, валидация email и формата кода, cooldown, лимит попыток, просроченный и уже использованный код, вход существующего и новый пользователь |
| **Токены** | обновление пары JWT + Refresh Token, отказ при использованном, истёкшем и несуществующем токене |
| **Каталог** (`/products`) | список товаров, удаление, загрузка / замена / удаление изображений, избранное |
| **Профиль** (`/profile`) | данные пользователя, баллы, избранное, редактирование имени, смена email по коду |
| **Пользователи** (`/api/users`) | получение и создание / обновление по email |

Запуск локально:

    dotnet test

### Непрерывная интеграция
При каждом push и pull request в `main` GitHub Actions автоматически:

1. восстанавливает зависимости и собирает серверную часть (.NET 9, Release);
2. запускает интеграционные тесты и сохраняет отчёт (`.trx`) как артефакт;
3. проверяет, что Docker-образ API собирается без ошибок.

Статус последнего прогона виден по бейджу выше.

## **🔒 Безопасность авторизации**

Вход выполняется по одноразовому 4-значному коду, отправляемому на email.
Защита реализована на сервере и покрыта тестами:

- **Cooldown на запрос кода:** повторный запрос для того же email раньше чем через 120 секунд отклоняется.
- **Ограничение попыток:** после 5 неверных вводов код блокируется, нужно запросить новый.
- **Срок жизни кода:** 10 минут, после этого код отклоняется.
- **Одноразовость:** использованный код повторно принять нельзя.
- **Ротация refresh-токенов:** при обновлении выдаётся новая пара токенов, а использованный, истёкший или несуществующий refresh-токен возвращает 401.
- **Валидация входных данных:** формат email и кода проверяется до обращения к БД.

## **📂 Структура проекта (Основные модули)**
```bash
my-sweetshop/
├── .github/workflows/ci.yml            # CI: сборка, тесты, проверка Docker-образа
├── docker-compose.yml                  # Развёртывание БД и API
├── screenshots/                        # Скриншоты приложения
└── my-sweetshop/src/
    ├── frontend/                       # КЛИЕНТ (.NET MAUI)
    │   ├── MauiProgram.cs              # Точка входа, регистрация DI-сервисов
    │   ├── Dtos/                       # Контракты данных API
    │   ├── Models/                     # Локальные модели (Product, UserModel)
    │   ├── ViewModels/                 # Логика экранов (MVVM)
    │   ├── Views/                      # XAML-страницы (Auth, Catalog, Profile, ...)
    │   ├── Services/                   # ApiClient, JwtAuthHandler, AuthSession, ProfileService
    │   ├── Platforms/                  # Код под Android / iOS / Windows / ...
    │   └── Resources/                  # Иконки, шрифты, стили, splash
    │
    └── backend/
        ├── MySweetShop.Api.sln
        ├── MySweetShop.Api/            # СЕРВЕР (ASP.NET Core Web API)
        │   ├── Program.cs              # Конфигурация, middleware, DI
        │   ├── Dockerfile
        │   ├── Controllers/            # Auth, Products, Profile, Users
        │   ├── Contracts/              # Запросы и ответы API
        │   ├── Entities/               # User, Product, RefreshToken, коды подтверждения
        │   ├── Data/                   # AppDbContext (EF Core)
        │   ├── Migrations/             # Миграции БД
        │   ├── Services/               # JwtService, HashService, CodeGenerator
        │   ├── Options/                # Настройки JWT
        │   ├── IScript/                # Сидинг изображений товаров
        │   ├── SeedImages/             # Изображения для сидинга
        │   └── wwwroot/                # Статические файлы
        └── MySweetShop.IntegrationTests/  # Интеграционные тесты (xUnit)
```
## **🛠 Запуск проекта**
### 1. Требования
- .NET 9 SDK (для серверной части и тестов)
- .NET 10 SDK и рабочая нагрузка .NET MAUI (для мобильного клиента)
- IDE: Visual Studio 2022 / JetBrains Rider / VS Code с установленными рабочими нагрузками .NET MAUI.
- Docker (для поднятия локальной БД и API).
### 2. Клонирование репозитория
```
git clone https://github.com/dmitrykaras/my-sweetshop
cd my-sweetshop
```
### 3. Запуск бэкенда (Docker)
Если вы используете готовое Docker-окружение для базы данных и API:
```
docker-compose up -d
```
### 4. Применение миграций БД
```
cd my-sweetshop/src/backend/MySweetShop.Api
dotnet ef database update
```
### 5. Запуск мобильного приложения
Откройте решение в IDE, выберите целевую платформу (например, Android Emulator или Windows Machine) и нажмите **Start / Run**.
## **📝 Планы по развитию (Roadmap)**
- [ ] Расширенная админ-панель для управления ассортиментом через web-интерфейс.
- [ ] Отправка писем через валидную почту всем пользователям (Mail.ru / Yandex.ru).
- [ ] Интеграция платежной системы.
- [ ] Deploy в официальные магазины приложений (Google Play и App Store)
## **📄 Лицензия и авторское право**
Исходный код предоставлен исключительно в ознакомительных целях и для демонстрации портфолио. Использование, копирование, модификация или распространение данного кода без предварительного согласия автора запрещены.

© 2026 Карась Дмитрий. Все права защищены (All rights reserved).
