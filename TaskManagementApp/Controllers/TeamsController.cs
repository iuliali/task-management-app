using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext db;
        public TeamsController(ApplicationDbContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            var teams = db.Teams;
            ViewBag.Teams = teams;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }
        public IActionResult Show(int id)
        {
            var members = from user in db.ApplicationUsers
                        //join tm in db.TeamMembers on user.Id equals  tm.ApplicationUserId
                        //where tm.TeamId == id
                        select new { UserId = user.Id };




            //var members = db.TeamMembers.Include("Team").Where(t => t.TeamId == id);

            ViewBag.Members = members;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }
    }
}