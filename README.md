# **🍬 My Sweetshop (Моя кондитерская) *(demo)***
Коммерческий проект созданный на платформе **.NET MAUI**, **C# 13** и **C# 14 (preview)**. Данный проект представляет собой кроссплатформенное мобильное приложение (iOS, Android), интегрированное с полноценной серверной экосистемой включающую в себя систему скидок по QR-кодам.
## **🚀 Основной функционал**
* **Авторизация и сессии:** Безопасная аутентификация пользователей с сохранением сессий (JWT Tokens & Refresh Tokens) и интеграцией с API. 
* **Главная страница:** QR-код системы лояльности и скидок, интерактивное местонахождение кондитерских магазинов, заказ доставки, а также карточки с быстрыми ссылками на социальные сети. 
* **Каталог товаров:** Интерактивная витрина сладостей с динамическим обновлением данных (Pull-to-Refresh). 
* **Поддержка:** Удобный экран с прямыми ссылками для оперативного получения техподдержки по любым вопросам. 
* **Профиль:** Отображение скидочных баллов, детальная информация о профиле с возможностью редактирования личных данных и смены Email, страница избранных товаров и ссылки на полезные ресурсы.
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
    <td width="25%" align="center"><b>Детали товара</b><br/><img src="/screenshots/CatalogPage_image.png" width="100%"/></td>
    <td width="25%" align="center"><b>Избранное</b><br/><img src="/screenshots/SplashScreen_image.png" width="100%"/></td>
  </tr>
</table>

</details>

<details>
<summary>🏪 Поддержка</summary>

<br>

<table width="100%">
  <tr>
    <td width="33%" align="center"><b>Экран поддержка</b><br/><img src="/screenshots/ContactPage_image.png" width="100%"/></td>
    <td width="33%" align="center"><b>Экран поддержки</b><br/><img src="/screenshots/ProductPage_image.png" width="100%"/></td>
    <td width="33%" align="center"><b>Сплеш-скрин</b><br/><img src="/screenshots/SplashScreen_image.png" width="100%"/></td>
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
- **.NET Web API (C# 13)** — архитектура серверной части.
- **Entity Framework Core** — ORM для работы с данными.
- **PostgreSQL** — хранение основных данных и логов/метаданных.
- **Docker** — контейнеризация бэкенда для быстрой развертки.
## **📂 Структура проекта (Основные модули)**
```bash
my-sweetshop/
├── docker-compose.yml              # Скрипт развертывания инфраструктуры
│
├── src/
│   ├── my_sweetshop/               # КЛИЕНТСКОЕ ПРИЛОЖЕНИЕ (.NET MAUI)
│   │   ├── my-sweetshop.csproj     # Файл конфигурации проекта MAUI
│   │   ├── MauiProgram.cs          # Точка входа, регистрация DI-сервисов
│   │   ├── Dtos/                   # Контракты данных API (Token, VerifyCode, UpdateProfile)
│   │   ├── Models/                 # Локальные доменные модели (Product, UserModel)
│   │   ├── ViewModels/             # Логика экранов (Catalog, Profile, Home)
│   │   ├── Views/                  # XAML UI-страницы (Каталог, Авторизация, Профиль)
│   │   ├── Services/               # Клиентские сервисы (ApiClient, AuthSession, UserService)
│   │   ├── Platforms/              # Специфичный код платформ (Android, iOS, Windows, Tizen)
│   │   └── Resources/              # Ресурсы приложения (Иконки, Шрифты, Стили, Splash)
│   │
│   └── MySweetshop.Api/            # СЕРВЕРНАЯ ЧАСТЬ (ASP.NET Core Web API)
│       ├── my-sweetshop.api.csproj # Файл конфигурации проекта бэкенда
│       ├── Program.cs              # Конфигурация приложения, Middleware и DI
│       ├── Dockerfile              # Инструкции контейнеризации API
│       ├── Contracts/              # Запросы и ответы API (Requests/Responses)
│       ├── Controllers/            # REST-контроллеры (Auth, Products, Profile, Users)
│       ├── Data/                   # Контекст базы данных Entity Framework (AppDbContext)
│       ├── Entities/               # Сущности БД (User, Product, Category, RefreshToken)
│       ├── Migrations/             # История миграций базы данных (EF Core)
│       ├── Services/               # Серверная логика (JwtService, HashService, CodeGenerator)
│       ├── IScript/                # Логика первичного наполнения базы данных (Seed)
│       ├── SeedImages/             # Статические изображения сладостей для сидинга БД
│       └── wwwroot/                # Корневая папка статических файлов (Загруженные изображения товаров)
```
## **🛠 Запуск проекта**
### 1. Требования
- .NET 8 SDK или выше
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
cd src/MySweetshop.Api
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
