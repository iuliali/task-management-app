using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Authorize]
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
            var users = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).ToList();
            ViewBag.Users = users;

            var project = db.Projects.Include("Team")
              .Where(p => p.Id == id)
              .First();
            ViewBag.Project = project;


            var team = db.Teams.Where(t => t.ProjectId == id).FirstOrDefault();
            ViewBag.Team = team;
            if (team != null)
            {// nu e null -> exista exhipa
             // si tb sa verific daca are membrii ca sa stiu daca afisez sau nu formular pt adaugare membrii
                ViewBag.Users = GetAllUsersExceptTeammembers(team.Id);
                ViewBag.TeamMembers = GetAllTeammembers(team.Id);
            }
            else
            {// nu exista echipa dar nu afisez formularul pt adaugare membrii 
                ViewBag.TeamMembers = new List<ApplicationUser>();
                ViewBag.Users = new List<ApplicationUser>();
                ViewBag.TeamMembers = new List<ApplicationUser>();
            }

            var tasks = db.Tasks.Include("User").Where(t => t.ProjectId == id);
            ViewBag.Tasks = tasks;


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
            ViewBag.Users = GetAllUsersExceptTeammembers(teamMember.TeamId);
            ViewBag.TeamMembers = GetAllTeammembers(teamMember.TeamId);
            if (ModelState.IsValid)
            {
                db.TeamMembers.Add(teamMember);
                db.SaveChanges();
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

                return View(team);

            }

        }



        //TODO -> 2 * NEW METHOD 
        [HttpGet]
        public IActionResult New()
        {
            
            Project project = new Project();

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
        [NonAction]
        public List<ApplicationUser> GetAllUsersExceptTeammembers(int? team_id)
        {

            var team_proj = db.Teams.Include("Project").Where(t=> t.Id==team_id).First(); 
            var organizer = db.Users.Where(u => u.Id == team_proj.Project.UserId).First();

            //aici mai tb verificat rolul de organizator si daca proiectul e finalizat
            // ar trebui sa am si membrii care pot fi adaugati 
            var users_to_be_added = from user in db.Users select user;
            var teammembers = from user in db.Users
                              join member in db.TeamMembers
                              on user.Id equals member.ApplicationUserId
                              where member.TeamId == team_id || user.Id == organizer.Id
                              select user;
                                      
            users_to_be_added = users_to_be_added.Except(teammembers);

            var list = users_to_be_added.ToList();
            return list;

        }
        [NonAction]
        public List<ApplicationUser> GetAllTeammembers(int? team_id)
        {
            var team_proj = db.Teams.Include("Project").Where(t => t.Id == team_id).First();
            var organizer = db.Users.Where(u => u.Id == team_proj.Project.UserId).First();

            var user_in_this_team = from user in db.Users
                                    join member in db.TeamMembers
                                    on user.Id equals member.ApplicationUserId
                                    where member.TeamId == team_id || user.Id == organizer.Id
                                    select user
                                      ;
            return user_in_this_team.ToList();
        }




    }
}
