using Microsoft.AspNetCore.Mvc;

using API.Controllers;
using API.Services;
using API.Repository;
using Shared;
using Shared.DTOs;

namespace API_testing
{
    public class DatabaseControllerTests : IDisposable
    {
        private readonly DatabaseController _controller;
        private readonly string _dbDirectory = @"C:\Users\ostre\OneDrive\Books\4th_course\IT\LAB1-DBMS\Storage";
        private string _dbName = "UnitTestDatabase";

        public DatabaseControllerTests()
        {
            var repository = new DatabaseRepository();
            var service = new DatabaseService(repository);
            _controller = new DatabaseController(service);


            var config_controller = new ConfigController();
            Constants.BasePath = _dbDirectory;
            var dto = new UpdatePathDTO()
            {
                NewBasePath = Constants.BasePath
            };
            config_controller.UpdateBasePath(dto);
        }

        [Fact]
        public void CreateDatabase_ShouldCreateDatabaseSuccessfully()
        {
            // Arrange
            var request = new CreateDatabaseDTO
            {
                DatabaseName = _dbName
            };

            // Act
            var result = _controller.CreateDatabase(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal($"Database '{request.DatabaseName}' created successfully", result.Value);


            string expectedFilePath = Path.Combine(_dbDirectory, $"{request.DatabaseName}.json");
            Assert.True(File.Exists(expectedFilePath));
        }

        [Fact]
        public void CreateDatabase_ShouldReturnErrorIfDatabaseExists()
        {

            var request = new CreateDatabaseDTO
            {
                DatabaseName = _dbName
            };

            // Act
            _controller.CreateDatabase(request);
            var result = _controller.CreateDatabase(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("An error occurred: Database already exists.", result.Value);
        }


        public void Dispose()
        {

            if (Directory.Exists(_dbDirectory))
            {
                File.Delete($"{_dbDirectory}\\{_dbName}.json");
            }
        }
    }
}