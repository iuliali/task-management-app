using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    [Authorize(Roles = "Admin")]
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

        public IActionResult AdministrationPage()
        {
            SetAccessRights();

            //a link to projects
            //a link to teams
            //a link to users index page

            return View();
        }
        public IActionResult Index()
        {
            SetAccessRights();

            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;

            return View();
        }

        public async Task<ActionResult> Show(string id)
        {
            SetAccessRights();

            ApplicationUser user = db.Users.Find(id);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;

            return View(user);
        }

        public async Task<ActionResult> Edit(string id)
        {
            SetAccessRights();

            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user); 

            var currentUserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Id)
                                              .First(); 
            ViewBag.UserRole = currentUserRole;

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            SetAccessRights();

            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();


            if (ModelState.IsValid)
            {
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.PhoneNumber = newData.PhoneNumber;


                var roles = db.Roles.ToList();

                foreach (var role in roles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                var roleName = await _roleManager.FindByIdAsync(newRole);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                db.SaveChanges();

            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Delete(string id)
        {
            SetAccessRights();
            var user = db.Users.Find(id);
            // Delete user Objects

            //delete comments
            var comments = db.Comments.Where(c => c.UserId == user.Id).ToList();
            db.Comments.RemoveRange(comments);
            //delete tasks

            var tasks = db.Tasks.Where(t => t.UserId == user.Id).ToList();

            foreach (var task in tasks)
            {

                var comms = db.Comments.Where(c => c.TaskId == task.Id).ToList();

                foreach (var comment in comms)
                {
                    db.Comments.Remove(comment);
                }
                db.Tasks.Remove(task);


            }

            //db.Tasks.RemoveRange(tasks);

            //delete teammember
            var teammembers = db.TeamMembers.Where(t => t.ApplicationUserId == user.Id).ToList();
            db.TeamMembers.RemoveRange(teammembers);

            //delete teams if organizer 
            var teams = db.Teams.Include("Project").Where(t => t.Project.UserId == user.Id);
            db.Teams.RemoveRange(teams);

            //delete projects
            
            var projects = db.Projects.Include("Tasks").Where(p=>p.UserId == user.Id);
            foreach (var project in projects)
            {
                var tasks_proj = db.Tasks.Where(t => t.ProjectId == project.Id).ToList();
                db.Tasks.RemoveRange(tasks_proj);
            }

            db.Projects.RemoveRange(projects);

            //delete user
            db.Users.Remove(user);
            db.SaveChanges();


            return RedirectToAction("Index");
        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");

            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        [NonAction]
        public void SetTempDataMessage(string message, string style)
        {
            TempData["Message"] = message;
            TempData["MessageStyle"] = style;


        }

    }
}

