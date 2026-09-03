using System.ComponentModel.DataAnnotations;

namespace dress_ordering_system.Models
{
    public class Dress
    {
        [Key]
        public int dress_id { get; set; }
        public string dress_name { get; set; }
        public string dress_price { get; set; }
        public string dress_description{ get; set; }
        public string dress_image { get; set; }
        public int cat_id { get; set; }
        public Category Category { get; set; }

        public int dress_quantity { get; set; }


    }
}
