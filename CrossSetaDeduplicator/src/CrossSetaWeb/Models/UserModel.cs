using System;

namespace CrossSetaWeb.Models
{
    public class UserModel
    {
        public int UserID { get; set; }
        public string IDType { get; set; }
        public string NationalID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Province { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
    }
}
