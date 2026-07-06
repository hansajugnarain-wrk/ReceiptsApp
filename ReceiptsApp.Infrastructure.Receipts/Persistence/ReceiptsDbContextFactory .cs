using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ReceiptsApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptsApp.Infrastructure.Receipts.Persistence
{
    public class ReceiptsDbContextFactory : IDesignTimeDbContextFactory<ReceiptsDbContext>
    {
        public ReceiptsDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ReceiptsDbContext>();

            options.UseSqlServer(
                "Server=localhost,1433;Database=ReceiptsDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;");
            return new ReceiptsDbContext(options.Options);
        }
    }
}
