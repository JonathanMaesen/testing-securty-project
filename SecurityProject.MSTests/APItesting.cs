using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using security_testing_project;

namespace ApiTests
{
    [TestClass]
    public class ApiServiceTests
    {
        private ApiService _api;

        [TestInitialize]
        public void Setup()
        {
            var httpClient = new HttpClient();
            _api = new ApiService(httpClient, "http://localhost:3001");
        }

        // ---------------- REGISTER ----------------

        [TestMethod]
        public async Task Register_ValidPassword_ReturnsSuccess()
        {
            var (success, message) = await _api.RegisterAsync(
                username: "newuser",
                password: "password123",
                role: "Player"
            );

            Assert.IsTrue(success);
            Assert.IsTrue(message.ToLower().Contains("succes"));
        }

        [TestMethod]
        public async Task Login_WrongCredentials_Fails()
        {
            var (success, message) = await _api.LoginAsync(
                username: "wrong",
                password: "wrong"
            );

            Assert.IsFalse(success);
            Assert.IsFalse(_api.IsLoggedIn);
        }

        [TestMethod]
        public async Task KeyShare_NotLoggedIn_ReturnsNull()
        {
            var keyshare = await _api.GetKeyShareAsync("room_admin");

            Assert.IsNull(keyshare);
        }
    }
}
