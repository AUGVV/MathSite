using MathSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Functions
{
    public class TasksCreator
    {


        private TasksContext DataBase;

        public TasksCreator(TasksContext DataBase)
        {
            this.DataBase = DataBase;
        }

        public void TaskSave(string Author, string TaskName, string TaskCondition, string FirstAnswer, string SecondAnswer, string ThirdAnswer, string Tags, string images, string type)
        {
            List<string> ListOfAnswers = new List<string>() { FirstAnswer, SecondAnswer, ThirdAnswer };
            TasksModel CreateModel = new TasksModel() { TaskName = TaskName, Author = Author, Condition = TaskCondition, AddDate = DateTime.Now, IsDeleted = 0, Rating = 0, Type = type };
            DataBase.Tasks.Add(CreateModel);
            DataBase.SaveChanges();
            int CurrentId = CreateModel.Id;
            CreateAnswers(ListOfAnswers, CurrentId);
            if (Tags != null)
            {
                _ = new CreateTags(Tags, CurrentId, DataBase);
            }

            if (images != null)
            {
                _ = new UploadPictures(images, CurrentId, DataBase);
            }
        }

        private void CreateAnswers(List<string> ListOfAnswers, int CurrentId)
        {
            foreach (string Answer in ListOfAnswers)
            {
                if (Answer != null)
                {
                    AnswersModel AnswerToBase = new AnswersModel() { Answer = Answer.Replace(" ", ""), TaskId = CurrentId };
                    DataBase.Answers.Add(AnswerToBase);
                    DataBase.SaveChanges();
                }
            }
        }

    }
}
