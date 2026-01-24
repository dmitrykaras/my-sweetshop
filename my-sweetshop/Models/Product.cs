namespace my_sweetshop.Models
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        // Имя файла из Resources/Images или URL
        public string Image { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // К какой категории относится товар
        public string CategoryId { get; set; } = string.Empty;
    }
}
