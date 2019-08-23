using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace exam.Models
{
  public class Acty
  {
    [Key]
    public int ActyId {get;set;}
    public int CreatorId {get;set;}

    [Required(ErrorMessage="Required")]
    [Display(Name="Title")]
    [MinLength(2, ErrorMessage="Minimum 2 characters")]
    public string Title {get;set;}

    [Required(ErrorMessage="Required")]
    [Display(Name="Start Date")]
    [DataType(DataType.Date)]
    [DateChecker]
    public DateTime Date {get;set;}

    [Required(ErrorMessage="Required")]
    [Display(Name="Time")]
    [DataType(DataType.Time)]
    public DateTime Time {get;set;}

    [Required(ErrorMessage="Required")]
    [Display(Name="Duration")]
    [Range(1,9999, ErrorMessage="Must be greater than 0")]
    public int Duration {get;set;}
    
    [Required(ErrorMessage="Required")]
    [Display(Name="Duration Metric")]
    [DurationMetricChecker]
    public string DurationMetric {get;set;}

    [Required(ErrorMessage="Required")]
    [Display(Name="Description")]
    [MinLength(10, ErrorMessage="Minimum 10 characters")]
    public string Description {get;set;}

    public DateTime CreatedAt {get;set;} = DateTime.Now;
    public DateTime UpdatedAt {get;set;} = DateTime.Now;

    public List<Participant> Attendees {get;set;}
    public UserModel Creator {get;set;}

    [NotMapped]
    public bool IsCreator {get;set;}
    [NotMapped]
    public bool IsAttending {get;set;}
  }

  public class DurationMetricChecker : ValidationAttribute
  {
    private List<string> _dm = new List<string> {"Minutes", "Hours", "Days"};
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      foreach(string check in _dm)
      {
        if (check == value.ToString())
          return ValidationResult.Success;
      }
      return new ValidationResult("Please choose from the given list");
    }
  }
  public class DateChecker : ValidationAttribute
  {
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      DateTime dateToCheck = (DateTime)value;
      if (dateToCheck < DateTime.Now.Date)
      {
        return new ValidationResult("Date must be in the future");
      }
      return ValidationResult.Success;
    }
  }
}