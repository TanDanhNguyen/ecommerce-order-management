using ECommerce.OrderManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Infrastructure.Persistence.Configurations
{
    public sealed class StockMovementConfiguration
        : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(
            EntityTypeBuilder<StockMovement> builder)
        {
            builder.ToTable("StockMovements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.StockAfter)
                .IsRequired();

            builder.Property(x => x.Operation)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ProductId,
                x.CreatedAtUtc
            });

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
