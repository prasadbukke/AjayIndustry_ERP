/*
============================================================
File: ProductionJobStepConfiguration.cs

Purpose:
Configures ProductionJobStep for Entity Framework Core.

Production Structure:

Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Map executable Production Job Steps.
- Configure Production Job Item relationship.
- Configure Production Operation relationship.
- Configure Default Machine relationship.
- Configure Actual Assigned Machine relationship.
- Configure estimated time and quantity precision.
- Configure Routing snapshot and execution fields.
- Prevent duplicate active Sequence Numbers inside one
  Production Job Item Pipeline.

Important:
- Same Production Operation may appear multiple times.
- Sequence Number must be unique within one active
  Production Job Item Step set.
- Default Machine is copied from Routing.
- Assigned Machine represents actual shop-floor execution.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ProductionJobStepConfiguration
        : IEntityTypeConfiguration<ProductionJobStep>
    {
        public void Configure(
            EntityTypeBuilder<ProductionJobStep> builder)
        {
            #region Table

            builder.ToTable(
                "ProductionJobSteps");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Production Job Item Relationship

            builder.Property(x =>
                    x.ProductionJobItemId)
                .IsRequired();


            builder.HasOne(x =>
                    x.ProductionJobItem)
                .WithMany(x =>
                    x.Steps)
                .HasForeignKey(x =>
                    x.ProductionJobItemId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Sequence

            builder.Property(x =>
                    x.SequenceNumber)
                .IsRequired();


            /*
             * Sequence Number is unique only inside one
             * Production Job Item Pipeline.
             *
             * Example:
             *
             * Item A:
             * 10 Cutting
             * 20 Drilling
             *
             * Item B:
             * 10 Cutting
             * 20 Turning
             *
             * Both Items may independently use Sequence 10.
             */
            builder.HasIndex(x =>
                    new
                    {
                        x.ProductionJobItemId,
                        x.SequenceNumber
                    })
                .IsUnique()
                .HasFilter(
                    "[IsDeleted] = 0");

            #endregion


            #region Production Operation

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


            builder.Property(x =>
                    x.OperationCode)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(x =>
                    x.OperationName)
                .IsRequired()
                .HasMaxLength(150);


            builder.Property(x =>
                    x.OperationType)
                .IsRequired();

            #endregion


            #region Default Machine

            builder.HasOne(x =>
                    x.DefaultMachine)
                .WithMany()
                .HasForeignKey(x =>
                    x.DefaultMachineId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Assigned Machine

            builder.HasOne(x =>
                    x.AssignedMachine)
                .WithMany()
                .HasForeignKey(x =>
                    x.AssignedMachineId)
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


            #region Routing Snapshot

            builder.Property(x =>
                    x.OperationInstruction)
                .HasMaxLength(1000);


            builder.Property(x =>
                    x.RoutingRemarks)
                .HasMaxLength(1000);

            #endregion


            #region Step Status

            builder.Property(x =>
                    x.Status)
                .IsRequired();

            #endregion


            #region Production Quantity

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


            #region Execution Remarks

            builder.Property(x =>
                    x.ExecutionRemarks)
                .HasMaxLength(1000);

            #endregion


            #region Step History

            builder.HasMany(x =>
                    x.History)
                .WithOne(x =>
                    x.ProductionJobStep)
                .HasForeignKey(x =>
                    x.ProductionJobStepId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.ProductionOperationId);


            builder.HasIndex(x =>
                x.DefaultMachineId);


            builder.HasIndex(x =>
                x.AssignedMachineId);


            builder.HasIndex(x =>
                x.Status);


            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}