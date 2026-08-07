using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using System.Net.Http.Json;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    
    public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = "GET /porducts - возввращает список начальных товаров после запуска")]
    public async Task GetProducts_ReturnsInitialSeededProducts()
    {
        // Arrange (подготовка)
        var client = _factory.CreateClient();

        // Act (действие)
        var response = await client.GetAsync("/products");

        // Asseert (проверка)
        response.EnsureSuccessStatusCode(); // я 200 ОК
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();

        Assert.NotNull(products);
        Assert.NotEmpty(products); // Проверяем, что базовые товары вернулись
    }

    [Fact(DisplayName = "GET /products - возвращает торт 'Наполеон', если он был добавлен в БД")]
    public async Task GetProducts_ReturnsNewlyAddedProduct()
    {
        // Arrange
        Guid newProductID = Guid.NewGuid();
        string newProductName = "Наполион" + Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // берем любую категорию, которая уже есть в базе после сидирования
            var category = await db.Categories.FirstOrDefaultAsync();

            // Если вдруг категорий нет — создаем и сразу сохраняем
            if (category == null)
            {
                category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Тестовая категория"
                };
                db.Categories.Add(category);
                await db.SaveChangesAsync();
            }


            // привязывает продук к созданной категории
            db.Products.Add(new Product
            {
                Id = newProductID,
                Name = newProductName,
                Price = 1200,
                Description = "Калссический слоеный торт",
                CategoryId = category.Id
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();

        Assert.NotNull(products);
        Assert.Contains(products, p => p.Id == newProductID && p.Name == newProductName);
    }

    [Fact(DisplayName = "DELETE /products/{id} - возвращает 204 No Content и удаляет продукт из базы")]
    public async Task DeleteProduct_Returns204NoContent_WhenProductExists()
    {
        // Arrange - создам продукт в базе, который будем удалять
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Торт для удаления" + productId,
                Price = 500,
                CategoryId = category.Id
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act - отправляет DELETE запрос
        var resopne = await client.DeleteAsync($"/products/{productId}");

        //Assert - проверяем статус 204 и отсутсвие записи в БД
        Assert.Equal(System.Net.HttpStatusCode.NoContent, resopne.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var deletedProduct = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);
            Assert.Null(deletedProduct);
        }
    }

    [Fact(DisplayName = "DELETE /prodcuts/{id} - возвращает 404 Not Found, если продукт не существует")]
    public async Task DeleteProduct_Return404NoFound_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        Guid nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/products/{nonExistentId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact (DisplayName = "POST /prodocts/{id}/image - Загружает изображение товара и возвращает 200 ОК")]
    public async Task UploadImage_Returns200OK_WhenFileIsValid()
    {
        // Arrange - создаём продукт
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Products.Add(new Product 
            { 
                Id = productId,
                Name = "Товар с картинкой" + productId,
                Price = 800,
                CategoryId = category.Id
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Формируем multipart/form-data запрос с текстовым файлом в памяти
        using var content = new MultipartFormDataContent();
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var byteContent = new ByteArrayContent(bytes);
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(byteContent, "file", "test-cake.jpg");

        // Act
        var response = await client.PostAsync($"/products/{productId}/image", content);

        // Assert
        response.EnsureSuccessStatusCode(); // 200 OK

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedProduct = await db.Products.FirstAsync(p => p.Id == productId);

            // Проверяем, что ImageKey записался в базу
            Assert.NotNull(updatedProduct.ImageKey);
            Assert.Contains(productId.ToString(), updatedProduct.ImageKey);
        }
    }

    [Fact (DisplayName = "POST /products/{id}/image - Возврает 400 Bad Request, если файл не передан")]
    public async Task UploadImage_Returns400BadRequest_WhenFileIsEmpty()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        var client = _factory.CreateClient();
        using var emptyContent = new MultipartFormDataContent();

        // Act
        var response = await client.PostAsync($"/products/{productId}/image", emptyContent);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact (DisplayName = "POST /products/{id}/toggle-favorite - добавляет товар в избранное авторизованного пользователя")]
    public async Task ToggleFavorite_AddsProductToFavorite_WhenUserIsAuthorize()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        string EmailUser = $"user_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            // Создаём пользователя
            db.Users.Add(new User
            {
               Id = userId,
               Email = EmailUser,
               CreatedAt = DateTimeOffset.UtcNow
            });

            // Создаём товар
            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Торт для лайка" + productId,
                Price = 1200,
                CategoryId = category.Id
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        // Передаём ID пользователя через кастомный заголовок
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.PostAsync($"/products/{productId}/toggle-favorite", null);

        // Assert
        response.EnsureSuccessStatusCode(); // 200 OK

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Проверяем, что запись действительно появилась в табилце UserFavorites
            var favorite = await db.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.ProductId == productId);

            Assert.NotNull(favorite);
        }
    }

    [Fact (DisplayName = "GET /products/favorite - возвращает список только избранный товаров текущего пользователя")]
    public async Task GetFavoriteProducts_RetrutnUserFavoriteOnly()
    {
        // Arrange
        Guid favoriteProductId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        string EmailUser = $"user_{userId}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            // Создаём пользователя
            db.Users.Add(new User
            {
                Id = userId,
                Email = EmailUser,
                CreatedAt = DateTime.UtcNow
            });

            // Создаём товар
            var product = new Product
            {
                Id = favoriteProductId,
                Name = "Любиый торт" + favoriteProductId,
                Price = 1500,
                CategoryId = category.Id
            };
            db.Products.Add(product);

            // Создаём запись в избранном напрямую в БД
            db.UserFavorites.Add(new UserFavorite
            {
                UserId = userId,
                ProductId = favoriteProductId
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.GetAsync("/products/favorites");

        // Assert
        response.EnsureSuccessStatusCode(); // 200 OK

        var favorites = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(favorites);
        Assert.Contains(favorites, f => f.Id == favoriteProductId && f.IsFavorite == true);
    }

    [Fact (DisplayName = "DELETE /products/{id}/image - удаляет изображения товара и возвращает 204 No Content")]
    public async Task DeleteImage_Return204NoContent_WhenImageExists()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Товар с картинкой" + productId,
                Price = 1200,
                CategoryId = category.Id,
                ImageKey = "products/dummy_image.jpg"
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act
        var respone = await client.DeleteAsync($"/products/{productId}/image");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, respone.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var product = await db.Products.FirstAsync(p => p.Id == productId);
            Assert.Null(product.ImageKey); // Проверяем, что ImageKey обнулился
        }
    }

    [Fact (DisplayName = "DELETE /products/{id}/image - возвращает 400 Bad Request, если у товара нет изображения")]
    public async Task DeleteImage_Return400BadRequest_WhenImageDoesNotExist()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var catergory = await db.Categories.FirstAsync();

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Товар без карточки" + productId,
                Price = 400,
                CategoryId = catergory.Id,
                ImageKey = null
            });
            await db.SaveChangesAsync();    
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/products/{productId}/image");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact (DisplayName = "POST /products/{id}/toggle-favorite - удаляет товар из избранного при повторном вызове")]
    public async Task ToggleFavorite_RemovesProductFromFavorite_WhenAlreadyFavorited ()
    {
        // Arange
        Guid productId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"user_{userId}@test.com", 
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Торт" + productId,
                Price = 1000,
                CategoryId = category.Id
            });

            // Товар уже в избранном
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
        var resopnse = await client.PostAsync($"/products/{productId}/toggle-favorite", null);

        // Assert
        resopnse.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var favorite = await db.UserFavorites.FirstOrDefaultAsync(uf => uf.UserId == userId && uf.ProductId == productId);

            Assert.Null(favorite);
        }
    }

    [Fact(DisplayName = "POST /products/{id}/image - возвращает 404 Not Found, если продукт не существует")]
    public async Task UploadImage_Returns404NotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8 });
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(byteContent, "file", "test.jpg");

        // Act
        var response = await client.PostAsync($"/products/{Guid.NewGuid()}/image", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "POST /products/{id}/image - перезаписывает старое изображение при повторной загрузке")]
    public async Task UploadImage_ReplacesOldImage_WhenImageAlreadyExists()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Товар со старой картинкой",
                Price = 500,
                CategoryId = category.Id,
                ImageKey = "products/old_image.jpg"
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8 });
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(byteContent, "file", "new-cake.jpg");

        // Act
        var response = await client.PostAsync($"/products/{productId}/image", content);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedProduct = await db.Products.FirstAsync(p => p.Id == productId);

            Assert.NotEqual("products/old_image.jpg", updatedProduct.ImageKey);
            Assert.Contains(productId.ToString(), updatedProduct.ImageKey);
        }
    }

    [Fact(DisplayName = "DELETE /products/{id}/image - возвращает 404 Not Found, если продукт не найден")]
    public async Task DeleteImage_Returns404NotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/products/{Guid.NewGuid()}/image");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /products/{id} - удаляет товар и связанную картинку")]
    public async Task DeleteProduct_DeletesProductAndImage_WhenProductHasImage()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Товар с картинкой для удаления",
                Price = 1000,
                CategoryId = category.Id,
                ImageKey = "products/image_to_delete.jpg"
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/products/{productId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);
            Assert.Null(product);
        }
    }

    [Fact(DisplayName = "POST /products/{id}/toggle-favorite - возвращает 404 Not Found, если продукт не существует")]
    public async Task ToggleFavorite_Returns404NotFound_WhenProductDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        // Act
        var response = await client.PostAsync($"/products/{Guid.NewGuid()}/toggle-favorite", null);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "GET /products - возвращает IsFavorite = true для авторизованного пользователя")]
    public async Task GetProducts_ReturnsIsFavoriteTrue_WhenUserIsAuthenticatedAndHasFavorite()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = await db.Categories.FirstAsync();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"user_{userId}@test.com",
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.Products.Add(new Product
            {
                Id = productId,
                Name = "Избранный товар в каталоге",
                Price = 900,
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
        var response = await client.GetAsync("/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);
        var targetProduct = products.FirstOrDefault(p => p.Id == productId);
        Assert.NotNull(targetProduct);
        Assert.True(targetProduct.IsFavorite);
    }

    [Fact(DisplayName = "GET /products/favorites - возвращает 401 Unauthorized для неавторизованного пользователя")]
    public async Task GetFavoriteProducts_Returns401Unauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/products/favorites");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}