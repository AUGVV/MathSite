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

        private TasksContext DataBase;

        private readonly SignInManager<IdentityUser> SignInManager;

        public ChangeLanguageModel(TasksContext TasksContext, SignInManager<IdentityUser> SignInManager)
        {
            DataBase = TasksContext;
            this.SignInManager = SignInManager;
        }

        public IActionResult OnPost(string Language)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(Language)));
            ChangeLanguage(Language);
            return Redirect($"/Identity/Account/Manage/ChangeLanguage");
        }

        void ChangeLanguage(string Language)
        {
            string SingInAuthor = SignInManager.Context.User.Identity.Name;
            LanguageChange LanguageChange = new LanguageChange(DataBase);
            LanguageChange.ChangeLanguage(Language, SingInAuthor);
        }
    }
}
