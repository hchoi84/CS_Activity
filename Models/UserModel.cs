using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace exam.Models
{
  public class UserModel
  {
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage="Required")]
    [Display(Name="First Name")]
    [MinLength(2, ErrorMessage="Minimum 2 characters")]
    public string FirstName { get; set; }

    [Required(ErrorMessage="Required")]
    [Display(Name="Last Name")]
    [MinLength(2, ErrorMessage="Minimum 2 characters")]
    public string LastName { get; set; }

    [Required(ErrorMessage="Required")]
    [Display(Name="Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage="Required")]
    [Display(Name="Password", Prompt="8-20, a-z, A-Z, 0-9, special")]
    [MinLength(7, ErrorMessage="Minimum 8 characters")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=(.*[a-z]))(?=.*[A-Z])(?=(.*[\d]))(?=(.*[\W]))(?!.*\s).{8,20}$", 
                      ErrorMessage="8-20 characters and minimum 1 of each (lower, upper, number, and special character)")]
    public string Password { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [NotMapped]
    [Required(ErrorMessage="Required")]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string Confirm { get; set; }
  }
}