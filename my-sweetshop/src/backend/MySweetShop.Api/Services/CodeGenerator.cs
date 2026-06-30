namespace MySweetShop.Api.Services;

public static class CodeGenerator
{
    // Вспомогательный метод для генирации кода
    public static string Generate4Digits()
        => Random.Shared.Next(0, 10000).ToString("D4");
}