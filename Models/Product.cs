using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopCart.Models
{
    public class Product
    {
        [Key] // This property is the primary key for the Product entity
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")] // This property is required and will display an error message if not provided
        public string? Name { get; set; }

        [Required(ErrorMessage = "Description is required."), StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")] // This property has a maximum length of 500 characters and will display an error message if exceeded
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required."), Range(0.00, 9999999.99, ErrorMessage = "Price must be between 0.00 and 9999999.99.")] // This property must be within the specified range and will display an error message if not
        public decimal Price { get; set; }
        public string? ImagePath{ get; set; }
        public DateTime CreatedAt { get; set; }

        [NotMapped] // This property will not be mapped to the database
        public IFormFile? ImageFile { get; set; }
    }
}
