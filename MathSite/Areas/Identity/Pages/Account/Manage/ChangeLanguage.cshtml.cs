using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace MathSite.Areas.Identity.Pages.Account.Manage
{
    public class ChangeLanguageModel : PageModel
    {

        private TasksContext Db;

        private readonly SignInManager<IdentityUser> _signInManager;

   
        public ChangeLanguageModel(TasksContext TasksContext, SignInManager<IdentityUser> signInManager)
        {
            Db = TasksContext;
            _signInManager = signInManager;
        }


        public void OnGet()
        {
        }

        public IActionResult OnPost(string language)
        {
            string SingInAuthor = _signInManager.Context.User.Identity.Name;

            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language)));
            LanguageChange languageChange = new LanguageChange(language, SingInAuthor, Db);
            return Redirect($"/Identity/Account/Manage/ChangeLanguage");
        }
    }

}
