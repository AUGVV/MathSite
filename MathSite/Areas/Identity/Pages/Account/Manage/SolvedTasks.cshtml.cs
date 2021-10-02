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
    public class SolvedTasksModel : PageModel
    {

        private TasksContext TasksDb;
        public List<TasksModel> Tasks { get; set; }

        private readonly SignInManager<IdentityUser> _signInManager;

        public SolvedTasksModel(TasksContext TasksContext, SignInManager<IdentityUser> signInManager)
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
            if (act == "ShowTask")
            {
                return Redirect($"/Home/TaskSolve?CurrentId={ChoiseId}");
            }
            ViewListCreate();
            return Page();
        }

        void ViewListCreate()
        {
            Tasks = TasksDb.UserTaskState.Where(x=>x.UserName == _signInManager.Context.User.Identity.Name).Join(TasksDb.Tasks,f=>f.TaskId,t=>t.Id,(f,t)=>new TasksModel() { Id = f.TaskId, TaskName = t.TaskName, Condition = t.Condition, Type = t.Type }).ToList();
        }
    }
}
