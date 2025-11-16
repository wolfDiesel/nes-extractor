using System;
using System.Collections.Generic;
using NesExtractor.Core.Models;
using SkiaSharp;

namespace NesExtractor.Core.Services;

/// <summary>
/// Сервис для извлечения и обработки графики из CHR ROM
/// </summary>
public class ChrRomExtractor
{
    private const int DefaultTileScale = 2;
    private const int DefaultTilesPerRow = 16;
    private const int DefaultSpacing = 1;
    private const int DefaultIndividualTileScale = 4;
    private const int PngQuality = 100;
    
    private const byte DarkBackgroundR = 32;
    private const byte DarkBackgroundG = 32;
    private const byte DarkBackgroundB = 32;
    
    private const byte BorderColorR = 64;
    private const byte BorderColorG = 64;
    private const byte BorderColorB = 64;
    
    private const byte CheckerboardLightR = 128;
    private const byte CheckerboardLightG = 128;
    private const byte CheckerboardLightB = 128;
    
    private const byte CheckerboardDarkR = 96;
    private const byte CheckerboardDarkG = 96;
    private const byte CheckerboardDarkB = 96;
    
    private const int MinCheckerboardSize = 2;
    private const float BorderStrokeWidth = 1.0f;
    private const float BorderOffset = 0.5f;
    
    /// <summary>
    /// Палитра по умолчанию (Greyscale, честный способ)
    /// </summary>
    private static readonly SKColor[] DefaultPalette = NesPalette.Greyscale;

    /// <summary>
    /// Извлечение всех тайлов из CHR ROM
    /// </summary>
    /// <param name="chrRom">Массив байтов CHR ROM</param>
    /// <returns>Список декодированных тайлов</returns>
    public static List<NesTile> ExtractTiles(byte[] chrRom)
    {
        if (chrRom == null || chrRom.Length == 0)
            return new List<NesTile>();

        var tiles = new List<NesTile>();
        int tileCount = chrRom.Length / NesTile.TileSizeInBytes;

        for (int i = 0; i < tileCount; i++)
        {
            int offset = i * NesTile.TileSizeInBytes;
            var tileData = new byte[NesTile.TileSizeInBytes];
            Array.Copy(chrRom, offset, tileData, 0, NesTile.TileSizeInBytes);

            var tile = NesTile.Decode(tileData, i);
            tiles.Add(tile);
        }

        return tiles;
    }

    /// <summary>
    /// Конвертация тайла в SKBitmap
    /// </summary>
    /// <param name="tile">Тайл для конвертации</param>
    /// <param name="palette">Палитра цветов (если null, используется стандартная)</param>
    /// <param name="scale">Масштаб (1 = 8x8, 2 = 16x16 и т.д.)</param>
    public static SKBitmap TileToBitmap(NesTile tile, SKColor[]? palette = null, int scale = 1)
    {
        palette ??= DefaultPalette;
        
        int scaledSize = NesTile.TileSize * scale;
        var bitmap = new SKBitmap(scaledSize, scaledSize, SKColorType.Rgba8888, SKAlphaType.Premul);

        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { IsAntialias = false };

        // Заливаем фон цветом индекса 0 или прозрачным
        canvas.Clear(palette[0] == SKColor.Empty ? SKColors.Transparent : palette[0]);

        for (int y = 0; y < NesTile.TileSize; y++)
        {
            for (int x = 0; x < NesTile.TileSize; x++)
            {
                byte colorIndex = tile.Pixels[y, x];
                if (colorIndex < palette.Length)
                {
                    var color = palette[colorIndex];
                    if (color != SKColor.Empty) // Пропускаем только прозрачный
                    {
                        paint.Color = color;
                        canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                    }
                }
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Создание панно из всех тайлов
    /// </summary>
    /// <param name="tiles">Список тайлов</param>
    /// <param name="tilesPerRow">Количество тайлов в одном ряду</param>
    /// <param name="tileScale">Масштаб каждого тайла</param>
    /// <param name="spacing">Отступ между тайлами в пикселях</param>
    /// <param name="palette">Палитра цветов</param>
    /// <param name="useTransparency">Использовать прозрачность для индекса 0 (шахматный фон)</param>
    public static SKBitmap CreateTileSheet(
        List<NesTile> tiles,
        int tilesPerRow = DefaultTilesPerRow,
        int tileScale = DefaultTileScale,
        int spacing = DefaultSpacing,
        SKColor[]? palette = null,
        bool useTransparency = false)
    {
        if (tiles == null || tiles.Count == 0)
            throw new ArgumentException("Tiles list cannot be empty", nameof(tiles));

        palette ??= DefaultPalette;
        
        int scaledTileSize = NesTile.TileSize * tileScale;
        int tileWithSpacing = scaledTileSize + spacing;
        
        int rows = (int)Math.Ceiling(tiles.Count / (double)tilesPerRow);
        int width = tilesPerRow * tileWithSpacing - spacing;
        int height = rows * tileWithSpacing - spacing;

        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        
        // Dark background for contrast
        canvas.Clear(new SKColor(DarkBackgroundR, DarkBackgroundG, DarkBackgroundB));

        using var paint = new SKPaint { IsAntialias = false };

        for (int i = 0; i < tiles.Count; i++)
        {
            int row = i / tilesPerRow;
            int col = i % tilesPerRow;
            
            int x = col * tileWithSpacing;
            int y = row * tileWithSpacing;

            var tile = tiles[i];

            // Шахматный фон для прозрачности
            if (useTransparency && palette[0] == SKColor.Empty)
            {
                DrawCheckerboard(canvas, paint, x, y, scaledTileSize, tileScale);
            }

            // Рисуем тайл
            for (int ty = 0; ty < NesTile.TileSize; ty++)
            {
                for (int tx = 0; tx < NesTile.TileSize; tx++)
                {
                    byte colorIndex = tile.Pixels[ty, tx];
                    if (colorIndex < palette.Length)
                    {
                        var color = palette[colorIndex];
                        // Если прозрачность включена и цвет прозрачный, пропускаем
                        if (useTransparency && color == SKColor.Empty)
                            continue;
                        
                        // Если прозрачность выключена, всегда рисуем
                        if (color != SKColor.Empty)
                        {
                            paint.Color = color;
                            canvas.DrawRect(
                                x + tx * tileScale,
                                y + ty * tileScale,
                                tileScale,
                                tileScale,
                                paint);
                        }
                    }
                }
            }

            // Border around tile for better visibility
            if (spacing > 0)
            {
                paint.Color = new SKColor(BorderColorR, BorderColorG, BorderColorB);
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = BorderStrokeWidth;
                canvas.DrawRect(x - BorderOffset, y - BorderOffset, scaledTileSize, scaledTileSize, paint);
                paint.Style = SKPaintStyle.Fill;
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Draws checkerboard background (like in Photoshop)
    /// </summary>
    private static void DrawCheckerboard(SKCanvas canvas, SKPaint paint, int x, int y, int size, int scale)
    {
        var lightGray = new SKColor(CheckerboardLightR, CheckerboardLightG, CheckerboardLightB);
        var darkGray = new SKColor(CheckerboardDarkR, CheckerboardDarkG, CheckerboardDarkB);
        
        int checkSize = Math.Max(scale, MinCheckerboardSize);
        
        for (int cy = 0; cy < size; cy += checkSize)
        {
            for (int cx = 0; cx < size; cx += checkSize)
            {
                bool isLight = ((cx / checkSize) + (cy / checkSize)) % 2 == 0;
                paint.Color = isLight ? lightGray : darkGray;
                canvas.DrawRect(x + cx, y + cy, checkSize, checkSize, paint);
            }
        }
    }

    /// <summary>
    /// Export tile sheet to PNG file
    /// </summary>
    public static void ExportTileSheet(SKBitmap bitmap, string filePath)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, PngQuality);
        using var stream = System.IO.File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// Export all tiles to separate files
    /// </summary>
    public static void ExportTilesIndividually(
        List<NesTile> tiles,
        string directory,
        int tileScale = DefaultIndividualTileScale,
        SKColor[]? palette = null)
    {
        if (!System.IO.Directory.Exists(directory))
            System.IO.Directory.CreateDirectory(directory);

        palette ??= DefaultPalette;

        for (int i = 0; i < tiles.Count; i++)
        {
            var bitmap = TileToBitmap(tiles[i], palette, tileScale);
            string fileName = System.IO.Path.Combine(directory, $"tile_{i:D4}.png");
            
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, PngQuality);
            using var stream = System.IO.File.OpenWrite(fileName);
            data.SaveTo(stream);
            
            bitmap.Dispose();
        }
    }
}

