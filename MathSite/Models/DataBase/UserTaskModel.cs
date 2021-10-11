using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class UserTaskModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int TaskId { get; set; }
        public bool isVoted { get; set; }
        public bool isAnswered { get; set; }
    }
}
