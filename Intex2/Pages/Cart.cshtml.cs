using Intex2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Intex2.Infrastructure;

namespace Intex2.Pages
{
    public class CartModel : PageModel
    {
        private ILegoRepository _repo;
        public Cart? Cart { get; set; }

        public CartModel(ILegoRepository temp, Cart cartService)
        {
            _repo = temp;
            Cart = cartService;
        }

        public string ReturnUrl { get; set; } = "/";
        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";
        }

        public IActionResult OnPost(int productId, string returnUrl)
        {
            Product proj = _repo.Products
                .FirstOrDefault(x => x.ProductId == productId);

            if (proj != null)
            {
                Cart.AddItem(proj, 1);
            }

            return RedirectToPage(new { returnUrl = returnUrl });
        }

        public IActionResult OnPostRemove(int productId, string returnUrl)
        {
            Cart.RemoveLine(Cart.Lines.First(x => x.Product.ProductId == productId).Product);
            return RedirectToPage(new { returnUrl = returnUrl });
        }

        public Product Product { get; set; }
    }
}
