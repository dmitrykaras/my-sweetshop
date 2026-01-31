namespace MyCandyShop.Api.Entities
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int CategoryId { get; set; }
    }
}
