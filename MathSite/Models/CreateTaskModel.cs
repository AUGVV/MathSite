using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class CreateTaskModel
    {
        public SelectList TaskType { get; set; }
        public IQueryable<TagsModel> Tags { get; set; }
    }
}
