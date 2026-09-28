using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LaptopCart.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public string userId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public IActionResult CartView()
        {
            var cartItems = _context.CartItems.Where(c => c.UserId == userId).Include(c => c.Product).ToList();
            return View(cartItems);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb != null)
            {
                cartFromDb.Quantity += 1;
                _context.CartItems.Update(cartFromDb);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cart item quantity increased successfully.";
            }
            else
            {
                // Handle the case where the cart item is not found
                return NotFound();
            }
            return RedirectToAction("CartView");
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb != null)
            {
                if (cartFromDb.Quantity > 1)
                {
                    cartFromDb.Quantity -= 1;
                    _context.CartItems.Update(cartFromDb);
                }
                else
                {
                    _context.CartItems.Remove(cartFromDb);
                    var count = _context.CartItems.Count(c => c.UserId == cartFromDb.UserId);
                    HttpContext.Session.SetInt32(SDClass.SessionCart, count - 1);
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cart item quantity decreased successfully.";
            }
            else
            {
                // Handle the case where the cart item is not found
                return NotFound();
            }
            return RedirectToAction("CartView");
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb != null)
            {
                _context.CartItems.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SDClass.SessionCart, _context.CartItems.Count(c => c.UserId == cartFromDb.UserId)-1);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cart item removed successfully.";
            }
            else
            {
                // Handle the case where the cart item is not found
                return NotFound();
            }
            return RedirectToAction("CartView");
        }
    }
}
