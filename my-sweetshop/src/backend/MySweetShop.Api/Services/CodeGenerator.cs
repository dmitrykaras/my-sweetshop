namespace MySweetShop.Api.Services;

public static class CodeGenerator
{
    public static string Generate4Digits()
        => Random.Shared.Next(0, 10000).ToString("D4");
}