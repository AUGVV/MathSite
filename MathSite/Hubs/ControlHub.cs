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
                if(Answer.Answer.ToLower() == CurrentAnswer.ToLower())
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


        
    }
}
