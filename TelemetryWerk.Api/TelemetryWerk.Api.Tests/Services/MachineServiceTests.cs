using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Exceptions;
using TelemetryWerk.Api.Application.Services;
using TelemetryWerk.Api.Domain.Entities;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Tests.Services;

public class MachineServiceTests
{
    private readonly IMachineRepository _machineRepository;
    private readonly ChannelWriter<MachineStateUpdateMessage> _channelWriter;
    private readonly ILogger<MachineService> _logger;
    private readonly MachineService _sut;

    public MachineServiceTests()
    {
        _machineRepository = Substitute.For<IMachineRepository>();
        _channelWriter = Substitute.For<ChannelWriter<MachineStateUpdateMessage>>();
        _logger = Substitute.For<ILogger<MachineService>>();
        
        _sut = new MachineService(_machineRepository, _channelWriter, _logger);
    }

    [Fact]
    public async Task GetNodesAsync_ShouldReturnPagedCollection_WithCorrectPagination()
    {
        // Arrange
        var mockData = new List<MachineNode>
        {
            new() { Id = "Node1", Status = "Running", CoreTemperature = 50 },
            new() { Id = "Node2", Status = "Stopped", CoreTemperature = 30 }
        };
        _machineRepository.GetAllAsync(3, null).Returns(mockData);

        // Act
        var result = await _sut.GetNodesAsync(2, null);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.LastId.Should().Be("Node2");
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetNodesAsync_WhenHasMoreRecords_ShouldSetHasMoreTrue()
    {
        // Arrange
        var mockData = new List<MachineNode>
        {
            new() { Id = "Node1" },
            new() { Id = "Node2" },
            new() { Id = "Node3" }
        };
        _machineRepository.GetAllAsync(3, null).Returns(mockData);

        // Act
        var result = await _sut.GetNodesAsync(2, null);

        // Assert
        result.HasMore.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.LastId.Should().Be("Node2");
    }

    [Fact]
    public async Task GetNodesAsync_WhenNoMoreRecords_ShouldSetHasMoreFalse()
    {
        // Arrange
        var mockData = new List<MachineNode>
        {
            new() { Id = "Node1" },
            new() { Id = "Node2" }
        };
        _machineRepository.GetAllAsync(3, null).Returns(mockData);

        // Act
        var result = await _sut.GetNodesAsync(2, null);

        // Assert
        result.HasMore.Should().BeFalse();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddNodeAsync_ShouldAddToRepository_AndPublishToChannel()
    {
        // Arrange
        var request = new MachineNodeDto { Id = "NodeNew", Status = "Running", CoreTemperature = 45 };
        _machineRepository.GetByIdAsync(request.Id).ReturnsNull();

        // Act
        var result = await _sut.AddNodeAsync(request);

        // Assert
        await _machineRepository.Received(1).AddAsync(Arg.Is<MachineNode>(m => m.Id == request.Id && m.Status == "Running"));
        await _channelWriter.Received(1).WriteAsync(Arg.Is<MachineStateUpdateMessage>(m => m.Action == "AddOrUpdate" && m.Machine.Id == request.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddNodeAsync_ShouldReturnCorrectDto()
    {
        // Arrange
        var request = new MachineNodeDto { Id = "NodeNew", Status = "Running", CoreTemperature = 45 };
        _machineRepository.GetByIdAsync(request.Id).ReturnsNull();

        // Act
        var result = await _sut.AddNodeAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("NodeNew");
        result.Status.Should().Be("Running");
        result.CoreTemperature.Should().Be(45);
    }

    [Fact]
    public async Task UpdateNodeAsync_WhenNodeExists_ShouldUpdateAndPublish()
    {
        // Arrange
        var id = "NodeUpdate";
        var request = new UpdateNodeRequestDto { Status = "Stopped", CoreTemperature = 25 };
        var updatedNode = new MachineNode { Id = id, Status = "Stopped", CoreTemperature = 25 };
        
        _machineRepository.UpdateAsync(id, request.Status, request.CoreTemperature).Returns(updatedNode);

        // Act
        var result = await _sut.UpdateNodeAsync(id, request);

        // Assert
        result.Id.Should().Be(id);
        result.Status.Should().Be("Stopped");
        await _channelWriter.Received(1).WriteAsync(Arg.Is<MachineStateUpdateMessage>(m => m.Action == "AddOrUpdate" && m.Machine.Id == id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateNodeAsync_WhenNodeNotExists_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = "NodeNotFound";
        var request = new UpdateNodeRequestDto { Status = "Stopped" };
        _machineRepository.UpdateAsync(id, request.Status, request.CoreTemperature).ReturnsNull();

        // Act
        var act = async () => await _sut.UpdateNodeAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Machine with ID {id} not found.");
    }

    [Fact]
    public async Task AddNodeAsync_WhenNodeAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var request = new MachineNodeDto { Id = "NodeExisting" };
        _machineRepository.GetByIdAsync(request.Id).Returns(new MachineNode { Id = request.Id });

        // Act
        var act = async () => await _sut.AddNodeAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage($"Machine with ID {request.Id} already exists.");
    }

    [Fact]
    public async Task DeleteNodeAsync_WhenNodeIsProtected_ShouldThrowConflictException()
    {
        // Arrange
        var protectedId = "TK-001";

        // Act
        var act = async () => await _sut.DeleteNodeAsync(protectedId);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage($"Machine with ID {protectedId} is protected and cannot be deleted.");
        await _machineRepository.DidNotReceive().DeleteAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task DeleteNodeAsync_WhenNodeNotExists_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = "NodeNotFound";
        _machineRepository.DeleteAsync(id).Returns(false);

        // Act
        var act = async () => await _sut.DeleteNodeAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Machine with ID {id} not found.");
    }

    [Fact]
    public async Task DeleteNodeAsync_WhenValid_ShouldDeleteAndPublishEvent()
    {
        // Arrange
        var id = "NodeToDelete";
        _machineRepository.DeleteAsync(id).Returns(true);

        // Act
        await _sut.DeleteNodeAsync(id);

        // Assert
        await _channelWriter.Received(1).WriteAsync(Arg.Is<MachineStateUpdateMessage>(m => m.Action == "Delete" && m.Machine.Id == id), Arg.Any<CancellationToken>());
    }
}
