using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Hubs
{
    public class ControlHub : Hub
    {

        private TasksContext db;

        public ControlHub(TasksContext context)
        {
            db = context;
        }


        public async Task AddToComment(string TaskId, string WhatSay, string UserName)
        {
            db.Comments.Add(new CommentsModel() { Author = UserName, Text = WhatSay, TaskId = Convert.ToInt32(TaskId) });
            db.SaveChanges();
            await Clients.All.SendAsync($"CommentDo", "Reload");
        }

        public async Task AddRating(int Grade, string TaskId, string UserName)
        {
            TasksModel CurrentTask = new TasksModel();
            CurrentTask = db.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            CurrentTask = NewRaiting(CurrentTask.Rating, CurrentTask.SumRating + Grade, CurrentTask.SumVotes + 1, CurrentTask);
            Debug.WriteLine(Convert.ToInt32(Grade));
            Debug.WriteLine(CurrentTask.SumRating);
            UserTaskModel TaskState = new UserTaskModel();
            TaskState = db.UserTaskState.Where(x => x.TaskId == Convert.ToInt32(TaskId) && x.UserName == UserName).FirstOrDefault();
            TaskState.Voted = 1;
            db.SaveChanges();
            await Clients.All.SendAsync($"Raiting", CurrentTask.Rating);
        }


        TasksModel NewRaiting(int NewRating, int NewSumRating, int NewSumVotes, TasksModel CurrentTask)
        {
            NewRating = NewSumRating / NewSumVotes;
            CurrentTask.Rating = NewRating;
            CurrentTask.SumRating = NewSumRating;
            CurrentTask.SumVotes = NewSumVotes;
            return CurrentTask;
        }


        public async Task QuestionAnswer(string TaskId, string CurrentAnswer, string UserName)
        {
            bool Result = false;
            List<AnswersModel> AnswersList = new List<AnswersModel>();
            AnswersList = db.Answers.Where(x => x.TaskId == Convert.ToInt32(TaskId)).ToList();

            foreach (AnswersModel Answer in AnswersList)
            {
                Debug.WriteLine(Answer.Answer.ToLower());
                if (Answer.Answer.ToLower() == CurrentAnswer.ToLower())
                {
                    UserTaskModel TaskState = new UserTaskModel();
                    TaskState = db.UserTaskState.Where(x => x.TaskId == Convert.ToInt32(TaskId) && x.UserName == UserName).FirstOrDefault();
                    if (TaskState.Answered == 0)
                    {
                        TaskState.Answered = 1;
                        db.SaveChanges();
                    }
                    Debug.WriteLine(CurrentAnswer.ToLower() + "is true");
                    Result = true;
                }
            }
            await Clients.All.SendAsync($"Result", Result);
        }
        public async Task ChangeTaskName(string TaskId, string NewName)
        {
             TasksModel Task = db.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
             Task.TaskName = NewName;
             db.SaveChanges();
             await Clients.All.SendAsync($"NameChanged", Task.TaskName);
        }


        public async Task ChangeTaskType(string TaskId, string NewType)
        {
            TasksModel Task = db.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            Task.Type = NewType;
            db.SaveChanges();
            await Clients.All.SendAsync($"TypeChanged", Task.Type);
        }


        public async Task ChangeTaskCondition(string TaskId, string NewCondition)
        {
            TasksModel Task = db.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            Task.Condition = NewCondition;
            db.SaveChanges();
            await Clients.All.SendAsync($"ConditionChanged", Task.Condition);
        }


        public async Task ChangeTaskAnswers(string TaskId, string FirstAnswer, string SecondAnswer, string ThirdAnswer)
        {
            int Id = Convert.ToInt32(TaskId);
            List<AnswersModel> AnswersForDelete = db.Answers.Where(x => x.TaskId == Id).ToList();

            string[] AnswersForAdd = new string[] { FirstAnswer, SecondAnswer, ThirdAnswer };
            foreach (AnswersModel Answer in AnswersForDelete)
            {
                db.Answers.Remove(Answer);
            }
            foreach (string Answer in AnswersForAdd)
            {
                if(Answer.Length>0)
                db.Add(new AnswersModel() {Answer = Answer, TaskId = Id});
            }
            db.SaveChanges();

           await Clients.All.SendAsync($"AnswersChanged", FirstAnswer, SecondAnswer, ThirdAnswer);
        }


        public async Task ChangeTaskTags(string TaskId, string NewTags)
        {
            int Id = Convert.ToInt32(TaskId);
            List<TaskTagModel> TagsForDelete = db.TaskTag.Where(x => x.TaskId == Id).ToList();
            foreach (TaskTagModel Tag in TagsForDelete)
            {
                db.TaskTag.Remove(Tag);
            }
            db.SaveChanges();
            CreateTags TagsCreator = new CreateTags(NewTags, Id, db);
            await Clients.All.SendAsync($"TagsChanged");
        }

    }
}
