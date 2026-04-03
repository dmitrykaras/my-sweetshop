using my_sweetshop.Views.Auth;

namespace my_sweetshop.Services;

public class CodePageFactory
{
    private readonly AuthApi _authApi;
    private readonly AuthSession _session;

    public CodePageFactory(AuthApi authApi, AuthSession session)
    {
        _authApi = authApi;
        _session = session;
    }

    public CodePage Create(string email) => new CodePage(_authApi, _session, email);
}