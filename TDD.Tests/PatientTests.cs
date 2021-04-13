using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TDD.Tests.PatientTests
{
    public class PatientTests : IClassFixture<PatientTestsDbWAF<Startup>>
    {
        // HttpClient to call our api's
        private readonly HttpClient httpClient;
        public WebApplicationFactory<Startup> _factory;

        public PatientTests(PatientTestsDbWAF<Startup> factory)
        {
            _factory = factory;

            // Initiate the HttpClient
            httpClient = _factory.CreateClient();
        }

        [Theory]
        [InlineData("Test Name 2", "1234567891", 20, "Male", HttpStatusCode.Created)]
        [InlineData("T", "1234567891", 20, "Male", HttpStatusCode.BadRequest)]
        [InlineData("A very very very very very very loooooooooong name", "1234567891", 20, "Male", HttpStatusCode.BadRequest)]
        [InlineData(null, "1234567890", 20, "Invalid Gender", HttpStatusCode.BadRequest)]
        [InlineData("Test Name", "InvalidNumber", 20, "Male", HttpStatusCode.BadRequest)]
        [InlineData("Test Name", "1234567890", -10, "Male", HttpStatusCode.BadRequest)]
        [InlineData("Test Name", "1234567890", 20, "Invalid Gender", HttpStatusCode.BadRequest)]
        [InlineData("Test Name", "12345678901234444", 20, "Invalid Gender", HttpStatusCode.BadRequest)]
        public async Task PatientTestsAsync(String Name, String PhoneNumber, int Age, String Gender, HttpStatusCode ResponseCode)
        {
            var scopeFactory = _factory.Services;
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                await DBUtilities.InitializeDbForTestsAsync(context);

                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Post, "api/patient");

                request.Content = new StringContent(JsonSerializer.Serialize(new Patient
                {
                    Name = Name,
                    PhoneNumber = PhoneNumber,
                    Age = Age,
                    Gender = Gender
                }), Encoding.UTF8, "application/json");

                // Act
                var response = await httpClient.SendAsync(request);

                // Assert
                var StatusCode = response.StatusCode;
                Assert.Equal(ResponseCode, StatusCode);
            }
        }

        [Fact]
        public async Task PatientDuplicationTestsAsync()
        {
            var scopeFactory = _factory.Services;
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                await DBUtilities.InitializeDbForTestsAsync(context);

                // Arrange
                var Patient = await context.Patient.FirstOrDefaultAsync();

                var Request = new HttpRequestMessage(HttpMethod.Post, "api/patient");
                Request.Content = new StringContent(JsonSerializer.Serialize(Patient), Encoding.UTF8, "application/json");

                // Act
                var Response = await httpClient.SendAsync(Request);

                // Assert
                var StatusCode = Response.StatusCode;
                Assert.Equal(HttpStatusCode.BadRequest, StatusCode);
            }
        }

        [Fact]
        public async Task PatientAdmitTwiceTestsAsync()
        {

            var scopeFactory = _factory.Services;
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                await DBUtilities.InitializeDbForTestsAsync(context);

                // Arrange
                var Patient = await context.Patient.FirstOrDefaultAsync();
                var Room = await context.Room.FirstOrDefaultAsync();

                var RoomPatient = new RoomPatient
                {
                    RoomId = Room.Id,
                    PatientId = Patient.Id
                };
                var Request1 = new HttpRequestMessage(HttpMethod.Post, "api/patient/admit");
                Request1.Content = new StringContent(JsonSerializer.Serialize(RoomPatient), Encoding.UTF8, "application/json");

                var Request2 = new HttpRequestMessage(HttpMethod.Post, "api/patient/admit");
                Request2.Content = new StringContent(JsonSerializer.Serialize(RoomPatient), Encoding.UTF8, "application/json");

                // Act
                var Response1 = await httpClient.SendAsync(Request1);
                Response1.EnsureSuccessStatusCode();

                var Response2 = await httpClient.SendAsync(Request2);

                // Assert
                var StatusCode = Response2.StatusCode;
                Assert.Equal(HttpStatusCode.BadRequest, StatusCode);
            }
        }

        [Fact]
        public async Task PatientCheckoutTwiceTestsAsync()
        {

            var scopeFactory = _factory.Services;
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                await DBUtilities.InitializeDbForTestsAsync(context);

                // Arrange
                var Patient = await context.Patient.FirstOrDefaultAsync();
                var Room = await context.Room.FirstOrDefaultAsync();

                var RoomPatient = new RoomPatient
                {
                    RoomId = Room.Id,
                    PatientId = Patient.Id
                };
                var Request = new HttpRequestMessage(HttpMethod.Post, "api/patient/admit");
                Request.Content = new StringContent(JsonSerializer.Serialize(RoomPatient), Encoding.UTF8, "application/json");

                var Request1 = new HttpRequestMessage(HttpMethod.Post, "api/patient/checkout");
                Request1.Content = new StringContent(JsonSerializer.Serialize(Patient), Encoding.UTF8, "application/json");

                var Request2 = new HttpRequestMessage(HttpMethod.Post, "api/patient/checkout");
                Request2.Content = new StringContent(JsonSerializer.Serialize(Patient), Encoding.UTF8, "application/json");

                // Act
                var Response = await httpClient.SendAsync(Request);
                Response.EnsureSuccessStatusCode();

                var Response1 = await httpClient.SendAsync(Request1);
                Response1.EnsureSuccessStatusCode();

                var Response2 = await httpClient.SendAsync(Request2);

                // Assert
                var StatusCode = Response2.StatusCode;
                Assert.Equal(HttpStatusCode.BadRequest, StatusCode);
            }
        }

        [Fact]
        public async Task PatientAdmitDifferentRoomsTestsAsync()
        {

            var scopeFactory = _factory.Services;
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                await DBUtilities.InitializeDbForTestsAsync(context);

                // Arrange
                var Patient = await context.Patient.FirstOrDefaultAsync();
                var Room1 = await context.Room.FirstOrDefaultAsync(x => x.RoomType == "ICU");
                var Room2 = await context.Room.FirstOrDefaultAsync(x => x.RoomType == "General");

                var RoomPatient = new RoomPatient
                {
                    RoomId = Room1.Id,
                    PatientId = Patient.Id
                };
                var Request1 = new HttpRequestMessage(HttpMethod.Post, "api/patient/admit");
                Request1.Content = new StringContent(JsonSerializer.Serialize(RoomPatient), Encoding.UTF8, "application/json");

                RoomPatient.RoomId = Room2.Id;
                var Request2 = new HttpRequestMessage(HttpMethod.Post, "api/patient/admit");
                Request2.Content = new StringContent(JsonSerializer.Serialize(RoomPatient), Encoding.UTF8, "application/json");

                // Act
                var Response1 = await httpClient.SendAsync(Request1);
                Response1.EnsureSuccessStatusCode();

                var Response2 = await httpClient.SendAsync(Request2);

                // Assert
                var StatusCode = Response2.StatusCode;
                Assert.Equal(HttpStatusCode.BadRequest, StatusCode);
            }
        }

        [Theory]
        [InlineData("Te", 1)]
        [InlineData("st", 1)]
        [InlineData("tient", 1)]
        [InlineData("Test Patient", 1)]
        [InlineData("123", 1)]
        [InlineData("7890", 1)]
        [InlineData("789", 1)]
        [InlineData("1234567890", 1)]
        [InlineData("Invalid Name", 0)]
        [InlineData("4028235", 0)]
        public async Task PatientSearchTestsAsync(String Search, int Count)
        {

            var scopeFactory = _factory.Services;
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                // Initialize the database, so that changes made by other tests are reset. 
                await DBUtilities.InitializeDbForTestsAsync(context);

                // Arrange
                var Request1 = new HttpRequestMessage(HttpMethod.Get, $"api/patient?Search={Search}");

                // Act
                var Response1 = await httpClient.SendAsync(Request1);
                Response1.EnsureSuccessStatusCode();

                var ResponseString = await Response1.Content.ReadAsStringAsync();
                var Patients = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Patient>>(ResponseString);

                // Assert
                Assert.Equal(Count, Patients.Count);
                if (Count > 0)
                {
                    Assert.Equal("Test Patient", Patients[0].Name);
                    Assert.Equal("1234567890", Patients[0].PhoneNumber);
                }
            }
        }
    }
}