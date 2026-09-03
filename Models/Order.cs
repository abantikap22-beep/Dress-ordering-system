using System;
using System.ComponentModel.DataAnnotations;

namespace dress_ordering_system.Models
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }

        public int cust_id { get; set; }

        public DateTime order_date { get; set; }

        public decimal total_amount { get; set; }

        public string status { get; set; }
    }
}
