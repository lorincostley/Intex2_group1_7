using Intex2.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

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

        public void AddProduct(Product product)
        {
            /*            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Products ON");
            */
            _context.Add(product);
            _context.SaveChanges();
            /*            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Products OFF");
            */
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
        public void AddCustomer(Customer customer)
        {
            _context.Add(customer);
            _context.SaveChanges();
        }
        public void AddOrder(Order order)
        {
            _context.Add(order);
            _context.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            _context.Update(order);
            _context.SaveChanges();
        }

        public void AdminUpdateOrder(Order order)
        {
            _context.Update(order);
            _context.SaveChanges();

        }

        public void AdminDeleteOrder(Order order)
        {
            _context.Remove(order);
            _context.SaveChanges();
        }

        public void DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }

        public Order AdminEditOrder(int id)
        {
            var recordToDelete = _context.Orders
                .FirstOrDefault(x => x.TransactionId == id);

            return recordToDelete;
        }
    }
}

