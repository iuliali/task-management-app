using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProjectsController(
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
            var projects = db.Projects.Include("User");
            ViewBag.Projects = projects;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }


            return View();
        }

        public IActionResult Show(int id)
        {
            //var project = db.Projects.Where(p => p.Id == id).First();

            var users = db.Users.Include("TeamMembers").Where(u => u.Id == _userManager.GetUserId(User));
            ViewBag.Users = users.ToList();
            ViewBag.AddMembersForm = false;
            var team = db.Teams.Where(t => t.ProjectId == id).FirstOrDefault();
            ViewBag.Team = team;
            if (team != null)
            {// nu e null -> exista exhipa
             // si tb sa verific daca are membrii ca sa stiu daca afisez sau nu formular pt adaugare membrii
                var teammembers = db.TeamMembers.Include("ApplicationUser").Where(tm => tm.TeamId == team.Id);

                ViewBag.TeamMembers = teammembers.ToList();

                //aici mai tb verificat rolul de organizator si daca proiectul e finalizat
                ViewBag.AddMembersForm = true;
                // ar trebui sa am si membrii care pot fi adaugati 

                var users_to_be_added = from user in db.Users select user;
                var user_in_this_team = from user in db.Users
                                        join member in db.TeamMembers
                                        on user.Id equals member.ApplicationUserId
                                        where member.TeamId == team.Id select user;
                users_to_be_added = users_to_be_added.Except(user_in_this_team);
                ViewBag.Users = users_to_be_added;
     
            }
            else
            {// nu exista echipa dar nu afisez formularul pt adaugare membrii 
                ViewBag.TeamMembers = null; 

                ViewBag.AddMembersForm = false;

            }
            var members = from teamm in db.Teams
                          join projectt in db.Projects on team.ProjectId equals projectt.Id
                          join member in db.TeamMembers on team.Id equals member.TeamId
                          where projectt.Id == id
                          select member;
            var members2 = from member in db.TeamMembers
                           where member.TeamId == team.Id && team.ProjectId == id
                           select member;
            ViewBag.Members = members2;

            var tasks = db.Tasks.Include("User").Where(t => t.ProjectId == id);
            ViewBag.Tasks = tasks;

            var project = db.Projects.Include("Team")
                         .Where(p => p.Id == id)
                         .First();
            ViewBag.Project = project;


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }

        [HttpPost]
        public IActionResult AddMember([FromForm] TeamMember teamMember)
        {
            var project = db.Teams.Where(t => t.Id == teamMember.TeamId).First();

            if (ModelState.IsValid)
            {
                db.TeamMembers.Add(teamMember);
                db.SaveChanges();
            }
            else
            {
                var users = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).ToList();
                ViewBag.Users = users;
                //SetAccessRights();
            }
            return Redirect("/Projects/Show/" + project.ProjectId);

        }

        [HttpPost]
        public IActionResult Show([FromForm] Team team)
        {
            if (ModelState.IsValid)
            {
                db.Teams.Add(team);
                db.SaveChanges();
                return Redirect("/Projects/Show/" + team.ProjectId);
            }
            else
            {
                Project project = db.Projects.Include("Team")
                                         .Where(p => p.Id == team.ProjectId)
                                         .First();

                var users = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).ToList();
                ViewBag.Users = users;


                return Redirect("/Projects/Show/" + team.ProjectId);
            }
        }
        [HttpPost]
        public IActionResult AddTask([FromForm] Task task)
        {
            task.CreatedDate = DateTime.Now;
            task.status = "Not started";
            if (ModelState.IsValid)
            {
                db.Tasks.Add(task);
                db.SaveChanges();
                TempData["message"] = "Task has been added";
            }
            else
            {
                TempData["message"] = ":((";
                TempData["messageType"] = "alert-danger";

            }
            return Redirect("/Projects/Show/" + task.ProjectId);
        }

        

        //TODO -> 2 * NEW METHOD 
        [HttpGet]
        public IActionResult New()
        {
            
            Project project = new Project();
            ViewBag.User = _userManager.GetUserId(User);
            
            return View(project);
        }

        [HttpPost]
        public IActionResult New(Project project)
        {

            project.CreatedDate = DateTime.Now;
            project.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                TempData["message"] = "Project successfully added";
               

                return RedirectToAction("Show", "Projects", new { id = project.Id });
            }
            else
            {
                return View(project);
            }
        }

    }
}
