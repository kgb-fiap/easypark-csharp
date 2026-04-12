namespace EasyPark.IntegrationTests.Support;

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<EasyParkApiFactory>
{
    public const string Name = "EasyPark API";
}
