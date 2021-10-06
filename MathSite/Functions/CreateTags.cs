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

        public CreateTags(string Tags, int Id, TasksContext context)
        {
            DataBase = context;
            string[] SplitTags = Tags.ToLower().Split('#');

            foreach (string Tag in SplitTags)
            {
                if(DataBase.Tags.Where(x=>x.TagName == Tag).FirstOrDefault() == null)
                {
                    DataBase.Tags.Add(new TagsModel() { TagName = Tag });
                    DataBase.SaveChanges();
                }

                TagsModel CurrentTag = DataBase.Tags.Where(x => x.TagName == Tag).FirstOrDefault();
                CurrentTag.Count += 1;
                DataBase.TaskTag.Add(new TaskTagModel() { Tag = CurrentTag.Id, TaskId = Id });
                DataBase.SaveChanges();
            }
        }
    }
}
