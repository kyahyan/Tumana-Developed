using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TumanaDeveloped.Core.ViewModel
{
    public class ContactFormViewModel
    {
        [Required]
        [MaxLength(80, ErrorMessage = "Please try and limit to 80 characters")]
        public string Name { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = "Please try and limit your comments to 500 letters")]
        public string Comment { get; set; }
        [MaxLength(255, ErrorMessage = "Please try and limit to 255 characters")]
        public string Subject { get; set; }

    }
}
