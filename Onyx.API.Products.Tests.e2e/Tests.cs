using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Reflection;
using IdentityModel.Client;
using static IdentityModel.OidcConstants;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Xunit.Abstractions;
using Onyx.API.Products.Tests.e2e;

namespace Onyx.API.Products.Tests.e2e
{
    public class TestsFixture : IDisposable
    {
        Process _authProcess = null;
        Process _apiProcess = null;

        public TestsFixture() {
            startServers();
        }
        public void Dispose()
        {
            stopServers();
        }

        void startServers()
        {
            var current = Assembly.GetExecutingAssembly().Location;
            var cutPoint = current.IndexOf(@"\Onyx.API.Products.Tests.e2e\bin\Debug\net8.0\Onyx.API.Products.Tests.e2e.dll");
            var root = current.Substring(0, cutPoint);
            const string authExeName = "Onyx.AuthService.exe";
            const string authExePartialPath = @"\Onyx.AuthService\bin\Debug\net8.0\";
            var authExe = root + authExePartialPath + authExeName;
            const string apiExeName = "Onyx.API.Products.exe";
            const string apiExePartialPath = @"\Onyx.API.Products\bin\Debug\net8.0\";
            var apiExe = root + apiExePartialPath + apiExeName;
            StartExe(ref _authProcess, root, authExePartialPath, authExe);
            StartExe(ref _apiProcess, root, apiExePartialPath, apiExe);
            Task.Delay(10 * 1000).Wait();
        }

        void StartExe(ref Process process, string root, string exePartialPath, string exe)
        {
            process = new Process();
            process.StartInfo.WorkingDirectory = root + exePartialPath;
            process.StartInfo.FileName = exe;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        void stopServers()
        {
            _authProcess?.Kill();
            _apiProcess?.Kill();
        }
    }

    public class TestsHelper() 
    {
        public static void DeleteProducts()
        {
            using (var context = new ProductsDbContext())
            {
                var entitiesToDelete = context.Products.ToList();
                context.RemoveRange(entitiesToDelete);
                context.SaveChanges();
            }
        }
    }

    public class Tests : IClassFixture<TestsFixture>
    {
        private HttpClient _client = new();
        const string authServerUrl = "https://localhost:5001";
        const string healthCheckUrl = "https://localhost:6001/hc";
        const string productsUrl = "https://localhost:6001/products";

        public Tests() {
            TestsHelper.DeleteProducts();
        }

        [Fact]
        public async Task ApiServerHealthCheckIsPublicAndHealthy()
        {
            var apiResult = await _client.GetStringAsync(healthCheckUrl);
            Assert.Equal("Healthy", apiResult);
        }

        private async Task<IdentityModel.Client.TokenResponse> GetTokenForValidUser() {
            var disco = await _client.GetDiscoveryDocumentAsync(authServerUrl);
            return await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "onyx_products_admin",
                ClientSecret = "secret",
                Scope = "products"
            });
        }

        [Fact]
        public async Task ApiServerProductsRejectsAnonRequest()
        {
            Task result () => _client.GetStringAsync(productsUrl); 
            await Assert.ThrowsAsync<HttpRequestException>(result);
        }
        [Fact]
        public async Task ApiServerProductsAllowsAccessToProductsForAuthenticatdUser()
        {
            var token = await GetTokenForValidUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var apiResult = await _client.GetStringAsync(productsUrl);
        }

        [Fact]
        public async void ApiServerProductsReturnsBadRequestForBadProductsFilter()
        {
            var token = await GetTokenForValidUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var apiResult = await _client.GetAsync($"{productsUrl}/badfield/yellow");
            Assert.True(apiResult.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void ApiServerProductsReturnsProductForValidFilter()
        {
            var token = await GetTokenForValidUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var productToPut = new Product
            {
                Name = $"Product-ApiServerProductsReturnsProductForValidFilter",
                Colour = "Yellow"
            };
            var putResult = await _client.PutAsJsonAsync(productsUrl, productToPut);
            var apiResult = await _client.GetStringAsync($"{productsUrl}/colour/yellow");
            var results = JsonConvert.DeserializeObject<List<Product>>(apiResult);
            foreach (var r in results) {
                Assert.Equal("Yellow", r.Colour);
            }
        }

        [Fact]
        public async void ApiServerReturnsConflictForDuplicateProductName()
        {
            var token = await GetTokenForValidUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var duplicateName = "Product-ApiServerReturnsBadRequestForDuplicateProductName";
            var productToPut = new Product
            {
                Name = duplicateName,
                Colour = "Orange"
            };
            var putResult = await _client.PutAsJsonAsync(productsUrl, productToPut);
            productToPut = new Product
            {
                Name = duplicateName,
                Colour = "Green"
            };
            putResult = await _client.PutAsJsonAsync(productsUrl, productToPut);
            Assert.True(putResult.StatusCode == System.Net.HttpStatusCode.Conflict);
        }

        [Fact]
        public async void ApiServerCreatesProductForNewProduct()
        {
            var token = await GetTokenForValidUser();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var productToPut = new Product
            {
                Name = "Product-ApiServerCreatesProductForNewProduct",
                Colour = "Orange"
            };
            var putResult = await _client.PutAsJsonAsync(productsUrl, productToPut);
            var apiResult = await _client.GetStringAsync(productsUrl);
            var results = JsonConvert.DeserializeObject<List<Product>>(apiResult);
            var matches = results.Where(r => r.Name == productToPut.Name);
            Assert.True(matches.Count() == 1);             
        }

        [Fact]
        public async void AuthServerCanServeDiscoveryDoc()
        {
            var disco = await _client.GetDiscoveryDocumentAsync(authServerUrl);
            Assert.False(disco.IsError);
        }

        [Fact]
        public async void AuthServeReturnTokenForValidUser()
        {
            var disco = await _client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "onyx_products_admin",
                ClientSecret = "secret",
                Scope = "products"
            });
            Assert.False(tokenResponse.IsError);
        }

        [Fact]
        public async void AuthServeDoesNotReturnTokenForInvalidUser()
        {
            var disco = await _client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "NOT_A_USER",
                ClientSecret = "secret",
                Scope = "products"
            });
            Assert.True(tokenResponse.IsError);
        }

        [Fact]
        public async void AuthServeDoesNotReturnTokenForBadSecret()
        {
            var disco = await _client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "onyx_products_admin",
                ClientSecret = "BAD_SECRET",
                Scope = "products"
            });
            Assert.True(tokenResponse.IsError);
        }
    }
}