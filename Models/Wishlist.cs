using System.ComponentModel.DataAnnotations;

namespace dress_ordering_system.Models
{
    public class Wishlist
    {
        [Key]
        public int wishlist_id { get; set; }

        public int customer_id { get; set; }

        public int dress_id { get; set; }

        public Customer Customer { get; set; }

        public Dress Dress { get; set; }
    }
}