/*
============================================================
File: ProductionOperationConfiguration.cs

Purpose:
Configures ProductionOperation for Entity Framework Core.

Responsibilities:
- Map ProductionOperation to database table.
- Configure required fields and maximum lengths.
- Enforce unique Operation Code.
- Prevent duplicate active Operation Names.
- Configure indexes used by Operation Master queries.

Important:
- Deleted Operation Codes are never reused.
- Operation Name must be unique among non-deleted records.
- Setup Time, Cycle Time and Machine relationships belong to
  future Item Process Routing.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ProductionOperationConfiguration
        : IEntityTypeConfiguration<ProductionOperation>
    {
        public void Configure(
            EntityTypeBuilder<ProductionOperation> builder)
        {
            #region Table

            builder.ToTable(
                "ProductionOperations");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Operation Code

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();

            #endregion


            #region Operation Information

            builder.Property(x =>
                    x.OperationName)
                .IsRequired()
                .HasMaxLength(150);


            builder.Property(x =>
                    x.OperationType)
                .IsRequired();


            builder.Property(x =>
                    x.Description)
                .HasMaxLength(500);

            #endregion


            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Unique Operation Name

            builder.HasIndex(x =>
                    x.OperationName)
                .IsUnique()
                .HasFilter(
                    "[IsDeleted] = 0");

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.OperationType);


            builder.HasIndex(x =>
                x.IsActive);


            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}