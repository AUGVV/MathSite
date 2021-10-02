
using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        private TasksContext db;

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, TasksContext context)
        {
            _logger = logger;
            _signInManager = signInManager;
            db = context;
        }
        public IActionResult Index(string SearchText, int? Tag, int? Id)
        {
            if(Id!=null)
            {
                return Redirect($"/Home/TaskSolve?CurrentId={Id}");
            }
            else if(SearchText != null)
            {
                return Redirect($"/Home/SearchPage?SearchText={SearchText}");
            }
            else if(Tag!= null)
            {
                return Redirect($"/Home/SearchPage?Tag={Tag}");
            }    


            IQueryable<TasksModel> NewTasks;
            NewTasks = db.Tasks.Where(x=>x.IsDeleted != 1).OrderByDescending(x => x.AddDate).Take(10);
            IQueryable<TasksModel> TopTasks;
            TopTasks = db.Tasks.Where(x => x.IsDeleted != 1).OrderByDescending(x => x.Rating).Take(10);
            IQueryable<TagsModel> Tags;
            Tags = db.Tags;

            IndexModel indexModel = new IndexModel
            {
                NewTasks = NewTasks,
                TopTasks = TopTasks,
                Tags = Tags
            };

            return View(indexModel);
        }
        
        [Authorize]
        public IActionResult CreateTask(string Id, string Act, string TaskName, string TaskCondition, string FirstAnswer, string SecondAnswer, string ThirdAnswer, string Tags, string images, string type)
        {
            string SingInAuthor = _signInManager.Context.User.Identity.Name;

            List<string> ListOfAnswers = new List<string>() { FirstAnswer, SecondAnswer, ThirdAnswer };
            List<ThemesModel> Themes = db.MathTheme.ToList();
            SelectList SelectList = new SelectList(Themes, "Theme", "Theme");
            IQueryable<TagsModel> TagsList = db.Tags;

            ViewData["CurrentUser"] = SingInAuthor;
            if (Id == null && Act == "create")
            {
                    TasksModel CreateModel = new TasksModel() { TaskName = TaskName, Author = SingInAuthor, Condition = TaskCondition, AddDate = DateTime.Now, IsDeleted = 0, Rating = 0, Type = type };
                    db.Tasks.Add(CreateModel);
                    db.SaveChanges();
                    int CurrentId = CreateModel.Id;
                    CreateAnswers(ListOfAnswers, CurrentId);
                   if (Tags != null)
                   {
                       CreateTags TagsCreator = new CreateTags(Tags, CurrentId, db);
                   }

                    if (images != null)
                    {
                        string[] ImageList = images.Split("|image|");
                        foreach (string image in ImageList)
                        {
                            if (image.Length != 0)
                            {
                                Uploader(image, CurrentId);
                            }
                        }
                    }
                    return Redirect("/Identity/Account/Manage/YouTasks");
            }

            CreateTaskModel CreateTaskModel = new CreateTaskModel()
            {
                Tags = TagsList,
                TaskType = SelectList
            }; 
            return View(CreateTaskModel);
        }

        public void CreateAnswers(List<string> ListOfAnswers, int CurrentId)
        {
            foreach(string Answer in ListOfAnswers)
            {
                if (Answer != null)
                {
                    AnswersModel AnswerToBase = new AnswersModel() { Answer = Answer.Replace(" ", ""), TaskId = CurrentId };
                    db.Answers.Add(AnswerToBase);
                    db.SaveChanges();
                }
            }     
        }

        public void Uploader(string image, int id)
        {
            byte[] Response;
            string json="";
            using (var WebClient = new WebClient())
            {
                NameValueCollection parameters = new NameValueCollection();
                parameters.Add("key", "b626821a9813f418d523698ba34376d6"); //Запихнуть в secret! НЕ ЗАБЫТЬ !
                parameters.Add("image",  image);
                try
                {
                    Response = WebClient.UploadValues("https://api.imgbb.com/1/upload", "POST", parameters);
                    json = Encoding.Default.GetString(Response);
                }
                catch { }
            }
            json.Root ResponseJson = JsonConvert.DeserializeObject<json.Root>(json);
            if (ResponseJson.data.url != null)
            {
                ReferenceToBase(ResponseJson.data.url, ResponseJson.data.delete_url, id);
            }
        }

        public void ReferenceToBase(string Url, string DeleteUrl, int Taskid)
        {
            db.PicturesRef.Add(new PictureRefModel() { Reference = Url, TaskId = Taskid, DeleteReference = DeleteUrl });
            db.SaveChanges();
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

        public IActionResult TaskSolve(int CurrentId, string SearchText)
        {
            if (SearchText != null)
            {
                return Redirect($"/Home/SearchPage?SearchText={SearchText}");
            }

            TasksModel CurrentTask = new TasksModel();
            CurrentTask = db.Tasks.Where(x => x.Id == CurrentId).FirstOrDefault();
            if(CurrentTask == null)
            {
                return Redirect("/Home/Index");
            }

            string SingInAuthor = _signInManager.Context.User.Identity.Name;
            bool IsAutorize = false;
            if(SingInAuthor != null)
            {
                IsAutorize = true;
            }
            ViewData["IsAutorize"] = IsAutorize;
            ViewData["IsVoted"] = false;
            ViewData["Rating"] = 0;

                if ((IsAutorize != false) && (db.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == CurrentId).FirstOrDefault() == null))
                {   
                        db.UserTaskState.Add(new UserTaskModel() { UserName = SingInAuthor, TaskId = CurrentId });
                        db.SaveChanges();
                }

            List<PictureRefModel> Pictures = new List<PictureRefModel>();
            Pictures = db.PicturesRef.Where(x => x.TaskId == CurrentId)?.ToList();
            List<string> Urls = new List<string>();
            IQueryable<TagsModel> Tags = db.TaskTag.Where(x=>x.TaskId == CurrentId).Join(db.Tags, f => f.Tag, t => t.Id, (f, t) => new TagsModel() { TagName = t.TagName });
        
            foreach(PictureRefModel picture in Pictures)
            {
                Urls.Add(picture.Reference);
            }

            if ((IsAutorize != false) && (db.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == CurrentId).FirstOrDefault().Voted == 1))
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

            TaskSolveModel TaskSolveModel = new TaskSolveModel()
            {
                Urls = Urls,
                Tags = Tags
            };
            return View(TaskSolveModel);
        }



        [Authorize]
        public IActionResult EditTaskPage(int? CurrentId, string SearchText, int? Tag)
        {
            string SingInAuthor = _signInManager.Context.User.Identity.Name;

            List<ThemesModel> Themes = db.MathTheme.ToList();
            SelectList SelectList = new SelectList(Themes, "Theme", "Theme");

            TasksModel CurrentTask = new TasksModel();
            CurrentTask = db.Tasks.Where(x => x.Id == CurrentId).FirstOrDefault();

            if (CurrentId != null)
            {
                if(CurrentTask == null)
                {
                    return Redirect($"/Identity/Account/Manage/YouTasks");
                }    
                else if(CurrentTask.Author != SingInAuthor)
                {
                    return Redirect($"/Identity/Account/Manage/YouTasks");
                }
            }
            else
            {
                return Redirect($"/Identity/Account/Manage/YouTasks");
            }

            ViewData["CurrentId"] = CurrentId;
            ViewData["CurrentUser"] = SingInAuthor;
            ViewData["TaskName"] = CurrentTask.TaskName;
            ViewData["TaskType"] = CurrentTask.Type;
            ViewData["TaskCondition"] = CurrentTask.Condition;

            IQueryable<TagsModel> TaskTags = db.TaskTag.Where(x=>x.TaskId == CurrentId).Join(db.Tags, f => f.Tag, t => t.Id, (f, t) => new TagsModel() { TagName = t.TagName });

            IQueryable<TagsModel> AllTags = db.Tags;

            IQueryable<AnswersModel> Answers = db.Answers.Where(x=>x.TaskId == CurrentId);

            IQueryable<PictureRefModel> Pictures = db.PicturesRef.Where(x => x.TaskId == CurrentId);

            EditTaskModel editTaskModel = new EditTaskModel()
            {
                SelectList = SelectList,
                Tags = TaskTags,
                AllTags = AllTags,
                Answers = Answers,
                Pictures = Pictures
            };

            return View(editTaskModel);
        }




            public IActionResult SearchPage(string SearchText, int? Tag)
        {
            ViewData["SearchText"] = SearchText;
            if(Tag != null)
            {
                ViewData["SearchText"] = db.Tags.Where(x=>x.Id == Tag).FirstOrDefault().TagName;
                IQueryable<TasksModel> results = db.TaskTag.Where(x => x.Tag == Tag).Join(db.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = t.Id, TaskName = t.TaskName, Type = t.Type });
                return View(results);
            }    
            if (SearchText != null)
            {
                IQueryable<TasksModel> results = db.Tasks.Where(x => EF.Functions.FreeText(x.TaskName, SearchText) || EF.Functions.FreeText(x.Condition, SearchText) || EF.Functions.FreeText(x.Type, SearchText));
                return View(results);
            }
            else
            {
                return View();
            }
        }
        
    }
}
