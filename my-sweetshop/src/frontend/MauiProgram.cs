using CommunityToolkit.Maui;
using my_sweetshop.Dtos;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.Services.AuthStep;
using my_sweetshop.Services.UserService;
using my_sweetshop.ViewModels;
using my_sweetshop.ViewModels.Profile;
using my_sweetshop.ViewModels.Profile.NewEmailOrName;
using my_sweetshop.Views;
using my_sweetshop.Views.Auth;
using my_sweetshop.Views.Catalog;
using my_sweetshop.Views.Contact;
using my_sweetshop.Views.Main;
using my_sweetshop.Views.Profile;
using my_sweetshop.Views.Profile.Cashier;
using my_sweetshop.Views.Profile.ProfileChanges;

namespace my_sweetshop
{
    public static class MauiProgram
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddTransient<AppShell>();

            // Сессия авторизации
            builder.Services.AddSingleton<AuthSession>();

            string apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5107/" : "http://10.0.2.2:5107/";

            // HttpClient для рефреша (Строго БЕЗ JwtAuthHandler)
            builder.Services.AddHttpClient("refresh_client", c =>
            {
                c.BaseAddress = new Uri(apiBaseUrl);
            });

            // Регистрация JwtAuthHandler
            builder.Services.AddTransient<JwtAuthHandler>(sp =>
            {
                var session = sp.GetRequiredService<AuthSession>();
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var refreshClient = factory.CreateClient("refresh_client");
                return new JwtAuthHandler(session, refreshClient);
            });

            // Регистрация ProfileService как Singleton с поддержкой HttpClient
            // Сначала настраиваем именованный клиент для профиля
            builder.Services.AddHttpClient("ProfileClient", c =>
            {
                c.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<JwtAuthHandler>();

            // Теперь регистрируем сам сервис как AddScoped, внедряя в него настроенный клиент
            builder.Services.AddScoped<IProfileService>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = factory.CreateClient("ProfileClient");
                return new ProfileService(httpClient);
            });

            // Остальные API сервисы (AddHttpClient здесь работает нормально для Transient)
            builder.Services.AddHttpClient<AuthApi>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddHttpClient<GetProducts>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddHttpClient<ApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddHttpClient<ChangeEmail>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();

            // Вспомогательные сервисы
            builder.Services.AddSingleton<ApiException>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<CodePageFactory>();
            builder.Services.AddSingleton<EmailCache>();

            // Регистрация вью-моделей
            builder.Services.AddTransient<UserProfileViewModel>();
            builder.Services.AddTransient<UserPointsViewModel>();
            builder.Services.AddTransient<EditProfileRootViewModel>();
            builder.Services.AddTransient<NewEmailViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<CatalogViewModel>();
            builder.Services.AddTransient<CategoryViewModel>();
            builder.Services.AddTransient<ProductViewModel>();

            // Pages
            RegisterPages(builder.Services);

            var app = builder.Build();
            ServiceProvider = app.Services;

            return app;
        }

        private static void RegisterPages(IServiceCollection services)
        {
            services.AddTransient<HomePage>();
            services.AddTransient<CatalogPage>();
            services.AddTransient<ProductPage>();
            services.AddTransient<ContactPage>();
            services.AddTransient<ProfilePage>();
            services.AddTransient<EditProfilePage>();
            services.AddTransient<NewEmailPage>();
            services.AddTransient<FavoritesProduct>();
            services.AddTransient<CashierPage>();
            services.AddTransient<AuthStartPage>();
            services.AddTransient<EmailPage>();
            services.AddTransient<CodePage>();
            services.AddTransient<CompletionProfilePage>();
            services.AddTransient<SplashPage>();
        }
    }
}