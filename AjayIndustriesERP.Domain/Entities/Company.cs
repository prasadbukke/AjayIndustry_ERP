using System.ComponentModel.DataAnnotations;
using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class Company : BaseEntity
    {
        [Key]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CompanyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string GstNumber { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? PanNumber { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(100)]
        public string Country { get; set; } = "India";

        [MaxLength(10)]
        public string? PostalCode { get; set; }
    }
}