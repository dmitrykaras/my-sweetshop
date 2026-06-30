using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using System.Security.Claims;

namespace MySweetShop.Api.Controllers;

[ApiController]
[Route("/products")]
public class ProductsController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IWebHostEnvironment _env = env;

    // Получаем изображения и данные о продуктах
    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/";

        // Безопасно пытаемся получить ID пользователя (если гость — будет null)
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? userId = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;

        var products = await _db.Products
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,

                // Если юзер авторизован — ищем лайк. Если гость — всегда false.
                IsFavorite = userId.HasValue && _db.UserFavorites.Any(uf => uf.ProductId == p.Id && uf.UserId == userId.Value),

                ImageUrl = string.IsNullOrEmpty(p.ImageKey) ? null : $"{baseUrl}{p.ImageKey}"
            })
            .ToListAsync(cancellationToken);

        return Ok(products);
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

    // Удаление изображения
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

    // Удаление продукта
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

    // Получение только избранных товаров
    [Authorize]
    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavoriteProducts(CancellationToken cancellationToken)
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/";

        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Идем в таблицу связей, фильтруем по юзеру, вытягиваем продукты
        var favoriteProducts = await _db.UserFavorites
            .AsNoTracking()
            .Where(uf => uf.UserId == userId)
            .Select(uf => uf.Product) // Берем связанный продукт
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                IsFavorite = true, // Он 100% избранный, раз мы его тут нашли
                ImageUrl = string.IsNullOrEmpty(p.ImageKey) ? null : $"{baseUrl}{p.ImageKey}"
            })
            .ToListAsync(cancellationToken);

        return Ok(favoriteProducts);
    }

    // Переключение статуса "Избранное"
    [Authorize]
    [HttpPost("{productId:guid}/toggle-favorite")]
    public async Task<IActionResult> ToggleFavorite(Guid productId, CancellationToken cancellationToken)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Проверяем, существует ли вообще такой товар
        var productExists = await _db.Products.AnyAsync(x => x.Id == productId, cancellationToken);
        if (!productExists) return NotFound("Продукт не найден");

        // Ищем, есть ли уже этот товар в избранном у ЭТОГО пользователя
        var existingFavorite = await _db.UserFavorites
            .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.ProductId == productId, cancellationToken);

        bool isNowFavorite;

        if (existingFavorite != null)
        {
            // Если нашли — удаляем (убираем лайк)
            _db.UserFavorites.Remove(existingFavorite);
            isNowFavorite = false;
        }
        else
        {
            // Если не нашли — добавляем (ставим лайк)
            _db.UserFavorites.Add(new UserFavorite { UserId = userId, ProductId = productId });
            isNowFavorite = true;
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Возвращаем новый статус на фронтенд
        return Ok(new { IsFavorite = isNowFavorite });
    }
}