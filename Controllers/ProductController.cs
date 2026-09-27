using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaptopCart.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public string userId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        public IActionResult ProductView() //Index view for products
        {
            if (userId != null)
            {
                HttpContext.Session.SetInt32(SDClass.SessionCart, _context.CartItems.Count(c => c.UserId == userId));
            }
            var products = _context.Products.ToList();
            return View(products);
        }

        public ActionResult ProductDetails(int id)
        {
            
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            return View(product);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductDetails(CartItemViewModel cartItem)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (userId == null) return Challenge(); // or RedirectToAction("Login", "Account");

            // Validate ProductId
            if (cartItem == null || cartItem.ProductId <= 0 || !_context.Products.Any(p => p.Id == cartItem.ProductId))
            {
                // invalid product - return to product details or show an error
                ModelState.AddModelError(string.Empty, "Invalid product.");
                return RedirectToAction("ProductView");
            }

            var existingCartItem = _context.CartItems.FirstOrDefault(c => c.ProductId == cartItem.ProductId && c.UserId == userId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += cartItem.Quantity;
                _context.CartItems.Update(existingCartItem);
            }
            else
            {
                cartItem.Id = 0; // Ensure the Id is set to 0 for a new cart item
                cartItem.UserId = userId;
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ProductView");
        }
    }
}
