namespace Intex2.Models.ViewModels
{
    public class OrderPredictionViewModel
    {
        public Order Orders { get; set; }
        public Customer Customer { get; set; }
        public string Prediction { get; set; }
        public string ReturnUrl { get; set; }
    }
}
