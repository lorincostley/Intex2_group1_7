using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Intex2.Controllers
{
    public class HomeController : Controller
    {

        private ILegoRepository _repo;

        public HomeController(ILegoRepository temp)
        {
            _repo = temp;
        } 

        [Authorize]
        public IActionResult Secrets()
        {
            return View();
        }

        public IActionResult ProductList(int pageNum, string? productType)
        {
            int pageSize = 9;

            var blah = new ProductListViewModel
            {
                Products = _repo.Products
                    .Where(x => x.Category == productType || productType == null)
                    .OrderBy(x => x.Name)
                    .Skip(pageSize * (pageNum - 1))
                    .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = productType == null ? _repo.Products.Count() : _repo.Products.Where(x => x.Category == productType).Count()
                },

                CurrentProductType = productType
            };
            return View(blah);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult AddCartItem()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Confirmation()
        {
            return View();
        }

        public IActionResult Confirmation_ContactUs()
        {
            return View();
        }

        public IActionResult Confirmation_NeedsVerification()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Help()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Help(string comment)
        {
            return RedirectToAction("Confirmation_ContactUs");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string var)
        {
            return RedirectToAction("SignUp");
        }

        public IActionResult ProductDetails()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
