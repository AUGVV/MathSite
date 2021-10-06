using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class UserConfigModel
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public int DarkTheme { get; set; }
        public int IsAdmin { get; set; }
        public string User { get; set; }
    }
}
