using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CrossSetaWeb.Validation
{
    public class LuhnAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow nulls, use [Required] to enforce presence
            }

            string idNumber = value.ToString();

            // Check if it is a valid number
            if (!long.TryParse(idNumber, out _))
            {
                return new ValidationResult("ID Number must contain only digits.");
            }

            // Check length (SA ID is 13 digits)
            if (idNumber.Length != 13)
            {
                return new ValidationResult("ID Number must be exactly 13 digits.");
            }

            if (!IsValidLuhn(idNumber))
            {
                return new ValidationResult("Invalid ID Number (Luhn Check Failed).");
            }

            return ValidationResult.Success;
        }

        private bool IsValidLuhn(string id)
        {
            int sum = 0;
            bool alternate = false;
            for (int i = id.Length - 1; i >= 0; i--)
            {
                char c = id[i];
                if (!char.IsDigit(c)) return false;

                int n = int.Parse(c.ToString());

                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                    {
                        n = (n % 10) + 1;
                    }
                }
                sum += n;
                alternate = !alternate;
            }
            return (sum % 10 == 0);
        }
    }
}
