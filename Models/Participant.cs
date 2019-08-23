using System.ComponentModel.DataAnnotations;

namespace exam.Models
{
  public class Participant
  {
    [Key]
    public int ParticipantId {get;set;}
    public int UserId {get;set;}
    public int ActyId {get;set;}

    public UserModel Attendee {get;set;}
    public Acty Attending {get;set;}
  }
}