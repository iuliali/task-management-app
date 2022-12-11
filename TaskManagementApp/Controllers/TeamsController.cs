using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TeamsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
            /*
            var members = from user in db.Users
                        join tm in db.TeamMembers on user.Id equals  tm.ApplicationUserId
                        where tm.TeamId == id
                        select user.FirstName; // atentie la metoda asta nu se trimit obiecte in view, ci direct strings
            */


            var members = db.TeamMembers.Include("ApplicationUser").Where(m=>m.TeamId == id);
            var teamneame = from team in db.Teams where team.Id == id select team.Name;
            ViewBag.TeamName = teamneame.First();
            ViewBag.Members = members;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }
    }
}