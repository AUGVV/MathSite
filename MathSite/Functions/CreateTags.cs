﻿using MathSite.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Functions
{
    public class CreateTags
    {

        private TasksContext db;

        public CreateTags(string Tags, int Id, TasksContext context)
        {
            db = context;
            string[] SplitTags = Tags.ToLower().Split('#');

            foreach (string Tag in SplitTags)
            {
                if(db.Tags.Where(x=>x.TagName == Tag).FirstOrDefault() == null)
                {
                    db.Tags.Add(new TagsModel() { TagName = Tag });
                    db.SaveChanges();
                }

                TagsModel CurrentTag = db.Tags.Where(x => x.TagName == Tag).FirstOrDefault();
                CurrentTag.Count += 1; 
                db.TaskTag.Add(new TaskTagModel() { Tag = CurrentTag.Id, TaskId = Id });
                db.SaveChanges();
            }
        }
    }
}