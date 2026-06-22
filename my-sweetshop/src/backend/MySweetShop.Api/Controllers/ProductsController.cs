using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;

namespace MySweetShop.Api.Controllers;

[ApiController]
[Route("/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    // Внедряем IWebHostEnvironment вместо старого Storage
    public ProductsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Получаем изображения и данные о продуктах
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // Динамически формируем базовый URL сервера (схемы + хост)
        // Например: https://localhost:7001/uploads/ или http://your-vps-ip/uploads/
        string baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/";

        var products = await _db.Products.ToListAsync();

        var result = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId,
            ImageUrl = string.IsNullOrEmpty(p.ImageKey)
                ? null
                : $"{baseUrl}{p.ImageKey}"
        }).ToList();

        return Ok(result);
    }

    // Отдаём изображения
    [HttpPost("{productId:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid productId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл пустой");

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();

        // Если у продукта уже было локальное изображение — удаляем его с диска
        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            var oldPath = Path.Combine(_env.WebRootPath, "uploads", product.ImageKey);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        // Строим пути для сохранения
        var relativeFolder = Path.Combine("products", productId.ToString());
        var absoluteFolder = Path.Combine(_env.WebRootPath, "uploads", relativeFolder);

        if (!Directory.Exists(absoluteFolder))
        {
            Directory.CreateDirectory(absoluteFolder);
        }

        // Генерируем уникальное имя файла, чтобы избежать проблем с кэшем
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var absoluteFilePath = Path.Combine(absoluteFolder, fileName);

        // Превращаем системный путь в URL-совместимый (заменяем обратные слэши \ на прямые / для Linux/Windows)
        var relativeFilePath = Path.Combine(relativeFolder, fileName).Replace('\\', '/');

        // Сохраняем файл на сервере
        using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Записываем относительный путь в БД
        product.ImageKey = relativeFilePath;
        await _db.SaveChangesAsync();

        string baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/";
        return Ok(new { ImageUrl = $"{baseUrl}{relativeFilePath}" });
    }

    [HttpDelete("{productId:guid}/image")]
    public async Task<IActionResult> DeleteImage(Guid productId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);

        if (product == null) return NotFound("Продукт не найден");
        if (string.IsNullOrEmpty(product.ImageKey)) return BadRequest("У продукта нет изображения");

        // Удаляем физический файл с сервера
        var absolutePath = Path.Combine(_env.WebRootPath, "uploads", product.ImageKey);
        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }

        product.ImageKey = null;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();

        // Сначала удаляем файл с диска
        if (!string.IsNullOrEmpty(product.ImageKey))
        {
            var absolutePath = Path.Combine(_env.WebRootPath, "uploads", product.ImageKey);
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}