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




        public async Task QuestionAnswer(string id, string CurrentAnswer, string UserName)
        {
            bool Result = false;
            List<AnswersModel> AnswersList = new List<AnswersModel>();
            AnswersList = db.Answers.Where(x => x.TaskId == Convert.ToInt32(id)).ToList();
  
            foreach (AnswersModel Answer in AnswersList)
            {
                Debug.WriteLine(Answer.Answer.ToLower());
                if(Answer.Answer.ToLower() == CurrentAnswer.ToLower())
                {
                    Debug.WriteLine(CurrentAnswer.ToLower() + "is true");
                    Result = true;
                }
            }
            await Clients.All.SendAsync($"Result", Result);
        }
    }
}
