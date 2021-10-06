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
        public int Voted { get; set; }
        public int Answered { get; set; }
    }
}
