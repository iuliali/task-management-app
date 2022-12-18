using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
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
            Comment comm = db.Comments.Where(c => c.Id == id).First();

            // trebuie verificate drepturi de edit

            return View(comm);
            
        }
        [HttpPost]
        public IActionResult Edit(int id, Comment requestComm)
        {
            Comment comm = db.Comments.Where(c => c.Id == id).First();

            // trebuie verificate drepturi de edit

            if (ModelState.IsValid)
            {
                comm.Content = requestComm.Content;

                db.SaveChanges();

                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
            else
            {
                //add unsuccessfull  message
                return View( requestComm);
            }



        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Where(c => c.Id == id).First();

            //VERIFICARI DE DREPTURI DE STERGERE


            db.Comments.Remove(comm);
            db.SaveChanges();
            return Redirect("/Tasks/Show/" + comm.TaskId);
            

        }
    }
}
