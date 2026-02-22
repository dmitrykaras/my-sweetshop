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

            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("http://10.0.2.2:5107/")
                };
            });
            builder.Services.AddSingleton<AppShell>();

            //Services 
            // Api
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<ApiException>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AuthApi>();

            // Domain
            builder.Services.AddSingleton<IUserService, UserService>();

            // AuthStep
            builder.Services.AddSingleton<AuthSession>();
            builder.Services.AddSingleton<CodePageFactory>();
            builder.Services.AddSingleton<ChangeEmail>();
            builder.Services.AddSingleton<EmailCache>();

            // Pages
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<CatalogPage>();
            builder.Services.AddTransient<ProductPage>();
            builder.Services.AddTransient<ContactPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<NewEmailPage>();
            builder.Services.AddTransient<FavoritesProduct>();
            builder.Services.AddTransient<CashierPage>();

            // AuthStep Pages
            builder.Services.AddTransient<AuthStartPage>();
            builder.Services.AddTransient<EmailPage>();
            builder.Services.AddTransient<CodePage>();
            builder.Services.AddTransient<CompletionProfilePage>();

            builder.Services.AddTransient<SplashPage>();

            // ViewModels
            // NewEmailOrName
            builder.Services.AddTransient<EditProfileViewModel>();
            builder.Services.AddTransient<NewEmailViewModel>();

            // BaseViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<CatalogViewModel>();
            builder.Services.AddTransient<CategoryViewModel>();
            builder.Services.AddTransient<ProductViewModel>();

            var app = builder.Build();
            ServiceProvider = app.Services;

            return app;
        }
    }
}