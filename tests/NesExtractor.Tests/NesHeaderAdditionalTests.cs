using Xunit;
using NesExtractor.Core.Models;

namespace NesExtractor.Tests;

public class NesHeaderAdditionalTests
{
    [Fact]
    public void HasTrainer_WithTrainerFlag_ShouldReturnTrue()
    {
        // Arrange
        var header = new NesHeader { Flags6 = 0x04 }; // Bit 2 = trainer

        // Act & Assert
        Assert.True(header.HasTrainer);
    }

    [Fact]
    public void HasTrainer_WithoutTrainerFlag_ShouldReturnFalse()
    {
        // Arrange
        var header = new NesHeader { Flags6 = 0x00 };

        // Act & Assert
        Assert.False(header.HasTrainer);
    }

    [Fact]
    public void PrgRomSizeInBytes_ShouldCalculateCorrectly()
    {
        // Arrange
        var header = new NesHeader { PrgRomSize = 2 };

        // Act
        var size = header.PrgRomSizeInBytes;

        // Assert
        Assert.Equal(32768, size); // 2 * 16 KB
    }

    [Fact]
    public void ChrRomSizeInBytes_ShouldCalculateCorrectly()
    {
        // Arrange
        var header = new NesHeader { ChrRomSize = 3 };

        // Act
        var size = header.ChrRomSizeInBytes;

        // Assert
        Assert.Equal(24576, size); // 3 * 8 KB
    }

    [Fact]
    public void TrainerSizeInBytes_WithTrainer_ShouldReturn512()
    {
        // Arrange
        var header = new NesHeader { Flags6 = 0x04 }; // Has trainer

        // Act
        var size = header.TrainerSizeInBytes;

        // Assert
        Assert.Equal(512, size);
    }

    [Fact]
    public void TrainerSizeInBytes_WithoutTrainer_ShouldReturn0()
    {
        // Arrange
        var header = new NesHeader { Flags6 = 0x00 }; // No trainer

        // Act
        var size = header.TrainerSizeInBytes;

        // Assert
        Assert.Equal(0, size);
    }

    [Fact]
    public void IsValid_ValidMagic_ShouldReturnTrue()
    {
        // Arrange
        var header = new NesHeader
        {
            Magic = new byte[] { (byte)'N', (byte)'E', (byte)'S', 0x1A }
        };

        // Act
        var isValid = header.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_InvalidMagic_ShouldReturnFalse()
    {
        // Arrange
        var header = new NesHeader
        {
            Magic = new byte[] { (byte)'X', (byte)'X', (byte)'X', 0x1A }
        };

        // Act
        var isValid = header.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WrongMagicLength_ShouldReturnFalse()
    {
        // Arrange
        var header = new NesHeader
        {
            Magic = new byte[] { (byte)'N', (byte)'E' } // Only 2 bytes
        };

        // Act
        var isValid = header.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetMapperName_AllKnownMappers_ShouldReturnCorrectNames()
    {
        // Test all known mappers
        var mapperTests = new[]
        {
            (0, "NROM"),
            (1, "MMC1"),
            (2, "UxROM"),
            (3, "CNROM"),
            (4, "MMC3"),
            (5, "MMC5"),
            (7, "AxROM"),
            (9, "MMC2"),
            (10, "MMC4"),
            (11, "Color Dreams")
        };

        foreach (var (mapperNum, expectedName) in mapperTests)
        {
            // Arrange
            var header = new NesHeader
            {
                Flags6 = (byte)((mapperNum & 0x0F) << 4),
                Flags7 = (byte)(mapperNum & 0xF0)
            };

            // Act
            var name = header.GetMapperName();

            // Assert
            Assert.Equal(expectedName, name);
        }
    }

    [Fact]
    public void GetMapperName_UnknownMapper_ShouldReturnUnknown()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags6 = 0xF0, // Mapper 15 (unknown)
            Flags7 = 0x00
        };

        // Act
        var name = header.GetMapperName();

        // Assert
        Assert.Contains("Unknown", name);
        Assert.Contains("15", name);
    }
}

