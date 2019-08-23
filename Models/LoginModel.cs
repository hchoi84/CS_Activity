using System.ComponentModel.DataAnnotations;

namespace exam.Models
{
  public class LoginModel
  {
    [Required(ErrorMessage="Required")]
    [Display(Name="Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage="Required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
  }
}