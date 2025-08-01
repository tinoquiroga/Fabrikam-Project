using FabrikamApi.Services.Authentication;
using FabrikamApi.Models.Authentication;
using System.Security.Claims;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FabrikamTests.Unit.Services
{
    /// <summary>
    /// Unit tests for NullJwtService - ensures it properly throws exceptions for Disabled auth mode
    /// </summary>
    public class NullJwtServiceTests
    {
        private readonly NullJwtService _nullJwtService;

        public NullJwtServiceTests()
        {
            _nullJwtService = new NullJwtService();
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user = new FabrikamUser { Id = "test-id", Email = "test@example.com" };
            var roles = new List<string> { "User" };
            var claims = new List<Claim> { new Claim("test", "value") };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _nullJwtService.GenerateAccessTokenAsync(user, roles, claims));
            
            exception.Message.Should().Contain("JWT tokens are not available in Disabled authentication mode");
        }

        [Fact]
        public void GenerateRefreshToken_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => _nullJwtService.GenerateRefreshToken());
            
            exception.Message.Should().Contain("JWT tokens are not available in Disabled authentication mode");
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var token = "test-token";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _nullJwtService.ValidateTokenAsync(token));
            
            exception.Message.Should().Contain("JWT tokens are not available in Disabled authentication mode");
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var token = "expired-token";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => _nullJwtService.GetPrincipalFromExpiredToken(token));
            
            exception.Message.Should().Contain("JWT tokens are not available in Disabled authentication mode");
        }

        [Fact]
        public void GetTokenExpiration_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => _nullJwtService.GetTokenExpiration());
            
            exception.Message.Should().Contain("JWT tokens are not available in Disabled authentication mode");
        }

        [Fact]
        public void NullJwtService_ShouldImplementIJwtServiceInterface()
        {
            // Act & Assert
            _nullJwtService.Should().BeAssignableTo<IJwtService>();
        }
    }
}
