using MathSite.Models;
using System;
using System.Collections.Generic;

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

            int CurrentId = AddNewTask(Author, TaskName, TaskCondition, type);

            CreateAnswers(ListOfAnswers, CurrentId);
            CrateTaskTags(CurrentId, Tags);
            AddPictures(CurrentId, images);
        }

        private int AddNewTask(string Author, string TaskName, string TaskCondition, string type)
        {
            TasksModel CreateModel = new TasksModel() { TaskName = TaskName, Author = Author, Condition = TaskCondition, AddDate = DateTime.Now, isDeleted = false, Rating = 0, Type = type };
            DataBase.Tasks.Add(CreateModel);
            DataBase.SaveChanges();
            return CreateModel.Id;
        }

        private void AddPictures(int CurrentId, string images)
        {
            if (images != null)
            {
                UploadPictures UploadPictures = new UploadPictures(DataBase);
                UploadPictures.Upload(CurrentId, images);
            }
        }

        private void CrateTaskTags(int CurrentId, string Tags)
        {
            if (Tags != null)
            {
                CreateTags CreateTags = new CreateTags(DataBase);
                CreateTags.Create(Tags, CurrentId);
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
