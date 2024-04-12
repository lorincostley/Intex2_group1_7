using Intex2.Models;
using Intex2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Humanizer;
using Microsoft.AspNetCore.Http;

namespace Intex2.Controllers 
{
    public class HomeController : Controller
    {

        private ILegoRepository _repo;
        private readonly InferenceSession _session;
        private readonly string _onnxModelPath;

        public HomeController(ILegoRepository temp, IHostEnvironment hostEnvironment)
        {
            _repo = temp;

            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.ContentRootPath, "gradient_model.onnx");
            _session = new InferenceSession(_onnxModelPath);

        } 

        [Authorize]
        public IActionResult Secrets()
        {
            return View();
        }

        public ActionResult ProductDetails(int ProductId, string Name, string ImgLink, int Price, string Description)
        {

            var product = new Product
            {
                ProductId = ProductId,
                Name = Name,
                ImgLink = ImgLink,
                Price = Price,
                Description = Description
            };

            return View(product);
        }

        public IActionResult ProductList(int pageNum, string? productType)
        {
            int pageSize = 9;
            if (pageNum < 1)
            {
                pageNum = 1;
            }

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

        [HttpPost]
        public IActionResult Predict(OrderPredictionViewModel OrderModel)
        {

            int id = OrderModel.Customer.CustomerId;

            OrderModel.Orders.CustomerId = id;

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
                var fraudprediction = (int)prediction[0];

                if (fraudprediction == 1)
                {
                    view = "Confirmation_NeedsVerification";
                }
            OrderModel.Orders.Fraud = fraudprediction;
            }



            // Save the order object to the database
            _repo.AddOrder(OrderModel.Orders);

            return View(view);
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

        public IActionResult Checkout()
        {
/*            // Get the current date and time
            DateTime now = DateTime.Now;

            // Format the date in the "1/1/2022" format
            string dateOnly = now.ToString("M/d/yyyy");

            // Get the day of the week (e.g., "Tues")
            string dayOfWeek = now.ToString("ddd");

            // Get the time as an integer (e.g., 13 for 1:00 PM)
            int timeAsInteger = now.Hour;

            Order order = new Order
            {
                Date = dateOnly,
                DayOfWeek = dayOfWeek,
                Time = timeAsInteger
            };

            // Save the order object to the database
            _repo.Order.Add(order);
            _repo.SaveChanges();*/

            return View();

        }
    }
}
