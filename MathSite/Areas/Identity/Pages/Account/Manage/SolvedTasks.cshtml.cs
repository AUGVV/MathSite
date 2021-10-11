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
    public class SolvedTasksModel : PageModel
    {

        private TasksContext DataBase;
        public List<TasksModel> Tasks { get; set; }

        public SelectList MathTheme { get; set; }

        private readonly SignInManager<IdentityUser> SignInManager;

        public SolvedTasksModel(TasksContext TasksContext, SignInManager<IdentityUser> SignInManager)
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

        public IActionResult OnPost(string PageAct, int ChoisedId)
        {
            if (PageAct == "ShowTask")
            {
                return Redirect($"/Home/TaskSolve?CurrentId={ChoisedId}");
            }
            return Page();
        }

        public enum SortTasks
        {
            TaskNameAsc,
            TaskNameDesc,
            ConditionAsc,
            ConditionDesc,
            TypeAsc,
            TypeDesc,
        }

        void ViewListCreate(SortTasks Sort, string Search = "Все")
        {
            Tasks = TableSort(DataBase.UserTaskState.Where(x => x.UserName == SignInManager.Context.User.Identity.Name).Join(DataBase.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = f.TaskId, TaskName = t.TaskName, Condition = t.Condition, Type = t.Type }).ToList(), Sort, Search);
        }

        SelectList CreateMathList()
        {
            List<ThemesModel> MathTheme = new List<ThemesModel>();
            MathTheme.Add(new ThemesModel() { Theme = "Все" });
            foreach (var Theme in DataBase.MathTheme.ToList())
            {
                MathTheme.Add(Theme);
            }
            return new SelectList(MathTheme, "Theme", "Theme");
        }

        private List<TasksModel> TableSort(List<TasksModel> ForSort, SortTasks Sort, string Search)
        {
            ViewData["TaskName"] = Sort == SortTasks.TaskNameAsc ? SortTasks.TaskNameDesc : SortTasks.TaskNameAsc;
            ViewData["Condition"] = Sort == SortTasks.ConditionAsc ? SortTasks.ConditionDesc : SortTasks.ConditionAsc;
            ViewData["Type"] = Sort == SortTasks.TypeAsc ? SortTasks.TypeDesc : SortTasks.TypeAsc;

            ForSort = Sort switch
            {
                SortTasks.TaskNameDesc => ForSort.OrderByDescending(s => s.TaskName).ToList(),
                SortTasks.ConditionAsc => ForSort.OrderBy(s => s.Condition).ToList(),
                SortTasks.ConditionDesc => ForSort.OrderByDescending(s => s.Condition).ToList(),
                SortTasks.TypeAsc => ForSort.OrderBy(s => s.Type).ToList(),
                SortTasks.TypeDesc => ForSort.OrderByDescending(s => s.Type).ToList(),
                SortTasks.TaskNameAsc => ForSort.OrderBy(s => s.TaskName).ToList(),
                _ => throw new NotImplementedException(),
            };
            if (Search != "Все" && Search != null)
            {
                ForSort = ForSort.Where(x => x.Type == Search).ToList();
            }
            return ForSort;
        }
    }
}
