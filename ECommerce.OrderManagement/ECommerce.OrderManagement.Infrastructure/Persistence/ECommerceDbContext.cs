using ECommerce.OrderManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderManagement.Infrastructure.Persistence
{
    public class ECommerceDbContext : DbContext
    {
        public ECommerceDbContext(
            DbContextOptions<ECommerceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ECommerceDbContext).Assembly);
        }
    }
}
