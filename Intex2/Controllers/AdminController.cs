﻿using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;
using System.Drawing.Printing;
using System.Reflection.Metadata;

namespace Intex2.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : Controller
    {
        private ILegoRepository _repo;
        private readonly InferenceSession _session;
        private readonly string _onnxModelPath;
        private LegoContext _context;
        public AdminController(ILegoRepository temp, IHostEnvironment hostEnvironment, LegoContext context)
        {
            _repo = temp;
            _context = context;

            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.ContentRootPath, "wwwroot\\wwwroot\\gradient_model.onnx");
            _session = new InferenceSession(_onnxModelPath);

        }

        public IActionResult Admin_Orders(int pageNum)
        {

            var records = _repo.Orders
                .OrderByDescending(o => o.Date)
                .Take(1000)
                .ToList();
            var predictions = new List<OrderPredictionViewModel>(); 

            var class_type_dict = new Dictionary<int, string>
            {
                { 0, "Not Fraud" },
                { 1, "Fraud" }
            };

            foreach (var record in records)
            {

                var input = new List<float>
                {
                    (float)record.Time,
                    (float)(record.Amount ?? 0),

                    record.DayOfWeek == "Mon" ? 1 : 0,
                    record.DayOfWeek == "Tue" ? 1 : 0,
                    record.DayOfWeek == "Wed" ? 1 : 0,
                    record.DayOfWeek == "Thu" ? 1 : 0,
                    record.DayOfWeek == "Sat" ? 1 : 0,
                    record.DayOfWeek == "Sun" ? 1 : 0,

                    record.EntryMode == "PIN" ? 1 : 0,
                    record.EntryMode == "Tap" ? 1 : 0,

                    record.TypeOfTransaction == "Online" ? 1 : 0,
                    record.TypeOfTransaction == "POS" ? 1 : 0,


                    record.CountryOfTransaction == "India" ? 1 : 0,
                    record.CountryOfTransaction == "Russia" ? 1 : 0,
                    record.CountryOfTransaction == "USA" ? 1 : 0,
                    record.CountryOfTransaction == "UnitedKingdom" ? 1 : 0,

                    (record.ShippingAddress ?? record.CountryOfTransaction) == "India" ? 1 : 0,
                    (record.ShippingAddress ?? record.CountryOfTransaction) == "Russia" ? 1 : 0,
                    (record.ShippingAddress ?? record.CountryOfTransaction) == "USA" ? 1 : 0,
                    (record.ShippingAddress ?? record.CountryOfTransaction) == "UnitedKingdom" ? 1 : 0,

                    record.Bank == "HSBC" ? 1 : 0,
                    record.Bank == "Halifax" ? 1 : 0,
                    record.Bank == "Lloyds" ? 1 : 0,
                    record.Bank == "Metro" ? 1 : 0,
                    record.Bank == "Monzo" ? 1 : 0,
                    record.Bank == "RBS" ? 1 : 0,

                    record.TypeOfCard == "Visa" ? 1 : 0
                };

                var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
                };

                string predictionResult;
                using (var results = _session.Run(inputs))
                {
                    var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                    predictionResult = prediction != null && prediction.Length > 0 ? class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown") : "Error in prediction";
                }

                predictions.Add(new OrderPredictionViewModel { Orders = record, Prediction = predictionResult });

            }


            return View(predictions);
        }

        //EDIT ORDER



        //DELETE ORDER
        [HttpGet]
        public IActionResult Admin_Delete_Order(int id)
        {
            var recordToDelete = _repo.Orders
                .SingleOrDefault(x => x.TransactionId == id);

            if (recordToDelete == null)
            {
                return NotFound(); // or handle the case where the product is not found
            }

            return View(recordToDelete); // Pass the Product directly to the view
        }


        [HttpPost]
        public IActionResult Admin_Delete_Order(Order order)
        {
            _repo.AdminDeleteOrder(order);
            return RedirectToAction("Admin_Orders");
        }



        //--------------------------------------------------------------------------------

        // PRODUCTS CONTROLLERS
        public IActionResult Admin_Products(int pageNum)
        {
            int pageSize = 5;
            if (pageNum < 1)
            {
                pageNum = 1;
            }
            var products = _repo.Products.ToList();
            var viewModel = new AdminViewModel
            {
                Products = _repo.Products
                            .OrderBy(x => x.ProductId)
                            .Skip(pageSize * (pageNum - 1))
                            .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = _repo.Products.Count()
                },
            };

            return View(viewModel);

        }

        //DELETE PRODUCT
        [HttpGet]
        public IActionResult Admin_Delete_Product(int id)
        {
            var recordToDelete = _repo.Products
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
            _repo.AdminDeleteProduct(product);
            return RedirectToAction("Admin_Products");
        }

        //EDIT

        [HttpGet]
        public IActionResult Admin_Edit_Product(int id)
        {
            var recordToEdit = _repo.Products
                .SingleOrDefault(x => x.ProductId == id);

            return View(recordToEdit); // Pass the Product directly to the view
        }

        [HttpPost]
        public IActionResult Admin_Edit_Product(Product updatedInfo)
        {
            _repo.AdminUpdateProduct(updatedInfo); // Update the existing record

            return RedirectToAction("Admin_Products");
        }

        [HttpGet]
        public IActionResult Admin_Add_Product()
        {
            // Create a new instance of Product to pass to the view
            var newProduct = new Product();
            return View(newProduct);
        }

        //ADD PRODUCT

        // POST: Handle the form submission for adding a new product
        [HttpPost]
        public IActionResult Admin_Add_Product(Product newProduct)
        {
            int id = _repo.Products.Max(order => order.ProductId);

            id++;

            newProduct.ProductId = id;
            //_context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Products ON");
            if (ModelState.IsValid)
            {
                _repo.AddProduct(newProduct); // Add the new product to the repository
                //_context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Products OFF");

                return RedirectToAction("Admin_Products"); // Redirect to the product list page
            }
            else
            {
                // If the model state is not valid, return the view with validation errors
                //_context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Products OFF");

                return View(newProduct);
            }
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
            var customers = _repo.Customers.ToList();
            var viewModel = new AdminViewModel
            {
                Customers = _repo.Customers
                .OrderBy(x => x.CustomerId)
                .Skip(pageSize * (pageNum - 1))
                .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = _repo.Customers.Count()
                },
            };

            return View(viewModel);

        }

        //DELETE USER
        [HttpGet]
        public IActionResult Admin_Delete_User(int id)
        {
            var recordToDelete = _repo.Customers
                .Single(x => x.CustomerId == id);

            return View(recordToDelete);
        }

        [HttpPost]
        public IActionResult Admin_Delete_User(Customer user)
        {
            _repo.AdminDeleteCustomer(user);
            return RedirectToAction("Admin_Users");
        }

        //EDIT USER
        [HttpGet]
        public IActionResult Admin_Edit_User(int id)
        {

            var recordToEdit = _repo.Customers
                .SingleOrDefault(x => x.CustomerId == id);

            return View("Admin_Edit_User", recordToEdit);
        }

        [HttpPost]
        public IActionResult Admin_Edit_User(Customer updatedInfo)
        {
            _repo.AdminUpdateCustomer(updatedInfo);

            return RedirectToAction("Admin_Users");
        }

        //ADD USER
        [HttpGet]
        public IActionResult Admin_Add_User()
        {
            // Create a new instance of Customer to pass to the view
            var newCustomer = new Customer();
            return View(newCustomer);
        }

        // POST: Handle the form submission for adding a new product
        [HttpPost]
        public IActionResult Admin_Add_User(Customer newCustomer)
        {
            short id = _repo.Customers.Max(order => order.CustomerId);

            id++;

            newCustomer.CustomerId = id;

            if (ModelState.IsValid)
            {
                _repo.AddCustomer(newCustomer); // Add the new product to the repository
                return RedirectToAction("Admin_Users"); // Redirect to the product list page
            }
            else
            {
                // If the model state is not valid, return the view with validation errors
                return View(newCustomer);
            }
        }

        //--------------------------------------------------------------------------------

    }
}

