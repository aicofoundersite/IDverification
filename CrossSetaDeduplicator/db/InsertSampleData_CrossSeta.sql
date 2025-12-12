USE CrossSetaDB;
GO

-- Clear existing data (optional, be careful in prod!)
-- TRUNCATE TABLE Learners; 

-- 1. W&RSETA Learners (Base Set)
INSERT INTO Learners (NationalID, FirstName, LastName, DateOfBirth, Gender, Role, IsVerified, SetaName)
VALUES 
('9001015000080', 'John', 'Doe', '1990-01-01', 'Male', 'Learner', 1, 'W&RSETA'),
('9505055000081', 'Thabo', 'Molefe', '1995-05-05', 'Male', 'Learner', 1, 'W&RSETA'),
('8808080050082', 'Lerato', 'Khumalo', '1988-08-08', 'Female', 'Assessor', 1, 'W&RSETA');
GO

-- 2. CHIETA Learners (Contains a duplicate ID: 9001015000080)
INSERT INTO Learners (NationalID, FirstName, LastName, DateOfBirth, Gender, Role, IsVerified, SetaName)
VALUES 
('9001015000080', 'John', 'Doe', '1990-01-01', 'Male', 'Learner', 1, 'CHIETA'), -- Duplicate ID!
('9202025000083', 'Sipho', 'Nkosi', '1992-02-02', 'Male', 'Learner', 1, 'CHIETA');
GO

-- 3. MERSETA Learners (Contains a Fuzzy Name Match: "Tabo Molefe" vs "Thabo Molefe")
INSERT INTO Learners (NationalID, FirstName, LastName, DateOfBirth, Gender, Role, IsVerified, SetaName)
VALUES 
('9909095000084', 'Tabo', 'Molefe', '1999-09-09', 'Male', 'Learner', 1, 'MERSETA'), -- Fuzzy Name Match
('9303030050085', 'Precious', 'Dlamini', '1993-03-03', 'Female', 'Learner', 1, 'MERSETA');
GO
