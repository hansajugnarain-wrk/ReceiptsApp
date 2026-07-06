using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ReceiptsApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptsApp.Infrastructure.Persistence
{
    public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
    {
        public UsersDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<UsersDbContext>();

            options.UseSqlServer(
                "Server=localhost,1433;Database=UsersDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;");
            return new UsersDbContext(options.Options);
        }
    }
}
