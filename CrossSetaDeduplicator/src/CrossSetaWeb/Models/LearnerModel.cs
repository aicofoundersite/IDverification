using System;
using System.ComponentModel.DataAnnotations;
using CrossSetaWeb.Validation;

namespace CrossSetaWeb.Models
{
    public class LearnerModel
    {
        public int LearnerID { get; set; }

        [Required]
        [Luhn(ErrorMessage = "Invalid South African ID Number.")]
        public string NationalID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Role { get; set; } // Learner, Assessor, Moderator
        public string BiometricHash { get; set; }
        public bool IsVerified { get; set; }
        public string SetaName { get; set; }

        // New fields from design
        public string Nationality { get; set; }
        public string Title { get; set; }
        public string MiddleName { get; set; }
        public int Age { get; set; }
        public string EquityCode { get; set; }
        public string HomeLanguage { get; set; }
        public string PreviousLastName { get; set; }
        public string Municipality { get; set; }
        public string DisabilityStatus { get; set; }
        public string CitizenStatus { get; set; }
        public string StatsAreaCode { get; set; }
        public string SocioEconomicStatus { get; set; }
        public bool PopiActConsent { get; set; }
        public DateTime PopiActDate { get; set; }

        // Contact Details
        public string PhoneNumber { get; set; }
        public string POBox { get; set; }
        public string CellphoneNumber { get; set; }
        public string StreetName { get; set; }
        public string PostalSuburb { get; set; }
        public string StreetHouseNo { get; set; }
        public string PhysicalSuburb { get; set; }
        public string City { get; set; }
        public string FaxNumber { get; set; }
        public string PostalCode { get; set; }
        public string EmailAddress { get; set; }
        public string Province { get; set; }
        public string UrbanRural { get; set; }
        public bool IsResidentialAddressSameAsPostal { get; set; }

        // Disability Details
        public string Disability_Communication { get; set; }
        public string Disability_Hearing { get; set; }
        public string Disability_Remembering { get; set; }
        public string Disability_Seeing { get; set; }
        public string Disability_SelfCare { get; set; }
        public string Disability_Walking { get; set; }

        // Education Details
        public string LastSchoolAttended { get; set; }
        public string LastSchoolYear { get; set; }
    }
}
