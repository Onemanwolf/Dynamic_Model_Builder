
using System;

namespace DynamicModelBuilder.Models
{
    public class Person
    {
        public string FirstName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public int Salary { get; set; }
        public string Department { get; set; }

        /// <summary>
        /// Person must be of working age
        /// </summary>
        public bool IsValidAge()
        {
            return Age >= 18 && Age <= 65;
        }
        /// <summary>
        /// Must be a valid .com email
        /// </summary>
        public bool HasValidEmail()
        {
            return Email.Contains("@") && Email.EndsWith(".com");
        }

        /// <summary>
        /// Domain rule: CanRetire
        /// </summary>
        public bool CanRetire()
        {
            return Age >= 65;
        }
        /// <summary>
        /// Domain rule: GetDisplayName
        /// </summary>
        public string GetDisplayName()
        {
            return FirstName + " (" + Age + " years old)";
        }
    }
}