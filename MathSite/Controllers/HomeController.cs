
using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MathSite.Controllers
{

    public class HomeController : Controller
    {
        private readonly SignInManager<IdentityUser> SignInManager;

        private TasksContext DataBase;

        public HomeController(SignInManager<IdentityUser> SignInManager, TasksContext context)
        {
            this.SignInManager = SignInManager;
            DataBase = context;
        }

        public IActionResult Index(int? ChoisedTag, int? ChoisedId, string SearchText)
        {
            if (ChoisedId != null)
            {
                return Redirect($"/Home/TaskSolve?CurrentId={ChoisedId}");
            }
            else if (SearchText != null)
            {
                return LocalRedirect($"/Home/SearchPage?SearchText={StringToHex(SearchText)}");
            }
            else if (ChoisedTag != null)
            {
                return Redirect($"/Home/SearchPage?Tag={ChoisedTag}");
            }
            return View(GetIndexModel());
        }

        [Authorize]
        public IActionResult CreateTask(string PageAct, string TaskName, string TaskCondition, string FirstAnswer, string SecondAnswer, string ThirdAnswer, string Tags, string images, string type)
        {
            string SingInAuthor = SignInManager.Context.User.Identity.Name;
            if (PageAct == "create")
            {
                TasksCreator tasksCreator = new TasksCreator(DataBase);
                tasksCreator.TaskSave(SingInAuthor, TaskName, TaskCondition, FirstAnswer, SecondAnswer, ThirdAnswer, Tags, images, type);
                return Redirect("/Identity/Account/Manage/YouTasks");
            }
            ViewData["CurrentUser"] = SingInAuthor;
            return View(GetCreateTaskModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult TaskSolve(int? CurrentId, string SearchText)
        {
            if (SearchText != null)
            {
                return Redirect($"/Home/SearchPage?SearchText={StringToHex(SearchText)}");
            }

            TasksModel CurrentTask = GetCurrentTask(CurrentId);

            if (CurrentTask == null)
            {
                return Redirect("/Home/Index");
            }

            string SingInAuthor = SignInManager.Context.User.Identity.Name;
            bool isAutorize = isAutorizated(SingInAuthor);

            AddStateForTask(CurrentId, SingInAuthor, isAutorize);
            CheckUserVote(CurrentId, SingInAuthor, isAutorize);
            SetSolvesView(CurrentId, SingInAuthor, CurrentTask);

            return View(GetTaskSolveModel(CurrentId));
        }

        [Authorize]
        public IActionResult EditTaskPage(int? CurrentId)
        {
            string SingInAuthor = SignInManager.Context.User.Identity.Name;

            TasksModel CurrentTask = GetCurrentTask(CurrentId);

            if (CurrentTask == null || (CurrentTask.Author != SingInAuthor && !isAdmin(SingInAuthor)))
            {
                    return Redirect($"/Identity/Account/Manage/YouTasks");
            }
            SetEditTaskView(CurrentId, SingInAuthor, CurrentTask);

            return View(GetEditTaskModel(CurrentId));
        }

        public IActionResult BannedPage()
        {     
                return View();         
        }

        public IActionResult SearchPage(int? Tag, string SearchText)
        {      
            PagesSearch pagesSearch = new PagesSearch(DataBase);
            IQueryable<TasksModel> Results = pagesSearch.GetResult(Tag, HexToString(SearchText));
            ViewData["SearchText"] = pagesSearch.SearchViewData;
            if (Tag != null || HexToString(SearchText) != null)
            {
                return View(Results);
            }
            else
            {
                return View();
            }
        }

        private IndexModel GetIndexModel()
        {
            IndexModel indexModel = new()
            {
                NewTasks = GetNewTasks(),
                TopTasks = GetTopTasks(),
                Tags = DataBase.Tags
            };
            return indexModel;
        }

        private IEnumerable<TasksModel> GetNewTasks()
        {
            return DataBase.Tasks.Where(x => x.isDeleted != true).OrderByDescending(x => x.AddDate).Take(10);
        }

        private IEnumerable<TasksModel> GetTopTasks()
        {
            return DataBase.Tasks.Where(x => x.isDeleted != true).OrderByDescending(x => x.Rating).Take(10);
        }

        private CreateTaskModel GetCreateTaskModel()
        {
            CreateTaskModel CreateTaskModel = new CreateTaskModel()
            {
                Tags = DataBase.Tags,
                TaskType = GetMathThemes()
            };
            return CreateTaskModel;
        }

        private void SetSolvesView(int? CurrentId, string SingInAuthor, TasksModel CurrentTask)
        {
            ViewData["isAutorize"] = isAutorizated(SingInAuthor);
            ViewData["Rating"] = CurrentTask.Rating;
            ViewData["TaskType"] = CurrentTask.Type;
            ViewData["CurrentTask"] = CurrentTask.TaskName;
            ViewData["CurrentCondition"] = CurrentTask.Condition;
            ViewData["UserName"] = SingInAuthor;
            ViewData["Id"] = CurrentId;
        }

        private bool isAutorizated(string SingInAuthor)
        {
            if (SingInAuthor != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddStateForTask(int? CurrentId, string SingInAuthor, bool isAutorize)
        {
            if (isAutorize && isUserTaskStateNull(CurrentId, SingInAuthor))
            {
                DataBase.UserTaskState.Add(new UserTaskModel() { UserName = SingInAuthor, TaskId = (int)CurrentId });
                DataBase.SaveChanges();
            }
        }

        private void CheckUserVote(int? CurrentId, string SingInAuthor, bool isAutorize)
        {
            if (isAutorize && isVoted(CurrentId, SingInAuthor))
            {
                ViewData["isVoted"] = true;
            }
            else
            {
                ViewData["isVoted"] = false;
            }
        }

        private bool isUserTaskStateNull(int? CurrentId, string SingInAuthor)
        {
            if (DataBase.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == CurrentId).FirstOrDefault() == null)
            {
                return true;
            }
            return false;
        }

        private TaskSolveModel GetTaskSolveModel(int? CurrentId)
        {
            TaskSolveModel TaskSolveModel = new TaskSolveModel()
            {
                Urls = GetPicturesUrls(CurrentId),
                Tags = GetTags(CurrentId),
                Comments = GetTaskComments(CurrentId)
            };
            return TaskSolveModel;
        }

        private IQueryable<CommentsModel> GetTaskComments(int? CurrentId)
        {
            return DataBase.Comments.Where(x => x.TaskId == CurrentId);
        }

        private bool isVoted(int? CurrentId, string SingInAuthor)
        {
            return DataBase.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == CurrentId).FirstOrDefault().isVoted;
        }

        private IQueryable<TagsModel> GetTags(int? CurrentId)
        {
            return DataBase.TaskTag.Where(x => x.TaskId == CurrentId).Join(DataBase.Tags, f => f.Tag, t => t.Id, (f, t) => new TagsModel() { TagName = t.TagName });
        }

        private List<string> GetPicturesUrls(int? CurrentId)
        {
            List<PictureRefModel> Pictures = GetPictures(CurrentId)?.ToList();
            List<string> Urls = new List<string>();
            foreach (PictureRefModel picture in Pictures)
            {
                Urls.Add(picture.Reference);
            }
            ViewData["PicturesCount"] = Pictures.Count;
            return Urls;
        }

        private void SetEditTaskView(int? CurrentId, string SingInAuthor, TasksModel CurrentTask)
        {
            ViewData["CurrentId"] = CurrentId;
            ViewData["CurrentUser"] = SingInAuthor;
            ViewData["TaskName"] = CurrentTask.TaskName;
            ViewData["TaskType"] = CurrentTask.Type;
            ViewData["TaskCondition"] = CurrentTask.Condition;
        }

        private bool isAdmin(string SingInAuthor)
        {
            return DataBase.UserConfig.Where(x => x.User == SingInAuthor).FirstOrDefault().isAdmin;
        }

        private TasksModel GetCurrentTask(int? CurrentId)
        {
            return DataBase.Tasks.Where(x => x.Id == CurrentId).FirstOrDefault();
        }

        private EditTaskModel GetEditTaskModel(int? CurrentId)
        {
            EditTaskModel EditTaskModel = new EditTaskModel()
            {
                SelectList = GetMathThemes(),
                Tags = GetTags(CurrentId),
                AllTags = DataBase.Tags,
                Answers = GetAnswers(CurrentId),
                Pictures = GetPictures(CurrentId)
            };
            return EditTaskModel;
        }

        private IQueryable<AnswersModel> GetAnswers(int? CurrentId)
        {
            return DataBase.Answers.Where(x => x.TaskId == CurrentId);
        }

        private SelectList GetMathThemes()
        {
            return new SelectList(DataBase.MathTheme.ToList(), "Theme", "Theme");
        }

        private IQueryable<PictureRefModel> GetPictures(int? CurrentId)
        {
            return DataBase.PicturesRef.Where(x => x.TaskId == CurrentId);
        }

        private string StringToHex(string Word)
        {
            return Uri.EscapeDataString(Word);
        }

        private string HexToString(string Word)
        {
            if (Word != null)
            {
                return Uri.UnescapeDataString(Word);
            }
            return null;
        }
    }
}
