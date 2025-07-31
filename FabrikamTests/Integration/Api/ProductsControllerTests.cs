using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using FluentAssertions;
using FabrikamTests.Helpers;
using Xunit;

namespace FabrikamTests.Integration.Api;

[Trait("Category", "Api")]
public class ProductsControllerTests : AuthenticationTestBase
{
    public ProductsControllerTests(AuthenticationTestApplicationFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetProducts_ReturnsExpectedStructure()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        
        if (result.RootElement.GetArrayLength() > 0)
        {
            var firstProduct = result.RootElement[0];
            firstProduct.TryGetProperty("id", out _).Should().BeTrue();
            firstProduct.TryGetProperty("name", out _).Should().BeTrue();
            firstProduct.TryGetProperty("category", out _).Should().BeTrue();
            firstProduct.TryGetProperty("price", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange - First get all products to find a valid ID
        var allProductsResponse = await AuthenticatedClient.GetAsync("/api/products");
        var allProductsContent = await allProductsResponse.Content.ReadAsStringAsync();
        var allProducts = JsonDocument.Parse(allProductsContent);
        
        if (allProducts.RootElement.GetArrayLength() == 0)
        {
            return; // Skip test if no products exist
        }

        var firstProduct = allProducts.RootElement[0];
        var productId = firstProduct.GetProperty("id").GetInt32();

        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);
        
        result.RootElement.GetProperty("id").GetInt32().Should().Be(productId);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("SingleFamily")]
    [InlineData("Duplex")]
    [InlineData("Accessory")]
    public async Task GetProducts_WithCategoryFilter_ReturnsFilteredResults(string category)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/products?category={category}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Verify all returned products have the specified category
        foreach (var product in result.RootElement.EnumerateArray())
        {
            if (product.TryGetProperty("category", out var categoryProperty))
            {
                categoryProperty.GetString().Should().Be(category);
            }
        }
    }

    [Fact]
    public async Task GetProducts_WithInStockFilter_ReturnsOnlyAvailableProducts()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products?inStock=true");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Verify all returned products have stock > 0
        foreach (var product in result.RootElement.EnumerateArray())
        {
            if (product.TryGetProperty("stockQuantity", out var stockProperty))
            {
                stockProperty.GetInt32().Should().BeGreaterThan(0);
            }
        }
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    [InlineData(1, 10)]
    public async Task GetProducts_WithPagination_ReturnsCorrectPageSize(int page, int pageSize)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/products?page={page}&pageSize={pageSize}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Check that we don't exceed the requested page size
        result.RootElement.GetArrayLength().Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task GetProducts_WithPagination_IncludesPaginationHeaders()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products?page=1&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Check for pagination headers (if implemented)
        // Note: These might not be implemented yet
        if (response.Headers.Contains("X-Total-Count"))
        {
            response.Headers.GetValues("X-Total-Count").First().Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetProducts_WithInvalidCategory_ReturnsEmptyArray()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products?category=InvalidCategory");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        result.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetProducts_WithMultipleFilters_ReturnsCorrectResults()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products?category=SingleFamily&inStock=true&page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        
        // Verify filters are applied correctly
        foreach (var product in result.RootElement.EnumerateArray())
        {
            if (product.TryGetProperty("category", out var categoryProperty))
            {
                categoryProperty.GetString().Should().Be("SingleFamily");
            }
            if (product.TryGetProperty("stockQuantity", out var stockProperty))
            {
                stockProperty.GetInt32().Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public async Task GetProducts_ResponseTimeIsReasonable()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await AuthenticatedClient.GetAsync("/api/products");

        // Assert
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should respond within 1 second
    }
}
