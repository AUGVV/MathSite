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

        private TasksContext DataBase;

        public ControlHub(TasksContext context)
        {
            DataBase = context;
        }

        public async Task DisLikeComment(int CommentId, string UserName, string DisOrLike)
        {
            int NewLikeCount = 0;
            if (DataBase.CommentsState.Where(x=>x.CommentId == CommentId && x.User == UserName).FirstOrDefault() == null)
            {
                DataBase.CommentsState.Add(new CommentsStateModel() { CommentId = CommentId, User = UserName });
                CommentsModel Comment = DataBase.Comments.Where(x => x.Id == CommentId).FirstOrDefault();
                if (DisOrLike == "like")
                {
                    NewLikeCount = AddLike(Comment);
                }
                else if(DisOrLike == "dislike")
                {
                    NewLikeCount = AddDislike(Comment);
                }               
            }
            else
            {
                DisOrLike = "";
            }    
            await Clients.All.SendAsync($"CommentLiked", CommentId, NewLikeCount, DisOrLike);
        }

        int AddLike(CommentsModel Comment)
        {
            Comment.Like += 1;
            DataBase.SaveChanges();
            return Comment.Like;
        }

        int AddDislike(CommentsModel Comment)
        {
            Comment.Dislike += 1;
            DataBase.SaveChanges();
            return Comment.Dislike;
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
            DataBase.Comments.Add(Comment);
            DataBase.SaveChanges();
            await Clients.All.SendAsync($"CommentsDo", TaskId, Comment.Id, UserName, WhatSay);
        }

        public async Task AddRating(int Grade, string TaskId, string UserName)
        {
            TasksModel CurrentTask = DataBase.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            CurrentTask = NewRaiting(CurrentTask.Rating, CurrentTask.SumRating + Grade, CurrentTask.SumVotes + 1, CurrentTask);
            UserTaskModel TaskState = DataBase.UserTaskState.Where(x => x.TaskId == Convert.ToInt32(TaskId) && x.UserName == UserName).FirstOrDefault();
            TaskState.isVoted = true;
            DataBase.SaveChanges();
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
            List<AnswersModel> AnswersList = DataBase.Answers.Where(x => x.TaskId == Convert.ToInt32(TaskId)).ToList();

            foreach (AnswersModel Answer in AnswersList)
            {
                if (Answer.Answer.ToLower() == CurrentAnswer.ToLower())
                {
                    UserTaskModel TaskState = DataBase.UserTaskState.Where(x => x.TaskId == Convert.ToInt32(TaskId) && x.UserName == UserName).FirstOrDefault();
                    if (!TaskState.isAnswered)
                    {
                        TaskState.isAnswered = true;
                        DataBase.SaveChanges();
                    }
                    Result = true;
                }
            }
            await Clients.Caller.SendAsync($"Result", Result);
        }

        public async Task ChangeTaskName(string TaskId, string NewName)
        {
             TasksModel Task = DataBase.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
             Task.TaskName = NewName;
             DataBase.SaveChanges();
             await Clients.Caller.SendAsync($"NameChanged", Task.TaskName);
        }

        public async Task ChangeTaskType(string TaskId, string NewType)
        {
            TasksModel Task = DataBase.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            Task.Type = NewType;
            DataBase.SaveChanges();
            await Clients.Caller.SendAsync($"TypeChanged", Task.Type);
        }

        public async Task ChangeTaskCondition(string TaskId, string NewCondition)
        {
            TasksModel Task = DataBase.Tasks.Where(x => x.Id == Convert.ToInt32(TaskId)).FirstOrDefault();
            Task.Condition = NewCondition;
            DataBase.SaveChanges();
            await Clients.Caller.SendAsync($"ConditionChanged", Task.Condition);
        }

        public async Task ChangeTaskAnswers(string TaskId, string FirstAnswer, string SecondAnswer, string ThirdAnswer)
        {
            int Id = Convert.ToInt32(TaskId);
            List<AnswersModel> AnswersForDelete = DataBase.Answers.Where(x => x.TaskId == Id).ToList();

            string[] AnswersForAdd = new string[] { FirstAnswer, SecondAnswer, ThirdAnswer };
            foreach (AnswersModel Answer in AnswersForDelete)
            {
                DataBase.Answers.Remove(Answer);
            }
            foreach (string Answer in AnswersForAdd)
            {
                if(Answer.Length>0)
                   DataBase.Add(new AnswersModel() {Answer = Answer, TaskId = Id});
            }
            DataBase.SaveChanges();

           await Clients.Caller.SendAsync($"AnswersChanged", FirstAnswer, SecondAnswer, ThirdAnswer);
        }

        public async Task ChangeTaskTags(string TaskId, string NewTags)
        {
            int Id = Convert.ToInt32(TaskId);
            List<TaskTagModel> TagsForDelete = DataBase.TaskTag.Where(x => x.TaskId == Id).ToList();
            foreach (TaskTagModel Tag in TagsForDelete)
            {
                DataBase.TaskTag.Remove(Tag);
            }
            DataBase.SaveChanges();
            ChangeTags(Id, NewTags);
            await Clients.Caller.SendAsync($"TagsChanged");
        }

        void ChangeTags(int TaskId, string NewTags)
        {
            CreateTags TagsCreator = new CreateTags(DataBase);
            TagsCreator.Create(NewTags, TaskId);
        }

        public async Task ChangePictures(string TaskId, string NewPictures, string DeletePictures)
        {
            if (DeletePictures != "")
            {
                DeleteTaskPictures(DeletePictures);
            }
            if (NewPictures != "")
            {
                AddPictures(Convert.ToInt32(TaskId), NewPictures); 
            }
            await Clients.Caller.SendAsync($"PictureChanged");
        }

        void AddPictures(int TaskId, string Pictures)
        {
            UploadPictures UploadPictures = new UploadPictures(DataBase);
            UploadPictures.Upload(TaskId, Pictures);
        }

        void DeleteTaskPictures(string Images)
        {
            DeletePictures DeletePictures = new DeletePictures(DataBase);
            DeletePictures.Delete(Images);
        }
    }
}
