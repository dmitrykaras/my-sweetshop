using System;

namespace MyCandyShop.Api.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public required string Name { get; set; }
    public Category Category { get; set; } = default!;
    public string? Description { get; set; } 
    public string? ImageKey { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl => string.IsNullOrEmpty(ImageKey)
    ? null
    : $"https://bucket.ru/{ImageKey}"; // формируем публичный URL
}