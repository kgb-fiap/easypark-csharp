using System.Diagnostics;

namespace EasyPark.Api.Observability;

public static class EasyParkTelemetry
{
    public const string ActivitySourceName = "EasyPark.Api";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
