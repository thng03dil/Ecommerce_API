using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.HasIndex(x => x.TokenHash); 

            builder.Property(x => x.ExpiryDate)
                   .IsRequired();

            builder.Property(x => x.IsRevoked)
                   .IsRequired();
        }
    }
}
