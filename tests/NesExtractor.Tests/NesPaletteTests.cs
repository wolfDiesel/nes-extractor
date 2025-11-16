using Xunit;
using NesExtractor.Core.Models;
using SkiaSharp;

namespace NesExtractor.Tests;

public class NesPaletteTests
{
    [Fact]
    public void GetPalette_GreyscaleIndex_ShouldReturnGreyscale()
    {
        // Act
        var palette = NesPalette.GetPalette(0, transparent: false);

        // Assert
        Assert.NotNull(palette);
        Assert.Equal(NesPalette.TilePaletteSize, palette.Length);
        Assert.Equal(NesPalette.Greyscale, palette);
    }

    [Fact]
    public void GetPalette_GreyscaleWithTransparency_ShouldReturnTransparentGreyscale()
    {
        // Act
        var palette = NesPalette.GetPalette(0, transparent: true);

        // Assert
        Assert.NotNull(palette);
        Assert.Equal(NesPalette.TilePaletteSize, palette.Length);
        Assert.Equal(SKColor.Empty, palette[0]); // First color should be transparent
    }

    [Fact]
    public void GetPalette_ValidIndices_ShouldReturnPalettes()
    {
        // Act & Assert - Test all valid palette indices (1-9)
        for (int i = 1; i <= 9; i++)
        {
            var palette = NesPalette.GetPalette(i);
            Assert.NotNull(palette);
            Assert.Equal(NesPalette.TilePaletteSize, palette.Length);
            
            // All colors should be from standard palette
            foreach (var color in palette)
            {
                if (color != SKColor.Empty)
                {
                    Assert.Contains(color, NesPalette.Standard);
                }
            }
        }
    }

    [Fact]
    public void GetPalette_InvalidIndex_ShouldReturnDefaultPalette()
    {
        // Act
        var palette = NesPalette.GetPalette(999);

        // Assert
        Assert.NotNull(palette);
        Assert.Equal(NesPalette.TilePaletteSize, palette.Length);
    }

    [Fact]
    public void GetPalette_NegativeIndex_ShouldReturnDefaultPalette()
    {
        // Act
        var palette = NesPalette.GetPalette(-1);

        // Assert
        Assert.NotNull(palette);
        Assert.Equal(NesPalette.TilePaletteSize, palette.Length);
    }

    [Fact]
    public void GetPalette_WithTransparency_ShouldSetFirstColorToTransparent()
    {
        // Act
        var palette = NesPalette.GetPalette(1, transparent: true);

        // Assert
        Assert.Equal(SKColor.Empty, palette[0]);
        // Other colors should not be transparent
        for (int i = 1; i < palette.Length; i++)
        {
            Assert.NotEqual(SKColor.Empty, palette[i]);
        }
    }

    [Fact]
    public void GetPalette_WithoutTransparency_ShouldNotHaveTransparentColors()
    {
        // Act
        var palette = NesPalette.GetPalette(1, transparent: false);

        // Assert
        foreach (var color in palette)
        {
            Assert.NotEqual(SKColor.Empty, color);
        }
    }

    [Fact]
    public void GetPalette_DifferentPalettes_ShouldReturnDifferentColors()
    {
        // Act
        var palette1 = NesPalette.GetPalette(1);
        var palette2 = NesPalette.GetPalette(2);

        // Assert
        // At least some colors should be different (not all palettes are identical)
        bool hasDifference = false;
        for (int i = 0; i < palette1.Length; i++)
        {
            if (palette1[i] != palette2[i])
            {
                hasDifference = true;
                break;
            }
        }
        Assert.True(hasDifference);
    }

    [Fact]
    public void StandardPalette_ShouldHave64Colors()
    {
        // Assert
        Assert.Equal(NesPalette.StandardPaletteSize, NesPalette.Standard.Length);
    }

    [Fact]
    public void GreyscalePalette_ShouldHave4Colors()
    {
        // Assert
        Assert.Equal(NesPalette.TilePaletteSize, NesPalette.Greyscale.Length);
    }

    [Fact]
    public void GreyscaleTransparentPalette_ShouldHave4Colors()
    {
        // Assert
        Assert.Equal(NesPalette.TilePaletteSize, NesPalette.GreyscaleTransparent.Length);
        Assert.Equal(SKColor.Empty, NesPalette.GreyscaleTransparent[0]);
    }
}

