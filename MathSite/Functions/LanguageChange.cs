using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathSite.Models;

namespace MathSite.Functions
{
    public class LanguageChange
    {
        public LanguageChange(string Lang, string User, TasksContext DataBase)
        {
            UserConfigModel CurrentUser = DataBase.UserConfig.Where(x => x.User == User).FirstOrDefault();
            CurrentUser.Region = Lang;
            DataBase.SaveChanges();
        }

    }
}
