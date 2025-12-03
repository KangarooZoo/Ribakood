using BarcodeApp.Services;
using FluentAssertions;
using Xunit;

namespace BarcodeApp.Tests.Services;

public class MachineIdProviderTests
{
    [Fact]
    public void GetMachineId_ReturnsNonEmptyString()
    {
        // Arrange
        var provider = new MachineIdProvider();

        // Act
        var machineId = provider.GetMachineId();

        // Assert
        machineId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetMachineId_ReturnsConsistentValue()
    {
        // Arrange
        var provider = new MachineIdProvider();

        // Act
        var machineId1 = provider.GetMachineId();
        var machineId2 = provider.GetMachineId();

        // Assert
        machineId1.Should().Be(machineId2);
    }

    [Fact]
    public void GetMachineId_ReturnsLowercaseHexString()
    {
        // Arrange
        var provider = new MachineIdProvider();

        // Act
        var machineId = provider.GetMachineId();

        // Assert
        machineId.Should().MatchRegex("^[0-9a-f]{64}$"); // SHA256 produces 64 hex characters
    }

    [Fact]
    public void GetMachineId_NewInstance_ReturnsSameValue()
    {
        // Arrange
        var provider1 = new MachineIdProvider();
        var provider2 = new MachineIdProvider();

        // Act
        var machineId1 = provider1.GetMachineId();
        var machineId2 = provider2.GetMachineId();

        // Assert
        machineId1.Should().Be(machineId2);
    }
}

