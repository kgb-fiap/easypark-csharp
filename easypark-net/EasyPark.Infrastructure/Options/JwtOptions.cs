namespace EasyPark.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "EasyPark.Api";
    public string Audience { get; set; } = "EasyPark.Client";
    public string SecretKey { get; set; } = "easypark-super-secret-key-change-me";
    public int ExpirationMinutes { get; set; } = 120;
}
