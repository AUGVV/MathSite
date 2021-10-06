using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MathSite.Models;
using System.Diagnostics;

namespace MathSite.Areas.Identity.Pages.Account.Manage
{
    public partial class Admin : PageModel
    {
        private TasksContext DataBase;
        private readonly SignInManager<IdentityUser> _signInManager;

        public Admin(TasksContext TasksContext, SignInManager<IdentityUser> signInManager)
        {
            DataBase = TasksContext;
            _signInManager = signInManager;
        }
  
        public IActionResult OnGet()
        {
            if (DataBase.UserConfig.Where(x => x.User == _signInManager.Context.User.Identity.Name).FirstOrDefault().IsAdmin == 0)
            {
               return Redirect("/Home/Index");
            }

            return Page();
        }

        public IActionResult OnPost(string act, int ChoiseId)
        {
     

            return Page();
        }



    }
}
