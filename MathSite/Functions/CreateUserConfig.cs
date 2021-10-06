using MathSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Functions
{
    public class CreateUserConfig
    {

        private TasksContext db;

        public CreateUserConfig(string User, TasksContext context)
        {
            db = context;
            db.UserConfig.Add(new UserConfigModel() { User = User, Region = "en", DarkTheme = 0, IsAdmin = 0 });
            db.SaveChanges();
        }
    }
}
