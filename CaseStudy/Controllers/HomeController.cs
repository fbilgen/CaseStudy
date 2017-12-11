using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Models;
using CaseStudy.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using System.Timers;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace CaseStudy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductContext _context;
        private IMemoryCache _cache;

        public HomeController(ProductContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Products_Read([DataSourceRequest]DataSourceRequest request, string productName)
        {
            Dictionary<int, ProductViewModel> productsCache;
            //cache check
            if (!_cache.TryGetValue("Products", out productsCache))
            {
                FillProductCache();
                productsCache = (Dictionary<int, ProductViewModel>)_cache.Get("Products");

            }


            if (String.IsNullOrEmpty(productName)){

                var products = productsCache.Values.OrderBy(p => p.Id).ToList();
                return Json(products.ToDataSourceResult(request));

            }
            else
            {
                var products = productsCache.Values
                    .Where(p => p.Name.Contains(productName))
                    .OrderBy(p => p.Id)
                    .ToList();

                return Json(products.ToDataSourceResult(request));
            }

            //if (String.IsNullOrEmpty(searchStr))
            //{

            //    return View(productsCache.Values.OrderBy(p => p.Id).ToList());
            //}
            //else
            //{

            //    return View(productsCache.Values
            //        .Where(p => p.Name.Contains(searchStr))
            //        .OrderBy(p => p.Id)
            //        .ToList());
            //}


        }

        public void FillProductCache()
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20));
            DateTime? lastUpdatedTimeCache;
            Dictionary<int, ProductViewModel> productsCache;

            productsCache = _context.Products
               .Select(p => new ProductViewModel
               {
                   Id = p.Id,
                   Name = p.Name,
                   LastUpdatedTime = p.LastUpdatedTime
               }).ToDictionary(p => p.Id, p => p);

            _cache.Set("Products", productsCache, cacheEntryOptions);


            //set LastUpdatedTimeCache
            lastUpdatedTimeCache = productsCache.Where(p => p.Value.LastUpdatedTime != null)?.Max(p => p.Value.LastUpdatedTime);
            _cache.Set("LastUpdatedTime", lastUpdatedTimeCache ?? DateTime.Now, cacheEntryOptions);


        }


       

    }
}
