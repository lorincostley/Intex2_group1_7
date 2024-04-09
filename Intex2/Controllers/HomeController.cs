using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        public IActionResult ProductList(int pageNum, string? projectType)
        {
            int pageSize = 2;

            var blah = new ProductListViewModel
            {
                Products = _repo.Products
                    .Where(x => x.ProductType == productType || productType == null)
                    .OrderBy(x => x.ProductName)
                    .Skip(pageSize * (pageNum - 1))
                    .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = productType == null ? _repo.Products.Count() : _repo.Products.Where(x => x.ProductType == productType).Count()
                },

                CurrentProductType = projectType
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
