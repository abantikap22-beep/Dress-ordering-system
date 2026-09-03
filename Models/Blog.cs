using System;
using System.ComponentModel.DataAnnotations;

namespace dress_ordering_system.Models
{
    public class Blog
    {
        [Key]
        public int blog_id { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public DateTime created_date { get; set; }
    }
}
