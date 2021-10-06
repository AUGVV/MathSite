using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class CommentsStateModel
    {
        public int Id { get; set; }
        public string User { get; set; }
        public int CommentId { get; set; }
    }
}
