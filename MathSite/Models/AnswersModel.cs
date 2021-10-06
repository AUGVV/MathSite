using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class AnswersModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Answer { get; set; }
    }
}
