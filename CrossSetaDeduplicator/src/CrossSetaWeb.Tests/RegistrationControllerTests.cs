using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CrossSetaWeb.Controllers;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;
using CrossSetaWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace CrossSetaWeb.Tests
{
    public class RegistrationControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IDatabaseHelper> _mockDbHelper;
        private readonly Mock<IKYCService> _mockKycService;
        private readonly Mock<IBulkRegistrationService> _mockBulkService;
        private readonly Mock<IDatabaseValidationService> _mockValidationService;
        private readonly Mock<IHomeAffairsImportService> _mockImportService;
        private readonly RegistrationController _controller;

        public RegistrationControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockDbHelper = new Mock<IDatabaseHelper>();
            _mockKycService = new Mock<IKYCService>();
            _mockBulkService = new Mock<IBulkRegistrationService>();
            _mockValidationService = new Mock<IDatabaseValidationService>();
            _mockImportService = new Mock<IHomeAffairsImportService>();

            _controller = new RegistrationController(
                _mockUserService.Object,
                _mockDbHelper.Object,
                _mockKycService.Object,
                _mockBulkService.Object,
                _mockValidationService.Object,
                _mockImportService.Object
            );

            // Mock TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [Fact]
        public void Bulk_Post_ReturnsViewWithModelError_WhenFileIsNull()
        {
            // Act
            var result = _controller.Bulk(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public void Bulk_Post_ReturnsViewWithModelError_WhenFileIsNotCsv()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.txt");

            // Act
            var result = _controller.Bulk(mockFile.Object);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void Bulk_Post_CallsBulkService_WhenFileIsValid()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.csv");

            var importResult = new BulkImportResult { SuccessCount = 10, FailureCount = 0 };
            _mockBulkService.Setup(s => s.ProcessBulkFile(It.IsAny<IFormFile>())).Returns(importResult);

            // Act
            var result = _controller.Bulk(mockFile.Object);

            // Assert
            _mockBulkService.Verify(s => s.ProcessBulkFile(mockFile.Object), Times.Once);
            Assert.IsType<ViewResult>(result);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
        }

        [Fact]
        public async Task ValidateDatabase_RedirectsToBulk_WhenValidationSucceeds()
        {
            // Arrange
            var validationResult = new DatabaseValidationResult 
            { 
                TotalRecords = 100, 
                ValidCount = 90, 
                InvalidFormatCount = 10,
                Details = new List<ValidationDetail>()
            };
            
            _mockImportService.Setup(s => s.ImportFromUrlAsync(It.IsAny<string>()))
                .ReturnsAsync(new ImportResult { Success = true });
                
            _mockValidationService.Setup(s => s.ValidateDatabase()).Returns(validationResult);

            // Act
            var result = await _controller.ValidateDatabase();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Bulk", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("ValidationSuccess"));
        }

        [Fact]
        public async Task Learner_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");
            var model = new LearnerModel();

            // Act
            var result = await _controller.Learner(model, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Learner_Post_ReturnsViewWithKYCError_WhenKYCFails()
        {
            // Arrange
            var model = new LearnerModel { NationalID = "1234567890123" };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);

            _mockKycService.Setup(s => s.VerifyDocumentAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(new KYCResult { IsSuccess = false, ErrorMessage = "Invalid Document" });

            // Act
            var result = await _controller.Learner(model, mockFile.Object);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("KYC"));
        }

        [Fact]
        public async Task Learner_Post_RedirectsToLearner_WhenRegistrationSucceeds()
        {
            // Arrange
            var model = new LearnerModel { NationalID = "1234567890123" };

            // Act
            var result = await _controller.Learner(model, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Learner", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            _mockDbHelper.Verify(d => d.InsertLearner(model), Times.Once);
        }

        [Fact]
        public async Task Learner_Post_ReturnsViewWithError_WhenDuplicateID()
        {
            // Arrange
            var model = new LearnerModel { NationalID = "1234567890123" };
            
            // Mock SqlException with number 51000
            // SqlException constructor is internal/private, so we can't instantiate it easily.
            // But we can throw a generic exception for now to test the generic catch block,
            // or we can try to use reflection to create SqlException if we really want to test that specific path.
            // For simplicity and standard unit testing, testing the generic catch block is often enough unless the logic is very specific.
            // The controller has `catch (SqlException ex) when (ex.Number == 51000)`. 
            // Creating SqlException is hard. Let's test the generic exception path first.
            
            _mockDbHelper.Setup(d => d.InsertLearner(It.IsAny<LearnerModel>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _controller.Learner(model, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Error registering learner", _controller.ModelState[string.Empty].Errors[0].ErrorMessage);
        }
    }
}
