using MathSite.Models;
using System.Linq;

namespace MathSite.Functions
{
    public class CreateTags
    {

        private TasksContext DataBase;

        public CreateTags(TasksContext context)
        {
            DataBase = context;
        }

        public void Create(string TaskTags, int TaskId)
        {
            if (TaskTags.Length > 0)
            {
                string[] SplitTags = TaskTags.ToLower().Split('#');
                foreach (string Tag in SplitTags)
                {
                    if (GetTags(Tag) == null)
                    {
                        SaveTag(Tag);
                    }
                    TagAddToTask(Tag, TaskId);
                }
            }
        }

        private void SaveTag(string Tag)
        {
            DataBase.Tags.Add(new TagsModel() { TagName = Tag });
            DataBase.SaveChanges();
        }

        private void TagAddToTask(string Tag, int TaskId)
        {
            TagsModel CurrentTag = GetTags(Tag);
            CurrentTag.Count += 1;
            DataBase.TaskTag.Add(new TaskTagModel() { Tag = CurrentTag.Id, TaskId = TaskId });
            DataBase.SaveChanges();
        }

        private TagsModel GetTags(string Tag)
        {
            return DataBase.Tags.Where(x => x.TagName == Tag).FirstOrDefault();
        }
    }
}
