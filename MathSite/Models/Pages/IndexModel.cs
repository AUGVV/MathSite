using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class IndexModel
    {
       public IEnumerable<TasksModel> NewTasks { get; set; }
       public IEnumerable<TasksModel> TopTasks { get; set; }
       public IEnumerable<TagsModel> Tags { get; set; }
    }
}
