using System;
using System.Net.Http;
using System.Threading.Tasks;
using FabrikamMcp.Tools;

namespace FabrikamMcp.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Testing Fabrikam MCP Enhanced Tools");
            Console.WriteLine("=======================================");
            
            // Create HttpClient
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5235");
            
            // Test FabrikamSalesTools
            var salesTools = new FabrikamSalesTools(httpClient);
            
            Console.WriteLine("\n📊 Testing GetSalesAnalytics...");
            try
            {
                var result = await salesTools.GetSalesAnalytics();
                
                Console.WriteLine("✅ GetSalesAnalytics completed successfully!");
                Console.WriteLine("📋 Result type: " + result.GetType().Name);
                
                // Check if it's the enhanced structured response
                if (result is object structuredResult)
                {
                    Console.WriteLine("🎯 Enhanced structured response detected!");
                    var json = System.Text.Json.JsonSerializer.Serialize(structuredResult, new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                    Console.WriteLine("📄 Sample response (first 500 chars):");
                    Console.WriteLine(json.Length > 500 ? json.Substring(0, 500) + "..." : json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error testing GetSalesAnalytics: " + ex.Message);
                Console.WriteLine("🔍 Exception details: " + ex.ToString());
            }
            
            Console.WriteLine("\n✨ Test completed!");
        }
    }
}
