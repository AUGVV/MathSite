using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathSite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MathSite.Areas.Identity.Pages.Account.Manage
{
    public class YouTasksModel : PageModel
    {

        private TasksContext TasksDb;
        public List<TasksModel> Tasks { get; set; }

        private readonly SignInManager<IdentityUser> _signInManager;

        public YouTasksModel(TasksContext TasksContext, SignInManager<IdentityUser> signInManager)
        {
            TasksDb = TasksContext;
            _signInManager = signInManager;
        }
              
        public void OnGet()
        {
            ViewListCreate();
        }

        public IActionResult OnPost(string act, int ChoiseId)
        {
            if (act == "create")
            {
                return Redirect("/Home/CreateTask");
            }
            if(act == "DeleteTask")
            {
                TasksModel Task = TasksDb.Tasks.Where(x => x.Id == ChoiseId).FirstOrDefault();
                Task.IsDeleted = 1;
                TasksDb.SaveChanges();
            }
            if (act == "ShowTask")
            {
                return Redirect($"/Home/TaskSolve?id={ChoiseId}");
            }
            ViewListCreate();
            return Page();
        }

        void ViewListCreate()
        {
            Tasks = TasksDb.Tasks.Where(x => x.Author == _signInManager.Context.User.Identity.Name && x.IsDeleted == 0).ToList();
        }
    }
}
