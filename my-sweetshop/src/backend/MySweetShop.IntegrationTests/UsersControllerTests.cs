using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using Xunit;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region GET api/users/{email}

    [Fact(DisplayName = "GET api/users/{email} - возвращает 200 OK и данные пользователя, если он существует")]
    public async Task GetUserByEmail_ReturnsUser_WhenUserExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string email = $"get_user_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = email,
                FirstName = "Алексей",
                LastName = "Петров",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/users/{email}");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<User>();

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(email, result.Email);
        Assert.Equal("Алексей", result.FirstName);
        Assert.Equal("Петров", result.LastName);
    }

    [Fact(DisplayName = "GET api/users/{email} - возвращает 404 Not Found, если пользователь не найден")]
    public async Task GetUserByEmail_Returns404NotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        string email = $"non_existent_{Guid.NewGuid()}@sweetshop.test";

        // Act
        var response = await client.GetAsync($"/api/users/{email}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST api/users

    [Fact(DisplayName = "POST api/users - создает нового пользователя, если пользователя с таким Email нет")]
    public async Task CreateOrUpdateUser_CreatesNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        string email = $"create_user_{Guid.NewGuid()}@sweetshop.test";

        var newUser = new User
        {
            Email = email,
            FirstName = "Мария",
            LastName = "Сидорова"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", newUser);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<User>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(email, result.Email);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            Assert.NotNull(userInDb);
            Assert.Equal("Мария", userInDb.FirstName);
            Assert.Equal("Сидорова", userInDb.LastName);
        }
    }

    [Fact(DisplayName = "POST api/users - обновляет имя и фамилию существующего пользователя при совпадении Email")]
    public async Task CreateOrUpdateUser_UpdatesExistingUser_WhenEmailExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string email = $"update_user_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = email,
                FirstName = "СтароеИмя",
                LastName = "СтараяФамилия",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var updatePayload = new User
        {
            Email = email,
            FirstName = "НовоеИмя",
            LastName = "НоваяФамилия"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", updatePayload);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedUserInDb = await db.Users.FirstAsync(u => u.Id == userId);

            Assert.Equal("НовоеИмя", updatedUserInDb.FirstName);
            Assert.Equal("НоваяФамилия", updatedUserInDb.LastName);
        }
    }

    #endregion
}