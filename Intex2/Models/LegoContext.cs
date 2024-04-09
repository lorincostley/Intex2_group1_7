using Microsoft.EntityFrameworkCore;

namespace Intex2.Models
{
    public class LegoContext : DbContext

    {
        public LegoContext(DbContextOptions<LegoContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }
    }
}
