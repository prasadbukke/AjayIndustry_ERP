/*
============================================================
File: ItemProcessRoutingStepConfiguration.cs

Purpose:
Configures ItemProcessRoutingStep for Entity Framework Core.

Responsibilities:
- Map Routing Steps to database.
- Configure Routing relationship.
- Configure Production Operation relationship.
- Configure optional Default Machine relationship.
- Configure Setup and Cycle Time precision.
- Prevent duplicate Sequence Numbers inside one Routing.
- Configure instructions and remarks.

Important:
- Same Operation may appear multiple times in one Routing.
- Only SequenceNumber must be unique within a Routing.
- Default Machine is optional.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ItemProcessRoutingStepConfiguration
        : IEntityTypeConfiguration<ItemProcessRoutingStep>
    {
        public void Configure(
            EntityTypeBuilder<ItemProcessRoutingStep> builder)
        {
            #region Table

            builder.ToTable(
                "ItemProcessRoutingSteps");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Routing Relationship

            builder.Property(x =>
                    x.ItemProcessRoutingId)
                .IsRequired();

            #endregion


            #region Sequence

            builder.Property(x =>
                    x.SequenceNumber)
                .IsRequired();


            builder.HasIndex(x =>
        new
        {
            x.ItemProcessRoutingId,
            x.SequenceNumber
        })
    .IsUnique()
    .HasFilter(
        "[IsDeleted] = 0");

            #endregion


            #region Production Operation Relationship

            builder.Property(x =>
                    x.ProductionOperationId)
                .IsRequired();


            builder.HasOne(x =>
                    x.ProductionOperation)
                .WithMany()
                .HasForeignKey(x =>
                    x.ProductionOperationId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Default Machine Relationship

            builder.HasOne(x =>
                    x.DefaultMachine)
                .WithMany()
                .HasForeignKey(x =>
                    x.DefaultMachineId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Estimated Time

            builder.Property(x =>
                    x.SetupTimeMinutes)
                .HasPrecision(
                    18,
                    3);


            builder.Property(x =>
                    x.CycleTimeMinutes)
                .HasPrecision(
                    18,
                    3);

            #endregion


            #region Instructions

            builder.Property(x =>
                    x.OperationInstruction)
                .HasMaxLength(1000);


            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.ProductionOperationId);


            builder.HasIndex(x =>
                x.DefaultMachineId);


            builder.HasIndex(x =>
                x.IsActive);


            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}