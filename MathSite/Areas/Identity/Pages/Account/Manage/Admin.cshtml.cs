using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathSite.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MathSite.Areas.Identity.Pages.Account.Manage
{
    public partial class Admin : PageModel
    {
        private TasksContext DataBase;
        private readonly SignInManager<IdentityUser> SignInManager;

        private List<UserTaskModel> UserTaskState { get; set; }

        private List<TasksModel> UserTasks { get; set; }

        public List<UserConfigModel> Users { get; set; }



        public List<TasksModel> UserTasksForView { get; set; }

        public SelectList MathThemes { get; set; }

        public SelectList UsersList { get; set; }

        public Admin(TasksContext TasksContext, SignInManager<IdentityUser> SignInManager)
        {
            DataBase = TasksContext;
            this.SignInManager = SignInManager;
        }

        public IActionResult OnPost(string PageAct, int ChoisedId, string ChoisedUser, string SearchTheme, SortTasks Sort = SortTasks.TaskNameAsc)
        {
            if (PageAct == "DeleteTask")
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
            PrepareView(ChoisedUser, SearchTheme, Sort, PageAct);
            return Page();
        }

        public IActionResult OnGet(string ChoisedUser, string SearchTheme, SortTasks Sort = SortTasks.TaskNameAsc)
        {
            if (!isAdmin())
            {
               return Redirect("/Home/Index");
            }
            PrepareView(ChoisedUser, SearchTheme, Sort);
            return Page();
        }

        private bool isAdmin()
        {
            return DataBase.UserConfig.Where(x => x.User == SignInManager.Context.User.Identity.Name).FirstOrDefault().isAdmin;
        }

        private void PrepareView(string ChoisedUser, string SearchTheme, SortTasks Sort = SortTasks.TaskNameAsc, string PageAct = "")
        {
            bool isSelectedUser = false;
            bool isBannedUser = false;

            if (ChoisedUser != null)
            {
                isSelectedUser = true;
                isBannedUser = BanCheck(ChoisedUser, PageAct);
                GetAllUserInfo(ChoisedUser);
                ViewListCreate(Sort, SearchTheme);
            }

            ViewData["SelectUser"] = ChoisedUser;
            ViewData["isSelectedUser"] = isSelectedUser;
            ViewData["isBan"] = isBannedUser;

            CreateUsersList();
        }

        private bool BanCheck(string ChoisedUser, string PageAct)
        {
            UserConfigModel CurrentUserConfig = GetCurrentUserConfig(ChoisedUser);
            ViewData["DoNotBanYorself"] = false;
            if (PageAct == "BanUser")
            {
                BanUser(ChoisedUser, CurrentUserConfig);
            }
            else if (PageAct == "UnbanUser")
            {
                UnbanUser(ChoisedUser, CurrentUserConfig);
            }
            return CurrentUserConfig.isBaned;
        }

        private UserConfigModel GetCurrentUserConfig(string ChoisedUser)
        {
            return DataBase.UserConfig.Where(x => x.User == ChoisedUser).FirstOrDefault();
        }

        private void BanUser(string ChoisedUser, UserConfigModel CurrentUserConfig)
        {
            if(SignInManager.Context.User.Identity.Name == ChoisedUser)
            {
                ViewData["DoNotBanYorself"] = true;
            }
            else
            {
                CurrentUserConfig.isBaned = true;
                DataBase.SaveChanges();
            }
        }

        private void UnbanUser(string ChoisedUser, UserConfigModel CurrentUserConfig)
        {
            CurrentUserConfig.isBaned = false;
            DataBase.SaveChanges();
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

        void ViewListCreate(SortTasks Sort, string SearchTheme)
        {
            CreateThemesList();
            UserTasksForView = TableSort(UserTasks, Sort, SearchTheme);
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
                _ => throw new NotImplementedException(),
            };
            if (Search != "Все" && Search != null)
            {
                ForSort = ForSort.Where(x => x.Type == Search).ToList();
            }
            return ForSort;
        }

        private void DeleteTask(int ChoisedId)
        {
            TasksModel Task = GetTask(ChoisedId);
            Task.isDeleted = true;
            DataBase.SaveChanges();
        }

        private TasksModel GetTask(int ChoisedId)
        {
            return DataBase.Tasks.Where(x => x.Id == ChoisedId).FirstOrDefault();
        }

        private void GetAllUserInfo(string ChoisedUser)
        {
            UserTasks = GetUserTasksList(ChoisedUser);
            UserTaskState = GetUserStateList(ChoisedUser);
            SetUserInfo();
        }

        private List<TasksModel> GetUserTasksList(string ChoisedUser)
        {
            return DataBase.Tasks.Where(x => x.Author == ChoisedUser).ToList();
        }

        private List<UserTaskModel> GetUserStateList(string ChoisedUser)
        {
            return DataBase.UserTaskState.Where(x => x.UserName == ChoisedUser).ToList();
        }


        private void SetUserInfo()
        {
            ViewData["AnsweredCount"] = GetAnswersCount();
            ViewData["VotedCount"] = GetVotedCount();
            ViewData["CreatedTasks"] = GetCreatedTasks();
            ViewData["ResultRaiting"] = GetRating();
        }

        private int GetAnswersCount()
        {
            return UserTaskState.Where(x =>x.isAnswered == true).Count();
        }

        private int GetVotedCount()
        {
            return UserTaskState.Where(x =>x.isVoted == true).Count();
        }

        private int GetCreatedTasks()
        {
            return UserTasks.Count();
        }

        private int GetRating()
        {
            int AllRatings = GetAllRatings();
            int AllVotes = GetAllVotes();
            int ResultRaiting = 0;

            if (AllRatings != 0 || AllVotes != 0)
            {
                ResultRaiting = AllRatings / AllVotes;
            }
            return ResultRaiting;
        }

        private int GetAllRatings()
        {
            return UserTasks.Sum(x => x.SumRating);
        }

        private int GetAllVotes()
        {
            return UserTasks.Sum(x => x.SumVotes);
        }

        private void CreateUsersList()
        {
            UsersList = new SelectList(DataBase.UserConfig.ToList(), "User", "User");
        }

        private void CreateThemesList()
        {
            List<ThemesModel> MathTheme = new List<ThemesModel>();
            MathTheme.Add(new ThemesModel() { Theme = "Все" });
            foreach (var Theme in DataBase.MathTheme.ToList())
            {
                MathTheme.Add(Theme);
            }
            MathThemes = new SelectList(MathTheme, "Theme", "Theme");
        }
    }
}
