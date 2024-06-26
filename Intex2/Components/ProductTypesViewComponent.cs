﻿using Microsoft.AspNetCore.Mvc;
using Intex2.Models;

namespace Intex2.Components
{
    public class ProductTypesViewComponent : ViewComponent
    {
        private ILegoRepository _legoRepo;
        //Constructor
        public ProductTypesViewComponent(ILegoRepository temp)
        {
            _legoRepo = temp;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedProductType = RouteData?.Values["productType"];

            var productTypes = _legoRepo?.Products
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x);

            return View(productTypes);
        }
    }
}
