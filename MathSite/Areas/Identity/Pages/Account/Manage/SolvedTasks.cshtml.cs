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

        private readonly SignInManager<IdentityUser> _signInManager;

        public SolvedTasksModel(TasksContext TasksContext, SignInManager<IdentityUser> signInManager)
        {
            DataBase = TasksContext;
            _signInManager = signInManager;
        }
              
        public void OnGet(string Search, Sort Sort = Sort.TaskNameAsc)
        {
            ViewData["SearchSave"] = Search;
            MathTheme = CreateMathList();
            ViewListCreate(Sort, Search);
        }

        public IActionResult OnPost(string act, string Search, int ChoiseId, Sort Sort = Sort.TaskNameAsc)
        {
            if (act == "ShowTask")
            {
                return Redirect($"/Home/TaskSolve?CurrentId={ChoiseId}");
            }
            ViewListCreate(Sort, Search);
            MathTheme = CreateMathList();
            return Page();
        }

        public enum Sort
        {
            TaskNameAsc,
            TaskNameDesc,
            ConditionAsc,
            ConditionDesc,
            TypeAsc,
            TypeDesc,
        }

        void ViewListCreate(Sort Sort, string Search = "Все")
        {
            Tasks = TableSort(DataBase.UserTaskState.Where(x => x.UserName == _signInManager.Context.User.Identity.Name).Join(DataBase.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = f.TaskId, TaskName = t.TaskName, Condition = t.Condition, Type = t.Type }).ToList(), Sort, Search);
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

        private List<TasksModel> TableSort(List<TasksModel> ForSort, Sort Sort, string Search)
        {
            ViewData["TaskName"] = Sort == Sort.TaskNameAsc ? Sort.TaskNameDesc : Sort.TaskNameAsc;
            ViewData["Condition"] = Sort == Sort.ConditionAsc ? Sort.ConditionDesc : Sort.ConditionAsc;
            ViewData["Type"] = Sort == Sort.TypeAsc ? Sort.TypeDesc : Sort.TypeAsc;

            ForSort = Sort switch
            {
                Sort.TaskNameDesc => ForSort.OrderByDescending(s => s.TaskName).ToList(),
                Sort.ConditionAsc => ForSort.OrderBy(s => s.Condition).ToList(),
                Sort.ConditionDesc => ForSort.OrderByDescending(s => s.Condition).ToList(),
                Sort.TypeAsc => ForSort.OrderBy(s => s.Type).ToList(),
                Sort.TypeDesc => ForSort.OrderByDescending(s => s.Type).ToList(),
                Sort.TaskNameAsc => ForSort.OrderBy(s => s.TaskName).ToList(),
            };
            if (Search != "Все" && Search != null)
            {
                ForSort = ForSort.Where(x => x.Type == Search).ToList();
            }
            return ForSort;
        }



    }
}
