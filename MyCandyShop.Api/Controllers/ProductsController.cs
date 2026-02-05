using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Services;

namespace MyCandyShop.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public ProductsController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _db.Products.ToListAsync();

        var result = products.Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            ImageUrl = string.IsNullOrEmpty(p.ImageKey)
                ? null
                : _storage.GetPreSignedUrl(p.ImageKey, TimeSpan.FromHours(6))
        });

        return Ok(result);
    }

    [HttpPost("{productId:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid productId, IFormFile file)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();

        if (!string.IsNullOrEmpty(product.ImageKey))
            await _storage.DeleteAsync(product.ImageKey);

        var (key, _) = await _storage.UploadAsync(file, $"products/{productId}");
        product.ImageKey = key;

        await _db.SaveChangesAsync();
        return Ok();
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