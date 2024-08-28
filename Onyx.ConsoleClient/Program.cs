using IdentityModel.Client;
using System.Net.Http.Headers;

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

Console.WriteLine(tokenResponse.AccessToken);
Console.ReadKey();
