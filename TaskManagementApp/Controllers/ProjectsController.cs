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
using Task = TaskManagementApp.Models.Task;

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
            SetAccessRights();
            var projects = db.Projects.Include("User");
            ViewBag.Projects = projects;


            return View();
        }

       

        public IActionResult Show(int id)
        {
            SetAccessRights();
            var users = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).ToList();
            ViewBag.Users = users;

            var project = db.Projects.Include("Team")
              .Where(p => p.Id == id)
              .First();
            ViewBag.Project = project;


            var team = db.Teams.Include("Project").Where(t => t.ProjectId == id).FirstOrDefault();
            ViewBag.Team = team;

            if (team != null)
            {// nu e null -> exista exhipa
             // si tb sa verific daca are membrii ca sa stiu daca afisez sau nu formular pt adaugare membrii
                ViewBag.Users = GetAllUsersExceptTeammembers(team.Id);
                ViewBag.TeamMembers = GetAllTeammembersWithOrganizer(team.Id);
            }
            else
            {// nu exista echipa dar nu afisez formularul pt adaugare membrii 
                ViewBag.TeamMembers = new List<ApplicationUser>();
                ViewBag.Users = new List<ApplicationUser>();
                ViewBag.TeamMembers = new List<ApplicationUser>();
            }

            var members = from member in db.TeamMembers
                           where member.TeamId == team.Id && team.ProjectId == id
                           select member;
            ViewBag.Members = members;

            var tasks = db.Tasks.Include("User").Where(t => t.ProjectId == id);
            ViewBag.Tasks = tasks;

        
            return View();
        }

        [HttpPost]
        public IActionResult AddMember([FromForm] TeamMember teamMember)
        {
            SetAccessRights();

            var team = db.Teams.Where(t => t.Id == teamMember.TeamId).First();

            var project = db.Projects.Include("Team")
              .Where(p => p.Id == team.ProjectId)
              .First();
            ViewBag.Project = project;

            ViewBag.Members = GetAllTeammembersWithoutOrganizer(team.Id);

            ViewBag.Users = GetAllUsersExceptTeammembers(teamMember.TeamId);
            ViewBag.TeamMembers = GetAllTeammembersWithOrganizer(teamMember.TeamId);
            var organizer = GetProjectOrganizerByProjectId(team.ProjectId);

            if (_userManager.GetUserId(User) == organizer.Id || ViewBag.IsAdmin) //check if current user is the organizer
            {
                if (ModelState.IsValid)
                {
                    db.TeamMembers.Add(teamMember);
                    db.SaveChanges();
                    SetTempDataMessage("Member added", "alert-success");
                }
                else
                {
                    SetTempDataMessage("Member could not be added", "alert-danger");

                }
            }
            else
            {
                SetTempDataMessage("You don't have rights to add members to the team", "alert-danger");

            }

            return Redirect("/Projects/Show/" + project.Id);

        }

        [HttpPost]
        public IActionResult Show([FromForm] Team team)
        {
            SetAccessRights();
            Project project = db.Projects.Include("Team")
                         .Where(p => p.Id == team.ProjectId)
                         .First();

            var users = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).ToList();
            ViewBag.Users = users;

            if (ModelState.IsValid)
            {
                db.Teams.Add(team);
                db.SaveChanges();
                SetTempDataMessage("Team has been added", "alert-success");

                return Redirect("/Projects/Show/" + team.ProjectId);
            }
            else
            {
                SetTempDataMessage("Team could not have been added!", "alert-danger");

                return View(team);
            }
        }
        [HttpPost]
        public IActionResult AddTask([FromForm] Task task)
        {
            SetAccessRights();
            var organizer = GetProjectOrganizerByProjectId(task.ProjectId);

            task.CreatedDate = DateTime.Now;
            task.status = "Not started";
            

            if (_userManager.GetUserId(User) == organizer.Id || ViewBag.IsAdmin) //check if current user is the organizer
            {
                if (ModelState.IsValid)
                {
                    db.Tasks.Add(task);
                    db.SaveChanges();

                    SetTempDataMessage("Task has been added", "alert-success");
                }
                else
                {
                    SetTempDataMessage("Task could not have been added!", "alert-danger");
                    
                    //return View(task);

                }
            }else
            {
                SetTempDataMessage("You don't have rights to add task for this team", "alert-danger");

            }
            return Redirect("/Projects/Show/" + task.ProjectId);
        }




        //TODO -> 2 * NEW METHOD 
        [HttpGet]
        public IActionResult New()
        {
            SetAccessRights();
            Project project = new Project();

            return View(project);
        }

        [HttpPost]
        public IActionResult New(Project project)
        {
            SetAccessRights();
            project.CreatedDate = DateTime.Now;
            project.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();

                SetTempDataMessage("Project successfully added", "alert-success");

                return RedirectToAction("Show", "Projects", new { id = project.Id });
            }
            else
            {
                SetTempDataMessage("Project could not be added!", "alert-danger");

                return View(project);
            }
        }
        [NonAction]
        public List<ApplicationUser> GetAllUsersExceptTeammembers(int? team_id)
        {

            var team_proj = db.Teams.Include("Project").Where(t=> t.Id==team_id).First(); 
            var organizer = db.Users.Where(u => u.Id == team_proj.Project.UserId);

            //aici mai tb verificat rolul de organizator si daca proiectul e finalizat
            // ar trebui sa am si membrii care pot fi adaugati 
            var users_to_be_added = from user in db.Users select user;
            var teammembers = from user in db.Users
                              join member in db.TeamMembers
                              on user.Id equals member.ApplicationUserId
                              where member.TeamId == team_id 
                              select user;
                                      
            users_to_be_added = users_to_be_added.Except(teammembers);
            users_to_be_added = users_to_be_added.Except(organizer);


            var list = users_to_be_added.ToList();
            return list;

        }

        [NonAction]
        public List<ApplicationUser> GetAllTeammembersWithOrganizer(int? team_id)
        {
            var team_proj = db.Teams.Include("Project").Where(t => t.Id == team_id).First();
            var organizer = db.Users.Where(u => u.Id == team_proj.Project.UserId);

            var user_in_this_team = from user in db.Users
                                    join member in db.TeamMembers
                                    on user.Id equals member.ApplicationUserId
                                    where member.TeamId == team_id
                                    select user;
            return user_in_this_team.Union(organizer).ToList();
        }
        [NonAction]
        public List<ApplicationUser> GetAllTeammembersWithoutOrganizer(int? team_id)
        {
            var team_proj = db.Teams.Include("Project").Where(t => t.Id == team_id).First();
           
            var user_in_this_team = from user in db.Users
                                    join member in db.TeamMembers
                                    on user.Id equals member.ApplicationUserId
                                    where member.TeamId == team_id 
                                    select user;
            return user_in_this_team.ToList();
        }

        [NonAction]
        public void SetTempDataMessage(string message, string style)
        {
            TempData["Message"] = message;
            TempData["MessageStyle"] = style;


        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");

            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }

        [NonAction]
        public Project GetProjectById(int? id)
        {
            return db.Projects.Where(p => p.Id == id).FirstOrDefault();
        }
        [NonAction]
        public Team GetTeamById(int? id)
        {
            return db.Teams.Where(t => t.Id == id).FirstOrDefault();
        }

        [NonAction]
        private Project? GetProjectByTeamId(int? task_id)
        {
            var team = GetTeamById(task_id);

            return db.Projects.Where(p => p.Id == team.ProjectId).FirstOrDefault();
        }

        [NonAction]
        private ApplicationUser? GetProjectOrganizerByProjectId(int? project_id)
        {
            var project = db.Projects.Where(p => p.Id == project_id).FirstOrDefault();
            return db.Users.Where(u => u.Id == project.UserId).FirstOrDefault();
        }

    }

}
