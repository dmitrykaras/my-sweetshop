using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using MySweetShop.Api.Services;
using Xunit;

public class ProfileControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProfileControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region GET /profile/me & /profile/points

    [Fact(DisplayName = "GET /profile/me - возвращает 200 OK и данные текущего пользователя")]
    public async Task GetProfile_ReturnsUserProfile_WhenUserIsAuthorized()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string email = $"user_{userId}@sweetshop.test";
        string firstName = "Иван";
        string lastName = "Тестов";
        int points = 250;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Points = points,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.GetAsync("/profile/me");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserProfileResponse>();

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(email, result.Email);
        Assert.Equal(firstName, result.FirstName);
        Assert.Equal(lastName, result.LastName);
        Assert.Equal(points, result.Points);
    }

    [Fact(DisplayName = "GET /profile/me - возвращает 401 Unauthorized, если заголовок авторизации не передан")]
    public async Task GetProfile_ReturnsUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/profile/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "GET /profile/points - возвращает количество баллов авторизованного пользователя")]
    public async Task GetPoints_ReturnsUserPoints_WhenUserIsAuthorized()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        int expectedPoints = 500;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"points_user_{userId}@sweetshop.test",
                Points = expectedPoints,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.GetAsync("/profile/points");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserPointsResponse>();

        Assert.NotNull(result);
        Assert.Equal(expectedPoints, result.Points);
    }

    #endregion

    #region POST & DELETE /profile/favorites/{productId}

    [Fact(DisplayName = "POST /profile/favorites/{productId} - успешно добавляет товар в избранное")]
    public async Task AddFavorite_Returns200OK_WhenProductIsNotInFavorites()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"add_fav_{userId}@sweetshop.test",
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Товар для добавления " + productId,
                Price = 300,
                CategoryId = category.Id
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.PostAsync($"/profile/favorites/{productId}", null);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var favorite = await db.UserFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            Assert.NotNull(favorite);
        }
    }

    [Fact(DisplayName = "POST /profile/favorites/{productId} - возвращает 400 Bad Request, если товар уже в избранном")]
    public async Task AddFavorite_Returns400BadRequest_WhenAlreadyInFavorites()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"dup_fav_{userId}@sweetshop.test",
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Дублируемый товар " + productId,
                Price = 400,
                CategoryId = category.Id
            });

            db.UserFavorites.Add(new UserFavorite
            {
                UserId = userId,
                ProductId = productId
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.PostAsync($"/profile/favorites/{productId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /profile/favorites/{productId} - успешно удаляет товар из избранного")]
    public async Task RemoveFavorite_Returns200OK_WhenFavoriteExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"rem_fav_{userId}@sweetshop.test",
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Товар для удаления " + productId,
                Price = 500,
                CategoryId = category.Id
            });

            db.UserFavorites.Add(new UserFavorite
            {
                UserId = userId,
                ProductId = productId
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.DeleteAsync($"/profile/favorites/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var favorite = await db.UserFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            Assert.Null(favorite);
        }
    }

    [Fact(DisplayName = "DELETE /profile/favorites/{productId} - возвращает 404 Not Found, если товара нет в избранном")]
    public async Task RemoveFavorite_Returns404NotFound_WhenFavoriteDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"not_found_fav_{userId}@sweetshop.test",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.DeleteAsync($"/profile/favorites/{productId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region PATCH /profile/UpdateProfile

    [Fact(DisplayName = "PATCH /profile/UpdateProfile - успешно обновляет имя и фамилию")]
    public async Task UpdateProfile_UpdatesNameAndLastName_WhenDataIsValid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"update_{userId}@sweetshop.test",
                FirstName = "СтароеИмя",
                LastName = "СтараяФамилия",
                LastProfileUpdate = null,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var updateRequest = new
        {
            FirstName = "НовоеИмя",
            LastName = "НоваяФамилия"
        };

        // Act
        var response = await client.PatchAsJsonAsync("/profile/UpdateProfile", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedUser = await db.Users.FirstAsync(u => u.Id == userId);

            Assert.Equal("НовоеИмя", updatedUser.FirstName);
            Assert.Equal("НоваяФамилия", updatedUser.LastName);
            Assert.NotNull(updatedUser.LastProfileUpdate);
        }
    }

    [Fact(DisplayName = "PATCH /profile/UpdateProfile - возвращает 400 Bad Request при повторной попытке раньше чем через 5 минут")]
    public async Task UpdateProfile_Returns400BadRequest_WhenCooldownIsActive()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"cooldown_{userId}@sweetshop.test",
                FirstName = "Иван",
                LastName = "Иванов",
                LastProfileUpdate = DateTimeOffset.UtcNow.AddMinutes(-1),
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var updateRequest = new
        {
            FirstName = "Пётр"
        };

        // Act
        var response = await client.PatchAsJsonAsync("/profile/UpdateProfile", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /profile/request-code-for-change-email & /profile/verify-change-email

    [Fact(DisplayName = "POST /profile/request-code-for-change-email - успешно создает запись с кодом в БД")]
    public async Task RequestChangeEmail_CreatesEmailChangeCode_WhenDataIsValid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string currentEmail = $"current_{userId}@sweetshop.test";
        string newEmail = $"new_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = currentEmail,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var requestBody = new
        {
            Email = currentEmail,
            NewEmail = newEmail
        };

        // Act
        var response = await client.PostAsJsonAsync("/profile/request-code-for-change-email", requestBody);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var codeEntity = await db.EmailChangeCodes.FirstOrDefaultAsync(x => x.UserId == userId && x.NewEmail == newEmail);

            Assert.NotNull(codeEntity);
            Assert.False(codeEntity.IsUsed);
        }
    }

    [Fact(DisplayName = "POST /profile/request-code-for-change-email - возвращает 400 Bad Request, если новый Email уже занят")]
    public async Task RequestChangeEmail_Returns400BadRequest_WhenNewEmailIsAlreadyTaken()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string currentEmail = $"user_{userId}@sweetshop.test";
        string existingEmail = $"occupied_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = currentEmail,
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = existingEmail,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var requestBody = new
        {
            Email = currentEmail,
            NewEmail = existingEmail
        };

        // Act
        var response = await client.PostAsJsonAsync("/profile/request-code-for-change-email", requestBody);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /profile/verify-change-email - успешно меняет Email пользователя при правильном коде")]
    public async Task VerifyChangeEmail_UpdatesEmail_WhenCodeIsValid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string currentEmail = $"old_{userId}@sweetshop.test";
        string newEmail = $"new_{userId}@sweetshop.test";
        string code = "123456";
        string hash = HashService.Sha256($"{userId}:{newEmail}:{code}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = currentEmail,
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.EmailChangeCodes.Add(new EmailChangeCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = currentEmail,
                NewEmail = newEmail,
                CodeHash = hash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var verifyRequest = new
        {
            NewEmail = newEmail,
            Code = code
        };

        // Act
        var response = await client.PostAsJsonAsync("/profile/verify-change-email", verifyRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            var codeEntity = await db.EmailChangeCodes.FirstAsync(x => x.UserId == userId && x.NewEmail == newEmail);

            Assert.Equal(newEmail, user.Email);
            Assert.True(codeEntity.IsUsed);
        }
    }

    [Fact(DisplayName = "POST /profile/verify-change-email - возвращает 400 Bad Request при неверном коде")]
    public async Task VerifyChangeEmail_Returns400BadRequest_WhenCodeIsInvalid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string currentEmail = $"user_{userId}@sweetshop.test";
        string newEmail = $"new_{userId}@sweetshop.test";
        string correctCode = "123456";
        string hash = HashService.Sha256($"{userId}:{newEmail}:{correctCode}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = currentEmail,
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.EmailChangeCodes.Add(new EmailChangeCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = currentEmail,
                NewEmail = newEmail,
                CodeHash = hash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var verifyRequest = new
        {
            NewEmail = newEmail,
            Code = "999999" // Неверный код
        };

        // Act
        var response = await client.PostAsJsonAsync("/profile/verify-change-email", verifyRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /profile/request-code-for-change-email - возвращает 400, если код запрашивали менее 2 минут назад")]
    public async Task RequestChangeEmail_Returns400BadRequest_WhenCooldownIsActive()
    {
        Guid userId = Guid.NewGuid();
        string currentEmail = $"current_{userId}@sweetshop.test";
        string newEmail = $"new_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User { Id = userId, Email = currentEmail, CreatedAt = DateTimeOffset.UtcNow });
            db.EmailChangeCodes.Add(new EmailChangeCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = currentEmail,
                NewEmail = newEmail,
                CodeHash = "hash",
                CreatedAt = DateTimeOffset.UtcNow.AddSeconds(-30),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // тот же адрес
        var r1 = await client.PostAsJsonAsync("/profile/request-code-for-change-email",
            new { Email = currentEmail, NewEmail = newEmail });
        Assert.Equal(HttpStatusCode.BadRequest, r1.StatusCode);

        // другой адрес — cooldown действует на пользователя
        var r2 = await client.PostAsJsonAsync("/profile/request-code-for-change-email",
            new { Email = currentEmail, NewEmail = $"other_{userId}@sweetshop.test" });
        Assert.Equal(HttpStatusCode.BadRequest, r2.StatusCode);
    }

    [Fact(DisplayName = "POST /profile/request-code-for-change-email - нормализует NewEmail (регистр и пробелы)")]
    public async Task RequestChangeEmail_StoresNormalizedNewEmail()
    {
        Guid userId = Guid.NewGuid();
        string currentEmail = $"current_{userId}@sweetshop.test";
        string newEmail = $"new_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User { Id = userId, Email = currentEmail, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var response = await client.PostAsJsonAsync("/profile/request-code-for-change-email",
            new { Email = currentEmail, NewEmail = $"  {newEmail.ToUpper()}  " });

        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.NotNull(await db.EmailChangeCodes.FirstOrDefaultAsync(x => x.UserId == userId && x.NewEmail == newEmail));
        }
    }

    [Fact(DisplayName = "POST /profile/request-code-for-change-email - возвращает 400, если формат нового Email неверный")]
    public async Task RequestChangeEmail_Returns400BadRequest_WhenNewEmailFormatIsInvalid()
    {
        Guid userId = Guid.NewGuid();
        string currentEmail = $"current_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User { Id = userId, Email = currentEmail, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var response = await client.PostAsJsonAsync("/profile/request-code-for-change-email",
            new { Email = currentEmail, NewEmail = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /profile/verify-change-email - параллельные запросы не превышают лимит попыток")]
    public async Task VerifyChangeEmail_DoesNotExceedMaxAttempts_WhenRequestsAreParallel()
    {
        Guid userId = Guid.NewGuid();
        string currentEmail = $"current_{userId}@sweetshop.test";
        string newEmail = $"new_{userId}@sweetshop.test";
        string correctCode = "123456";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User { Id = userId, Email = currentEmail, CreatedAt = DateTimeOffset.UtcNow });
            db.EmailChangeCodes.Add(new EmailChangeCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = currentEmail,
                NewEmail = newEmail,
                CodeHash = HashService.Sha256($"{userId}:{newEmail}:{correctCode}"),
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // 30 параллельных неверных догадок
        var tasks = Enumerable.Range(0, 30)
            .Select(i => client.PostAsJsonAsync("/profile/verify-change-email",
                new { NewEmail = newEmail, Code = (100000 + i).ToString() }))
            .ToArray();
        await Task.WhenAll(tasks);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.EmailChangeCodes.FirstAsync(x => x.UserId == userId && x.NewEmail == newEmail);

            Assert.Equal(5, entity.Attempts); // не больше лимита
            Assert.True(entity.IsUsed); // код аннулирован
        }
    }

    #endregion

    private record UserProfileResponse(Guid Id, string Email, string? FirstName, string? LastName, int Points);
    private record UserPointsResponse(int Points);
}