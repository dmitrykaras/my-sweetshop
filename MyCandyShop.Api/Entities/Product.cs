using System;

namespace MyCandyShop.Api.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    public string? Description { get; set; }
    public string? ImageKey { get; set; }
    public decimal? Price { get; set; } // опционально
}