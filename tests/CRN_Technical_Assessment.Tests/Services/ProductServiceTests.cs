using CRN_Technical_Assessment.Application.DTOs;
using CRN_Technical_Assessment.Application.Interfaces;
using CRN_Technical_Assessment.Application.Mapping;
using CRN_Technical_Assessment.Application.Services;
using CRN_Technical_Assessment.Domain.Entities;
using CRN_Technical_Assessment.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRN_Technical_Assessment.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new ProductService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    // ── GetAllPagedAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllPagedAsync_ReturnsPagedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, ProductName = "Product A", CreatedBy = "admin", CreatedOn = DateTime.UtcNow, Items = new List<Item>() },
            new() { Id = 2, ProductName = "Product B", CreatedBy = "admin", CreatedOn = DateTime.UtcNow, Items = new List<Item>() }
        };

        _productRepoMock
            .Setup(r => r.GetPagedAsync(1, 10, default))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _sut.GetAllPagedAsync(1, 10);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Pagination.TotalRecords);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetAllPagedAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.GetPagedAsync(1, 10, default))
            .ReturnsAsync((new List<Product>(), 0));

        // Act
        var result = await _sut.GetAllPagedAsync(1, 10);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalRecords);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Test Product",
            CreatedBy = "admin",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item> { new() { Id = 1, Quantity = 10 } }
        };

        _productRepoMock
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.ProductName);
        Assert.Single(result.Items);
        Assert.Equal(10, result.Items[0].Quantity);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProduct_ThrowsNotFoundException()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.GetByIdAsync(999, default))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(999));
    }

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedProduct()
    {
        // Arrange
        var dto = new ProductCreateDto
        {
            ProductName = "New Widget",
            CreatedBy = "admin",
            Items = new List<ItemDto> { new() { Quantity = 5 } }
        };

        _productRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.Equal("New Widget", result.ProductName);
        Assert.Equal("admin", result.CreatedBy);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesAndReturnsDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Old Name",
            CreatedBy = "admin",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item>()
        };

        var dto = new ProductUpdateDto
        {
            ProductName = "Updated Name",
            ModifiedBy = "admin",
            Items = new List<ItemDto> { new() { Quantity = 99 } }
        };

        _productRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.Update(It.IsAny<Product>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        Assert.Equal("Updated Name", result.ProductName);
        Assert.Equal("admin", result.ModifiedBy);
        Assert.Single(result.Items);
        _productRepoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ThrowsNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Product?)null);

        var dto = new ProductUpdateDto { ProductName = "Name", ModifiedBy = "admin" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(999, dto));
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingProduct_DeletesSuccessfully()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "To Delete",
            CreatedBy = "admin",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item>()
        };

        _productRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.Delete(It.IsAny<Product>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _productRepoMock.Verify(r => r.Delete(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProduct_ThrowsNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(999));
    }
}
