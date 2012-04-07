using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace LanLordz.Models
{
    public class ContactUsModel : IValidatableObject
    {
        [DisplayName("Your email address")]
        public string Email { get; set; }

        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Exception emailError = null;
            try
            {
                var foo = new MailAddress(this.Email);
            }
            catch (Exception ex)
            {
                if (!(ex is FormatException ||
                    ex is ArgumentException ||
                    ex is ArgumentNullException))
                {
                    throw;
                }

                emailError = ex;
            }

            if (emailError != null)
            {
                yield return new ValidationResult(emailError.Message, new[] { "Email" });
            }
        }
    }
}