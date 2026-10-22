using ECommerce.OrderManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.OrderManagement.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(i => i.ProductSku)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Ignore(i => i.LineTotal);

            builder.HasIndex(i => i.ProductId);
        }
    }
}
