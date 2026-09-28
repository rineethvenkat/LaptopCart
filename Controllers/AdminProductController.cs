using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopCart.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor to initialize the controller with the database context and web host environment
        public AdminProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        // Action method to handle the creation of a new product, including image upload
        public async Task<IActionResult> Create(Product product)
        {
            // custom validation to prevent the product name "test" from being used
            if (product.Name?.Trim().ToLower() == "test")
            {
                ModelState.AddModelError("Name", "The product name 'test' is not allowed.");
            }

            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                // Get the wwwroot path from the web host environment
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                /* Generate a unique file name for the uploaded image by combining the original file name (without extension), a GUID, and the original file extension. 
                   This ensures that each uploaded image has a unique name to prevent overwriting existing files.
                */
                string originalFileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName).Replace(" ", "_");
                string extension = Path.GetExtension(product.ImageFile.FileName);
                string uniqueFileName = $"{originalFileName}_{Guid.NewGuid():N}{extension}";
                string imagesFolder = Path.Combine(wwwRootPath, "images");
                /* Check if the "images" folder exists in the wwwroot directory. If it does not exist, create the folder to store uploaded images. */
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }
                string filePath = Path.Combine(imagesFolder, uniqueFileName);
                /* Save the uploaded image file to the specified path using a FileStream. 
                   The CopyToAsync method is used to copy the contents of the uploaded file to the new file stream asynchronously.
                */
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(fileStream);
                }
                // Set the ImagePath property of the product to the relative path of the saved image, which will be used for displaying the image in the application
                product.ImagePath = $"/images/{uniqueFileName}";
                // Verify that the image file was saved correctly by checking if the file exists at the expected path
                string confirmPath = Path.Combine(wwwRootPath, product.ImagePath.TrimStart('/'));
                // If the file does not exist at the expected path, throw a FileNotFoundException with a message indicating that the image file was not saved correctly.
                if (!System.IO.File.Exists(confirmPath))
                {
                    throw new FileNotFoundException($"Image file was not saved correctly. Expected path: {confirmPath}");
                }
            }

            // Set the CreatedAt property of the product to the current date and time
            product.CreatedAt = DateTime.Now;
            // Check if the model state is valid before adding the product to the database
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                // Save the changes to the database asynchronously
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                // If the model state is not valid, return the Create view with the product model to display validation errors
                return View(product);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            // how to handle image upload of an existing product in edit action or if the image is not changed, keep the existing image path
            // Check if a new image file has been uploaded for the product and add current date and time to the product's CreatedAt property
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                // Get the wwwroot path from the web host environment
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                // Generate a unique file name for the uploaded image
                string originalFileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName).Replace(" ", "_");
                string extension = Path.GetExtension(product.ImageFile.FileName);
                string uniqueFileName = $"{originalFileName}_{Guid.NewGuid():N}{extension}";
                string imagesFolder = Path.Combine(wwwRootPath, "images");
                // Check if the "images" folder exists, and create it if it doesn't
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }
                string filePath = Path.Combine(imagesFolder, uniqueFileName);
                // Save the uploaded image file to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(fileStream);
                }
                // Set the ImagePath property of the product to the relative path of the saved image
                product.ImagePath = $"/images/{uniqueFileName}";
                // current date and time to the product's CreatedAt property
                product.CreatedAt = DateTime.Now;

            }

            //keep the existing image path if no new image is uploaded
            else
            {
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
                if (existingProduct != null)
                {
                    product.ImagePath = existingProduct.ImagePath;
                    product.CreatedAt = existingProduct.CreatedAt; // Keep the original CreatedAt value
                }
            }


            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                //_context.Products.Remove(product);
                //await _context.SaveChangesAsync();
                return View(product);

            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                /* Check if the product has an associated image file (ImagePath is not null or empty). 
                 * If it does, construct the full path to the image file in the wwwroot/images directory and delete the file from the server. 
                 * This ensures that when a product is deleted, its associated image file is also removed from the server to free up storage space.
                */
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string imagePath = Path.Combine(wwwRootPath, product.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }

        }
    }
}
