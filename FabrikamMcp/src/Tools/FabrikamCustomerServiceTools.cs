using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace FabrikamMcp.Tools;

[McpServerToolType]
public class FabrikamCustomerServiceTools
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FabrikamCustomerServiceTools(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [McpServerTool, Description("Get support tickets with optional filtering by status, priority, category, region, assigned agent, or specific ticket ID. Use ticketId for detailed ticket info, or use filters for ticket lists. Set urgent=true for high/critical priority tickets.")]
    public async Task<string> GetSupportTickets(
        int? ticketId = null,
        string? status = null,
        string? priority = null,
        string? category = null,
        string? region = null,
        string? assignedTo = null,
        bool urgent = false,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            
            // If ticketId is provided, get specific ticket details
            if (ticketId.HasValue)
            {
                var ticketResponse = await _httpClient.GetAsync($"{baseUrl}/api/supporttickets/{ticketId.Value}");
                
                if (ticketResponse.IsSuccessStatusCode)
                {
                    var ticket = await ticketResponse.Content.ReadAsStringAsync();
                    return $"Support ticket details for ID {ticketId.Value}:\n{ticket}";
                }
                
                if (ticketResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return $"Support ticket with ID {ticketId.Value} not found";
                }
                
                return $"Error retrieving support ticket {ticketId.Value}: {ticketResponse.StatusCode} - {ticketResponse.ReasonPhrase}";
            }
            
            // Build query parameters for ticket list
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(priority)) queryParams.Add($"priority={Uri.EscapeDataString(priority)}");
            if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");
            if (!string.IsNullOrEmpty(region)) queryParams.Add($"region={Uri.EscapeDataString(region)}");
            if (!string.IsNullOrEmpty(assignedTo)) queryParams.Add($"assignedTo={Uri.EscapeDataString(assignedTo)}");
            
            // Handle urgent tickets filter
            if (urgent)
            {
                queryParams.Add("urgent=true");
            }
            
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/supporttickets{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var tickets = await response.Content.ReadAsStringAsync();
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                var urgentText = urgent ? " urgent" : "";
                return $"Found {totalCount ?? "unknown"} total{urgentText} support tickets. Page {page} results:\n{tickets}";
            }
            
            return $"Error retrieving support tickets: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving support tickets: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get customer service analytics including ticket volume, resolution times, and breakdowns by status, priority, and category.")]
    public async Task<string> GetCustomerServiceAnalytics(string? fromDate = null, string? toDate = null)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(fromDate)) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate)}");
            if (!string.IsNullOrEmpty(toDate)) queryParams.Add($"toDate={Uri.EscapeDataString(toDate)}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/supporttickets/analytics{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var analytics = await response.Content.ReadAsStringAsync();
                return $"Customer service analytics:\n{analytics}";
            }
            
            return $"Error retrieving customer service analytics: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving customer service analytics: {ex.Message}";
        }
    }

    [McpServerTool, Description("Add a note to an existing support ticket. Specify if the note is internal (visible only to staff) or external (visible to customer).")]
    public async Task<string> AddTicketNote(int ticketId, string note, string createdBy, bool isInternal = false)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var noteData = new
            {
                note = note,
                createdBy = createdBy,
                isInternal = isInternal
            };

            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/supporttickets/{ticketId}/notes", noteData);
            
            if (response.IsSuccessStatusCode)
            {
                return $"Successfully added {(isInternal ? "internal" : "external")} note to ticket {ticketId} by {createdBy}";
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return $"Support ticket with ID {ticketId} not found";
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return $"Error adding note to ticket {ticketId}: {response.StatusCode} - {errorContent}";
        }
        catch (Exception ex)
        {
            return $"Error adding note to ticket {ticketId}: {ex.Message}";
        }
    }

    [McpServerTool, Description("Update a support ticket's status, priority, and/or assignment. Available statuses: Open, InProgress, PendingCustomer, Resolved, Closed, Cancelled")]
    public async Task<string> UpdateTicketStatus(
        int ticketId, 
        string status, 
        string? assignedTo = null, 
        string? priority = null)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var updateData = new
            {
                status = status,
                assignedTo = assignedTo,
                priority = priority
            };

            var response = await _httpClient.PatchAsJsonAsync($"{baseUrl}/api/supporttickets/{ticketId}/status", updateData);
            
            if (response.IsSuccessStatusCode)
            {
                var details = new List<string> { $"Status updated to: {status}" };
                if (!string.IsNullOrEmpty(assignedTo)) details.Add($"Assigned to: {assignedTo}");
                if (!string.IsNullOrEmpty(priority)) details.Add($"Priority set to: {priority}");
                
                return $"Successfully updated ticket {ticketId}:\n{string.Join("\n", details)}";
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return $"Support ticket with ID {ticketId} not found";
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return $"Error updating ticket {ticketId}: {response.StatusCode} - {errorContent}";
        }
        catch (Exception ex)
        {
            return $"Error updating ticket {ticketId}: {ex.Message}";
        }
    }
}
