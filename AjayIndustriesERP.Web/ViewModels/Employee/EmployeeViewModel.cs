/*
==============================================================

File : EmployeeViewModel.cs

Purpose :
Represents Employee View Model.

==============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Employee
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Employee Code")]
        public string? EmployeeCode { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10 digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Display(Name = "Joining Date")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Salary")]
        public decimal Salary { get; set; }

        public bool IsActive { get; set; } = true;
    }
}