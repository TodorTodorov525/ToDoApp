using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoApp.Contexts;

namespace ToDoApp.Entities
{
    public class Article
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }


        public string Status { get; set; }

        public string Owner { get; set; }

        public int UserID { get; set; }

       public User User { get; set; }

        public static bool IsStatusValid(string[] ArrayOfStatuses, string status)
        {
            if (Array.Find(ArrayOfStatuses, element => element == status) == null)
                return false;

            return true;
        }
    }
}
