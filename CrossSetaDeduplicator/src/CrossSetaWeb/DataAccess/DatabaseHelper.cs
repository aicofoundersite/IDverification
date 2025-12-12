using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CrossSetaWeb.Models;
using System.Collections.Generic;

namespace CrossSetaWeb.DataAccess
{
    public class DatabaseHelper
    {
        private string _connectionString = "Server=localhost;Database=CrossSetaDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public DatabaseHelper(string connectionString = null)
        {
            // Priority 1: Constructor Argument
            if (!string.IsNullOrEmpty(connectionString))
            {
                _connectionString = connectionString;
                return;
            }

            // Priority 2: Environment Variable (Production/Docker/Cloud Proxy)
            string envConn = Environment.GetEnvironmentVariable("CROSS_SETA_DB_CONNECTION");
            if (!string.IsNullOrEmpty(envConn))
            {
                _connectionString = envConn;
                return;
            }

            // Priority 3: Environment Variable for Password only (Local App -> Cloud DB Proxy)
            string dbPassword = Environment.GetEnvironmentVariable("CROSS_SETA_DB_PASSWORD");
            if (!string.IsNullOrEmpty(dbPassword))
            {
                // Assumes localhost proxy on default port 1433
                _connectionString = $"Server=127.0.0.1,1433;Database=CrossSetaDB;User Id=sa;Password={dbPassword};Encrypt=False;";
                return;
            }
        }

        public void InsertLearner(LearnerModel learner)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertLearner", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Basic Fields
                cmd.Parameters.AddWithValue("@NationalID", GetValue(learner.NationalID));
                cmd.Parameters.AddWithValue("@FirstName", GetValue(learner.FirstName));
                cmd.Parameters.AddWithValue("@LastName", GetValue(learner.LastName));
                cmd.Parameters.AddWithValue("@DateOfBirth", learner.DateOfBirth);
                cmd.Parameters.AddWithValue("@Gender", GetValue(learner.Gender));
                cmd.Parameters.AddWithValue("@Role", string.IsNullOrEmpty(learner.Role) ? "Learner" : learner.Role);
                cmd.Parameters.AddWithValue("@BiometricHash", GetValue(learner.BiometricHash));
                cmd.Parameters.AddWithValue("@IsVerified", learner.IsVerified);
                cmd.Parameters.AddWithValue("@SetaName", string.IsNullOrEmpty(learner.SetaName) ? "W&RSETA" : learner.SetaName);

                // New Personal Info Fields
                cmd.Parameters.AddWithValue("@Nationality", GetValue(learner.Nationality));
                cmd.Parameters.AddWithValue("@Title", GetValue(learner.Title));
                cmd.Parameters.AddWithValue("@MiddleName", GetValue(learner.MiddleName));
                cmd.Parameters.AddWithValue("@Age", learner.Age == 0 ? DBNull.Value : (object)learner.Age);
                cmd.Parameters.AddWithValue("@EquityCode", GetValue(learner.EquityCode));
                cmd.Parameters.AddWithValue("@HomeLanguage", GetValue(learner.HomeLanguage));
                cmd.Parameters.AddWithValue("@PreviousLastName", GetValue(learner.PreviousLastName));
                cmd.Parameters.AddWithValue("@Municipality", GetValue(learner.Municipality));
                cmd.Parameters.AddWithValue("@DisabilityStatus", GetValue(learner.DisabilityStatus));
                cmd.Parameters.AddWithValue("@CitizenStatus", GetValue(learner.CitizenStatus));
                cmd.Parameters.AddWithValue("@StatsAreaCode", GetValue(learner.StatsAreaCode));
                cmd.Parameters.AddWithValue("@SocioEconomicStatus", GetValue(learner.SocioEconomicStatus));
                cmd.Parameters.AddWithValue("@PopiActConsent", learner.PopiActConsent);
                cmd.Parameters.AddWithValue("@PopiActDate", learner.PopiActDate == DateTime.MinValue ? DBNull.Value : (object)learner.PopiActDate);

                // Contact Details
                cmd.Parameters.AddWithValue("@PhoneNumber", GetValue(learner.PhoneNumber));
                cmd.Parameters.AddWithValue("@POBox", GetValue(learner.POBox));
                cmd.Parameters.AddWithValue("@CellphoneNumber", GetValue(learner.CellphoneNumber));
                cmd.Parameters.AddWithValue("@StreetName", GetValue(learner.StreetName));
                cmd.Parameters.AddWithValue("@PostalSuburb", GetValue(learner.PostalSuburb));
                cmd.Parameters.AddWithValue("@StreetHouseNo", GetValue(learner.StreetHouseNo));
                cmd.Parameters.AddWithValue("@PhysicalSuburb", GetValue(learner.PhysicalSuburb));
                cmd.Parameters.AddWithValue("@City", GetValue(learner.City));
                cmd.Parameters.AddWithValue("@FaxNumber", GetValue(learner.FaxNumber));
                cmd.Parameters.AddWithValue("@PostalCode", GetValue(learner.PostalCode));
                cmd.Parameters.AddWithValue("@EmailAddress", GetValue(learner.EmailAddress));
                cmd.Parameters.AddWithValue("@Province", GetValue(learner.Province));
                cmd.Parameters.AddWithValue("@UrbanRural", GetValue(learner.UrbanRural));
                cmd.Parameters.AddWithValue("@IsResidentialAddressSameAsPostal", learner.IsResidentialAddressSameAsPostal);

                // Disability Details
                cmd.Parameters.AddWithValue("@Disability_Communication", GetValue(learner.Disability_Communication));
                cmd.Parameters.AddWithValue("@Disability_Hearing", GetValue(learner.Disability_Hearing));
                cmd.Parameters.AddWithValue("@Disability_Remembering", GetValue(learner.Disability_Remembering));
                cmd.Parameters.AddWithValue("@Disability_Seeing", GetValue(learner.Disability_Seeing));
                cmd.Parameters.AddWithValue("@Disability_SelfCare", GetValue(learner.Disability_SelfCare));
                cmd.Parameters.AddWithValue("@Disability_Walking", GetValue(learner.Disability_Walking));

                // Education Details
                cmd.Parameters.AddWithValue("@LastSchoolAttended", GetValue(learner.LastSchoolAttended));
                cmd.Parameters.AddWithValue("@LastSchoolYear", GetValue(learner.LastSchoolYear));

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertUser(UserModel user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertUser", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IDType", GetValue(user.IDType));
                cmd.Parameters.AddWithValue("@NationalID", GetValue(user.NationalID));
                cmd.Parameters.AddWithValue("@Title", GetValue(user.Title));
                cmd.Parameters.AddWithValue("@FirstName", GetValue(user.FirstName));
                cmd.Parameters.AddWithValue("@LastName", GetValue(user.LastName));
                cmd.Parameters.AddWithValue("@Email", GetValue(user.Email));
                cmd.Parameters.AddWithValue("@Province", GetValue(user.Province));
                cmd.Parameters.AddWithValue("@UserName", GetValue(user.UserName));
                cmd.Parameters.AddWithValue("@PasswordHash", GetValue(user.PasswordHash));
                cmd.Parameters.AddWithValue("@SecurityQuestion", GetValue(user.SecurityQuestion));
                cmd.Parameters.AddWithValue("@SecurityAnswer", GetValue(user.SecurityAnswer));

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private object GetValue(string value)
        {
            return string.IsNullOrEmpty(value) ? DBNull.Value : (object)value;
        }
    }
}
