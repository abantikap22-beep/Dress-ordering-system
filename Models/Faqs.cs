using System.ComponentModel.DataAnnotations;

namespace dress_ordering_system.Models
{
    public class Faqs
    {
        [Key]
        public int faq_id { get; set; }
        public string faq_question { get; set; }
        public string faq_answer { get; set; }
    }
}
