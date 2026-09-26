using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaptopCart.Controllers
{
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
            return View();
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
                string originalFileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName).Replace(" ","_");
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
                return RedirectToAction("Index");
            }
            else
            {
                // If the model state is not valid, return the Create view with the product model to display validation errors
                return View(product);
            }
        }
    }
}
