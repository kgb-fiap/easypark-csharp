using Microsoft.AspNetCore.Authentication;

namespace EasyPark.Api.Security;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-API-Key";
}
