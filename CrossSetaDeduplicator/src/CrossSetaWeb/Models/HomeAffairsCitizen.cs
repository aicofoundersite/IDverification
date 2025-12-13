using System;
using System.ComponentModel.DataAnnotations;

namespace CrossSetaWeb.Models
{
    public class HomeAffairsCitizen
    {
        [Required]
        [StringLength(13, MinimumLength = 13)]
        public string NationalID { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string Surname { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public bool IsDeceased { get; set; }
        
        public string VerificationSource { get; set; }
    }
}
