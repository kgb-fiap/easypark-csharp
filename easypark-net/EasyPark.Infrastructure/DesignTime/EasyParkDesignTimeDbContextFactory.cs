using EasyPark.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyPark.Infrastructure.DesignTime;

public class EasyParkDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EasyParkContext>
{
    public EasyParkContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EasyParkContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "User Id=easypark;Password=easypark;Data Source=localhost:1521/XEPDB1";

        optionsBuilder.UseOracle(connectionString);
        return new EasyParkContext(optionsBuilder.Options);
    }
}
