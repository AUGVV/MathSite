using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class CommentsModel
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public int Order { get; set; }

        public string Author { get; set; }

        public string Text { get; set; }

        public int Like { get; set; }

        public int Dislike { get; set; }

    }
}
