using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dress_ordering_system.Models
{
    public class Cart
    {
        [Key]
        public int cart_id { get; set; }

        public int dress_id { get; set; }

        public int cust_id { get; set; }

        // Quantity selected by customer
        public int dress_quantity { get; set; }

        public int cart_status { get; set; }

        [ForeignKey("dress_id")]
        public Dress products { get; set; }

        [ForeignKey("cust_id")]
        public Customer customers { get; set; }
    }
}