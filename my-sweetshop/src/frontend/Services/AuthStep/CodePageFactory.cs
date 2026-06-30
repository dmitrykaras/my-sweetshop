using my_sweetshop.Services.Api;
using my_sweetshop.Views.Auth;

namespace my_sweetshop.Services.AuthStep;

public class CodePageFactory
{
    private readonly AuthApi _authApi;
    private readonly AuthSession _session;

    public CodePageFactory(AuthApi authApi, AuthSession session)
    {
        _authApi = authApi;
        _session = session;
    }

    // Обёртка для удобности (передаём DI сразу в страницу обходя DI в MauiProgram.cs)
    public CodePage Create(string email) => new CodePage(_authApi, _session, email);
}