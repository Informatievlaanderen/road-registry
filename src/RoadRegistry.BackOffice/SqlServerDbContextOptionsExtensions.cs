using Microsoft.EntityFrameworkCore;

namespace RoadRegistry.BackOffice
{
    public static class SqlServerDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder UseRoadRegistryInMemorySqlServer(this DbContextOptionsBuilder optionsBuilder)
        {
            return optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
        }
    }
}
