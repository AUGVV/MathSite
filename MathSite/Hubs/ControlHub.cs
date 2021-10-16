using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (isNotLiked(CommentId, UserName))
            {
                DataBase.CommentsState.Add(new CommentsStateModel() { CommentId = CommentId, User = UserName });
                CommentsModel Comment = GetCurrentComment(CommentId);
                if (DisOrLike == "like")
                {
                    NewLikeCount = AddLike(Comment);
                }
                else if (DisOrLike == "dislike")
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

        public async Task NightMode(bool State)
        {

            if (State == true)
            {


            }
            else if (State == false)
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
            TasksModel CurrentTask = GetTask(Convert.ToInt32(TaskId));
            CurrentTask = NewRaiting(CurrentTask.Rating, CurrentTask.SumRating + Grade, CurrentTask.SumVotes + 1, CurrentTask);
            UserTaskModel TaskState = GetUserTaskState(Convert.ToInt32(TaskId), UserName);
            TaskState.isVoted = true;
            DataBase.SaveChanges();
            await Clients.Caller.SendAsync($"Raiting", CurrentTask.Rating);
        }

        public async Task QuestionAnswer(string TaskId, string CurrentAnswer, string UserName)
        {
            bool Result = false;
            int CurrentId = Convert.ToInt32(TaskId);

            foreach (AnswersModel Answer in GetAnswersList(CurrentId))
            {
                if (Answer.Answer.ToLower() == CurrentAnswer.ToLower())
                {
                    SaveAnswerToUser(CurrentId, UserName);
                    Result = true;
                    break;
                }
            }
            await Clients.Caller.SendAsync($"Result", Result);
        }

        public async Task ChangeTaskName(string TaskId, string NewName)
        {
            TasksModel Task = GetTask(Convert.ToInt32(TaskId));
            Task.TaskName = NewName;
            DataBase.SaveChanges();
            await Clients.Caller.SendAsync($"NameChanged", Task.TaskName);
        }

        public async Task ChangeTaskType(string TaskId, string NewType)
        {
            TasksModel Task = GetTask(Convert.ToInt32(TaskId));
            Task.Type = NewType;
            DataBase.SaveChanges();
            await Clients.Caller.SendAsync($"TypeChanged", Task.Type);
        }

        public async Task ChangeTaskCondition(string TaskId, string NewCondition)
        {
            TasksModel Task = GetTask(Convert.ToInt32(TaskId));
            Task.Condition = NewCondition;
            DataBase.SaveChanges();
            await Clients.Caller.SendAsync($"ConditionChanged", Task.Condition);
        }

        public async Task ChangeTaskAnswers(string TaskId, string FirstAnswer, string SecondAnswer, string ThirdAnswer)
        {
            int CurrentId = Convert.ToInt32(TaskId);
            string[] AnswersForAdd = new string[] { FirstAnswer, SecondAnswer, ThirdAnswer };

            RemoveOldAnswers(CurrentId);
            AddNewAnswers(CurrentId, AnswersForAdd);

            await Clients.Caller.SendAsync($"AnswersChanged", FirstAnswer, SecondAnswer, ThirdAnswer);
        }

        public async Task ChangeTaskTags(string TaskId, string NewTags)
        {
            int CurrentId = Convert.ToInt32(TaskId);
            DeleteTags(CurrentId);
            CreateNewTags(CurrentId, NewTags);
            await Clients.Caller.SendAsync($"TagsChanged");
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

  
        private void DeleteTags(int ChoisedId)
        {
            DeleteTags DeleteTags = new DeleteTags(DataBase);
            DeleteTags.Delete(ChoisedId);
        }

        private UserTaskModel GetUserTaskState(int TaskId, string UserName)
        {
            return DataBase.UserTaskState.Where(x => x.TaskId == TaskId && x.UserName == UserName).FirstOrDefault();
        }

        private CommentsModel GetCurrentComment(int CommentId)
        {
            return DataBase.Comments.Where(x => x.Id == CommentId).FirstOrDefault();
        }

        private bool isNotLiked(int CommentId, string UserName)
        {
            if (DataBase.CommentsState.Where(x => x.CommentId == CommentId && x.User == UserName).FirstOrDefault() == null)
            {
                return true;
            }
            return false;
        }

        private int AddLike(CommentsModel Comment)
        {
            Comment.Like += 1;
            DataBase.SaveChanges();
            return Comment.Like;
        }

        private int AddDislike(CommentsModel Comment)
        {
            Comment.Dislike += 1;
            DataBase.SaveChanges();
            return Comment.Dislike;
        }

        private TasksModel NewRaiting(int NewRating, int NewSumRating, int NewSumVotes, TasksModel CurrentTask)
        {
            NewRating = NewSumRating / NewSumVotes;
            CurrentTask.Rating = NewRating;
            CurrentTask.SumRating = NewSumRating;
            CurrentTask.SumVotes = NewSumVotes;
            return CurrentTask;
        }

        private void SaveAnswerToUser(int CurrentId, string UserName)
        {
            UserTaskModel TaskState = GetUserTaskState(CurrentId, UserName);
            if (!TaskState.isAnswered)
            {
                TaskState.isAnswered = true;
                DataBase.SaveChanges();
            }
        }
        private void RemoveOldAnswers(int CurrentId)
        {
            foreach (AnswersModel Answer in GetAnswersList(CurrentId))
            {
                DataBase.Answers.Remove(Answer);
            }
        }

        private void AddNewAnswers(int CurrentId, string[] AnswersForAdd)
        {
            foreach (string Answer in AnswersForAdd)
            {
                if (Answer.Length > 0)
                    DataBase.Add(new AnswersModel() { Answer = Answer, TaskId = CurrentId });
            }
            DataBase.SaveChanges();
        }

        private List<AnswersModel> GetAnswersList(int CurrentId)
        {
            return DataBase.Answers.Where(x => x.TaskId == CurrentId).ToList();
        }

        private void CreateNewTags(int TaskId, string NewTags)
        {
            CreateTags TagsCreator = new CreateTags(DataBase);
            TagsCreator.Create(NewTags, TaskId);
        }

        private TasksModel GetTask(int TaskId)
        {
            return DataBase.Tasks.Where(x => x.Id == TaskId).FirstOrDefault();
        }

        private void AddPictures(int TaskId, string Pictures)
        {
            UploadPictures UploadPictures = new UploadPictures(DataBase);
            UploadPictures.Upload(TaskId, Pictures);
        }

        private void DeleteTaskPictures(string Images)
        {
            DeletePictures DeletePictures = new DeletePictures(DataBase);
            DeletePictures.Delete(Images);
        }
    }
}
