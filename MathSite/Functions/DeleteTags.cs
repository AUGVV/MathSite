using MathSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Functions
{
    public class DeleteTags
    {

        private TasksContext DataBase;

        public DeleteTags(TasksContext context)
        {
            DataBase = context;
        }

        public void Delete(int CurrentId)
        {
            foreach (TaskTagModel Tag in GetTagsList(CurrentId))
            {
                DecrementTagCount(Tag.Tag);
                DataBase.TaskTag.Remove(Tag);
            }
            DataBase.SaveChanges();
        }

        private void DecrementTagCount(int Tag)
        {
            DataBase.Tags.Where(x => x.Id == Tag).FirstOrDefault().Count -= 1;
        }


        private List<TaskTagModel> GetTagsList(int CurrentId)
        {
            return DataBase.TaskTag.Where(x => x.TaskId == CurrentId).ToList();
        }
    }
}
