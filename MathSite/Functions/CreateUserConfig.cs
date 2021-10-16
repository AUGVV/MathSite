using MathSite.Models;

namespace MathSite.Functions
{
    public class CreateUserConfig
    {

        private TasksContext DataBase;

        public CreateUserConfig(string User, TasksContext context)
        {
            DataBase = context;
            DataBase.UserConfig.Add(new UserConfigModel() { User = User, Region = "en", isDark = false, isAdmin = false });
            DataBase.SaveChanges();
        }
    }
}
