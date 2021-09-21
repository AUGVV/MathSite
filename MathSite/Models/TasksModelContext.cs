using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models
{
    public class TasksModelContext : DbContext
    {

        public DbSet<TasksModel> Tasks { get; set; }
        public TasksModelContext(DbContextOptions<TasksModelContext> options) : base(options)
        {
            Database.EnsureCreated();
        }



    }
}
