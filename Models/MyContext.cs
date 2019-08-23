using Microsoft.EntityFrameworkCore;

namespace exam.Models
{
  public class MyContext : DbContext
  {
    public MyContext(DbContextOptions options) : base(options) { }
    public DbSet<UserModel> Users {get;set;}
    public DbSet<Participant> Participants {get;set;}
    public DbSet<Acty> Activities {get;set;}
  }
}