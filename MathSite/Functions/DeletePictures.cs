﻿using MathSite.Functions;
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
    public class DeletePictures
    {
        private TasksContext db;

        public DeletePictures(string ImagesId, TasksContext context)
        {
            db = context;

            string[] ImageIdList = ImagesId.Split("#");

            foreach (string Id in ImageIdList)
            {
              int ImageId = Convert.ToInt32(Id);
              PictureRefModel OldPicture = db.PicturesRef.Where(x => x.Id == ImageId).FirstOrDefault();
              db.PicturesRef.Remove(OldPicture);
            }
            db.SaveChanges();
        } 
    }
}
