using MathSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure;
using MathSite.Models.Azure;
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
            return CheckAllResults(Tag, SearchText);
        }

        private IQueryable<TasksModel> CheckAllResults(int? Tag, string SearchText)
        {
            IQueryable<TasksModel> TagsResult = TagSearch(Tag);
            IQueryable<TasksModel> TextResult = TextSearch(SearchText);
          
            if(TextResult != null)
            {
                return TextResult;
            }
            else if(TagsResult != null)
            {
                return TagsResult;
            }
            return null;
        }

        private IQueryable<TasksModel> TagSearch(int? Tag)
        {
            if (Tag != null)
            {
                SearchViewData = GetTagName(Tag);
                return DataBase.TaskTag.Where(x => x.Tag == Tag).Join(DataBase.Tasks, f => f.TaskId, t => t.Id, (f, t) => new TasksModel() { Id = t.Id, TaskName = t.TaskName, Type = t.Type });
            }
            return null;
        }

        private string GetTagName(int? Tag)
        {
            return DataBase.Tags.Where(x => x.Id == Tag).FirstOrDefault().TagName;
        }

        private IQueryable<TasksModel> TextSearch(string SearchText)
        {
            if (SearchText != null)
            {
                SearchViewData = SearchText;
                return SearchInText(SearchText);
            }
            return null;
        }

        private IQueryable<TasksModel> SearchInText(string SearchText)
        {
            IEnumerable<TasksModel> TasksResult = AzureSearchTasks(SearchText);
            IEnumerable<TasksModel> CommentsResult = AzureSearchComments(SearchText);
            return (TasksResult.Concat(CommentsResult)).AsQueryable();
        }

        private List<TasksModel> AzureSearchComments(string SearchText)
        {
            var Options = new SearchOptions(){ IncludeTotalCount = true };
            string IndexName = "azuresql-commentsindex";
            return FindedComments(GetAzureIndex(SearchText, Options, IndexName));
        }

        private List<TasksModel> FindedComments(SearchResults<ResultIndexModel> SearchResult)
        {
            List<TasksModel> FindedComments = new List<TasksModel>();
            foreach (var Item in SearchResult.GetResults().ToList().Distinct())
            {
                FindedComments.Add(FindTaskViaComment(Item.Document.TaskId));
            }
            return FindedComments;
        }

        TasksModel FindTaskViaComment(int TaskId)
        {
            return DataBase.Tasks.Where(x => x.Id == TaskId).FirstOrDefault();
        }

        private List<TasksModel> AzureSearchTasks(string SearchText)
        {
            string IndexName = "azuresql-index";
            var Options = new SearchOptions()
            {
                IncludeTotalCount = true
            };
            return FindedTasks(GetAzureIndex(SearchText, Options, IndexName));
        }

        private List<TasksModel> FindedTasks(SearchResults<ResultIndexModel> SearchResult)
        {
            List<TasksModel> FindedTasks = new List<TasksModel>();
            foreach (var Item in SearchResult.GetResults().ToList())
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
