using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
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
            ViewBag.TasksProject = GetAllTasks(project_id);

            var project = db.Projects.Where(p => p.Id == project_id);
            ViewBag.ProjectTasks = project.FirstOrDefault();

            return View();
        }

        public IActionResult Show(int id)
        {
            ViewBag.TaskShow = GetTaskById(id);

            ViewBag.Comments = GetAllCommentsOfTask(id);

            return View();
        }

        public IActionResult AddComment(int id, [FromForm] Comment comment)
        {
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

                return View("Show",comment);
            }

            
        }
        [NonAction]
        public void SetTempDataMessage(string message, string style)
        {
            TempData["MessageTasks"] = message;
            TempData["MessageTypeTasks"] = style;


        }

        [NonAction]
        public List<Task> GetAllTasks(int project_id)
        {
            return db.Tasks.Include("User")
                .Where(t => t.ProjectId == project_id).ToList();
        }

        [NonAction]
        public Task? GetTaskById(int id)
        {
            
            return db.Tasks.Include("User")
                .Include("Project").Where(t => t.Id == id).FirstOrDefault();
            
        }
        [NonAction]
        public List<Comment> GetAllCommentsOfTask(int task_id)
        {
            return db.Comments.Include("User")
                    .Where(c => c.TaskId == task_id).ToList();
        }
    }


}
