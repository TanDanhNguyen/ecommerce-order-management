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
    public class IdempotencyRecordConfiguration
        : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("IdempotencyRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.ResponseBody)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.HasIndex(x => new { x.UserId, x.Key })
                .IsUnique();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ExpiresAtUtc)
                .IsRequired();
        }
    }
}
