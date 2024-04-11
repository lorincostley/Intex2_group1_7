namespace Intex2.Models
{
    public class EFLegoRepository : ILegoRepository
    {
        private LegoContext _context;
        public EFLegoRepository(LegoContext temp)
        {
            _context = temp;
        }
        public IQueryable<Product> Products => _context.Products;
        public IQueryable<Order> Orders => _context.Orders;
        public IQueryable<Customer> Customers => _context.Customers;

        public void AdminDeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public void AdminUpdateProduct(Product product)
        {
            _context.Update(product); // Update the existing record
            _context.SaveChanges();
        }

        public void AdminUpdateCustomer(Customer customer)
        {
            _context.Update(customer);
            _context.SaveChanges();
        }
        public void AdminDeleteCustomer(Customer customer)
        {
            _context.Remove(customer);
            _context.SaveChanges();
        }
    }
}

