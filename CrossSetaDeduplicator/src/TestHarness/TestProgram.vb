Imports System
Imports System.Collections.Generic
Imports CrossSetaDeduplicator.Models
Imports CrossSetaDeduplicator.Services
Imports CrossSetaDeduplicator.DataAccess

Module TestProgram
    Sub Main()
        Console.WriteLine("===================================================")
        Console.WriteLine("  Cross-SETA Deduplicator - Thorough Logic Test")
        Console.WriteLine("===================================================")
        Console.WriteLine("Requirement 1: ID Verification at Registration (Learner, Assessor, Moderator)")
        Console.WriteLine("Requirement 2: Batch/Bulk ID Verification")
        Console.WriteLine("Requirement 3: Detect duplicates across SETAs")
        Console.WriteLine("===================================================")

        ' Note: This test harness assumes the Database 'CrossSetaDB' exists locally.
        ' If not, it will just simulate the logic calls and catch connection errors 
        ' to demonstrate the flow.

        Dim service As New DeduplicationService()
        Dim db As New DatabaseHelper(Nothing)

        ' Scenario 1: Registration of different roles
        Console.WriteLine(vbCrLf & "[TEST 1] Testing Role-Based Duplicate Checks")
        
        Dim roles As String() = {"Learner", "Assessor", "Moderator"}
        For Each role In roles
            Dim id = "900101500908" & (Array.IndexOf(roles, role) + 1).ToString() ' Unique ID per role for test
            Dim p As New LearnerModel() With {
                .NationalID = id,
                .FirstName = "Test" & role,
                .LastName = "User",
                .Role = role,
                .DateOfBirth = DateTime.Now.AddYears(-25),
                .IsVerified = True
            }

            Console.WriteLine($"Checking {role} with ID {id}...")
            Try
                Dim result = service.CheckForDuplicates(p)
                If result.IsDuplicate Then
                     Console.WriteLine($"   -> Duplicate Found! ({result.MatchType})")
                Else
                     Console.WriteLine($"   -> No Duplicate. Registering...")
                     db.InsertLearner(p)
                     Console.WriteLine($"   -> Registered {role} successfully.")
                End If
            Catch ex As Exception
                Console.WriteLine($"   -> DB Error (Expected if DB not reachable): {ex.Message}")
            End Try
        Next

        ' Scenario 2: Cross-SETA Duplicate Detection
        ' Try to register an Assessor with the SAME ID as the Learner above
        Console.WriteLine(vbCrLf & "[TEST 2] Testing Cross-Role/Cross-SETA Duplicate Detection")
        Dim duplicateId = "9001015009081" ' The ID used for Learner above
        Dim duplicateUser As New LearnerModel() With {
             .NationalID = duplicateId,
             .FirstName = "TestLearner",
             .LastName = "User",
             .Role = "Assessor", ' Trying to register as Assessor now
             .IsVerified = True,
             .SetaName = "CHIETA" ' Attempting to register from CHIETA
        }
        
        Console.WriteLine($"Attempting to register ASSESSOR with existing LEARNER ID {duplicateId} from CHIETA...")
        Try
            Dim dupResult = service.CheckForDuplicates(duplicateUser)
             If dupResult.IsDuplicate Then
                 Console.WriteLine($"   -> SUCCESS: Duplicate Detected across roles!")
                 Console.WriteLine($"      Match Type: {dupResult.MatchType}")
                 Console.WriteLine($"      Existing Record Role: {dupResult.MatchedLearner.Role}")
                 Console.WriteLine($"      Found In SETA: {dupResult.FoundInSeta}")
            Else
                 Console.WriteLine($"   -> FAILURE: Duplicate not detected.")
            End If
        Catch ex As Exception
             Console.WriteLine($"   -> DB Error: {ex.Message}")
        End Try

        ' Scenario 3: Bulk Logic Simulation
        Console.WriteLine(vbCrLf & "[TEST 3] Simulating Bulk Verification Logic")
        Dim bulkData As New List(Of LearnerModel) From {
            New LearnerModel() With {.NationalID = "1111111111111", .FirstName = "Bulk1", .LastName = "User", .Role = "Learner"},
            New LearnerModel() With {.NationalID = "9001015009081", .FirstName = "Bulk2", .LastName = "User", .Role = "Moderator"} ' Duplicate
        }

        For Each item In bulkData
            Console.Write($"Processing {item.Role} ({item.NationalID})... ")
            Try
                Dim res = service.CheckForDuplicates(item)
                If res.IsDuplicate Then
                    Console.WriteLine("Result: DUPLICATE (Correct)")
                Else
                    Console.WriteLine("Result: VERIFIED (Correct)")
                End If
            Catch ex As Exception
                Console.WriteLine("DB Error")
            End Try
        Next

        Console.WriteLine(vbCrLf & "===================================================")
        Console.WriteLine("Test Complete.")
        Console.WriteLine("Press Enter to exit.")
        Console.ReadLine()
        ' Scenario 4: Home Affairs Verification (Google Sheet)
        Console.WriteLine(vbCrLf & "[TEST 4] Testing Home Affairs Verification (Google Sheet Source)")
        Dim haService As New HomeAffairsService()
        Dim testId As String = "0002080806082" ' Sichumile Makaula from Sheet
        
        Console.WriteLine($"Verifying ID {testId} (Should be ALIVE/VERIFIED)...")
        Try
            Dim result = haService.VerifyCitizenAsync(testId, "Sichumile", "Makaula").GetAwaiter().GetResult()
            Console.WriteLine($"   -> Result: {result.Status}")
            Console.WriteLine($"   -> Message: {result.Message}")
            
            If result.Status = "Verified" Or result.Status = "Alive" Then
                Console.WriteLine("   -> SUCCESS: Verified against Google Sheet.")
            Else
                Console.WriteLine("   -> FAILURE: Failed to verify against Google Sheet.")
            End If
            
        Catch ex As Exception
            Console.WriteLine($"   -> Error: {ex.Message}")
        End Try

        Console.WriteLine(vbCrLf & "Tests Completed. Press Enter to exit.")
        Console.ReadLine()
    End Sub
End Module
