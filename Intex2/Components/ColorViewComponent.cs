using Intex2.Models;
using Microsoft.AspNetCore.Mvc;
namespace Intex2.Components
{
    public class ColorViewComponent : ViewComponent
    {
        private ILegoRepository _legoRepo;
        //Constructor
        public ColorViewComponent(ILegoRepository temp)
        {
            _legoRepo = temp;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedPrimaryColor = RouteData?.Values[“color”];
            var color = _legoRepo?.Products
              .Select(x => x.PrimaryColor)
              .Distinct()
              .OrderBy(x => x);
            return View(color);
        }
    }
}
