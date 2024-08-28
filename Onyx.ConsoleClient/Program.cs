using IdentityModel.Client;
using Onyx.API.Products.Client.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

// discover endpoints from metadata
var client = new HttpClient();
var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    return;
}

// request token
var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,
    ClientId = "onyx_products_admin",
    ClientSecret = "secret",
    Scope = "products"
});

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    Console.WriteLine(tokenResponse.ErrorDescription);
    return;
}

client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
var apiResult = await client.GetStringAsync("https://localhost:6001/weatherforecast");

apiResult = await client.GetStringAsync("https://localhost:6001/products");
apiResult = await client.GetStringAsync("https://localhost:6001/products/colour/yellow");

var productToPut = new Product { 
    Name = "Eggs2",
    Colour = "Orange"
};
//var jsonString = @"{
//  ""name"": ""Updated Product"",
//  ""price"": 29.99
//}";


var putResult = await client.PutAsJsonAsync("https://localhost:6001/products/", productToPut);



Console.WriteLine(tokenResponse.AccessToken);
Console.ReadKey();
