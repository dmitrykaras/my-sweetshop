using System.Collections.Generic; // Добавлено для Dictionary
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Добавлено для IConfigurationBuilder
using Microsoft.Extensions.DependencyInjection;
using MySweetShop.Api.Data;
using Testcontainers.PostgreSql;
using Xunit;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("sweetshop_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Гарантируем, что в пустой базе Testcontainers создадутся таблицы
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    new public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Настройка окружения: изолируем тесты от appsettings.json
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SUPER_SECRET_DUMMY_KEY_FOR_INTEGRATION_TESTS_ONLY!",
                ["Jwt:Issuer"] = "MySweetShop.Api",
                ["Jwt:Audience"] = "MySweetShop.Mobile",
                ["Jwt:ExpiresMinutes"] = "30"
            });
        });

        // Настройка сервисов
        builder.ConfigureServices(services =>
        {
            // Поменяет базовую схему аутентификации на тестовую
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                  TestAuthHandler.SchemeName, options => { });

            // Находим существующую регистрацию AppDbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }
}