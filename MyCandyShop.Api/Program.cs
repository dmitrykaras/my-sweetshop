using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;
using MyCandyShop.Api.IScript;
using MyCandyShop.Api.Options;
using MyCandyShop.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtService>();

builder.Services.Configure<BucketSettings>(builder.Configuration.GetSection("BucketSettings"));
builder.Services.AddSingleton<IObjectStorage, BucketStorage>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(10)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Urls.Add("http://0.0.0.0:5107");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Прекрепление изображений к продуктам
if (args.Contains("--seed-images"))
{
    await ISeedProductImages.SeedAsync(app);
    Console.WriteLine("Done!");
    return;
}

// Очистка всех ImageUrl
if (args.Contains("--clear-images"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var products = await db.Products.ToListAsync();

    foreach (var p in products)
        p.ImageKey = null;

    await db.SaveChangesAsync();

    Console.WriteLine("IMAGE KEYS CLEARED");
    return; // сервер не стартует
}

// Метод удаляет все изображения и их ключи
if (args.Contains("--delete-all-images"))
{
    using (var scope = app.Services.CreateScope())
    {
        var host = scope.ServiceProvider.GetRequiredService<IHost>();
        await IClearAllProductImages.ClearAsync(host);
    }
}

app.Run();