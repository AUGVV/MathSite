
using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MathSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;

        private TasksContext DataBase;

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, TasksContext context)
        {
            _logger = logger;
            _signInManager = signInManager;
            DataBase = context;
        }

        public IActionResult Index(int? Tag, int? Id, string SearchText)
        {
            if (Id != null)
            {
                return Redirect($"/Home/TaskSolve?CurrentId={Id}");
            }
            else if (SearchText != null)
            {
                return Redirect($"/Home/SearchPage?SearchText={SearchText}");
            }
            else if (Tag != null)
            {
                return Redirect($"/Home/SearchPage?Tag={Tag}");
            }

            IndexModel indexModel = new()
            {
                NewTasks = DataBase.Tasks.Where(x => x.IsDeleted != 1).OrderByDescending(x => x.AddDate).Take(10),
                TopTasks = DataBase.Tasks.Where(x => x.IsDeleted != 1).OrderByDescending(x => x.Rating).Take(10),
                Tags = DataBase.Tags
            };
            return View(indexModel);
        }

        [Authorize]
        public IActionResult CreateTask(string Act, string TaskName, string TaskCondition, string FirstAnswer, string SecondAnswer, string ThirdAnswer, string Tags, string images, string type)
        {
            string SingInAuthor = _signInManager.Context.User.Identity.Name;
            if (Act == "create")
            {
                TasksCreator tasksCreator = new TasksCreator(DataBase);
                tasksCreator.TaskSave(SingInAuthor, TaskName, TaskCondition, FirstAnswer, SecondAnswer, ThirdAnswer, Tags, images, type);
                return Redirect("/Identity/Account/Manage/YouTasks");
            }
            ViewData["CurrentUser"] = SingInAuthor;
            CreateTaskModel CreateTaskModel = new CreateTaskModel()
            {
                Tags = DataBase.Tags,
                TaskType = new SelectList(DataBase.MathTheme.ToList(), "Theme", "Theme")
            };
            return View(CreateTaskModel);
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
                return Redirect($"/Home/SearchPage?SearchText={SearchText}");
            }

            TasksModel CurrentTask = DataBase.Tasks.Where(x => x.Id == CurrentId).FirstOrDefault();

            if (CurrentTask == null)
            {
                return Redirect("/Home/Index");
            }

            string SingInAuthor = _signInManager.Context.User.Identity.Name;
            bool IsAutorize = false;

            if (SingInAuthor != null)
            {
                IsAutorize = true;
            }
            ViewData["IsAutorize"] = IsAutorize;
            ViewData["IsVoted"] = false;
            ViewData["Rating"] = 0;

            if ((IsAutorize != false) && (DataBase.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == CurrentId).FirstOrDefault() == null))
            {
                DataBase.UserTaskState.Add(new UserTaskModel() { UserName = SingInAuthor, TaskId = (int)CurrentId });
                DataBase.SaveChanges();
            }

            List<PictureRefModel> Pictures = DataBase.PicturesRef.Where(x => x.TaskId == CurrentId)?.ToList();

            if ((IsAutorize != false) && (DataBase.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == CurrentId).FirstOrDefault().Voted == 1))
            {
                ViewData["IsVoted"] = true;
                ViewData["Rating"] = CurrentTask.Rating;
            }

            ViewData["TaskType"] = CurrentTask.Type;
            ViewData["CurrentTask"] = CurrentTask.TaskName;
            ViewData["CurrentCondition"] = CurrentTask.Condition;
            ViewData["UserName"] = _signInManager.Context.User.Identity.Name;
            ViewData["Id"] = CurrentId;
            ViewData["PicturesCount"] = Pictures.Count;

            List<string> Urls = new List<string>();
            foreach (PictureRefModel picture in Pictures)
            {
                Urls.Add(picture.Reference);
            }

            TaskSolveModel TaskSolveModel = new TaskSolveModel()
            {
                Urls = Urls,
                Tags = DataBase.TaskTag.Where(x => x.TaskId == CurrentId).Join(DataBase.Tags, f => f.Tag, t => t.Id, (f, t) => new TagsModel() { TagName = t.TagName }),
                Comments = DataBase.Comments.Where(x => x.TaskId == CurrentId)
            };
            return View(TaskSolveModel);
        }

        [Authorize]
        public IActionResult EditTaskPage(int? CurrentId, int? Tag, string SearchText)
        {
            string SingInAuthor = _signInManager.Context.User.Identity.Name;

            TasksModel CurrentTask = DataBase.Tasks.Where(x => x.Id == CurrentId).FirstOrDefault();

            if (CurrentTask == null || CurrentTask.Author != SingInAuthor)
            {
                    return Redirect($"/Identity/Account/Manage/YouTasks");
            }

            ViewData["CurrentId"] = CurrentId;
            ViewData["CurrentUser"] = SingInAuthor;
            ViewData["TaskName"] = CurrentTask.TaskName;
            ViewData["TaskType"] = CurrentTask.Type;
            ViewData["TaskCondition"] = CurrentTask.Condition;

            EditTaskModel editTaskModel = new EditTaskModel()
            {
                SelectList = new SelectList(DataBase.MathTheme.ToList(), "Theme", "Theme"),
                Tags = DataBase.TaskTag.Where(x => x.TaskId == CurrentId).Join(DataBase.Tags, f => f.Tag, t => t.Id, (f, t) => new TagsModel() { TagName = t.TagName }),
                AllTags = DataBase.Tags,
                Answers = DataBase.Answers.Where(x => x.TaskId == CurrentId),
                Pictures = DataBase.PicturesRef.Where(x => x.TaskId == CurrentId)
            };
            return View(editTaskModel);
        }

        public IActionResult SearchPage(int? Tag, string SearchText)
        {
            PagesSearch pagesSearch = new PagesSearch(DataBase);
            IQueryable<TasksModel> Results = pagesSearch.GetResult(Tag, SearchText);
            ViewData["SearchText"] = pagesSearch.SearchViewData;
            if (Tag != null || SearchText != null)
            {
                return View(Results);
            }
            else
            {
                return View();
            }
        }
    }
}
