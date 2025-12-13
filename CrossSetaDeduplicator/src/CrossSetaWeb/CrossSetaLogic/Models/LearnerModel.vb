Imports System.ComponentModel.DataAnnotations
Imports CrossSetaLogic.Validation

Namespace Models
    Public Class LearnerModel
        Public Property LearnerID As Integer

        <Required>
        <Luhn(ErrorMessage:="Invalid South African ID Number.")>
        Public Property NationalID As String = String.Empty

        <Required>
        Public Property FirstName As String = String.Empty

        <Required>
        Public Property LastName As String = String.Empty

        <Required>
        Public Property DateOfBirth As DateTime

        <Required>
        Public Property Gender As String = String.Empty

        Public Property Role As String
        Public Property BiometricHash As String
        Public Property IsVerified As Boolean
        Public Property SetaName As String

        Public Property Nationality As String
        Public Property Title As String
        Public Property MiddleName As String
        Public Property Age As Integer
        Public Property EquityCode As String
        Public Property HomeLanguage As String
        Public Property PreviousLastName As String
        Public Property Municipality As String
        Public Property DisabilityStatus As String
        Public Property CitizenStatus As String
        Public Property StatsAreaCode As String
        Public Property SocioEconomicStatus As String
        Public Property PopiActConsent As Boolean
        Public Property PopiActDate As DateTime

        Public Property PhoneNumber As String
        Public Property POBox As String
        Public Property CellphoneNumber As String
        Public Property StreetName As String
        Public Property PostalSuburb As String
        Public Property StreetHouseNo As String
        Public Property PhysicalSuburb As String
        Public Property City As String
        Public Property FaxNumber As String
        Public Property PostalCode As String
        Public Property EmailAddress As String
        Public Property Province As String
        Public Property UrbanRural As String
        Public Property IsResidentialAddressSameAsPostal As Boolean

        Public Property Disability_Communication As String
        Public Property Disability_Hearing As String
        Public Property Disability_Remembering As String
        Public Property Disability_Seeing As String
        Public Property Disability_SelfCare As String
        Public Property Disability_Walking As String

        Public Property LastSchoolAttended As String
        Public Property LastSchoolYear As String
    End Class
End Namespace
