using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class TaskSolveModel
    {
        public List<string> Urls { get; set; }

        public List<CommentsModel> Comments { get; set; }
    }
}
