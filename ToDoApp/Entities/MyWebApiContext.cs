using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Entities;
using RabbitMQ.Client;

namespace ToDoApp.Contexts
{
    public class MyWebApiContext : DbContext
    {
        public MyWebApiContext(DbContextOptions<MyWebApiContext> options) : base(options) { }
        public DbSet<Article> Articles { get; set; }
        public DbSet<User> Users { get; set; }

        public IModel RabbitMQChannel { get; set; }
    }
}

