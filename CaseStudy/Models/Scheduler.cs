using CaseStudy.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CaseStudy.Models
{
    /// <summary>
    /// Scheduler gets the updated entries from database whenever the timer triggers the task 
    /// </summary>
    public class Scheduler : IScheduler
    {
        IMemoryCache _cache;
        ProductContext _context;
        private static Timer scheduleTimer;

        public Scheduler( IMemoryCache cache, ProductContext context)
        {
            _cache = cache;
            _context = context;

            //set up timer
            scheduleTimer = new Timer
            {
                Interval = 20000
            };

            //handler register
            scheduleTimer.Elapsed += OnTimedEvent;
            scheduleTimer.AutoReset = true;

            // timer start
            scheduleTimer.Enabled = true;
        }


        public Task HandleScheduleAsync()
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20));
            DateTime? lastUpdatedTimeCache;
            Dictionary<int, ProductViewModel> productsCache;

            // We keep two cahces: Products and LastUpdatedTime. 
            // If Products Cache is empty; we fill it with all data. And setup LastUpdatedTime Cache with the highest LastUpdatedDate value among products.
            // If Products Cache is already full; we get products from database which has higher LastUpdatedTime from our cached update time (holding in LastUpdatedTime cache)
            // Then update Products Cache with the returned products from database.
            
            if (!_cache.TryGetValue("Products", out productsCache))
            {
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
            else
            {
                lastUpdatedTimeCache = (DateTime)_cache.Get("LastUpdatedTime");

                //return updated products after the last cache update: add null values too
                List<ProductViewModel> lastUpdatedProducts = _context.Products.Where(p => p.LastUpdatedTime > lastUpdatedTimeCache || p.LastUpdatedTime == null)
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        LastUpdatedTime = p.LastUpdatedTime
                    }).ToList();

                foreach (ProductViewModel updatedProduct in lastUpdatedProducts)
                {
                    if (productsCache.ContainsKey(updatedProduct.Id))
                    {
                        productsCache[updatedProduct.Id] = updatedProduct;
                    }
                    else
                    {
                        productsCache.Add(updatedProduct.Id, updatedProduct);
                    }
                }

                _cache.Set("Products", productsCache, cacheEntryOptions);

                //set LastUpdatedTimeCache
                lastUpdatedTimeCache = productsCache.Where(p => p.Value.LastUpdatedTime != null)?.Max(p => p.Value.LastUpdatedTime);
                _cache.Set("LastUpdatedTime", lastUpdatedTimeCache ?? DateTime.Now, cacheEntryOptions);

            }

            return Task.CompletedTask;
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            HandleScheduleAsync();
        }


    }
}
