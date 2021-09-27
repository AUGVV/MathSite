
using MathSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            return View();
        }
        
        [Authorize]
        public IActionResult CreateTask(string Id, string Act, string TaskName, string TaskCondition, string FirstAnswer, string SecondAnswer, string ThirdAnswer, string TagName, string images)
        {
            string SingInAuthor = _signInManager.Context.User.Identity.Name;
            List<string> ListOfAnswers = new List<string>() { FirstAnswer, SecondAnswer, ThirdAnswer };
            ViewData["CurrentUser"] = SingInAuthor;
            if (Id == null)
            {
                Debug.WriteLine("id=0");
                if (Act == "create")
                {
                    Debug.WriteLine("Act");
                    TasksModel CreateModel = new TasksModel() { TaskName = TaskName, Author = SingInAuthor, Condition = TaskCondition, AddDate = DateTime.Now, IsDeleted = 0, Rating = 0 };
                    db.Tasks.Add(CreateModel);
                    db.SaveChanges();
                    int CurrentId = CreateModel.Id;
                    CreateAnswers(ListOfAnswers, CurrentId);
                    CreateTags(TagName, CurrentId);

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
            }
            return View();
        }

        public void CreateAnswers(List<string> ListOfAnswers, int CurrentId)
        {
            int Order = 0;
            foreach(string Answer in ListOfAnswers)
            {
                if (Answer != null)
                {
                    AnswersModel AnswerToBase = new AnswersModel() { Answer = Answer.Replace(" ", ""), TaskId = CurrentId, Order = Order };
                    Order++;
                    db.Answers.Add(AnswerToBase);
                    db.SaveChanges();
                }
            }     
        }

        public void CreateTags(string Tags, int CurrentId)
        {
            if (Tags != null)
            {
                string[] SplitTags = Tags.ToLower().Split(' ');

                foreach (string tag in SplitTags)
                {
                    TagModel OneTag = new TagModel() { TaskId = CurrentId, tag = tag };
                    db.Tags.Add(OneTag);
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
                ReferenceToBase(ResponseJson.data.url, id);
            }
        }

        public void ReferenceToBase(string Url, int Taskid)
        {
            db.PicturesRef.Add(new PictureRefModel() { Reference = Url, TaskId = Taskid});
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

        public IActionResult TaskSolve(int id)
        {
            string SingInAuthor = _signInManager.Context.User.Identity.Name;


            ViewData["IsVoted"] = false;
            ViewData["Rating"] = 0;

            if (db.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == id).FirstOrDefault() == null)
            {
                db.UserTaskState.Add(new UserTaskModel() { UserName = SingInAuthor, TaskId = id });
                db.SaveChanges();
            }

            TasksModel CurrentTask = new TasksModel();
            CurrentTask = db.Tasks.Where(x => x.Id == id).FirstOrDefault();
            List<PictureRefModel> Pictures = new List<PictureRefModel>();
            Pictures = db.PicturesRef.Where(x => x.TaskId == id)?.ToList();
            List<string> Urls = new List<string>();
            foreach(PictureRefModel picture in Pictures)
            {
                Urls.Add(picture.Reference);
            }

            if (db.UserTaskState.Where(x => x.UserName == SingInAuthor && x.TaskId == id).FirstOrDefault().Voted == 1)
            {
                ViewData["IsVoted"] = true;
                ViewData["Rating"] = CurrentTask.Rating;
                Debug.WriteLine(CurrentTask.Rating);
            }


            ViewData["CurrentTask"] = CurrentTask.TaskName;
            ViewData["CurrentCondition"] = CurrentTask.Condition;
            ViewData["UserName"] = _signInManager.Context.User.Identity.Name;
            ViewData["Id"] = id;
            ViewData["PicturesCount"] = Pictures.Count;

            TaskSolveModel TaskSolveModel = new TaskSolveModel()
            {
                Urls = Urls,
                Comments = null
            };


            return View(Urls);
        }
    }
}
