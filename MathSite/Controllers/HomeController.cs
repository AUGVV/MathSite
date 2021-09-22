using MathSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public IActionResult CreateTask(string Id, string Act, string TaskName, string TaskCondition, string FirstAnswer, string SecondAnswer, string ThirdAnswer, string TagName)
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
                AnswersModel AnswerToBase = new AnswersModel() {Answer = Answer, TaskId = CurrentId, Order = Order};
                Order++;
                db.Answers.Add(AnswerToBase);
                db.SaveChanges();
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




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
