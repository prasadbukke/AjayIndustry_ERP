/*
============================================================
File: DeliveryChallanConfiguration.cs

Purpose:
Configures DeliveryChallan entity for Entity Framework Core.

Responsibilities:
- Configure Delivery Challan table.
- Configure Challan identification and status.
- Configure Customer reference and editable address snapshot.
- Configure complete Customer Master JSON snapshot.
- Configure Company / Workshop reference.
- Configure complete Company Master JSON snapshot.
- Configure L.P.G. No.
- Configure dispatch / transport information.
- Configure Finalization information.
- Configure Delivery Challan Items relationship.
- Configure indexes and constraints.

Important:
- CustomerSnapshotJson and CompanySnapshotJson are stored as
  nvarchar(max) for extensible historical master snapshots.
- Editable Customer address is stored in dedicated columns.
- Company / Customer snapshots are historical data and are
  not EF navigation properties.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class DeliveryChallanConfiguration
        : IEntityTypeConfiguration<DeliveryChallan>
    {
        #region Configure

        public void Configure(
            EntityTypeBuilder<DeliveryChallan> builder)
        {
            #region Table

            builder.ToTable(
                "DeliveryChallans");

            #endregion


            #region Primary Key

            builder.HasKey(
                x => x.Id);

            #endregion


            #region Identification

            builder.Property(
                    x => x.Code)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.HasIndex(
                    x => x.Code)
                .IsUnique();


            builder.Property(
                    x => x.ChallanDate)
                .IsRequired();


            builder.Property(
                    x => x.Status)
                .IsRequired();

            #endregion


            #region LPG Information

            builder.Property(
                    x => x.LpgNumber)
                .HasMaxLength(
                    100);

            #endregion


            #region Customer Reference

            builder.Property(
                    x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(
                    250);


            builder.HasIndex(
                x => x.CustomerId);

            #endregion


            #region Customer Editable Address Snapshot

            builder.Property(
                    x => x.CustomerAddressLine1)
                .HasMaxLength(
                    500);


            builder.Property(
                    x => x.CustomerAddressLine2)
                .HasMaxLength(
                    500);


            builder.Property(
                    x => x.CustomerCity)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.CustomerDistrict)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.CustomerState)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.CustomerPincode)
                .HasMaxLength(
                    20);


            builder.Property(
                    x => x.CustomerCountry)
                .HasMaxLength(
                    100);

            #endregion


            #region Customer Master Snapshot

            /*
             * Complete Customer Master snapshot.
             *
             * nvarchar(max) allows future Customer Master
             * fields to be included without changing the
             * Delivery Challan schema for every new field.
             */

            builder.Property(
                    x => x.CustomerSnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");

            #endregion


            #region Company Workshop Reference

            builder.Property(
                    x => x.CompanyName)
                .HasMaxLength(
                    250);


            builder.HasIndex(
                x => x.CompanyId);

            #endregion


            #region Company Master Snapshot

            /*
             * Complete Company / Workshop Master snapshot.
             *
             * Stored as JSON for future extensibility.
             */

            builder.Property(
                    x => x.CompanySnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");

            #endregion


            #region Dispatch Information

            builder.Property(
                    x => x.TransporterName)
                .HasMaxLength(
                    250);


            builder.Property(
                    x => x.VehicleNumber)
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.TransportReference)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.DispatchFrom)
                .HasMaxLength(
                    250);


            builder.Property(
                    x => x.Destination)
                .HasMaxLength(
                    250);

            #endregion


            #region Remarks

            builder.Property(
                    x => x.Remarks)
                .HasMaxLength(
                    2000);

            #endregion


            #region Finalization

            builder.Property(
                    x => x.FinalizedBy)
                .HasMaxLength(
                    150);

            #endregion


            #region Delivery Challan Items

            builder.HasMany(
                    x => x.Items)
                .WithOne(
                    x => x.DeliveryChallan)
                .HasForeignKey(
                    x => x.DeliveryChallanId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Indexes

            builder.HasIndex(
                x => x.ChallanDate);


            builder.HasIndex(
                x => x.Status);


            builder.HasIndex(
                x => new
                {
                    x.CustomerId,
                    x.ChallanDate
                });

            #endregion
        }

        #endregion
    }
}