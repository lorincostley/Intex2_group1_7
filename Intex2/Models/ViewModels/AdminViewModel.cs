using System.Linq;

namespace Intex2.Models.ViewModels
{
    public class AdminViewModel
    {
        public IQueryable<Customer> Customers { get; set; }
        public IQueryable<Product> Products { get; set; }
        public IQueryable<Order> Orders { get; set; }

        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
        public Product Product { get; internal set; }
    }
}