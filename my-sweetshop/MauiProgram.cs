using CommunityToolkit.Maui;
using my_sweetshop.Dtos;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.AuthStep;
using my_sweetshop.Services.Domain;
using my_sweetshop.ViewModels;
using my_sweetshop.ViewModels.NewEmail;
using my_sweetshop.ViewModels.NewEmailOrName;
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

            builder.Services.AddSingleton<AppShell>();

            // 1. Auth session
            builder.Services.AddSingleton<AuthSession>();

            // 2. HttpClient для рефреша (БЕЗ хендлера, чтобы не было зацикливания)
            builder.Services.AddHttpClient("refresh_client", c =>
            {
                c.BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5107/" : "http://localhost:5107/");
            });

            // 3. Регистрируем JwtAuthHandler с использованием factory для refresh_client
            builder.Services.AddTransient<JwtAuthHandler>(sp =>
            {
                var session = sp.GetRequiredService<AuthSession>();
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var refreshClient = factory.CreateClient("refresh_client");
                return new JwtAuthHandler(session, refreshClient);
            });

            // 4. Основные API сервисы через AddHttpClient + JwtAuthHandler
            string apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5107/" : "http://localhost:5107/";

            builder.Services.AddHttpClient<AuthApi>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddHttpClient<ApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddHttpClient<ApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();

            // Остальные вспомогательные сервисы
            builder.Services.AddSingleton<ApiException>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<CodePageFactory>();
            builder.Services.AddHttpClient<ChangeEmail>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<JwtAuthHandler>();
            builder.Services.AddSingleton<EmailCache>();

            // Pages & ViewModels
            RegisterPagesAndViewModels(builder.Services);

            var app = builder.Build();
            ServiceProvider = app.Services;

            return app;
        }

        private static void RegisterPagesAndViewModels(IServiceCollection services)
        {
            // Pages
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

            // ViewModels
            services.AddTransient<EditProfileViewModel>();
            services.AddTransient<NewEmailViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<CatalogViewModel>();
            services.AddTransient<CategoryViewModel>();
            services.AddTransient<ProductViewModel>();
        }
    }
}