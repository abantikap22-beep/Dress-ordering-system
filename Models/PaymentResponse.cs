namespace dress_ordering_system.Models
{
    public class PaymentResponse
    {
        public string status { get; set; }

        public string failedreason { get; set; }

        public string sessionkey { get; set; }

        public string GatewayPageURL { get; set; }

        public string redirectGatewayURL { get; set; }
    }
}