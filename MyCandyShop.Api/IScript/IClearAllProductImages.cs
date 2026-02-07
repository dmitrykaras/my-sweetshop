using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using MyCandyShop.Api.Services;

namespace MyCandyShop.Api.IScript
{
    public static class IClearAllProductImages
    {
        public static async Task ClearAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var storage = scope.ServiceProvider.GetRequiredService<IObjectStorage>() as BucketStorage;
            if (storage == null)
                throw new InvalidOperationException("BucketStorage не зарегистрирован");

            Console.WriteLine("== Удаление всех файлов из бакета начато ==");

            var clientField = typeof(BucketStorage).GetField("_client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bucketField = typeof(BucketStorage).GetField("_bucket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var client = (IAmazonS3)clientField.GetValue(storage);
            var bucket = (string)bucketField.GetValue(storage);

            string? continuationToken = null;

            do
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = bucket,
                    ContinuationToken = continuationToken
                };

                var listResponse = await client.ListObjectsV2Async(listRequest);

                foreach (var obj in listResponse.S3Objects)
                {
                    try
                    {
                        await client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest
                        {
                            BucketName = bucket,
                            Key = obj.Key
                        });

                        Console.WriteLine($"Удалён {obj.Key}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при удалении {obj.Key}: {ex.Message}");
                    }
                }

                continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;

            } while (continuationToken != null);

            Console.WriteLine("== Все файлы из бакета удалены ==");
        }
    }
}
