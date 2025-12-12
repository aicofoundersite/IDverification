using System.ComponentModel.DataAnnotations;

namespace CrossSetaWeb.Models
{
    public class RegisterUserViewModel
    {
        // Personal Info
        public string IDType { get; set; }
        public string NationalID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Compare("Email", ErrorMessage = "Emails do not match")]
        public string ConfirmEmail { get; set; }
        
        public string Province { get; set; }

        // Login
        public string UserName { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Security
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }

        public UserModel ToUserModel()
        {
            return new UserModel
            {
                IDType = IDType,
                NationalID = NationalID,
                Title = Title,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Province = Province,
                UserName = UserName,
                SecurityQuestion = SecurityQuestion,
                SecurityAnswer = SecurityAnswer
            };
        }
    }
}
