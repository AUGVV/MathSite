using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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


        public async Task DisLikeComment(int CommentId, string UserName, string DisOrLike)
        {
            int NewLikeCount = 0;
            if (db.CommentsState.Where(x=>x.CommentId == CommentId && x.User == UserName).FirstOrDefault() == null)
            {
                db.CommentsState.Add(new CommentsStateModel() { CommentId = CommentId, User = UserName });
                CommentsModel Comment = db.Comments.Where(x => x.Id == CommentId).FirstOrDefault();
                if (DisOrLike == "like")
                {
                    Comment.Like += 1;
                    db.SaveChanges();
                    NewLikeCount = Comment.Like;
                }
                else if(DisOrLike == "dislike")
                {
                    Comment.Dislike += 1;
                    db.SaveChanges();
                    NewLikeCount = Comment.Dislike;
                }               
            }
            else
            {
                DisOrLike = "";
            }    
            await Clients.All.SendAsync($"CommentLiked", CommentId, NewLikeCount, DisOrLike);
        }




        public async Task NightMode(bool State)
        {
           
            if (State == true)
            {
             

            }
            else if(State == false)
            {


            }        
        }


        public async Task AddToComment(string TaskId, string WhatSay, string UserName)
        {
            CommentsModel Comment = new CommentsModel() { Author = UserName, Text = WhatSay, TaskId = Convert.ToInt32(TaskId) };
            db.Comments.Add(Comment);
            db.SaveChanges();
            await Clients.All.SendAsync($"CommentsDo", TaskId, Comment.Id, UserName, WhatSay);
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
            await Clients.Caller.SendAsync($"Raiting", CurrentTask.Rating);
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
            await Clients.Caller.SendAsync($"Result", Result);
        }
        public async Task ChangeTaskName(string TaskId, string NewName)
        {
             TasksModel Task = db.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
             Task.TaskName = NewName;
             db.SaveChanges();
             await Clients.Caller.SendAsync($"NameChanged", Task.TaskName);
        }


        public async Task ChangeTaskType(string TaskId, string NewType)
        {
            TasksModel Task = db.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            Task.Type = NewType;
            db.SaveChanges();
            await Clients.Caller.SendAsync($"TypeChanged", Task.Type);
        }


        public async Task ChangeTaskCondition(string TaskId, string NewCondition)
        {
            TasksModel Task = db.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            Task.Condition = NewCondition;
            db.SaveChanges();
            await Clients.Caller.SendAsync($"ConditionChanged", Task.Condition);
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

           await Clients.Caller.SendAsync($"AnswersChanged", FirstAnswer, SecondAnswer, ThirdAnswer);
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
            await Clients.Caller.SendAsync($"TagsChanged");
        }


        public async Task ChangePictures(string TaskId, string NewPictures, string DeletePictures)
        {
            if (DeletePictures != "")
            {
                DeletePictures DelPicture = new DeletePictures(DeletePictures, db);
            }
            if (NewPictures != "")
            {
                UploadPictures UploadPictures = new UploadPictures(NewPictures, Convert.ToInt32(TaskId), db);
            }
            await Clients.Caller.SendAsync($"PictureChanged");
        }

    }
}
