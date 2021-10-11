using MathSite.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
            string[] SplitTags = TaskTags.ToLower().Split('#');

            foreach (string Tag in SplitTags)
            {
                if (DataBase.Tags.Where(x => x.TagName == Tag).FirstOrDefault() == null)
                {
                    SaveTag(Tag);
                }
                TagAddToTask(Tag, TaskId);
            }
        }

        void SaveTag(string Tag)
        {
            DataBase.Tags.Add(new TagsModel() { TagName = Tag });
            DataBase.SaveChanges();
        }

        void TagAddToTask(string Tag, int TaskId)
        {
            TagsModel CurrentTag = DataBase.Tags.Where(x => x.TagName == Tag).FirstOrDefault();
            CurrentTag.Count += 1;
            DataBase.TaskTag.Add(new TaskTagModel() { Tag = CurrentTag.Id, TaskId = TaskId });
            DataBase.SaveChanges();
        }
    }
}
