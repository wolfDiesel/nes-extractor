using System;
using Xunit;
using NesExtractor.Core.Models;

namespace NesExtractor.Tests;

public class NesTileTests
{
    [Fact]
    public void Decode_ValidData_ShouldDecodeCorrectly()
    {
        // Arrange - Create a simple test tile (checkerboard pattern)
        // Low bitplane:  0xAA = 10101010
        // High bitplane: 0x55 = 01010101
        // Result: alternating pattern 1, 3, 1, 3...
        var tileData = new byte[16];
        
        // Fill low bitplane with 0xAA (alternating bits)
        for (int i = 0; i < 8; i++)
        {
            tileData[i] = 0xAA; // 10101010
        }
        
        // Fill high bitplane with 0x55 (alternating bits, opposite)
        for (int i = 8; i < 16; i++)
        {
            tileData[i] = 0x55; // 01010101
        }

        // Act
        var tile = NesTile.Decode(tileData, 0);

        // Assert
        Assert.NotNull(tile);
        Assert.Equal(0, tile.Index);
        Assert.Equal(16, tile.RawData.Length);
        
        // Check first row: 0xAA = 10101010 (MSB first), 0x55 = 01010101
        // First pixel (MSB, x=0): low bit from 0xAA[7] = 1, high bit from 0x55[7] = 0
        // Value = (high << 1) | low = (0 << 1) | 1 = 1
        // Second pixel (x=1): low bit from 0xAA[6] = 0, high bit from 0x55[6] = 1
        // Value = (1 << 1) | 0 = 2
        Assert.Equal(1, tile.Pixels[0, 0]); // First pixel
        Assert.Equal(2, tile.Pixels[0, 1]); // Second pixel
    }

    [Fact]
    public void Decode_EmptyTile_ShouldDecodeToZeros()
    {
        // Arrange - All zeros
        var tileData = new byte[16];

        // Act
        var tile = NesTile.Decode(tileData, 5);

        // Assert
        Assert.NotNull(tile);
        Assert.Equal(5, tile.Index);
        
        for (int y = 0; y < NesTile.TileSize; y++)
        {
            for (int x = 0; x < NesTile.TileSize; x++)
            {
                Assert.Equal(0, tile.Pixels[y, x]);
            }
        }
    }

    [Fact]
    public void Decode_FullTile_ShouldDecodeToThrees()
    {
        // Arrange - All pixels = 3 (both bitplanes = 0xFF)
        var tileData = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            tileData[i] = 0xFF; // All bits set
        }

        // Act
        var tile = NesTile.Decode(tileData, 0);

        // Assert
        for (int y = 0; y < NesTile.TileSize; y++)
        {
            for (int x = 0; x < NesTile.TileSize; x++)
            {
                Assert.Equal(3, tile.Pixels[y, x]);
            }
        }
    }

    [Fact]
    public void Decode_NullData_ShouldThrowArgumentException()
    {
        // Act & Assert - Method checks length first, so throws ArgumentException
        Assert.Throws<ArgumentException>(() =>
        {
            NesTile.Decode(null!, 0);
        });
    }

    [Fact]
    public void Decode_InsufficientData_ShouldThrowArgumentException()
    {
        // Arrange
        var insufficientData = new byte[15]; // Need 16 bytes

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            NesTile.Decode(insufficientData, 0);
        });
        
        Assert.Contains("16 bytes", ex.Message);
    }

    [Fact]
    public void Decode_ExcessData_ShouldUseFirst16Bytes()
    {
        // Arrange - More than 16 bytes
        var excessData = new byte[32];
        excessData[0] = 0xFF; // First byte
        excessData[8] = 0xFF; // First byte of high bitplane

        // Act
        var tile = NesTile.Decode(excessData, 0);

        // Assert
        Assert.Equal(16, tile.RawData.Length);
        Assert.Equal(0xFF, tile.RawData[0]);
    }

    [Fact]
    public void GetUsedColors_EmptyTile_ShouldReturnOnlyZero()
    {
        // Arrange
        var tileData = new byte[16];
        var tile = NesTile.Decode(tileData, 0);

        // Act
        var colors = tile.GetUsedColors();

        // Assert
        Assert.Single(colors);
        Assert.Equal(0, colors[0]);
    }

    [Fact]
    public void GetUsedColors_MultiColorTile_ShouldReturnAllColors()
    {
        // Arrange - Create tile with all 4 color values
        var tileData = new byte[16];
        // Set first row to have different patterns
        tileData[0] = 0x00; // First 4 pixels = 0
        tileData[8] = 0x00;
        
        tileData[1] = 0x0F; // Next row with pattern
        tileData[9] = 0x00; // Will create mix of values

        var tile = NesTile.Decode(tileData, 0);
        
        // Manually set some pixels to ensure we have all values
        tile.Pixels[0, 0] = 0;
        tile.Pixels[0, 1] = 1;
        tile.Pixels[0, 2] = 2;
        tile.Pixels[0, 3] = 3;

        // Act
        var colors = tile.GetUsedColors();

        // Assert
        Assert.Contains<byte>(0, colors);
        Assert.Contains<byte>(1, colors);
        Assert.Contains<byte>(2, colors);
        Assert.Contains<byte>(3, colors);
    }

    [Fact]
    public void GetUsedColors_ShouldReturnUniqueColors()
    {
        // Arrange
        var tileData = new byte[16];
        tileData[0] = 0xAA;
        tileData[8] = 0x55;
        var tile = NesTile.Decode(tileData, 0);

        // Act
        var colors = tile.GetUsedColors();

        // Assert - Should not have duplicates
        var uniqueColors = new System.Collections.Generic.HashSet<byte>(colors);
        Assert.Equal(colors.Length, uniqueColors.Count);
    }

    [Fact]
    public void IsEmpty_EmptyTile_ShouldReturnTrue()
    {
        // Arrange
        var tileData = new byte[16];
        var tile = NesTile.Decode(tileData, 0);

        // Act
        var isEmpty = tile.IsEmpty();

        // Assert
        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_NonEmptyTile_ShouldReturnFalse()
    {
        // Arrange
        var tileData = new byte[16];
        tileData[0] = 0x01; // Set at least one bit
        var tile = NesTile.Decode(tileData, 0);

        // Act
        var isEmpty = tile.IsEmpty();

        // Assert
        Assert.False(isEmpty);
    }

    [Fact]
    public void IsEmpty_TileWithOnlyZeros_ShouldReturnTrue()
    {
        // Arrange
        var tile = new NesTile
        {
            Index = 0,
            Pixels = new byte[NesTile.TileSize, NesTile.TileSize]
        };

        // Act
        var isEmpty = tile.IsEmpty();

        // Assert
        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_TileWithOneNonZeroPixel_ShouldReturnFalse()
    {
        // Arrange
        var tile = new NesTile
        {
            Index = 0,
            Pixels = new byte[NesTile.TileSize, NesTile.TileSize]
        };
        tile.Pixels[4, 4] = 1; // Set one pixel

        // Act
        var isEmpty = tile.IsEmpty();

        // Assert
        Assert.False(isEmpty);
    }
}

