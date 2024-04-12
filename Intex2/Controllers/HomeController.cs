using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Humanizer;
using Microsoft.AspNetCore.Http;
using System.Drawing.Printing;
using System.Drawing;
using Microsoft.AspNetCore.Identity;

namespace Intex2.Controllers 
{
    public class HomeController : Controller
    {

        private ILegoRepository _repo;
        private readonly InferenceSession _session;
        private readonly string _onnxModelPath;
        private readonly Cart _cart;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILegoRepository temp, IHostEnvironment hostEnvironment, Cart cart, UserManager<IdentityUser> userManager)
        {
            _repo = temp;

            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.ContentRootPath, "wwwroot\\wwwroot\\gradient_model.onnx");
            _session = new InferenceSession(_onnxModelPath);
            _cart = cart;
            _userManager = userManager;
        } 

        [Authorize]
        public IActionResult Secrets()
        {
            return View();
        }

        //public ActionResult ProductDetails(int ProductId, string Name, string ImgLink, int Price, string Description)
        //{

        //    var product = new Product
        //    {
        //        ProductId = ProductId,
        //        Name = Name,
        //        ImgLink = ImgLink,
        //        Price = Price,
        //        Description = Description
        //    };

        //    var blah = new ItemRecommendationViewModel
        //    {
        //        Products = product,
        //    };

        //    return View(product);
        //}
        public IActionResult ProductDetails(int id)
        {
            var product = _repo.Products
              .FirstOrDefault(x => x.ProductId == id);

            ViewBag.Product = product;

            var rec = _repo.recommendations
              .Where(x => x.primary_product_ID == id).FirstOrDefault();

            int[] recommendationIds =  new[]
            {
                rec.recommendation_1,
                rec.recommendation_2,
                rec.recommendation_3,
                rec.recommendation_4
              };

            List<Product> recommendationProducts = _repo.Products
              .Where(x => recommendationIds.Contains(x.ProductId))
              .ToList();

            ViewBag.Recommendations = recommendationProducts;

            return View();
        }

        public IActionResult ProductList(int pageNum, string? productType, string color, int pageSize = 5)
        {
            //int pageSize = 9;
            if (pageNum < 1)
            {
                pageNum = 1;
            }
            var blah = new ProductListViewModel
            {
                Products = _repo.Products
                .Where(x => (x.Category == productType || x.PrimaryColor == color) || (productType == null && color == null))
                .OrderBy(x => x.Name)
                .Skip(pageSize * (pageNum - 1))
                .Take(pageSize),
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = (productType == null && color == null) ? _repo.Products.Count() :
                 _repo.Products.Count(x => (x.Category == productType || x.PrimaryColor == color))
                },
                CurrentProductType = productType,
                CurrentColor = color
            };
            return View(blah);
        }

        [HttpPost]
        public IActionResult Predict(OrderPredictionViewModel OrderModel)
        {
            
            int id = _repo.Orders.Max(order => order.TransactionId);

            id++;
            
            OrderModel.Orders.TransactionId = id;

            if (OrderModel.Orders.CustomerId == null)
            {
                OrderModel.Orders.CustomerId = 29135;
            }


            string country = OrderModel.Customer.CountryOfResidence;
            OrderModel.Orders.ShippingAddress = country;


            // Get the current date and time
            DateTime now = DateTime.Now;

            DateOnly dateOnly = DateOnly.FromDateTime(DateTime.Today);

            // Get the day of the week (e.g., "Tues")
            string day = now.ToString("ddd");

            // Get the time as an integer (e.g., 13 for 1:00 PM)
            int time = now.Hour;

            OrderModel.Orders.Date = dateOnly;
            OrderModel.Orders.Time = time;
            OrderModel.Orders.DayOfWeek = day;

            string place = "USA";

            OrderModel.Orders.CountryOfTransaction = place;

            var input = new List<float>
                {
                    (float)time,
                    (float)OrderModel.Orders.Amount,

                    OrderModel.Orders.DayOfWeek == "Mon" ? 1 : 0,
                    OrderModel.Orders.DayOfWeek == "Sat" ? 1 : 0,
                    OrderModel.Orders.DayOfWeek == "Sun" ? 1 : 0,
                    OrderModel.Orders.DayOfWeek == "Thu" ? 1 : 0,
                    OrderModel.Orders.DayOfWeek == "Tue" ? 1 : 0,
                    OrderModel.Orders.DayOfWeek == "Wed" ? 1 : 0,


                    OrderModel.Orders.EntryMode == "PIN" ? 1 : 0,
                    OrderModel.Orders.EntryMode == "Tap" ? 1 : 0,

                    OrderModel.Orders.TypeOfCard == "Online" ? 1 : 0,
                    OrderModel.Orders.TypeOfCard == "POS" ? 1 : 0,


                    OrderModel.Orders.CountryOfTransaction == "India" ? 1 : 0,
                    OrderModel.Orders.CountryOfTransaction == "Russia" ? 1 : 0,
                    OrderModel.Orders.CountryOfTransaction == "USA" ? 1 : 0,
                    OrderModel.Orders.CountryOfTransaction == "UnitedKingdom" ? 1 : 0,


                    OrderModel.Orders.ShippingAddress == "India" ? 1 : 0,
                    OrderModel.Orders.ShippingAddress == "Russia" ? 1 : 0,
                    OrderModel.Orders.ShippingAddress == "USA" ? 1 : 0,
                    OrderModel.Orders.ShippingAddress == "UnitedKingdom" ? 1 : 0,

                    OrderModel.Orders.Bank == "HSBC" ? 1 : 0,
                    OrderModel.Orders.Bank == "Halifax" ? 1 : 0,
                    OrderModel.Orders.Bank == "Lloyds" ? 1 : 0,
                    OrderModel.Orders.Bank == "Metro" ? 1 : 0,
                    OrderModel.Orders.Bank == "Monzo" ? 1 : 0,
                    OrderModel.Orders.Bank == "RBS" ? 1 : 0,


                    OrderModel.Orders.TypeOfCard == "Visa" ? 1 : 0,
            };




            //var input = new List<float> { time, amount, feathers, mon, sat, sun, thu, tue, wed, pin, tap, online, pos, india, russia, usa, uk, hsbc, halifax, lloyds, metro, monzo, rbs, visa };
            var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

            var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            var view = "Confirmation";

            using (var results = _session.Run(inputs)) // makes the prediction with the inputs from the form (i.e. class_type 1-7)
            {
                var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                if (prediction != null)
                {
                    var fraudprediction = (int)prediction[0];

                    if (fraudprediction == 1)
                    {
                        view = "Confirmation_NeedsVerification";
                        OrderModel.Orders.Fraud = 1;
                    }
                    else {OrderModel.Orders.Fraud = 0; }
                }
                else { OrderModel.Orders.Fraud = 0; }

            }



            // Save the order object to the database
            _repo.AddOrder(OrderModel.Orders);
            _cart.Clear();
            return View(view);
        }

        public async Task<IActionResult> IndexAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null && user.Email == "legomasterBETH@example.com")
            {
                var blah = new ProductRecommendViewModel
                {
                    Products = _repo.Products
                               .Where(x => _repo.user_Recommendations.Select(tr => tr.product_ID).Contains(x.ProductId))
                };

                return View(blah);
            }
            else
            {
                var blah = new ProductRecommendViewModel
                {
                    Products = _repo.Products
                               .Where(x => _repo.top_Ratings.Select(tr => tr.product_ID).Contains(x.ProductId))
                };

                return View(blah);
            }
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
        public IActionResult SignUp()
        {
            return View();
        }

        //public IActionResult Admin_Orders()
        //{
        //    var records = _repo.Orders
        //        .OrderByDescending(o => o.Date)
        //        .Take(25)
        //        .ToList();
        //    var predictions = new List<OrderPredictionViewModel>(); //Viewmodel for the view

        //    var class_type_dict = new Dictionary<int, string>
        //    {
        //        { 0, "Not Fraud" },
        //        { 1, "Fraud" }
        //    };

        //    foreach (var record in records)
        //    {

        //        var input = new List<float>
        //        {
        //            (float)record.Time,
        //            (float)(record.Amount ?? 0),

        //            record.DayOfWeek == "Mon" ? 1 : 0,
        //            record.DayOfWeek == "Tue" ? 1 : 0,
        //            record.DayOfWeek == "Wed" ? 1 : 0,
        //            record.DayOfWeek == "Thu" ? 1 : 0,
        //            record.DayOfWeek == "Sat" ? 1 : 0,
        //            record.DayOfWeek == "Sun" ? 1 : 0,

        //            record.EntryMode == "PIN" ? 1 : 0,
        //            record.EntryMode == "Tap" ? 1 : 0,

        //            record.TypeOfTransaction == "Online" ? 1 : 0,
        //            record.TypeOfTransaction == "POS" ? 1 : 0,


        //            record.CountryOfTransaction == "India" ? 1 : 0,
        //            record.CountryOfTransaction == "Russia" ? 1 : 0,
        //            record.CountryOfTransaction == "USA" ? 1 : 0,
        //            record.CountryOfTransaction == "UnitedKingdom" ? 1 : 0,

        //            (record.ShippingAddress ?? record.CountryOfTransaction) == "India" ? 1 : 0,
        //            (record.ShippingAddress ?? record.CountryOfTransaction) == "Russia" ? 1 : 0,
        //            (record.ShippingAddress ?? record.CountryOfTransaction) == "USA" ? 1 : 0,
        //            (record.ShippingAddress ?? record.CountryOfTransaction) == "UnitedKingdom" ? 1 : 0,

        //            record.Bank == "HSBC" ? 1 : 0,
        //            record.Bank == "Halifax" ? 1 : 0,
        //            record.Bank == "Lloyds" ? 1 : 0,
        //            record.Bank == "Metro" ? 1 : 0,
        //            record.Bank == "Monzo" ? 1 : 0,
        //            record.Bank == "RBS" ? 1 : 0,

        //            record.TypeOfCard == "Visa" ? 1 : 0
        //        };

        //        var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count } );

        //        var inputs = new List<NamedOnnxValue>
        //        {
        //            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        //        };

        //        string predictionResult;
        //        using (var results = _session.Run(inputs))
        //        {
        //            var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
        //            predictionResult = prediction != null && prediction.Length > 0 ? class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown") : "Error in prediction";
        //        }

        //        predictions.Add(new OrderPredictionViewModel { Orders = record, Prediction = predictionResult });

        //    }


        //    return View(predictions);
        //}

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public IActionResult Checkout(decimal? totalPrice)
        {
            // Use totalPrice as needed
            ViewBag.TotalPrice = totalPrice;
            return View();
        }
        public IActionResult ClearCart(string returnUrl)
        {
            // Get the cart from session
            var cart = HttpContext.Session.GetJson<Cart>("Cart") ?? new Cart();

            // Clear the cart
            cart.Clear();

            // Update the cart in session
            HttpContext.Session.SetJson("Cart", cart);

            return RedirectToAction("Index", new { returnUrl = returnUrl });
        }
    }
}
