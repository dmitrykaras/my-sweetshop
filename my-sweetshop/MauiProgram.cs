using Microsoft.Extensions.Logging;
using my_sweetshop.Services;
using my_sweetshop.Views.Auth;

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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("http://10.0.2.2:5107/")
                };
            });
            builder.Services.AddSingleton<AppShell>();


            builder.Services.AddSingleton<AuthSession>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<AuthApi>();

            builder.Services.AddTransient<AuthStartPage>();
            builder.Services.AddTransient<EmailPage>();
            builder.Services.AddTransient<CodePage>();
            builder.Services.AddTransient<CompletionProfilePage>();
            builder.Services.AddSingleton<CodePageFactory>();

            var app = builder.Build();
            ServiceProvider = app.Services;

            return app;
        }
    }
}