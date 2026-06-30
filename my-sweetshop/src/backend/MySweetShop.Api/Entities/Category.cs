namespace MySweetShop.Api.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        // Название категории
        public required string Name { get; set; }
        // Список товаров
        public ICollection<Product> Products { get; set; } = [];
    }
}