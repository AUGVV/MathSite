using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class TextSearchModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string Condition { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public int IsDeleted { get; set; }
    }
}
