using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    [Authorize]

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
            // we need to check if the task is from a team we are part of 
            //otherwise -> error message
            ViewBag.TaskShow = GetTaskById(id);
            ViewBag.Organizer = GetProjectOrganizerByProjectId(ViewBag.TaskShow.ProjectId);
            ViewBag.Comments = GetAllCommentsOfTask(id);


            return View();
        }

        public IActionResult AddComment(int id, [FromForm] Comment comment)
        {
            SetAccessRights();

            comment.TaskId = id;
            comment.UserId = _userManager.GetUserId(User);
            comment.CreatedAt = DateTime.Now;


            ViewBag.TaskShow = GetTaskById(id);

            ViewBag.Comments = GetAllCommentsOfTask(id);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();

                SetTempDataMessage("Comment has been added", "alert-success");

                return Redirect("/Tasks/Show/" + id);

            } else
            {
                SetTempDataMessage("Comment could not have been added !", "alert-danger");

                return Redirect("/Tasks/Show/" + id);
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
                db.Tasks.Remove(task);
                db.SaveChanges();
                TempData["message"] = "The task has been deleted";
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
                    if(task.status == "Completed")
                    {
                        task.FinishedDate = DateTime.Now;
                    }
                    db.SaveChanges();
                    SetTempDataMessage("Status Updated!", "alert-success");

                    return Redirect("/Tasks/Show/" + id);
                }
                else
                {
                    //add unsuccessfull  message
                    SetTempDataMessage("Couldn't edit status !", "alert-danger");

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
    }


}
