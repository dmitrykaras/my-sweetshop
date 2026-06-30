using System.Security.Cryptography;
using System.Text;

namespace MySweetShop.Api.Services;

public static class HashService
{
    // Вспомогательный метод для хеширования
    public static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}