/*
============================================================
File: ProductionJobStepHistoryConfiguration.cs

Purpose:
Configures ProductionJobStepHistory for Entity Framework Core.

Responsibilities:
- Map Production Step execution history.
- Configure Production Job Step relationship.
- Configure Status transition fields.
- Configure Machine snapshots.
- Configure Quantity snapshots.
- Configure Remarks and audit information.
- Support chronological history queries.

Important:
- History records are append-only.
- Machine Code and Name are stored as snapshots.
- MachineId is retained as an optional reference value.
- Historical information must remain understandable even if
  Master data changes later.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ProductionJobStepHistoryConfiguration
        : IEntityTypeConfiguration<ProductionJobStepHistory>
    {
        public void Configure(
            EntityTypeBuilder<ProductionJobStepHistory> builder)
        {
            #region Table

            builder.ToTable(
                "ProductionJobStepHistories");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Production Job Step Relationship

            builder.Property(x =>
                    x.ProductionJobStepId)
                .IsRequired();

            #endregion


            #region Status Transition

            builder.Property(x =>
                    x.NewStatus)
                .IsRequired();

            #endregion


            #region Machine Snapshot

            builder.Property(x =>
                    x.MachineCode)
                .HasMaxLength(50);


            builder.Property(x =>
                    x.MachineName)
                .HasMaxLength(200);

            #endregion


            #region Quantity Snapshot

            builder.Property(x =>
                    x.GoodQuantity)
                .HasPrecision(
                    18,
                    3);


            builder.Property(x =>
                    x.RejectedQuantity)
                .HasPrecision(
                    18,
                    3);

            #endregion


            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Audit

            builder.Property(x =>
                    x.ChangedOn)
                .IsRequired();


            builder.Property(x =>
                    x.ChangedBy)
                .IsRequired()
                .HasMaxLength(150);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                    new
                    {
                        x.ProductionJobStepId,
                        x.ChangedOn
                    });


            builder.HasIndex(x =>
                x.MachineId);

            #endregion
        }
    }
}