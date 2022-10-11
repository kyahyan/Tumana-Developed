using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TumanaDeveloped.Core.ViewModel
{
    public class ResetPasswordViewModel
    {
        [UIHint("Password")]
        [Required(ErrorMessage = "Please enter your new password")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[$@$!%*#?&_])[A-Za-z\d$@$!%*#?&_]{10,}$", ErrorMessage = "Please enter at least 10 characters, one at least must be a special and a number ")]
        public string Password { get; set; }
        [UIHint("Password")]
        [Required(ErrorMessage = "Please enter your new password")]
        [EqualTo("Password", ErrorMessage = "Please ensure you're password match")]
        public string ConfirmPassword { get; set; }
    }
}
