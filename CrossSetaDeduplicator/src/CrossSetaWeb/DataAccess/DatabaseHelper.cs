using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CrossSetaWeb.Models;
using System.Collections.Generic;

namespace CrossSetaWeb.DataAccess
{
    public class DatabaseHelper : IDatabaseHelper
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

        public void InitializeHomeAffairsTable()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HomeAffairsCitizens' AND xtype='U')
                    BEGIN
                        CREATE TABLE HomeAffairsCitizens (
                            NationalID NVARCHAR(13) PRIMARY KEY,
                            FirstName NVARCHAR(100),
                            Surname NVARCHAR(100),
                            DateOfBirth DATE,
                            IsDeceased BIT DEFAULT 0,
                            LastUpdated DATETIME DEFAULT GETDATE(),
                            VerificationSource NVARCHAR(50),
                            RowVersion TIMESTAMP
                        );
                        CREATE INDEX IX_HomeAffairsCitizens_Surname ON HomeAffairsCitizens(Surname);
                    END";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InitializeUserSchema()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Create Users Table
                string sqlTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                    BEGIN
                        CREATE TABLE Users (
                            UserID INT IDENTITY(1,1) PRIMARY KEY,
                            IDType NVARCHAR(50),
                            NationalID NVARCHAR(50),
                            Title NVARCHAR(20),
                            FirstName NVARCHAR(100),
                            LastName NVARCHAR(100),
                            Email NVARCHAR(100),
                            Province NVARCHAR(50),
                            UserName NVARCHAR(100) UNIQUE,
                            PasswordHash NVARCHAR(MAX),
                            SecurityQuestion NVARCHAR(200),
                            SecurityAnswer NVARCHAR(MAX),
                            RegistrationDate DATETIME DEFAULT GETDATE(),
                            IsActive BIT DEFAULT 1
                        );
                    END";
                
                using (SqlCommand cmd = new SqlCommand(sqlTable, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Create sp_InsertUser
                string sqlProcDrop = "IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_InsertUser' AND xtype='P') DROP PROCEDURE sp_InsertUser";
                using (SqlCommand cmd = new SqlCommand(sqlProcDrop, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                string sqlProcCreate = @"
                    CREATE PROCEDURE sp_InsertUser
                        @IDType NVARCHAR(50),
                        @NationalID NVARCHAR(50),
                        @Title NVARCHAR(20),
                        @FirstName NVARCHAR(100),
                        @LastName NVARCHAR(100),
                        @Email NVARCHAR(100),
                        @Province NVARCHAR(50),
                        @UserName NVARCHAR(100),
                        @PasswordHash NVARCHAR(MAX),
                        @SecurityQuestion NVARCHAR(200),
                        @SecurityAnswer NVARCHAR(MAX)
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        IF EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName)
                        BEGIN
                            THROW 51000, 'Username already exists.', 1;
                        END

                        INSERT INTO Users (
                            IDType, NationalID, Title, FirstName, LastName, Email, Province, UserName, PasswordHash, SecurityQuestion, SecurityAnswer
                        )
                        VALUES (
                            @IDType, @NationalID, @Title, @FirstName, @LastName, @Email, @Province, @UserName, @PasswordHash, @SecurityQuestion, @SecurityAnswer
                        );
                    END";

                using (SqlCommand cmd = new SqlCommand(sqlProcCreate, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void BatchImportHomeAffairsData(List<HomeAffairsCitizen> citizens)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var citizen in citizens)
                        {
                            string sql = @"
                                MERGE HomeAffairsCitizens AS target
                                USING (SELECT @NationalID, @FirstName, @Surname, @DateOfBirth, @IsDeceased, @VerificationSource) AS source (NationalID, FirstName, Surname, DateOfBirth, IsDeceased, VerificationSource)
                                ON (target.NationalID = source.NationalID)
                                WHEN MATCHED THEN
                                    UPDATE SET FirstName = source.FirstName, 
                                               Surname = source.Surname, 
                                               DateOfBirth = source.DateOfBirth,
                                               IsDeceased = source.IsDeceased,
                                               LastUpdated = GETDATE(),
                                               VerificationSource = source.VerificationSource
                                WHEN NOT MATCHED THEN
                                    INSERT (NationalID, FirstName, Surname, DateOfBirth, IsDeceased, VerificationSource)
                                    VALUES (source.NationalID, source.FirstName, source.Surname, source.DateOfBirth, source.IsDeceased, source.VerificationSource);";

                            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@NationalID", citizen.NationalID);
                                cmd.Parameters.AddWithValue("@FirstName", citizen.FirstName);
                                cmd.Parameters.AddWithValue("@Surname", citizen.Surname);
                                cmd.Parameters.AddWithValue("@DateOfBirth", citizen.DateOfBirth);
                                cmd.Parameters.AddWithValue("@IsDeceased", citizen.IsDeceased);
                                cmd.Parameters.AddWithValue("@VerificationSource", "BulkImport");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<BulkInsertError> BatchInsertLearners(List<LearnerModel> learners)
        {
            var errors = new List<BulkInsertError>();
            
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Removed the single transaction to allow partial success. 
                // Each insert is its own atomic operation.
                
                foreach (var learner in learners)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_InsertLearner", conn))
                        {
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
                            cmd.Parameters.AddWithValue("@SetaName", string.IsNullOrEmpty(learner.SetaName) ? "BulkImport" : learner.SetaName);

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
                            cmd.Parameters.AddWithValue("@IsResidentialAddressSameAsPostal", learner.IsResidentialAddressSameAsPostal);

                            // Contact Details
                            cmd.Parameters.AddWithValue("@PhoneNumber", GetValue(learner.PhoneNumber));
                            cmd.Parameters.AddWithValue("@EmailAddress", GetValue(learner.EmailAddress));
                            
                            // Fill optional fields with nulls
                            cmd.Parameters.AddWithValue("@POBox", DBNull.Value);
                            cmd.Parameters.AddWithValue("@CellphoneNumber", DBNull.Value);
                            cmd.Parameters.AddWithValue("@StreetName", DBNull.Value);
                            cmd.Parameters.AddWithValue("@PostalSuburb", DBNull.Value);
                            cmd.Parameters.AddWithValue("@StreetHouseNo", DBNull.Value);
                            cmd.Parameters.AddWithValue("@PhysicalSuburb", DBNull.Value);
                            cmd.Parameters.AddWithValue("@City", DBNull.Value);
                            cmd.Parameters.AddWithValue("@FaxNumber", DBNull.Value);
                            cmd.Parameters.AddWithValue("@PostalCode", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Province", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UrbanRural", DBNull.Value);
                            
                            cmd.Parameters.AddWithValue("@Disability_Communication", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Disability_Hearing", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Disability_Remembering", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Disability_Seeing", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Disability_SelfCare", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Disability_Walking", DBNull.Value);

                            cmd.Parameters.AddWithValue("@LastSchoolAttended", DBNull.Value);
                            cmd.Parameters.AddWithValue("@LastSchoolYear", DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 51000 || ex.Number == 2627 || ex.Number == 2601)
                        {
                            errors.Add(new BulkInsertError { NationalID = learner.NationalID, Message = "Duplicate Record", IsDuplicate = true });
                        }
                        else
                        {
                            try
                            {
                                using (SqlCommand cmd2 = new SqlCommand("INSERT INTO Learners (NationalID, FirstName, LastName, DateOfBirth, Gender, Role, BiometricHash, IsVerified, SetaName) VALUES (@NationalID, @FirstName, @LastName, @DateOfBirth, @Gender, @Role, @BiometricHash, @IsVerified, @SetaName)", conn))
                                {
                                    cmd2.Parameters.AddWithValue("@NationalID", GetValue(learner.NationalID));
                                    cmd2.Parameters.AddWithValue("@FirstName", GetValue(learner.FirstName));
                                    cmd2.Parameters.AddWithValue("@LastName", GetValue(learner.LastName));
                                    cmd2.Parameters.AddWithValue("@DateOfBirth", learner.DateOfBirth == DateTime.MinValue ? (object)DBNull.Value : learner.DateOfBirth);
                                    cmd2.Parameters.AddWithValue("@Gender", GetValue(string.IsNullOrEmpty(learner.Gender) ? "Unknown" : learner.Gender));
                                    cmd2.Parameters.AddWithValue("@Role", GetValue(string.IsNullOrEmpty(learner.Role) ? "Learner" : learner.Role));
                                    cmd2.Parameters.AddWithValue("@BiometricHash", GetValue(learner.BiometricHash));
                                    cmd2.Parameters.AddWithValue("@IsVerified", learner.IsVerified);
                                    cmd2.Parameters.AddWithValue("@SetaName", GetValue(string.IsNullOrEmpty(learner.SetaName) ? "BulkImport" : learner.SetaName));
                                    cmd2.ExecuteNonQuery();
                                }
                            }
                            catch (SqlException ex2)
                            {
                                if (ex2.Number == 2627 || ex2.Number == 2601)
                                    errors.Add(new BulkInsertError { NationalID = learner.NationalID, Message = "Duplicate Record", IsDuplicate = true });
                                else
                                    errors.Add(new BulkInsertError { NationalID = learner.NationalID, Message = $"Database Error: {ex2.Message}", IsDuplicate = false });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new BulkInsertError { NationalID = learner.NationalID, Message = $"System Error: {ex.Message}", IsDuplicate = false });
                    }
                }
            }
            return errors;
        }

        public class BulkInsertError
        {
            public string NationalID { get; set; }
            public string Message { get; set; }
            public bool IsDuplicate { get; set; }
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

        public List<LearnerModel> GetAllLearners()
        {
            var learners = new List<LearnerModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Learners";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        learners.Add(MapReaderToLearner(reader));
                    }
                }
            }
            return learners;
        }

        public LearnerModel? GetLearnerByNationalID(string nationalID)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // We can use a simple query or the sp_FindPotentialDuplicates if suitable.
                // For exact lookup, a simple query is faster/easier for now if we just want to verify existence.
                string query = "SELECT TOP 1 * FROM Learners WHERE NationalID = @NationalID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NationalID", nationalID);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapReaderToLearner(reader);
                    }
                }
            }
            return null;
        }

        public HomeAffairsCitizen GetHomeAffairsCitizen(string nationalID)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Ensure table exists just in case (optional, but good for stability)
                // InitializeHomeAffairsTable(); 

                string sql = "SELECT * FROM HomeAffairsCitizens WHERE NationalID = @NationalID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@NationalID", nationalID);
                
                try 
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new HomeAffairsCitizen
                            {
                                NationalID = reader["NationalID"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                Surname = reader["Surname"].ToString(),
                                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                                IsDeceased = Convert.ToBoolean(reader["IsDeceased"]),
                                VerificationSource = reader["VerificationSource"] == DBNull.Value ? "Unknown" : reader["VerificationSource"].ToString()
                            };
                        }
                    }
                }
                catch (SqlException) 
                {
                    // Table might not exist if import hasn't run.
                    return null;
                }
            }
            return null;
        }

        public List<LearnerValidationResult> GetLearnerValidationResults()
        {
            var results = new List<LearnerValidationResult>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        L.NationalID, 
                        L.FirstName, 
                        L.LastName, 
                        H.FirstName AS HA_FirstName, 
                        H.Surname AS HA_Surname, 
                        ISNULL(H.IsDeceased, 0) AS IsDeceased,
                        CASE WHEN H.NationalID IS NOT NULL THEN 1 ELSE 0 END AS IsFound
                    FROM Learners L
                    LEFT JOIN HomeAffairsCitizens H ON L.NationalID = H.NationalID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = 300; // Increase timeout for large datasets

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new LearnerValidationResult
                            {
                                NationalID = reader["NationalID"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                HomeAffairsFirstName = reader["HA_FirstName"] == DBNull.Value ? null : reader["HA_FirstName"].ToString(),
                                HomeAffairsSurname = reader["HA_Surname"] == DBNull.Value ? null : reader["HA_Surname"].ToString(),
                                IsDeceased = Convert.ToBoolean(reader["IsDeceased"]),
                                IsFoundInHomeAffairs = Convert.ToInt32(reader["IsFound"]) == 1
                            });
                        }
                    }
                }
                catch (SqlException)
                {
                    // If HomeAffairsCitizens table doesn't exist, return basic list with IsFound=false
                     return GetAllLearners().ConvertAll(l => new LearnerValidationResult 
                     { 
                         NationalID = l.NationalID, 
                         FirstName = l.FirstName, 
                         LastName = l.LastName, 
                         IsFoundInHomeAffairs = false 
                     });
                }
            }
            return results;
        }

        public void InitializeUserActivitySchema()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserActivityLogs' AND xtype='U')
                    BEGIN
                        CREATE TABLE UserActivityLogs (
                            LogID INT IDENTITY(1,1) PRIMARY KEY,
                            UserName NVARCHAR(100),
                            ActivityType NVARCHAR(50),
                            ActivityDate DATETIME DEFAULT GETDATE(),
                            IPAddress NVARCHAR(50),
                            Details NVARCHAR(MAX)
                        );
                        CREATE INDEX IX_UserActivityLogs_UserName ON UserActivityLogs(UserName);
                        CREATE INDEX IX_UserActivityLogs_Date ON UserActivityLogs(ActivityDate);
                    END";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void LogUserActivity(string email, string activityType, string ipAddress, string details)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = "INSERT INTO UserActivityLogs (UserName, ActivityType, IPAddress, Details) VALUES (@UserName, @ActivityType, @IPAddress, @Details)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ActivityType", activityType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@IPAddress", ipAddress ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Details", details ?? (object)DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail logging to not disrupt user flow
            }
        }

        public List<UserActivityLog> GetUserActivityLogs()
        {
            var logs = new List<UserActivityLog>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Get latest 100 logs
                string sql = "SELECT TOP 100 * FROM UserActivityLogs ORDER BY ActivityDate DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                logs.Add(new UserActivityLog
                                {
                                    LogID = Convert.ToInt32(reader["LogID"]),
                                    UserName = reader["UserName"] == DBNull.Value ? "" : reader["UserName"].ToString(),
                                    ActivityType = reader["ActivityType"] == DBNull.Value ? "" : reader["ActivityType"].ToString(),
                                    ActivityDate = Convert.ToDateTime(reader["ActivityDate"]),
                                    IPAddress = reader["IPAddress"] == DBNull.Value ? "" : reader["IPAddress"].ToString(),
                                    Details = reader["Details"] == DBNull.Value ? "" : reader["Details"].ToString()
                                });
                            }
                        }
                    }
                    catch (SqlException)
                    {
                        // Table might not exist yet
                    }
                }
            }
            return logs;
        }

        private LearnerModel MapReaderToLearner(SqlDataReader reader)
        {
            return new LearnerModel
            {
                LearnerID = Convert.ToInt32(reader["LearnerID"]),
                NationalID = reader["NationalID"].ToString(),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                Gender = reader["Gender"] == DBNull.Value ? null : reader["Gender"].ToString(),
                Role = reader["Role"] == DBNull.Value ? "Learner" : reader["Role"].ToString(),
                BiometricHash = reader["BiometricHash"] == DBNull.Value ? null : reader["BiometricHash"].ToString(),
                IsVerified = Convert.ToBoolean(reader["IsVerified"]),
                // Add SetaName safely if column exists or default
                SetaName = HasColumn(reader, "SetaName") ? (reader["SetaName"] == DBNull.Value ? "Unknown" : reader["SetaName"].ToString()) : "Unknown"
            };
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        private object GetValue(string value)
        {
            return string.IsNullOrEmpty(value) ? DBNull.Value : (object)value;
        }
    }
}
