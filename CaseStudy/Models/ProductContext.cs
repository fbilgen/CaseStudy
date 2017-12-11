using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaseStudy.Models
{
    public class ProductContext : DbContext
    {
        public DbSet<ProductModel> Products { get; set; }

        public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductModel>(entity =>
           {
               entity.HasKey(e => e.Id);
               entity.ToTable("Products");
           });
        }
    }
}
