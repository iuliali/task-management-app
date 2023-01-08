using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Authorize]

    public class UsersController : Controller
    {
        
            private readonly ApplicationDbContext db;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            public UsersController(
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

            return View();
        }

        public IActionResult Show(string? id)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                SetTempDataMessage("User cannot be found!!", "alert-danger");

                return View("Error2");
            }

            if(_userManager.GetUserId(User) != id && !User.IsInRole("Admin"))
            {
                SetTempDataMessage("You don't have rights to access user page", "alert-danger");

                return View("Error2");
            }




            return View();
        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");

            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }

        [NonAction]
        private void SetTempDataMessage(string message, string style)
        {
            TempData["Message"] = message;
            TempData["MessageStyle"] = style;
        }
    }
}
