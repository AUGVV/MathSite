using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class TasksModel
    {
        public int Id { get; set; }

        public string Author { get; set; }

        public double Rating { get; set; }

        public string TaskName { get; set; }

        public string Condition { get; set; }

        public int IsDeleted { get; set; }

        public DateTime AddDate { get; set; }

    }
}