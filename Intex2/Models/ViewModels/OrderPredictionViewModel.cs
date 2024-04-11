namespace Intex2.Models.ViewModels
{
    public class OrderPredictionViewModel
    {
        public Order Orders { get; set; }
        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
        public string Prediction { get; set; }
    }
}
