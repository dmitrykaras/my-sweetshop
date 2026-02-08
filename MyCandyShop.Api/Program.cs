using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;
using MyCandyShop.Api.IScript;
using MyCandyShop.Api.Options;
using MyCandyShop.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------- Контроллеры и Swagger ---------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyCandyShop.Api", Version = "v1" });

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

// --------------------- Конфигурация JWT ---------------------
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
            ClockSkew = TimeSpan.FromSeconds(10)
        };
    });

builder.Services.AddAuthorization();

// --------------------- Конфигурация базы данных ---------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// --------------------- Сервисы работы с файлами ---------------------
builder.Services.Configure<BucketSettings>(builder.Configuration.GetSection("BucketSettings"));
builder.Services.AddSingleton<IObjectStorage, BucketStorage>();

var app = builder.Build();

// --------------------- Swagger ---------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --------------------- HTTPS ---------------------
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// --------------------- Middleware аутентификации ---------------------
app.UseAuthentication();
app.UseAuthorization();

// --------------------- Маршруты ---------------------
app.MapControllers();

// --------------------- Сценарии для работы с изображениями ---------------------
if (args.Contains("--seed-images"))
{
    await ISeedProductImages.SeedAsync(app);
    Console.WriteLine("Done!");
    return;
}

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

if (args.Contains("--delete-all-images"))
{
    using var scope = app.Services.CreateScope();
    var host = scope.ServiceProvider.GetRequiredService<IHost>();
    await IClearAllProductImages.ClearAsync(host);
    Console.WriteLine("All images deleted");
    return;
}

app.Run();