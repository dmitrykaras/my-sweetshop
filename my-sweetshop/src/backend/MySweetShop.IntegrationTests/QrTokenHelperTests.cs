using System.Text.RegularExpressions;
using MySweetShop.Api.Services;
using Xunit;

public class QrTokenHelperTests
{
    [Fact(DisplayName = "QrTokenHelper.NewToken - возвращает URL-safe строку из 43 символов")]
    public void NewToken_ReturnsUrlSafeToken_Of43Characters()
    {
        // Act
        string token = QrTokenHelper.NewToken();

        // Assert
        Assert.Equal(43, token.Length);
        Assert.Matches(new Regex("^[A-Za-z0-9_-]+$"), token);
    }

    [Fact(DisplayName = "QrTokenHelper.NewToken - каждый вызов возвращает уникальный токен")]
    public void NewToken_ReturnsUniqueTokens()
    {
        // Act
        var tokens = Enumerable.Range(0, 1000).Select(_ => QrTokenHelper.NewToken()).ToList();

        // Assert
        Assert.Equal(tokens.Count, tokens.Distinct().Count());
    }

    [Fact(DisplayName = "QrTokenHelper.Hash - возвращает SHA-256 в hex (64 символа) и детерминирован")]
    public void Hash_ReturnsDeterministicSha256Hex()
    {
        // Act
        string first = QrTokenHelper.Hash("abc");
        string second = QrTokenHelper.Hash("abc");

        // Assert
        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
        Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", first);
    }

    [Fact(DisplayName = "QrTokenHelper.Hash - разные токены дают разные хэши")]
    public void Hash_ReturnsDifferentHashes_ForDifferentTokens()
    {
        Assert.NotEqual(QrTokenHelper.Hash("token-1"), QrTokenHelper.Hash("token-2"));
    }
}