
using System;
using System.ComponentModel.DataAnnotations;

namespace DynamicModelBuilder.Models
{
    public class PersonDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Range(18, 100)]
        public int Age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Range(0, 1000000)]
        public int Salary { get; set; }

        [Required]
        public string Department { get; set; }

        public object IsManager { get; set; }
    }
}