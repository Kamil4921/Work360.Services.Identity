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
    private static readonly string ROPCPolicyId = Environment.GetEnvironmentVariable("ROPCPolicyId");
    private static readonly string TokenEndpoint = $"https://work36azemployes.b2clogin.com/work36azemployes/oauth2/v2.0/token?p={ROPCPolicyId}";
    
    private static readonly HttpClient httpClient = new HttpClient();
    
    [FunctionName("SignIn")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        ILogger log)
    {
        // !!
        //TODO: Try new function with startup and configure there authorization as in udemy with B2C 
        // !!
        
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        string email = data?.email;
        string password = data?.password;
        
        var requestBodySend = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", ClientId), 
            new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"), 
            new KeyValuePair<string, string>("client_secret", ClientSecret), 
            new KeyValuePair<string, string>("grant_type", "password"), 
            new KeyValuePair<string, string>("username", email), 
            new KeyValuePair<string, string>("password", password)
        });
        
        var response = await httpClient.PostAsync(TokenEndpoint, requestBodySend);
        /*
        var confidentialClient = ConfidentialClientApplicationBuilder.Create(ClientId)
            .WithClientSecret(ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{TenantId}"))
            .Build();
        
        var scopes = new string[] { "https://graph.microsoft.com/.default" };
        
        var result = await confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();
*/
        var responseContent = await response.Content.ReadAsStringAsync();
        dynamic token = JsonConvert.DeserializeObject(responseContent);
        
        return new OkObjectResult(new { accessToken = token?.access_token });
    }
}