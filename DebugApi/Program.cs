using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var accessToken = "polar_oat_BArndS0lOEbxHpy7ybDXaVAgQ2b3zVgxxen961iCVZs";
        var baseUrl = "https://sandbox-api.polar.sh";
        
        using var client = new HttpClient();
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "Polar.NET/1.0.0");
        
        Console.WriteLine($"Testing API connection to: {baseUrl}");
        Console.WriteLine($"Using token: {accessToken.Substring(0, 20)}...");
        
        try
        {
            // Test organizations endpoint (might require different permissions)
            Console.WriteLine("\nTesting GET /v1/organizations...");
            var response = await client.GetAsync("v1/organizations");
            
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response length: {content.Length}");
                Console.WriteLine($"First 200 chars: {content.Substring(0, Math.Min(200, content.Length))}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
