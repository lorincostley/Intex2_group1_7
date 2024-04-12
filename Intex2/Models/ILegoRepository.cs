namespace Intex2.Models
{
    public interface ILegoRepository
    {
        public IQueryable<Product> Products { get; }
        public IQueryable<Order> Orders { get; }
        public IQueryable<Customer> Customers { get; }
        public IQueryable<Top_Rating> top_Ratings { get; }
        public IQueryable<User_Recommendation> user_Recommendations { get; }
        public void AdminDeleteProduct(Product product);
        public void AdminUpdateProduct(Product product);
        public void AdminUpdateCustomer(Customer customer);
        public void AdminDeleteCustomer(Customer customer);
        public void AddOrder(Order order);
        public void UpdateOrder(Order order);
        public void DeleteOrder(Order order);
        public void AdminDeleteOrder(Order order);
        public void AddProduct(Product newProduct);
        public void AddCustomer(Customer customer);

        void AdminUpdateOrder(Order updatedInfo);
        public IQueryable<Recommendation> recommendations { get; }
    }
}
