using CaseStudy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaseStudy
{
    public static class DBInitilize
    {

        public static void Seed(ProductContext _context)
        {
            _context.Database.EnsureCreated();

            // if db was seeded before; do not initialize again
            if (_context.Products.Any())
            {
                return;
            }

            for(int i=0; i<10000; i++) 
            {
                _context.Products.Add(new ProductModel { Name = "Product" + i, LastUpdatedTime = DateTime.Now });
            };

            _context.SaveChanges();

        }
    }
}
