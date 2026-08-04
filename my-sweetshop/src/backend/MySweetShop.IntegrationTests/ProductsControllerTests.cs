using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using System.Net.Http.Json;
using Xunit;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    
    public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_ReturnsEmptyList_WhenNoProductsExist()
    {
        // Arrange (подготовка)
        var client = _factory.CreateClient();

        // Act (действие)
        var response = await client.GetAsync("/products");

        // Asseert (проверка)
        response.EnsureSuccessStatusCode(); // я 200 ОК
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();

        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
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
}