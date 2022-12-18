using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(
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
        public IActionResult Edit(int id)
        {
            SetAccessRights();
            Comment comm = db.Comments.Where(c => c.Id == id).First();

            // trebuie verificate drepturi de edit

            if (_userManager.GetUserId(User) == comm.UserId || ViewBag.IsAdmin)
            { //can edit the comment
                return View(comm);
            }
            else
            { // another user cannot edit the comment
                SetTempDataMessage("You don't have rights to edit the comment !", "alert-danger");
                return Redirect("/Tasks/Show/" + comm.TaskId);

            }

        }
        [HttpPost]
        public IActionResult Edit(int id, Comment requestComm)
        {
            SetAccessRights();
            Comment comm = db.Comments.Where(c => c.Id == id).First();

            // trebuie verificate drepturi de edit
            if (_userManager.GetUserId(User) == comm.UserId || ViewBag.IsAdmin)
            { //can edit the comment
                if (ModelState.IsValid)
                {
                    comm.Content = requestComm.Content;

                    db.SaveChanges();

                    return Redirect("/Tasks/Show/" + comm.TaskId);
                }
                else
                {
                    //add unsuccessfull  message
                    return View(requestComm);
                }
            }
            else
            { // another user cannot edit the comment
                SetTempDataMessage("You don't have rights to edit the comment !", "alert-danger");
                return Redirect("/Tasks/Show/" + comm.TaskId);



            }



        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            SetAccessRights();
            Comment comm = db.Comments.Where(c => c.Id == id).First();

            if (_userManager.GetUserId(User) == comm.UserId || ViewBag.IsAdmin)
            { //can delete the comment

                db.Comments.Remove(comm);
                db.SaveChanges();
            }
            else
            { // another user cannot delete the comment

                SetTempDataMessage("You don't have rights to delete the comment !", "alert-danger");

            }
            return Redirect("/Tasks/Show/" + comm.TaskId);

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
            TempData["MessageTasks"] = message;
            TempData["MessageTypeTasks"] = style;


        }
    }
}
