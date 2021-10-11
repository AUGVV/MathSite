using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathSite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MathSite.Areas.Identity.Pages.Account.Manage
{
    public class YouTasksModel : PageModel
    {
        private TasksContext DataBase;
        public List<TasksModel> Tasks { get; set; }
        public SelectList MathTheme { get; set; }

        private readonly SignInManager<IdentityUser> SignInManager;

        public YouTasksModel(TasksContext TasksContext, SignInManager<IdentityUser> SignInManager)
        {
            DataBase = TasksContext;
            this.SignInManager = SignInManager;
        }
              
        public void OnGet(string Search, SortTasks Sort = SortTasks.TaskNameAsc)
        {
            ViewData["SearchSave"] = Search;
            MathTheme = CreateMathList();
            ViewListCreate(Sort, Search);
        }

        public IActionResult OnPost(string PageAct, int ChoisedId, string Search, SortTasks Sort = SortTasks.TaskNameAsc)
        {
            if (PageAct == "CreateTask")
            {
                return Redirect("/Home/CreateTask");
            }
            else if(PageAct == "DeleteTask")
            {
                ViewData["SortSave"] = Sort;
                DeleteTask(ChoisedId);
            }
            else if (PageAct == "EditTask")
            {
                return Redirect($"/Home/EditTaskPage?CurrentId={ChoisedId}");
            }
            else if (PageAct == "ShowTask")
            {
                return Redirect($"/Home/TaskSolve?CurrentId={ChoisedId}");
            }
            MathTheme = CreateMathList();
            ViewListCreate(Sort, Search);
            return Page();
        }

        void DeleteTask(int ChoisedId)
        {  
            TasksModel Task = DataBase.Tasks.Where(x => x.Id == ChoisedId).FirstOrDefault();
            Task.isDeleted = true;
            DataBase.SaveChanges();
        }

        SelectList CreateMathList()
        {
            List<ThemesModel> MathTheme = new List<ThemesModel>();
            MathTheme.Add(new ThemesModel() { Theme = "Все"});
            foreach(var Theme in DataBase.MathTheme.ToList())
            {
                MathTheme.Add(Theme);
            }      
            return new SelectList(MathTheme, "Theme", "Theme");
        }

        public enum SortTasks
        {
            TaskNameAsc,
            TaskNameDesc,
            ConditionAsc,
            ConditionDesc,
            RaitingAsc,
            RaitingDesc,
        }

        void ViewListCreate(SortTasks Sort, string Search = "Все")
        {
                Tasks = TableSort(DataBase.Tasks.Where(x => x.Author == SignInManager.Context.User.Identity.Name && x.isDeleted == false).ToList(), Sort, Search);
        }

        private List<TasksModel> TableSort(List<TasksModel> ForSort, SortTasks Sort, string Search)
        {
            ViewData["TaskName"] = Sort == SortTasks.TaskNameAsc ? SortTasks.TaskNameDesc : SortTasks.TaskNameAsc;
            ViewData["Condition"] = Sort == SortTasks.ConditionAsc ? SortTasks.ConditionDesc : SortTasks.ConditionAsc;
            ViewData["Rating"] = Sort == SortTasks.RaitingAsc ? SortTasks.RaitingDesc : SortTasks.RaitingAsc;

            ForSort = Sort switch
            {
                SortTasks.TaskNameDesc => ForSort.OrderByDescending(s => s.TaskName).ToList(),
                SortTasks.ConditionAsc => ForSort.OrderBy(s => s.Condition).ToList(),
                SortTasks.ConditionDesc => ForSort.OrderByDescending(s => s.Condition).ToList(),
                SortTasks.RaitingAsc => ForSort.OrderBy(s => s.Rating).ToList(),
                SortTasks.RaitingDesc => ForSort.OrderByDescending(s => s.Rating).ToList(),
                SortTasks.TaskNameAsc => ForSort.OrderBy(s => s.TaskName).ToList(),
            };
            if (Search != "Все" && Search != null)
            {
                ForSort = ForSort.Where(x => x.Type == Search).ToList();
            }
            return ForSort;
        }
    }
}
