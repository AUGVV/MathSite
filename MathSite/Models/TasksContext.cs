using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class TasksContext : DbContext
    {
        public DbSet<AnswersModel> Answers { get; set; }
        public DbSet<CommentsModel> Comments { get; set; }
        public DbSet<TasksModel> Tasks { get; set; }
        public DbSet<PictureRefModel> PictureRefs { get; set; }
        public DbSet<TagModel> Tags { get; set; }
        public TasksContext(DbContextOptions<TasksContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
