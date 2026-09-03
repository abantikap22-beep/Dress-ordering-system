using System.ComponentModel.DataAnnotations;

namespace dress_ordering_system.Models
{
    public class Category
    {
        [Key]
        public int category_id { get; set; }
        public string category_name { get; set; }
        public List<Dress> Dress { get; set; }

    }

}
