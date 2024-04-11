namespace Intex2.Models
{
    public interface ILegoRepository
    {
        public IQueryable<Product> Products { get; }
        public IQueryable<Order> Orders { get; }
        public IQueryable<Customer> Customers { get; }
        public void AdminDeleteProduct(Product product);
        public void AdminUpdateProduct(Product product);
        public void AdminUpdateCustomer(Customer customer);
        public void AdminDeleteCustomer(Customer customer);

    }
}
