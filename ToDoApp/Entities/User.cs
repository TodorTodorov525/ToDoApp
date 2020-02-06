using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using ToDoApp.Entities;

namespace ToDoApp.Contexts
{
    public class User 
    {
       public int Id { get; set; }
       public string Username { get; set; }

        [JsonIgnore] 
        public List<Article> Articles { get; set; }
    }
}
