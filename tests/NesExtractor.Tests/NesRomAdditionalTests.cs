using System;
using System.IO;
using Xunit;
using NesExtractor.Core.Models;

namespace NesExtractor.Tests;

public class NesRomAdditionalTests
{
    [Fact]
    public void FileName_WithPath_ShouldReturnFileName()
    {
        // Arrange
        var rom = new NesRom
        {
            FilePath = "/path/to/game.nes"
        };

        // Act
        var fileName = rom.FileName;

        // Assert
        Assert.Equal("game.nes", fileName);
    }

    [Fact]
    public void FileName_WithoutPath_ShouldReturnUnknown()
    {
        // Arrange
        var rom = new NesRom
        {
            FilePath = null
        };

        // Act
        var fileName = rom.FileName;

        // Assert
        Assert.Equal("Unknown", fileName);
    }

    [Fact]
    public void FileName_WithWindowsPath_ShouldReturnFileName()
    {
        // Arrange - Use Path.Combine to get correct path separator for current OS
        var rom = new NesRom
        {
            FilePath = Path.Combine("C:", "Games", "SuperMario.nes")
        };

        // Act
        var fileName = rom.FileName;

        // Assert - Path.GetFileName works correctly on all platforms
        Assert.Equal("SuperMario.nes", fileName);
    }

    [Fact]
    public void IsValid_WithValidHeader_ShouldReturnTrue()
    {
        // Arrange
        var rom = new NesRom
        {
            Header = new NesHeader
            {
                Magic = new byte[] { (byte)'N', (byte)'E', (byte)'S', 0x1A }
            }
        };

        // Act
        var isValid = rom.IsValid;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithInvalidHeader_ShouldReturnFalse()
    {
        // Arrange
        var rom = new NesRom
        {
            Header = new NesHeader
            {
                Magic = new byte[] { (byte)'X', (byte)'X', (byte)'X', 0x1A }
            }
        };

        // Act
        var isValid = rom.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void PrgRomBankCount_ShouldReturnHeaderValue()
    {
        // Arrange
        var rom = new NesRom
        {
            Header = new NesHeader { PrgRomSize = 4 }
        };

        // Act
        var bankCount = rom.PrgRomBankCount;

        // Assert
        Assert.Equal(4, bankCount);
    }

    [Fact]
    public void ChrRomBankCount_ShouldReturnHeaderValue()
    {
        // Arrange
        var rom = new NesRom
        {
            Header = new NesHeader { ChrRomSize = 2 }
        };

        // Act
        var bankCount = rom.ChrRomBankCount;

        // Assert
        Assert.Equal(2, bankCount);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var rom = new NesRom
        {
            FilePath = "test.nes",
            Header = new NesHeader
            {
                PrgRomSize = 2,
                ChrRomSize = 1,
                Flags6 = 0x00,
                Flags7 = 0x00
            }
        };

        // Act
        var result = rom.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("test.nes", result);
        Assert.Contains("Mapper", result);
        Assert.Contains("PRG", result);
        Assert.Contains("CHR", result);
        Assert.Contains("Mirroring", result);
    }

    [Fact]
    public void TotalFileSize_WithoutTrainer_ShouldCalculateCorrectly()
    {
        // Arrange
        var rom = new NesRom
        {
            Header = new NesHeader
            {
                PrgRomSize = 1,
                ChrRomSize = 1,
                Flags6 = 0x00 // No trainer
            },
            PrgRom = new byte[16384],
            ChrRom = new byte[8192]
        };

        // Act
        var size = rom.TotalFileSize;

        // Assert
        // 16 (header) + 0 (trainer) + 16384 (PRG) + 8192 (CHR) = 24592
        Assert.Equal(24592, size);
    }
}

