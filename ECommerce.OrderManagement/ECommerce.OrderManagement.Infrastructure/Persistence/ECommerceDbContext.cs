using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderManagement.Infrastructure.Persistence
{
    public class ECommerceDbContext : DbContext, IOrderUnitOfWork
    {
        public ECommerceDbContext(
            DbContextOptions<ECommerceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ECommerceDbContext).Assembly);
        }

        Task IOrderUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
