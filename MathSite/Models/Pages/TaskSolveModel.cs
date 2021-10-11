using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class TaskSolveModel
    {
        public List<string> Urls { get; set; }
        public IQueryable<TagsModel> Tags { get; set; }
        public IQueryable<CommentsModel> Comments { get; set; }
    }
}
