namespace MyCandyShop.Api.Options
{
    public class BucketSettings
    {
        public string BucketName { get; set; } = null!;
        public string ServiceUrl { get; set; } = null!;
        public string AccessKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
    }
}