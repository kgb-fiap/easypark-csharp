using EasyPark.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.UnitTests.TestSupport;

public static class TestDbContextFactory
{
    public static EasyParkContext Create()
    {
        var options = new DbContextOptionsBuilder<EasyParkContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EasyParkContext(options);
    }
}
