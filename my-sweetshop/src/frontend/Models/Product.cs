namespace my_sweetshop.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}