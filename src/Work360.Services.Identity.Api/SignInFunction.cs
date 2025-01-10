using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace Work360.Services.Identity.Api;

public class SignInFunction(ILogger<SignInFunction> logger)
{
    
    private static readonly string ClientId = Environment.GetEnvironmentVariable("ClientId"); 
    private static readonly string TenantId = Environment.GetEnvironmentVariable("TenantId"); 
    private static readonly string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
    
    [Function("SignIn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody); 
        
        string? email = data?.email; 
        string? password = data?.password; 
        
        var token = await AuthenticateUserAsync(email, password); 
        
        return new OkObjectResult(new { token });
        
    }

    private async Task<string> AuthenticateUserAsync(string? email, string? password)
    {
        var app = ConfidentialClientApplicationBuilder
            .Create(ClientId)
            .WithClientSecret(ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{TenantId}")) 
            .Build();
        
        var scopes = new string[] { "https://graph.microsoft.com/.default" };
        
        var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
        return result.AccessToken;
    }
}