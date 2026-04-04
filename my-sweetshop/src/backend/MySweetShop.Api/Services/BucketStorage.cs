using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using MySweetShop.Api.Services;

public class BucketStorage : IObjectStorage
{
    private readonly IAmazonS3 _client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;

    public BucketStorage(IConfiguration config)
    {
        _bucket = config["BucketSettings:Name"];
        _publicBaseUrl = config["BucketSettings:PublicBaseUrl"];

        var s3Config = new AmazonS3Config
        {
            ServiceURL = config["BucketSettings:Endpoint"],
            ForcePathStyle = true,
            AuthenticationRegion = "ru-1",
            SignatureMethod = SigningAlgorithm.HmacSHA1
        };

        _client = new AmazonS3Client(
            config["BucketSettings:AccessKey"],
            config["BucketSettings:SecretKey"],
            s3Config
        );
    }

    // Метод генерации имени файла
    public async Task<(string Key, string Url)> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var key = $"{folder.TrimEnd('/')}/{fileName}";

        using var stream = file.OpenReadStream();

        var request = new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType,
            DisablePayloadSigning = true // Отключает подпись чанков, что убирает заголовок aws-chunked
        };

        request.Headers.ContentLength = file.Length;

        await _client.PutObjectAsync(request, ct);

        return (key, GetPublicUrl(key));
    }

    public async Task DeleteAsync(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        await _client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = key
        });
    }

    public string GetPublicUrl(string key) => $"{_publicBaseUrl}/{key}";

    public string GetPreSignedUrl(string key, TimeSpan expiresIn)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiresIn)
        };
        return _client.GetPreSignedURL(request);
    }
}