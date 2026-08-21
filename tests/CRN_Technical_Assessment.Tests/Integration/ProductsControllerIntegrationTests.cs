using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CRN_Technical_Assessment.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CRN_Technical_Assessment.Tests.Integration;

/// <summary>
/// Integration tests for ProductsController using WebApplicationFactory.
/// Tests run against the real application pipeline with the test database.
/// </summary>
public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── Helper: Login and get access token ───────────────────────────────────

    private async Task<string> GetAdminTokenAsync()
    {
        var loginDto = new LoginRequestDto { Username = "admin", Password = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, JsonOptions);
        return wrapper!.Data!.AccessToken;
    }

    private async Task<string> GetUserTokenAsync()
    {
        var loginDto = new LoginRequestDto { Username = "user1", Password = "User@123" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, JsonOptions);
        return wrapper!.Data!.AccessToken;
    }

    // ── Authentication Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var loginDto = new LoginRequestDto { Username = "admin", Password = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, JsonOptions);

        Assert.True(result!.Success);
        Assert.NotEmpty(result.Data!.AccessToken);
        Assert.NotEmpty(result.Data.RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var loginDto = new LoginRequestDto { Username = "admin", Password = "WrongPassword" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── GET /api/v1/products ──────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/products");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithValidToken_Returns200()
    {
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetProducts_ReturnsPaginatedResponse()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/products?pageNumber=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResponse<ProductListDto>>(content, JsonOptions);

        Assert.True(result!.Success);
        Assert.NotNull(result.Pagination);
        Assert.Equal(1, result.Pagination.PageNumber);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // ── POST /api/v1/products ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_AsAdmin_Returns201()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new ProductCreateDto
        {
            ProductName = "Integration Test Product",
            CreatedBy = "integration_test",
            Items = new List<ItemDto> { new() { Quantity = 5 } }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ProductResponseDto>>(content, JsonOptions);

        Assert.True(result!.Success);
        Assert.Equal("Integration Test Product", result.Data!.ProductName);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CreateProduct_AsUser_Returns403()
    {
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new ProductCreateDto
        {
            ProductName = "Should Fail",
            CreatedBy = "user",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", dto);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CreateProduct_InvalidDto_Returns400()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new ProductCreateDto
        {
            ProductName = "", // invalid — empty
            CreatedBy = "admin"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // ── GET /api/v1/products/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task GetProductById_NotFound_Returns404()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/products/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    // ── Health Check ──────────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheck_Returns200()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
