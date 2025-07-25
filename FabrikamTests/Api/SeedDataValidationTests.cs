using FabrikamTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace FabrikamTests.Api;

[Trait("Category", "SeedDataValidation")]
public class SeedDataValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SeedDataValidationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsAllSeedDataProducts()
    {
        // Arrange
        var expectedProducts = await SeedDataHelper.LoadProductsAsync();

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        products.Should().NotBeNull();
        products!.Should().HaveCount(expectedProducts.Count,
            "API should return exactly the same number of products as defined in seed data");

        // Validate against seed data using our helper
        await SeedDataHelper.ValidateProductListResponse(products);
    }

    [Theory]
    [InlineData(1)] // Cozy Cottage 1200
    [InlineData(3)] // Executive Manor 2500  
    [InlineData(5)] // Backyard Studio 400
    public async Task GetProduct_WithValidSeedDataId_ReturnsCorrectProduct(int productId)
    {
        // Arrange
        var expectedProduct = await SeedDataHelper.GetProductByIdAsync(productId);
        expectedProduct.Should().NotBeNull($"Product {productId} should exist in seed data");

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var productData = JsonSerializer.Deserialize<ProductSeedData>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        productData.Should().NotBeNull();
        productData!.Name.Should().Be(expectedProduct!.Name);
        productData.ModelNumber.Should().Be(expectedProduct.ModelNumber);
        productData.Price.Should().Be(expectedProduct.Price);
        productData.Category.Should().Be(expectedProduct.Category);
    }

    [Fact]
    public async Task GetProduct_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = await SeedDataHelper.GetNonExistentProductIdAsync();

        // Act
        var response = await _client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("SingleFamily")]
    [InlineData("Duplex")]
    [InlineData("Accessory")]
    public async Task GetProducts_ByCategory_ReturnsCorrectSeedDataProducts(string category)
    {
        // Arrange
        var expectedProducts = await SeedDataHelper.GetProductsByCategoryAsync(category);

        // Act
        var response = await _client.GetAsync($"/api/products?category={category}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductSeedData>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        products.Should().NotBeNull();
        products!.Should().HaveCount(expectedProducts.Count,
            $"API should return exactly {expectedProducts.Count} products for category {category}");

        // Validate each product is in the expected category
        products.Should().OnlyContain(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(1)] // Sarah Johnson
    [InlineData(4)] // David Rodriguez
    [InlineData(7)] // Amanda Brown
    public async Task GetCustomer_WithValidSeedDataId_ReturnsCorrectCustomer(int customerId)
    {
        // Arrange
        var expectedCustomer = await SeedDataHelper.GetCustomerByIdAsync(customerId);
        expectedCustomer.Should().NotBeNull($"Customer {customerId} should exist in seed data");

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var customerData = JsonSerializer.Deserialize<CustomerSeedData>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customerData.Should().NotBeNull();
        customerData!.FirstName.Should().Be(expectedCustomer!.FirstName);
        customerData.LastName.Should().Be(expectedCustomer.LastName);
        customerData.Email.Should().Be(expectedCustomer.Email);
        customerData.Region.Should().Be(expectedCustomer.Region);
    }

    [Fact]
    public async Task GetCustomer_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = await SeedDataHelper.GetNonExistentCustomerIdAsync();

        // Act
        var response = await _client.GetAsync($"/api/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Pacific Northwest")]
    [InlineData("Southwest")]
    [InlineData("Northeast")]
    public async Task GetCustomers_ByRegion_ReturnsCorrectSeedDataCustomers(string region)
    {
        // Arrange
        var expectedCustomers = await SeedDataHelper.GetCustomersByRegionAsync(region);

        // Act
        var response = await _client.GetAsync($"/api/customers?region={Uri.EscapeDataString(region)}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<List<CustomerSeedData>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customers.Should().NotBeNull();
        customers!.Should().HaveCount(expectedCustomers.Count,
            $"API should return exactly {expectedCustomers.Count} customers for region {region}");

        // Validate each customer is in the expected region
        customers.Should().OnlyContain(c => c.Region.Equals(region, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetSalesAnalytics_MatchesCalculatedSeedData()
    {
        // Arrange
        var expectedAnalytics = await SeedDataHelper.CalculateExpectedSalesAnalyticsAsync();

        // Act
        var response = await _client.GetAsync("/api/orders/analytics");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var analyticsData = JsonSerializer.Deserialize<SalesAnalyticsSeedData>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        analyticsData.Should().NotBeNull();
        analyticsData!.TotalOrders.Should().Be(expectedAnalytics.TotalOrders,
            "Total orders should match calculated seed data");
        analyticsData.TotalRevenue.Should().Be(expectedAnalytics.TotalRevenue,
            "Total revenue should match calculated seed data");
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Pending")]
    [InlineData("InProduction")]
    public async Task GetOrders_ByStatus_ReturnsCorrectSeedDataOrders(string status)
    {
        // Arrange
        var expectedOrders = await SeedDataHelper.GetOrdersByStatusAsync(status);

        // Act
        var response = await _client.GetAsync($"/api/orders?status={status}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderSeedData>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        orders.Should().NotBeNull();
        orders!.Should().HaveCount(expectedOrders.Count,
            $"API should return exactly {expectedOrders.Count} orders with status {status}");

        // Validate each order has the expected status
        orders.Should().OnlyContain(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetOrder_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = await SeedDataHelper.GetNonExistentOrderIdAsync();

        // Act
        var response = await _client.GetAsync($"/api/orders/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SeedData_JsonFiles_AreValid()
    {
        // Arrange & Act
        var products = await SeedDataHelper.LoadProductsAsync();
        var customers = await SeedDataHelper.LoadCustomersAsync();
        var orders = await SeedDataHelper.LoadOrdersAsync();

        // Assert
        products.Should().NotBeEmpty("Products seed data should be loaded");
        customers.Should().NotBeEmpty("Customers seed data should be loaded");
        orders.Should().NotBeEmpty("Orders seed data should be loaded");

        // Validate data integrity
        products.All(p => p.Id > 0).Should().BeTrue("All products should have valid IDs");
        customers.All(c => c.Id > 0).Should().BeTrue("All customers should have valid IDs");
        orders.All(o => o.Id > 0).Should().BeTrue("All orders should have valid IDs");

        // Validate foreign key relationships
        var customerIds = customers.Select(c => c.Id).ToHashSet();
        var productIds = products.Select(p => p.Id).ToHashSet();

        foreach (var order in orders)
        {
            customerIds.Should().Contain(order.CustomerId,
                $"Order {order.Id} references non-existent customer {order.CustomerId}");

            foreach (var item in order.Items)
            {
                productIds.Should().Contain(item.ProductId,
                    $"Order {order.Id} item references non-existent product {item.ProductId}");
            }
        }
    }
}
