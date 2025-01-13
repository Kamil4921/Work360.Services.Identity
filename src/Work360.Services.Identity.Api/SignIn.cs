using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace Work360.Services.Identity.Api;

public static class SignIn
{
    private static readonly string ClientId = Environment.GetEnvironmentVariable("ClientId"); 
    private static readonly string TenantId = Environment.GetEnvironmentVariable("TenantId"); 
    private static readonly string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
    private static readonly string TokenEndpoint = $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";    
    
    private static readonly HttpClient httpClient = new();
    
    [FunctionName("SignIn")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        ILogger log)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(); 
        dynamic data = JsonConvert.DeserializeObject(requestBody); 
        string email = data?.email; 
        string password = data?.password;

        var keyValues = new List<KeyValuePair<string, string>>
        {
            new("client_id", ClientId),
            new("scope", "https://graph.microsoft.com/.default"),
            new("client_secret", ClientSecret),
            new("grant_type", "password"),
            new("username", email), new("password", password)
        };
        
        var requestContent = new FormUrlEncodedContent(keyValues); 
        requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        
        var response = await httpClient.PostAsync(TokenEndpoint, requestContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        dynamic token = JsonConvert.DeserializeObject(responseContent);
        
        return new OkObjectResult(new { accessToken = token?.access_token });
    }
}