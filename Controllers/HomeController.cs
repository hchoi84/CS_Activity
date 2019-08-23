using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using exam.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace exam.Controllers
{
  public class HomeController : Controller
  {
    private MyContext dbContext;
    public HomeController(MyContext context) => dbContext = context;
    private int? _uid
    {
      get { return HttpContext.Session.GetInt32("uid"); }
      set { HttpContext.Session.SetInt32("uid", (int)value); }
    }
    private string _tempMsg
    {
      get { return HttpContext.Session.GetString("TempMsg"); }
      set { HttpContext.Session.SetString("TempMsg", value); }
    }

    [HttpGet("")]
    public IActionResult Index()
    {
      if(_uid == null)
        ViewBag.LogoutBtn = "disable";
      ViewBag.TempMsg = _tempMsg;
      return View();
    }
    [HttpGet("home")]
    public IActionResult Home()
    {
      if(!isLoggedIn())
        return RedirectToAction("Index");
      return View(AllActiveActivities());
    }
    [HttpGet("info/{id}")]
    public IActionResult Info(int id)
    {
      if(!isLoggedIn())
        return RedirectToAction("Index");
      Acty acty = dbContext.Activities
        .Include(o => o.Attendees).ThenInclude(a => a.Attendee)
        .Include(o => o.Creator)
        .FirstOrDefault(o => o.ActyId == id);
      CreatorOrAttending(acty);
      return View(acty);
    }

    [HttpGet("activity/create")]
    public IActionResult ActivityCreateForm() => View("New");
    [HttpPost("activity/create")]
    public IActionResult CreateActivity(Acty newActy)
    {
      if(ModelState.IsValid)
      {
        bool activityExists = dbContext.Activities.Any(o => o.Title == newActy.Title);
        if(activityExists)
        {
          ModelState.AddModelError("Title", "Title already exists");
          return View();
        }
        if(HasConflict(newActy))
          return View("New");
        newActy.CreatorId = (int)_uid;
        var createdActivity = dbContext.Activities.Add(newActy);
        dbContext.SaveChanges();
        return RedirectToAction("Info", new { id = createdActivity.Entity.ActyId });
      }
      return View("New");
    }
    [HttpGet("activity/join/{id}")]
    public IActionResult Join(int id)
    {
      Acty toJoin = dbContext.Activities.FirstOrDefault(o => o.ActyId == id);
      if(HasConflict(toJoin))
        return View("Home", AllActiveActivities());

      Participant newParticipant = new Participant();
      newParticipant.UserId = (int)_uid;
      newParticipant.ActyId = id;
      dbContext.Participants.Add(newParticipant);
      dbContext.SaveChanges();
      return RedirectToAction("Home");
    }
    [HttpGet("activity/leave/{id}")]
    public IActionResult Leave(int id)
    {
      Participant deleteOH = dbContext.Participants
        .FirstOrDefault(oh => oh.ActyId == id && oh.UserId == _uid);
      dbContext.Participants.Remove(deleteOH);
      dbContext.SaveChanges();
      return RedirectToAction("Home");
    }
    [HttpGet("activity/delete/{id}")]
    public IActionResult Delete(int id)
    {
      Acty deleteActivity = dbContext.Activities
        .Include(o => o.Attendees)
        .FirstOrDefault(o => o.ActyId == id);
      foreach(var attendee in deleteActivity.Attendees)
      {
        dbContext.Participants.Remove(attendee);
      }
      dbContext.Activities.Remove(deleteActivity);
      dbContext.SaveChanges();
      return RedirectToAction("Home");
    }

    [HttpPost("user/create")]
    public IActionResult CreateUser(LogRegModel newUser)
    {
      if (ModelState.IsValid)
      {
        UserModel emailCheck = dbContext.Users.FirstOrDefault(user => user.Email == newUser.User.Password);
        if (emailCheck == null)
        {
          PasswordHasher<UserModel> Hasher = new PasswordHasher<UserModel>();
          newUser.User.Password = Hasher.HashPassword(newUser.User, newUser.User.Password);
          var nu = dbContext.Users.Add(newUser.User);
          dbContext.SaveChanges();
          _uid = nu.Entity.UserId;
          HttpContext.Session.SetString("UserName", newUser.User.FirstName);
          return RedirectToAction("Home");
        }
        ModelState.AddModelError("User.Email", "Email already exists");
        return View("Index");
      }
      return View("Index");
    }
    [HttpPost("user/login")]
    public IActionResult LoginUser(LogRegModel loginUser)
    {
      if (ModelState.IsValid)
      {
        UserModel emailCheck = dbContext.Users.FirstOrDefault(user => user.Email == loginUser.Login.Email);
        if (emailCheck != null)
        {
          PasswordHasher<LoginModel> Hasher = new PasswordHasher<LoginModel>();
          var result = Hasher.VerifyHashedPassword(loginUser.Login, emailCheck.Password, loginUser.Login.Password);
          if (result != 0)
          {
            _uid = emailCheck.UserId;
            return RedirectToAction("Home");
          }
        }
        ModelState.AddModelError("Login.Email", "Email/Password is incorrect");
        return View("Index");
      }
      return View("Index");
    }
    [HttpGet("logout")]
    public IActionResult Logout()
    {
      HttpContext.Session.Clear();
      _tempMsg = "Goodbye!";
      return RedirectToAction("Index");
    }

		public bool isLoggedIn()
    {
      if(_uid == null)
      {
        _tempMsg = "Please login or register";
        return false;
      }
      return true;
    }
    
    public List<Acty> AllActiveActivities()
    {
      List<Acty> AAA = dbContext.Activities
        .Include(o => o.Creator)
        .Include(o => o.Attendees)
        .ThenInclude(a => a.Attendee)
        .Where(o => (o.Date + o.Time.TimeOfDay) > DateTime.Now)
        .OrderBy(o => o.Date).ThenBy(o => o.Time)
        .ToList();
      foreach (var activity in AAA)
      {
        CreatorOrAttending(activity);
      }
      return AAA;
    }
    public void CreatorOrAttending(Acty acty)
    {
      if(acty.CreatorId == _uid)
          acty.IsCreator = true;
      else if(acty.Attendees.Any(a => a.UserId == _uid))
        acty.IsAttending = true;
    }
		
    public bool HasConflict(Acty actyToCompare)
    {
      List<Acty> joinedActivities = dbContext.Activities
        .Include(o => o.Attendees)
        .ThenInclude(a => a.Attendee)
        .Where(o => o.Attendees.Any(a => a.UserId == _uid))
        .ToList();

      DateTime actyToCompareStart = actyToCompare.Date + actyToCompare.Time.TimeOfDay;
      DateTime actyToCompareEnd = CalculateEndDateTime(actyToCompareStart, actyToCompare.DurationMetric, actyToCompare.Duration);

      foreach(var jo in joinedActivities)
      {
        DateTime joinedStart =  jo.Date + jo.Time.TimeOfDay;
        DateTime joinedEnd = CalculateEndDateTime(joinedStart, jo.DurationMetric, jo.Duration);
        
        if(actyToCompareStart < joinedEnd && actyToCompareEnd > actyToCompareStart)
        {
          ViewBag.Conflict = $"There's a conflict with {jo.Title}";
          return true;
        }
      }
      return false;
    }
    public DateTime CalculateEndDateTime(DateTime start, string durationMetric, double duration)
    {
      DateTime end = new DateTime();
      if(durationMetric == "Days")
        end = start.AddDays(duration);
      else if(durationMetric == "Hours")
        end = start.AddHours(duration);
      else if(durationMetric == "Minutes")
        end = start.AddMinutes(duration);
      return end;
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }

	public static class SessionExtensions
  {
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
      session.SetString(key, JsonConvert.SerializeObject(value));
    }
    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
      string value = session.GetString(key);
      return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }
  }
}
