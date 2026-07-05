using FluentAssertions;
using TelemetryWerk.Api.Domain.Entities;
using TelemetryWerk.Api.Infrastructure.Repositories;

namespace TelemetryWerk.Api.Tests.Repositories;

public class InMemoryMachineRepositoryTests
{
    private readonly InMemoryMachineRepository _sut;

    public InMemoryMachineRepositoryTests()
    {
        _sut = new InMemoryMachineRepository();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPreSeededData()
    {
        // Arrange
        // (Act)
        var result = await _sut.GetAllAsync(100);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(8); // Pre-seeded with 8 records
    }

    [Fact]
    public async Task GetAllAsync_WithLimit_ShouldRespectLimit()
    {
        // Act
        var result = await _sut.GetAllAsync(3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithAfterId_ShouldReturnOnlyAfterThatId()
    {
        // Act
        var result = await _sut.GetAllAsync(100, "TK-004");

        // Assert
        var list = result.ToList();
        list.Should().HaveCount(4); // TK-005 to TK-008
        list.First().Id.Should().Be("TK-005");
    }

    [Fact]
    public async Task AddAsync_ShouldAddNewMachine()
    {
        // Arrange
        var newMachine = new MachineNode { Id = "TK-009", Status = "Running", CoreTemperature = 40.0 };

        // Act
        var result = await _sut.AddAsync(newMachine);
        
        // Assert
        result.Should().Be(newMachine);
        var all = await _sut.GetAllAsync(100);
        all.Should().Contain(m => m.Id == "TK-009");
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_ShouldUpdateFields()
    {
        // Arrange
        var id = "TK-001";
        
        // Act
        var result = await _sut.UpdateAsync(id, "Warning", 45.0);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Status.Should().Be("Warning");
        result.CoreTemperature.Should().Be(45.0);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _sut.UpdateAsync("Unknown", "Warning", 45.0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var id = "TK-008"; // Make sure to use one that exists
        
        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        var afterDelete = await _sut.GetByIdAsync(id);
        afterDelete.Should().BeNull();
    }
}
