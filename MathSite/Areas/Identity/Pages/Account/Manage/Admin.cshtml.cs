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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MathSite.Areas.Identity.Pages.Account.Manage
{
    public partial class Admin : PageModel
    {
        private TasksContext DataBase;
        private readonly SignInManager<IdentityUser> SignInManager;

        public List<UserConfigModel> Users { get; set; }

        public SelectList UsersList { get; set; }

        public Admin(TasksContext TasksContext, SignInManager<IdentityUser> SignInManager)
        {
            DataBase = TasksContext;
            this.SignInManager = SignInManager;
        }
  
        public IActionResult OnGet(string User)
        {
            if (!DataBase.UserConfig.Where(x => x.User == SignInManager.Context.User.Identity.Name).FirstOrDefault().isAdmin)
            {
               return Redirect("/Home/Index");
            }
            CreateUsersList();



            return Page();
        }

        public IActionResult OnPost(string act, int ChoiseId)
        {

            CreateUsersList();
            return Page();
        }

        void CreateUsersList()
        {
            UsersList = new SelectList(DataBase.UserConfig.ToList(), "User", "User");
        }





    }
}
