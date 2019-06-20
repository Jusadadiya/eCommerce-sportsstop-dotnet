using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sportsstop.Models
{
    public class ItemComment
    {
        public int Id { get; set; }
        public int Rating {get;set;}
        public string Comment {get;set;}
        public string ImagePath { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
    }
}
