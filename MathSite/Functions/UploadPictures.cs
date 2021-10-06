using MathSite.Functions;
using MathSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace MathSite.Functions
{
    public class UploadPictures
    {
        private TasksContext db;

        public UploadPictures(string Images, int Id, TasksContext context)
        {
            db = context;
            string[] ImageList = Images.Split("|image|");
            foreach (string image in ImageList)
            {
                if (image.Length != 0)
                {
                    Uploader(image, Id);
                }
            }
        }

        public void Uploader(string image, int id)
        {
            byte[] Response;
            string json = "";
            using (var WebClient = new WebClient())
            {
                NameValueCollection parameters = new NameValueCollection();
                parameters.Add("key", "b626821a9813f418d523698ba34376d6"); //Запихнуть в secret! НЕ ЗАБЫТЬ !
                parameters.Add("image", image);
                try
                {
                    Response = WebClient.UploadValues("https://api.imgbb.com/1/upload", "POST", parameters);
                    json = Encoding.Default.GetString(Response);
                }
                catch { }
            }
            json.Root ResponseJson = JsonConvert.DeserializeObject<json.Root>(json);
            if (ResponseJson.data.url != null)
            {
                ReferenceToBase(ResponseJson.data.url, ResponseJson.data.delete_url, id);
            }
        }

        public void ReferenceToBase(string Url, string DeleteUrl, int Taskid)
        {
            db.PicturesRef.Add(new PictureRefModel() { Reference = Url, TaskId = Taskid, DeleteReference = DeleteUrl });
            db.SaveChanges();
        }
    }
}
