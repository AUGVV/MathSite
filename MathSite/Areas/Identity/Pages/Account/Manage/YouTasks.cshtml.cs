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

        private readonly SignInManager<IdentityUser> _signInManager;

        public YouTasksModel(TasksContext TasksContext, SignInManager<IdentityUser> signInManager)
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
            if (act == "create")
            {
                return Redirect("/Home/CreateTask");
            }
            else if(act == "DeleteTask")
            {
                TasksModel Task = DataBase.Tasks.Where(x => x.Id == ChoiseId).FirstOrDefault();
                Task.IsDeleted = 1;
                DataBase.SaveChanges();
            }
            else if (act == "EditTask")
            {
                return Redirect($"/Home/EditTaskPage?CurrentId={ChoiseId}");
            }
            else if (act == "ShowTask")
            {
                return Redirect($"/Home/TaskSolve?CurrentId={ChoiseId}");
            }
            MathTheme = CreateMathList();
            ViewListCreate(Sort, Search);
            return Page();
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

        public enum Sort
        {
            TaskNameAsc,
            TaskNameDesc,
            ConditionAsc,
            ConditionDesc,
            RaitingAsc,
            RaitingDesc,
        }

        void ViewListCreate(Sort Sort, string Search = "Все")
        {
                Tasks = TableSort(DataBase.Tasks.Where(x => x.Author == _signInManager.Context.User.Identity.Name && x.IsDeleted == 0).ToList(), Sort, Search);
        }

        private List<TasksModel> TableSort(List<TasksModel> ForSort, Sort Sort, string Search)
        {
            ViewData["TaskName"] = Sort == Sort.TaskNameAsc ? Sort.TaskNameDesc : Sort.TaskNameAsc;
            ViewData["Condition"] = Sort == Sort.ConditionAsc ? Sort.ConditionDesc : Sort.ConditionAsc;
            ViewData["Rating"] = Sort == Sort.RaitingAsc ? Sort.RaitingDesc : Sort.RaitingAsc;

            ForSort = Sort switch
            {
                Sort.TaskNameDesc => ForSort.OrderByDescending(s => s.TaskName).ToList(),
                Sort.ConditionAsc => ForSort.OrderBy(s => s.Condition).ToList(),
                Sort.ConditionDesc => ForSort.OrderByDescending(s => s.Condition).ToList(),
                Sort.RaitingAsc => ForSort.OrderBy(s => s.Rating).ToList(),
                Sort.RaitingDesc => ForSort.OrderByDescending(s => s.Rating).ToList(),
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
