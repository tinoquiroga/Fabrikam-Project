using System.Text.Json;

// Test the Business Dashboard tool directly
var httpClient = new HttpClient();

// Test with "year" timeframe parameter
var testData = new
{
    method = "call",
    @params = new
    {
        name = "GetBusinessDashboard",
        arguments = new
        {
            timeframe = "year",
            includeForecasts = false
        }
    }
};

var json = JsonSerializer.Serialize(testData, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine($"Testing Business Dashboard with payload:\n{json}\n");

try
{
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    var response = await httpClient.PostAsync("http://localhost:5000/", content);

    if (response.IsSuccessStatusCode)
    {
        var responseText = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"✅ Success! Response:\n{responseText}");

        // Parse and check for errors
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseText);
        if (responseObj.TryGetProperty("error", out var error))
        {
            Console.WriteLine($"❌ Tool returned error: {error}");
        }
        else if (responseObj.TryGetProperty("result", out var result))
        {
            Console.WriteLine("✅ Tool executed successfully!");
            if (result.TryGetProperty("content", out var content_prop) && content_prop.ValueKind == JsonValueKind.Array)
            {
                var firstContent = content_prop.EnumerateArray().FirstOrDefault();
                if (firstContent.TryGetProperty("text", out var text))
                {
                    var textValue = text.GetString();
                    Console.WriteLine($"📊 Dashboard preview (first 300 chars):\n{textValue?.Substring(0, Math.Min(300, textValue.Length ?? 0))}...");
                }
            }
        }
    }
    else
    {
        Console.WriteLine($"❌ HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
        var errorText = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error details: {errorText}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Exception: {ex.Message}");
}

httpClient.Dispose();
