namespace EasyPark.Infrastructure.Options;

public class MongoOptions
{
    public const string SectionName = "Mongo";

    public string? ConnectionString { get; set; }
    public string DatabaseName { get; set; } = "easypark";
    public string AuditCollectionName { get; set; } = "audit_events";
}
