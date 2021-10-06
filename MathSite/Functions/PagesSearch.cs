using MathSite.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Functions
{
    public class PagesSearch
    {
        public string SearchViewData { get; set; }

        private TasksContext DataBase;

        public PagesSearch(TasksContext DataBase)
        {
            this.DataBase = DataBase;
        }
        
        public IQueryable<TasksModel> GetResult(int? Tag, string SearchText)
        {
            IQueryable<TasksModel> Results = null;

            if (Tag != null)
            {
                SearchViewData = DataBase.Tags.Where(x => x.Id == Tag).FirstOrDefault().TagName;
                Results = DataBase.TaskTag.Where(x => x.Tag == Tag).Join(DataBase.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = t.Id, TaskName = t.TaskName, Type = t.Type });
            }
            if (SearchText != null)
            {
                SearchViewData = SearchText;
                Results = SearchInText(SearchText);
            }
            return Results;
        }

        IQueryable<TasksModel> SearchInText(string SearchText)
        {
             IEnumerable<TasksModel> TasksResult = DataBase.Tasks.Where(x => EF.Functions.FreeText(x.TaskName, SearchText) || EF.Functions.FreeText(x.Condition, SearchText) || EF.Functions.FreeText(x.Type, SearchText));
             IEnumerable<TasksModel> CommentsResult = DataBase.Comments.Where(x => EF.Functions.FreeText(x.Text, SearchText)).Join(DataBase.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = t.Id, Type = t.Type, TaskName = t.TaskName, IsDeleted = t.IsDeleted });
             return (TasksResult.Concat(CommentsResult)).AsQueryable();
        }
    }
}
