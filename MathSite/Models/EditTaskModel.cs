using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class EditTaskModel
    {

       public SelectList SelectList { get; set; }

       public List<string> Urls { get; set; }

       public IQueryable<TagsModel> AllTags { get; set; }

       public IQueryable<TagsModel> Tags { get; set; }

       public IQueryable<AnswersModel> Answers { get; set; }

       public IQueryable<PictureRefModel> Pictures { get; set; }

    }
}
