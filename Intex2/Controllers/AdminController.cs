using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;
using System.Drawing.Printing;

namespace Intex2.Controllers
{
    //[Authorize(Policy = "RequireAdministratorRole")]
    public class AdminController : Controller
    {
        private LegoContext _context;

        public AdminController(LegoContext temp)
        {
            _context = temp;
        }

        public IActionResult Admin_Orders(int pageNum)
        {
            int pageSize = 20;
            if (pageNum < 1)
            {
                pageNum = 1;
            }
            var products = _context.Orders.ToList();
            var viewModel = new AdminViewModel
            {
                Orders = _context.Orders
                            .OrderBy(x => x.TransactionId)
                            .Skip(pageSize * (pageNum - 1))
                            .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = _context.Products.Count()
                },
            };

            return View(viewModel);

        }

        //--------------------------------------------------------------------------------

        // PRODUCTS CONTROLLERS
        public IActionResult Admin_Products(int pageNum)
        {
            int pageSize = 20;
            if (pageNum < 1)
            {
                pageNum = 1;
            }
            var products = _context.Products.ToList();
            var viewModel = new AdminViewModel
            {
                Products = _context.Products
                            .OrderBy(x => x.ProductId)
                            .Skip(pageSize * (pageNum - 1))
                            .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = _context.Products.Count()
                },
            };

            return View(viewModel);

        }

        //DELETE PRODUCT
        [HttpGet]
        public IActionResult Admin_Delete_Product(int id)
        {
            var recordToDelete = _context.Products
                .SingleOrDefault(x => x.ProductId == id);

            if (recordToDelete == null)
            {
                return NotFound(); // or handle the case where the product is not found
            }

            return View(recordToDelete); // Pass the Product directly to the view
        }


        [HttpPost]
        public IActionResult Admin_Delete_Product(Product product)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Admin_Products");
        }

        [HttpGet]
        public IActionResult Admin_Edit_Product(int id)
        {
            var recordToEdit = _context.Products
                .SingleOrDefault(x => x.ProductId == id);

            return View(recordToEdit); // Pass the Product directly to the view
        }

        [HttpPost]
        public IActionResult Admin_Edit_Product(Product updatedInfo)
        {
            _context.Update(updatedInfo); // Update the existing record
            _context.SaveChanges();
            return RedirectToAction("Admin_Products");
        }


        //--------------------------------------------------------------------------------

        // USERS CONTROLLERS
        public IActionResult Admin_Users(int pageNum)
        {
            int pageSize = 30;
                if (pageNum < 1)
            {
                pageNum = 1;
            }
            var customers = _context.Customers.ToList();
            var viewModel = new AdminViewModel
            {
                Customers = _context.Customers
                .OrderBy(x => x.CustomerId)
                .Skip(pageSize * (pageNum - 1))
                .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = _context.Customers.Count()
                },
            };

            return View(viewModel);
        }

        //DELETE USER
        [HttpGet]
        public IActionResult Admin_Delete_User(int id)
        {
            var recordToDelete = _context.Customers
                .Single(x => x.CustomerId == id);

            return View(recordToDelete);
        }

        [HttpPost]
        public IActionResult Admin_Delete_User(Product user)
        {
            _context.Products.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("Admin_Users");
        }

        //EDIT USER
        [HttpGet]
        public IActionResult Admin_Edit_User(int id)
        {

            var recordToEdit = _context.Customers
                .SingleOrDefault(x => x.CustomerId == id);

            return View("Admin_Edit_User", recordToEdit);
        }

        [HttpPost]
        public IActionResult Admin_Edit_User(Customer updatedInfo)
        {
            _context.Update(updatedInfo);
            _context.SaveChanges();
            return RedirectToAction("Admin_Users");
        }

        //--------------------------------------------------------------------------------

    }
}

