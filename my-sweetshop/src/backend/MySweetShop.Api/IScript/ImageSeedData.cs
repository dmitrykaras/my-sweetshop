namespace MySweetShop.Api.IScript
{
    public class ImageSeedData
    {
        public Guid Id { get; set; }
        // Относительный путь к изображения
        public required string RelativePath { get; set; }
    }
}