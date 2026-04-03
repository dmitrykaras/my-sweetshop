namespace MySweetShop.Api.Services
{
    public interface IObjectStorage
    {
        Task<(string Key, string Url)> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
        Task DeleteAsync(string key);
        string GetPublicUrl(string key);
        string GetPreSignedUrl(string key, TimeSpan expiresIn);
    }
}   