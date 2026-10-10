using System.Security.Cryptography;

namespace MySweetShop.Api.Services;

public static class CodeGenerator
{
    // Вспомогательный метод для генирации кода
    public static string Generate6Digits()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
}