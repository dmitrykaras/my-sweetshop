using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace MySweetShop.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Загружаем переменные из .env файла (он должен быть в корне или рядом)
        DotNetEnv.Env.Load();

        // Собираем строку из переменных среды
        // Эти имена должны совпадать с тем, что у тебя в .env
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5433";
        var db = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (string.IsNullOrEmpty(db) || string.IsNullOrEmpty(pass))
        {
            throw new Exception("Критическая ошибка: Переменные базы данных не найдены в .env или среде!");
        }

        var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}