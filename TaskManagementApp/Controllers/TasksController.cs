using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    [Authorize(Roles ="Admin,User")]

    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        
        public IActionResult Index(int project_id)
        {
            SetAccessRights();

            ViewBag.TasksProject = GetAllTasks(project_id);

            var project = db.Projects.Where(p => p.Id == project_id);
            ViewBag.ProjectTasks = project.FirstOrDefault();

            return View();
        }

        public IActionResult MyTasks()
        {
            SetAccessRights();

            ViewBag.MyTasks = GetAllTasksCurrentUser();

            SetTaskByStatus(ViewBag.MyTasks);

            return View();
        }

       

        public IActionResult Show(int id)
        {
            SetAccessRights();
            var task = GetTaskById(id);

            if (task is null)
            {
                SetTempDataMessage("Task cannot be found!!", "alert-danger");
                return View("Error2");
            }
            var organizer = GetProjectOrganizerByProjectId(task.ProjectId);
            // we need to check if the task is from a team we are part of 
            var team_id = GetTeamIdByProjectId((int) task.ProjectId);

            if (!CheckTeamMember(_userManager.GetUserId(User), team_id) && !ViewBag.IsAdmin && _userManager.GetUserId(User) != organizer.Id)
            {
                SetTempDataMessage("You don't have rights to see the task!", "alert-danger");
                return Redirect("/Home/Index");
            }

            //otherwise -> error message
            ViewBag.TaskShow = task;
            ViewBag.Organizer = organizer;
            ViewBag.Comments = GetAllCommentsOfTask(id);
            ViewBag.CommentOpen = false;


            return View();
        }

       
        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            SetAccessRights();

            //comment.TaskId = id;
            int id = (int) comment.TaskId;
            comment.UserId = _userManager.GetUserId(User);
            comment.CreatedAt = DateTime.Now;


            var task = GetTaskById(id);

            var organizer = GetProjectOrganizerByProjectId(task.ProjectId);
            // we need to check if the task is from a team we are part of 

            ViewBag.TaskShow = task;
            var team_id = GetTeamIdByProjectId((int)task.ProjectId);

            // summernote
            var sanitizer = new HtmlSanitizer();

            if (!CheckTeamMember(_userManager.GetUserId(User), team_id) && !ViewBag.IsAdmin && _userManager.GetUserId(User) != organizer.Id)
            {
                SetTempDataMessage("You don't have rights to see the task!", "alert-danger");
                return Redirect("/Home/Index");
            }

            ViewBag.Comments = GetAllCommentsOfTask(id);

            if (ModelState.IsValid)
            {
                comment.Content = sanitizer.Sanitize(comment.Content);
                
                db.Comments.Add(comment);
                db.SaveChanges();

                SetTempDataMessage("Comment has been added", "alert-success");
                ViewBag.Organizer = GetProjectOrganizerByProjectId(ViewBag.TaskShow.ProjectId);

                return Redirect("/Tasks/Show/" + id);

            } 
            else
            {
                SetTempDataMessage("Comment could not have been added !", "alert-danger");
                SetAccessRights();
                
                ViewBag.TaskShow = GetTaskById(id);
                ViewBag.Organizer = GetProjectOrganizerByProjectId(ViewBag.TaskShow.ProjectId);
                ViewBag.Comments = GetAllCommentsOfTask(id);
                ViewBag.CommentOpen = true;
                return View(comment);
            }


        }
        
        [HttpPost]
        public IActionResult Delete(int id)
        {
            SetAccessRights();
            Task task = db.Tasks.Include("Comments").Where(t => t.Id == id).First();
            //fac o fct pt organizer cu getorganizerfortask

            var organizer = (from taskk in db.Tasks
                            join project in db.Projects on taskk.ProjectId equals project.Id
                            where taskk.Id == id
                            select project.UserId).FirstOrDefault();

            if(organizer == _userManager.GetUserId(User) || ViewBag.IsAdmin)
            {
                var comments = db.Comments.Where(t => t.TaskId == id).ToList();
                int count = comments.Count();
                foreach(var comment in comments)
                {
                    db.Comments.Remove(comment);
                }

                db.Tasks.Remove(task);
                db.SaveChanges();
                SetTempDataMessage("The task has been deleted (with " + count + " comments) !", "alert-success");

                return Redirect("/Tasks/Index/" + task.ProjectId);
            }
            else
            {
                SetTempDataMessage("You don't have rights to delete the task!", "alert-danger");
                return Redirect("/Tasks/Show/" + id);
            }
        }
        public IActionResult Edit(int id)
        {
            SetAccessRights();
            Task task = db.Tasks.Where(t => t.Id == id).First();
            var organizer = (from taskk in db.Tasks
                             join project in db.Projects on taskk.ProjectId equals project.Id
                             where taskk.Id == id
                             select project.UserId).FirstOrDefault();
            if(task.UserId == _userManager.GetUserId(User) || organizer == _userManager.GetUserId(User) || ViewBag.IsAdmin)
            {
                ViewBag.TaskToEdit = task;
                return View(task);
            }
            else
            {
                SetTempDataMessage("You don't have rights to edit the task!", "alert-danger");
                return Redirect("/Tasks/Show/" + id);
            }
        }
        [HttpPost]
        public IActionResult Edit(int id, Task requestTask)
        {
            SetAccessRights();
            Task task = db.Tasks.Where(t => t.Id == id).First();
            var organizer = (from taskk in db.Tasks
                             join project in db.Projects on taskk.ProjectId equals project.Id
                             where taskk.Id == id
                             select project.UserId).FirstOrDefault();
            if(task.UserId == _userManager.GetUserId(User) || organizer == _userManager.GetUserId(User) || ViewBag.IsAdmin)
            {
                if(ModelState.IsValid)
                {
                    // another checks needed -> what happens if task has been already completed?! can be reopened?!!!!
                    task.status = requestTask.status;

                    task.Name = requestTask.Name;

                    if(task.status == "Completed")
                    {
                        task.FinishedDate = DateTime.Now;
                    }
                    db.SaveChanges();
                    SetTempDataMessage("Task Updated!", "alert-success");

                    return Redirect("/Tasks/Show/" + id);
                }
                else
                {
                    //add unsuccessfull  message
                    SetTempDataMessage("Couldn't edit task!", "alert-danger");

                    ViewBag.TaskToEdit = task;
                    return View(requestTask);
                }
            }
            else
            { // another user cannot edit the comment
                SetTempDataMessage("You don't have rights to edit the task!", "alert-danger");
                return Redirect("/Tasks/Show/" + id);
            }
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        //implement change asignee
        public IActionResult ChangeAsignee(int? id)
        {
            SetAccessRights();
            var task = db.Tasks.FirstOrDefault(t=> t.Id==id);
            ViewBag.Task = task;
            if (task is null)
            {
                SetTempDataMessage("Task cannot be found!!", "alert-danger");
                return View("Error2");
            }

            ViewBag.Organizer = GetProjectOrganizerByProjectId(task.ProjectId);


            if (!ViewBag.IsAdmin)
            {
                SetTempDataMessage("You don't have rights to change the task asignee!", "alert-danger");
                return View("Error2");
            }

            var project = db.Projects.FirstOrDefault(p => p.Id == task.ProjectId);
            ViewBag.Project = project;
            var team = db.Teams.FirstOrDefault(t => t.ProjectId == project.Id);
            ViewBag.Members = GetAllTeammembersWithoutOrganizer(team.Id);
            return View();

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        //implement change asignee
        public IActionResult ChangeAsignee([FromForm] Task? task)
        {
            SetAccessRights();
            var task_db = db.Tasks.FirstOrDefault(t => t.Id == task.Id);

            if (ModelState.IsValid)
            {
                task_db.UserId = task.UserId;
                db.SaveChanges();
                SetTempDataMessage("Task Asignee has been updated !", "alert-success");

                
                return Redirect("/Tasks/Show/" + task.Id);
            }
            else
            {

                SetTempDataMessage("An error occurred", "alert-danger");
                return View("Error2");
            }

            if (!ViewBag.IsAdmin)
            {
                SetTempDataMessage("You don't have rights to change the task asignee!", "alert-danger");
                return View("Error2");
            }

            var project = db.Projects.FirstOrDefault(p => p.Id == task.ProjectId);
            ViewBag.Project = project;
            var team = db.Teams.FirstOrDefault(t => t.ProjectId == project.Id);
            ViewBag.Members = GetAllTeammembersWithoutOrganizer(team.Id);

        }


        [NonAction]
        private void SetTempDataMessage(string message, string style)
        {
            TempData["Message"] = message;
            TempData["MessageStyle"] = style;
        }

        [NonAction]
        private List<Task> GetAllTasks(int project_id)
        {
            return db.Tasks.Include("User")
                .Where(t => t.ProjectId == project_id).ToList();
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
        private Task? GetTaskById(int id)
        {
            
            return db.Tasks.Include("User")
                .Include("Project").Where(t => t.Id == id).FirstOrDefault();
            
        }
        [NonAction]
        private List<Comment> GetAllCommentsOfTask(int task_id)
        {
            return db.Comments.Include("User")
                    .Where(c => c.TaskId == task_id).ToList();
        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");

            ViewBag.CurrentUser = _userManager.GetUserId(User);
        }

        private List<Task> GetAllTasksCurrentUser()
        {
            var user_id = _userManager.GetUserId(User);
            return db.Tasks.Include("User").Include("Project").Where(t=>t.UserId == user_id).ToList();
        }

        private void SetTaskByStatus(List<Task> myTasks)
        {
            ViewBag.MyTasksNotStarted = new List<Task>();
            ViewBag.MyTasksInProgress = new List<Task>();
            ViewBag.MyTasksCompleted = new List<Task>();

            foreach (var task in myTasks)
            {
                if (task.status.ToLower() == "Not Started".ToLower())
                {
                    ViewBag.MyTasksNotStarted.Add(task);

                }
                else if (task.status.ToLower() == "In Progress".ToLower())
                {
                    ViewBag.MyTasksInProgress.Add(task);
                }
                else if (task.status.ToLower() == "Completed".ToLower())
                {
                    ViewBag.MyTasksCompleted.Add(task);
                }
            }

        }
        [NonAction]
        private ApplicationUser? GetProjectOrganizerByProjectId(int? project_id)
        {
            var project = db.Projects.Where(p => p.Id == project_id).FirstOrDefault();
            return db.Users.Where(u => u.Id == project.UserId).FirstOrDefault();
        }


        private int GetTeamIdByProjectId(int id)
        {
            var team = db.Teams.Where(team => team.ProjectId == id).FirstOrDefault();
            return team.Id;
        }

        [NonAction]
        private bool CheckTeamMember(string user_id, int team_id)
        {
            var team_member = db.TeamMembers.Where(tm => tm.ApplicationUserId == user_id).Where(tm => tm.TeamId == team_id).FirstOrDefault();
            if (team_member == null) return false;
            else return true;
        }

    }


}
