using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using MySweetShop.Api.IScript;
using MySweetShop.Api.Options;
using MySweetShop.Api.Services;
using System.Text;
using DotNetEnv;

var currentDir = Directory.GetCurrentDirectory();
var envPath = File.Exists(Path.Combine(currentDir, ".env"))
    ? Path.Combine(currentDir, ".env")
    : File.Exists(Path.Combine(currentDir, "..", ".env")) // если запустили из подпапки
        ? Path.Combine(currentDir, "..", ".env")
        : null;

if (envPath != null)
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Загружаем переменные среды в конфигурацию .NET
builder.Configuration.AddEnvironmentVariables();

// Контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MySweetShop.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставь токен так: Bearer {твой_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Конфигурация JWT
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtService>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var key = Encoding.UTF8.GetBytes(jwtOptions.Key);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Конфигурация базы данных
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? builder.Configuration.GetConnectionString("Default")
                    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Автоматически создаем таблицы в Postgres при каждом запуске сервера
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    // Автосид картинок в режиме разработки
    if (app.Environment.IsDevelopment())
    {
        await ISeedProductImages.SeedAsync(app);
    }
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Middleware аутентификации
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(); // разрешение раздачи статических файлов из папки wwwroot

// Маршруты
app.MapControllers();

if (args.Contains("--clear-images"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var products = await db.Products.ToListAsync();

    foreach (var p in products)
        p.ImageKey = null;

    await db.SaveChangesAsync();
    Console.WriteLine("IMAGE KEYS CLEARED");
    return;
}
app.Run();