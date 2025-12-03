using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BarcodeApp.Tests;

public static class TestLicenseActivation
{
    public static async Task TestAsync()
    {
        const string licenseKey = "HKLqUE";
        const string serverUrl = "https://v0-license-server-setup.vercel.app/api/validateKey";
        
        var machineId = "ed6ec7ae2fb5c9b5723dfeac6985eaed8a05d0ffd9c6c57792e680ea107c37e1"; // Example machine ID
        
        var request = new
        {
            Key = licenseKey,
            MachineId = machineId,
            AppVersion = "1.0"
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // PascalCase
        };

        var json = JsonSerializer.Serialize(request, jsonOptions);
        Console.WriteLine($"Request JSON: {json}");

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await httpClient.PostAsync(serverUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");
            
            var result = JsonSerializer.Deserialize<LicenseResponse>(responseContent, jsonOptions);
            if (result != null)
            {
                Console.WriteLine($"IsValid: {result.IsValid}");
                Console.WriteLine($"ExpiryDate: {result.ExpiryDate}");
                Console.WriteLine($"StatusMessage: {result.StatusMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private class LicenseResponse
    {
        public bool IsValid { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? StatusMessage { get; set; }
    }
}

