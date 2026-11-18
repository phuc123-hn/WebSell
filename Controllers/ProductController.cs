using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;

namespace web1.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Product
        public ActionResult Index(int? categoryId, decimal? minPrice, decimal? maxPrice, string searchKeyword)
        {
            var products = db.Products.AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice);
            }

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                products = products.Where(p => p.Name.Contains(searchKeyword) || p.Description.Contains(searchKeyword));
            }

            var categories = db.Categories.ToList();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products.ToList());
        }

        // GET: Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var product = db.Products.Include("Category").FirstOrDefault(p => p.Id == id);
            if (product == null)
                return HttpNotFound();

            var relatedProducts = db.Products
                .Include("Category")
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;
            return View(product);
        }

        // GET: Product/GetCategoryProducts
        public ActionResult GetCategoryProducts(int? categoryId)
        {
            if (categoryId == null || categoryId == 0)
            {
                var allProducts = db.Products.Where(p => p.IsHot).Take(6).ToList();
                return PartialView("_ProductGrid", allProducts);
            }

            var products = db.Products.Where(p => p.CategoryId == categoryId).ToList();
            return PartialView("_ProductGrid", products);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
