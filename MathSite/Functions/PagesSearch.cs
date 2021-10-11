using MathSite.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure;
using System.Diagnostics;
using MathSite.Models.Azure;
using Newtonsoft.Json;
using Azure.Search.Documents.Models;

namespace MathSite.Functions
{
    public class PagesSearch
    {
        public string SearchViewData { get; set; }

        private TasksContext DataBase;

        private AzureSecretKey AzureSecretKey { get; set; }

        public PagesSearch(TasksContext DataBase)
        {
            AzureSecretKey = new AzureSecretKey();
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
            IEnumerable<TasksModel> TasksResult = AzureSearchTasks(SearchText);
            IEnumerable<TasksModel> CommentsResult = AzureSearchComments(SearchText);
            return (TasksResult.Concat(CommentsResult)).AsQueryable();


            /* Non cloud search 
             IEnumerable<TasksModel> TasksResult = DataBase.Tasks.Where(x => EF.Functions.FreeText(x.TaskName, SearchText) || EF.Functions.FreeText(x.Condition, SearchText) || EF.Functions.FreeText(x.Type, SearchText));
             IEnumerable<TasksModel> CommentsResult = DataBase.Comments.Where(x => EF.Functions.FreeText(x.Text, SearchText)).Join(DataBase.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = t.Id, Type = t.Type, TaskName = t.TaskName, isDeleted = t.isDeleted });
             return (TasksResult.Concat(CommentsResult)).AsQueryable();
            */
        }

        private List<TasksModel> AzureSearchComments(string SearchText)
        {
            List<TasksModel> FindedComments = new List<TasksModel>();
            var Options = new SearchOptions(){ IncludeTotalCount = true };
            string IndexName = "azuresql-commentsindex";
            SearchResults<ResultIndexModel> Result = GetAzureIndex(SearchText, Options, IndexName);

            foreach (var Item in Result.GetResults().ToList().Distinct())
            {
                FindedComments.Add(FindTaskViaComment(Item.Document.TaskId));
            }
            return FindedComments;
        }

        TasksModel FindTaskViaComment(int TaskId)
        {
            TasksModel Task = DataBase.Tasks.Where(x => x.Id == TaskId).FirstOrDefault();
            return Task;
        }

        private List<TasksModel> AzureSearchTasks(string SearchText)
        {
            List<TasksModel> FindedTasks = new List<TasksModel>();
            string IndexName = "azuresql-index";
            var Options = new SearchOptions()
            {
                IncludeTotalCount = true
            };
            SearchResults<ResultIndexModel> Result = GetAzureIndex(SearchText, Options, IndexName);
            foreach (var Item in Result.GetResults().ToList())
            {
                FindedTasks.Add(new TasksModel() { Id = Convert.ToInt32(Item.Document.Id), TaskName = Item.Document.TaskName, Condition = Item.Document.Condition, Type = Item.Document.Type, isDeleted = Item.Document.isDeleted });
            }
            return FindedTasks;
        }

        SearchResults<ResultIndexModel> GetAzureIndex(string SearchText, SearchOptions options, string IndexName)
        {
            SearchIndexClient IndexClient = new SearchIndexClient(new Uri(GetSearchUri()), new AzureKeyCredential(GetSearchKey()));
            SearchClient SearchClient = IndexClient.GetSearchClient(IndexName);
            return SearchClient.Search<ResultIndexModel>(SearchText, options);
        }

        string GetSearchUri()
        {
            return AzureSecretKey.TakeSecretKey("UriIndexSearcher", "57918eb83749466d800b56c3101dd9c7");
        }

        string GetSearchKey()
        {
            return AzureSecretKey.TakeSecretKey("KeyIndexSearch", "2b35df665e4c4e3993414f21b889ea3e");
        }





    }
}
