using MathSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Functions
{
    public class PagesSearch
    {
        public IQueryable<TasksModel> GetResult(int? Tag, string SearchText)
        {
            IQueryable<TasksModel> Results;

            if (Tag != null)
            {
                // ViewData["SearchText"] = db.Tags.Where(x => x.Id == Tag).FirstOrDefault().TagName;
                //  IQueryable<TasksModel> Results = db.TaskTag.Where(x => x.Tag == Tag).Join(db.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = t.Id, TaskName = t.TaskName, Type = t.Type });
                //  return View(Results);
            }
            if (SearchText != null)
            {

                // IEnumerable<TasksModel> TasksResult = db.Tasks.Where(x => EF.Functions.FreeText(x.TaskName, SearchText) || EF.Functions.FreeText(x.Condition, SearchText) || EF.Functions.FreeText(x.Type, SearchText));
                //   IEnumerable<TasksModel> CommentsResult = db.Comments.Where(x => EF.Functions.FreeText(x.Text, SearchText)).Join(db.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = t.Id, Type = t.Type, TaskName = t.TaskName, IsDeleted = t.IsDeleted });
                //  IQueryable<TasksModel> Results = (TasksResult.Concat(CommentsResult)).AsQueryable();

             
            }



            return Results;
        }







    }
}
