using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;
using MyCandyShop.Api.Services;

namespace MyCandyShop.Api.Controllers;

[ApiController]
[Route("/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _db.Products.ToListAsync();
        var result = products.Select(p => new ProductDto // Используем класс DTO явно
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId, // ТЕПЕРЬ ПЕРЕДАЕМ ID КАТЕГОРИИ
            ImageUrl = string.IsNullOrEmpty(p.ImageKey)
                ? null
                : _storage.GetPreSignedUrl(p.ImageKey, TimeSpan.FromHours(6))
        });
        return Ok(result);
    }

    // Метод удаления изображения по его productId 
    [HttpPost("{productId:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid productId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл пустой");

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();

        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            try
            {
                await _storage.DeleteAsync(product.ImageKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось удалить старое изображение: {Key}", product.ImageKey);
            }
        }

        var folder = $"products/{productId}";
        var (uploadedKey, url) = await _storage.UploadAsync(file, folder);
        product.ImageKey = uploadedKey;

        await _db.SaveChangesAsync();

        return Ok(new { ImageUrl = url });
    }


    [HttpDelete("{productId:guid}/image")]
    public async Task<IActionResult> DeleteImage(Guid productId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);

        if (product == null) return NotFound("Продукт не найден");
        if (string.IsNullOrEmpty(product.ImageKey)) return BadRequest("У продукта нет изображения");

        // 1. Удаляем физический файл из S3 бакета
        await _storage.DeleteAsync(product.ImageKey);

        // 2. Стираем ключ в базе данных
        product.ImageKey = null;
        await _db.SaveChangesAsync();

        return NoContent(); // Успешно, без возврата данных
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();

        // Важно: Сначала удаляем файл из облака
        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            await _storage.DeleteAsync(product.ImageKey);
        }

        // Затем удаляем сам продукт
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}