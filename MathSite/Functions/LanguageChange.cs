using System.Linq;
using MathSite.Models;

namespace MathSite.Functions
{
    public class LanguageChange
    {
        private TasksContext DataBase;

        public LanguageChange(TasksContext context)
        {
            DataBase = context;
        }

        public void ChangeLanguage(string Lang, string User)
        {
            UserConfigModel CurrentUser = GetUserConfig(User);
            CurrentUser.Region = Lang;
            DataBase.SaveChanges();
        }

        private UserConfigModel GetUserConfig(string User)
        {
            return DataBase.UserConfig.Where(x => x.User == User).FirstOrDefault();
        }
    }
}
