using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Authorize(Roles="Admin, User")]
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
        [Authorize(Roles ="Admin")]
        public IActionResult Index()
        {
            SetAccessRights();
            var teams = db.Teams.Include(t=>t.TeamMembers).ToList();
            ViewBag.Teams = teams;

            return View();
        }
        public IActionResult Show(int id)
        {
            SetAccessRights();



            var team = db.Teams.Include("Project").Where(t => t.Id == id).FirstOrDefault();

            

            if(team is null)
            {
                SetTempDataMessage("Team cannot be found!!", "alert-danger");
                return View("Error2");
            }

            if (!CheckTeamMember(_userManager.GetUserId(User), team.Id) && !ViewBag.IsAdmin && 
                GetProjectOrganizerByProjectId(team.Project.Id).Id != ViewBag.CurrentUser)
            {
                //should not have access to see the team page 
                SetTempDataMessage("You don't have rights to access team page!", "alert-danger");
                return View("Error2");

            }


            var members = db.TeamMembers.Include("ApplicationUser").Where(m => m.TeamId == id).ToList(); ;
            ViewBag.Team = team;

            ViewBag.MembersTeam = members;
            ViewBag.Organizer = GetProjectOrganizerByProjectId(team.ProjectId);

            return View();
        }

        [HttpPost]
        public IActionResult DeleteMember(int? id)
        {
            SetAccessRights();


            var member = db.TeamMembers.Where(m => m.Id == id).FirstOrDefault();
            if (member is null)
            {
                SetTempDataMessage("Member not found!", "alert-danger");
                return View("Error2");
            }

            if(!ViewBag.IsAdmin)
            {
                SetTempDataMessage("You don't have rights to delete a member!", "alert-danger");
                return View("Error2");

            }

            //first we have to check if the member had any tasks
            var team = db.Teams.FirstOrDefault(t => t.Id == member.TeamId);
            var tasks = db.Tasks.Where(tsk => tsk.ProjectId == team.ProjectId).Where(t => t.UserId == member.ApplicationUserId);

            foreach(var task in tasks)
            {

                var comments = db.Comments.Where(c => c.TaskId == task.Id).ToList();

                foreach (var comment in comments)
                {
                    db.Comments.Remove(comment);
                }
                db.Tasks.Remove(task);


            }

            var tasks2 = db.Tasks.Include("Comments").Where(t => t.ProjectId == team.ProjectId);
            foreach(var task in tasks2)
            {
                foreach(var comm in task.Comments)
                {
                    if(comm.UserId == member.ApplicationUserId)
                    {
                        db.Comments.Remove(comm);
                    }
                }
            }

            var members = db.TeamMembers.Include("ApplicationUser").Where(m => m.TeamId == id).ToList(); ;
            ViewBag.Team = team;

            ViewBag.MembersTeam = members;
            ViewBag.Organizer = GetProjectOrganizerByProjectId(team.ProjectId);

            db.TeamMembers.Remove(member);
            db.SaveChanges();

            return Redirect("/Teams/Show/"+ team.Id);


        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");

            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }

        [NonAction]
        private ApplicationUser? GetProjectOrganizerByProjectId(int? project_id)
        {
            var project = db.Projects.Where(p => p.Id == project_id).FirstOrDefault();
            return db.Users.Where(u => u.Id == project.UserId).FirstOrDefault();
        }

        [NonAction]
        private bool CheckTeamMember(string user_id, int team_id)
        {
            var team_member = db.TeamMembers.Where(tm => tm.ApplicationUserId == user_id).Where(tm => tm.TeamId == team_id).FirstOrDefault();
            if (team_member == null) return false;
            else return true;
        }

        [NonAction]
        private void SetTempDataMessage(string message, string style)
        {
            TempData["Message"] = message;
            TempData["MessageStyle"] = style;
        }

    }
}