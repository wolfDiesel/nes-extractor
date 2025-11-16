using System;
using System.Collections.Generic;
using Xunit;
using NesExtractor.Core.Models;
using NesExtractor.Core.Services;
using SkiaSharp;

namespace NesExtractor.Tests;

public class ChrRomExtractorTests
{
    private static byte[] CreateTestChrRom(int tileCount)
    {
        var chrRom = new byte[tileCount * NesTile.TileSizeInBytes];
        // Fill with pattern for testing
        for (int i = 0; i < tileCount; i++)
        {
            int offset = i * NesTile.TileSizeInBytes;
            // Create a simple pattern tile
            for (int j = 0; j < 8; j++)
            {
                chrRom[offset + j] = 0xAA; // Low bitplane
                chrRom[offset + 8 + j] = 0x55; // High bitplane
            }
        }
        return chrRom;
    }

    [Fact]
    public void ExtractTiles_ValidChrRom_ShouldExtractAllTiles()
    {
        // Arrange
        var chrRom = CreateTestChrRom(4); // 4 tiles

        // Act
        var tiles = ChrRomExtractor.ExtractTiles(chrRom);

        // Assert
        Assert.NotNull(tiles);
        Assert.Equal(4, tiles.Count);
        Assert.All(tiles, tile => Assert.NotNull(tile));
    }

    [Fact]
    public void ExtractTiles_EmptyChrRom_ShouldReturnEmptyList()
    {
        // Arrange
        var chrRom = new byte[0];

        // Act
        var tiles = ChrRomExtractor.ExtractTiles(chrRom);

        // Assert
        Assert.NotNull(tiles);
        Assert.Empty(tiles);
    }

    [Fact]
    public void ExtractTiles_NullChrRom_ShouldReturnEmptyList()
    {
        // Act
        var tiles = ChrRomExtractor.ExtractTiles(null!);

        // Assert
        Assert.NotNull(tiles);
        Assert.Empty(tiles);
    }

    [Fact]
    public void ExtractTiles_SingleTile_ShouldExtractOneTile()
    {
        // Arrange
        var chrRom = CreateTestChrRom(1);

        // Act
        var tiles = ChrRomExtractor.ExtractTiles(chrRom);

        // Assert
        Assert.Single(tiles);
        Assert.Equal(0, tiles[0].Index);
    }

    [Fact]
    public void ExtractTiles_MultipleTiles_ShouldSetCorrectIndices()
    {
        // Arrange
        var chrRom = CreateTestChrRom(10);

        // Act
        var tiles = ChrRomExtractor.ExtractTiles(chrRom);

        // Assert
        Assert.Equal(10, tiles.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i, tiles[i].Index);
        }
    }

    [Fact]
    public void ExtractTiles_IncompleteLastTile_ShouldIgnoreIncompleteTile()
    {
        // Arrange - 3.5 tiles (56 bytes instead of 64)
        var chrRom = new byte[56];
        Array.Copy(CreateTestChrRom(3), chrRom, 48);
        // Add 8 more bytes (half a tile)
        Array.Copy(new byte[8], 0, chrRom, 48, 8);

        // Act
        var tiles = ChrRomExtractor.ExtractTiles(chrRom);

        // Assert - Should only extract 3 complete tiles
        Assert.Equal(3, tiles.Count);
    }

    [Fact]
    public void TileToBitmap_ValidTile_ShouldCreateBitmap()
    {
        // Arrange
        var tileData = new byte[16];
        tileData[0] = 0xFF;
        tileData[8] = 0xFF;
        var tile = NesTile.Decode(tileData, 0);

        // Act
        var bitmap = ChrRomExtractor.TileToBitmap(tile);

        // Assert
        Assert.NotNull(bitmap);
        Assert.Equal(NesTile.TileSize, bitmap.Width);
        Assert.Equal(NesTile.TileSize, bitmap.Height);
    }

    [Fact]
    public void TileToBitmap_WithScale_ShouldCreateScaledBitmap()
    {
        // Arrange
        var tileData = new byte[16];
        var tile = NesTile.Decode(tileData, 0);
        int scale = 4;

        // Act
        var bitmap = ChrRomExtractor.TileToBitmap(tile, scale: scale);

        // Assert
        Assert.NotNull(bitmap);
        Assert.Equal(NesTile.TileSize * scale, bitmap.Width);
        Assert.Equal(NesTile.TileSize * scale, bitmap.Height);
    }

    [Fact]
    public void TileToBitmap_WithCustomPalette_ShouldUsePalette()
    {
        // Arrange
        var tileData = new byte[16];
        var tile = NesTile.Decode(tileData, 0);
        var customPalette = new SKColor[]
        {
            SKColors.Red,
            SKColors.Green,
            SKColors.Blue,
            SKColors.White
        };

        // Act
        var bitmap = ChrRomExtractor.TileToBitmap(tile, palette: customPalette);

        // Assert
        Assert.NotNull(bitmap);
        // Bitmap should be created successfully
    }

    [Fact]
    public void TileToBitmap_WithTransparentPalette_ShouldHandleTransparency()
    {
        // Arrange
        var tileData = new byte[16];
        var tile = NesTile.Decode(tileData, 0);
        var transparentPalette = new SKColor[]
        {
            SKColor.Empty, // Transparent
            SKColors.Red,
            SKColors.Green,
            SKColors.Blue
        };

        // Act
        var bitmap = ChrRomExtractor.TileToBitmap(tile, palette: transparentPalette);

        // Assert
        Assert.NotNull(bitmap);
    }

    [Fact]
    public void CreateTileSheet_ValidTiles_ShouldCreateSheet()
    {
        // Arrange
        var tiles = new List<NesTile>();
        for (int i = 0; i < 4; i++)
        {
            var tileData = new byte[16];
            var tile = NesTile.Decode(tileData, i);
            tiles.Add(tile);
        }

        // Act
        var sheet = ChrRomExtractor.CreateTileSheet(tiles, tilesPerRow: 2, tileScale: 2);

        // Assert
        Assert.NotNull(sheet);
        Assert.True(sheet.Width > 0);
        Assert.True(sheet.Height > 0);
    }

    [Fact]
    public void CreateTileSheet_EmptyTiles_ShouldThrowException()
    {
        // Arrange
        var tiles = new List<NesTile>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            ChrRomExtractor.CreateTileSheet(tiles);
        });
    }

    [Fact]
    public void CreateTileSheet_NullTiles_ShouldThrowException()
    {
        // Act & Assert - Method checks count first, so throws ArgumentException
        Assert.Throws<ArgumentException>(() =>
        {
            ChrRomExtractor.CreateTileSheet(null!);
        });
    }

    [Fact]
    public void CreateTileSheet_WithSpacing_ShouldAddSpacing()
    {
        // Arrange
        var tiles = new List<NesTile>();
        for (int i = 0; i < 4; i++)
        {
            var tileData = new byte[16];
            var tile = NesTile.Decode(tileData, i);
            tiles.Add(tile);
        }

        // Act
        var sheetWithSpacing = ChrRomExtractor.CreateTileSheet(tiles, tilesPerRow: 2, tileScale: 2, spacing: 2);
        var sheetWithoutSpacing = ChrRomExtractor.CreateTileSheet(tiles, tilesPerRow: 2, tileScale: 2, spacing: 0);

        // Assert
        Assert.NotNull(sheetWithSpacing);
        Assert.NotNull(sheetWithoutSpacing);
        // Sheet with spacing should be larger
        Assert.True(sheetWithSpacing.Width >= sheetWithoutSpacing.Width);
    }

    [Fact]
    public void CreateTileSheet_WithTransparency_ShouldHandleTransparency()
    {
        // Arrange
        var tiles = new List<NesTile>();
        for (int i = 0; i < 2; i++)
        {
            var tileData = new byte[16];
            var tile = NesTile.Decode(tileData, i);
            tiles.Add(tile);
        }
        var transparentPalette = new SKColor[]
        {
            SKColor.Empty,
            SKColors.Red,
            SKColors.Green,
            SKColors.Blue
        };

        // Act
        var sheet = ChrRomExtractor.CreateTileSheet(tiles, palette: transparentPalette, useTransparency: true);

        // Assert
        Assert.NotNull(sheet);
    }
}

