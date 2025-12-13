namespace CrossSetaWeb.Models
{
    public class LearnerValidationResult
    {
        public string NationalID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HomeAffairsFirstName { get; set; }
        public string HomeAffairsSurname { get; set; }
        public bool IsDeceased { get; set; }
        public bool IsFoundInHomeAffairs { get; set; }
    }
}
