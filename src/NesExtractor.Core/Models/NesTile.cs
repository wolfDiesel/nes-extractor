using System;

namespace NesExtractor.Core.Models;

/// <summary>
/// Представление одного тайла NES (8x8 пикселей)
/// </summary>
public class NesTile
{
    /// <summary>
    /// Размер тайла в пикселях
    /// </summary>
    public const int TileSize = 8;

    /// <summary>
    /// Размер тайла в байтах (2 bitplanes по 8 байт)
    /// </summary>
    public const int TileSizeInBytes = 16;
    
    public const int BitPlaneCount = 2;
    public const int BitPlaneSize = 8;
    public const int PixelValueMax = 3; // 2 bits = 0-3
    public const int MsbBitPosition = 7; // Most significant bit position
    public const int SingleBitMask = 1;

    /// <summary>
    /// Индекс тайла в CHR ROM
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Массив пикселей 8x8, каждый пиксель - 2 бита (0-3)
    /// </summary>
    public byte[,] Pixels { get; set; } = new byte[TileSize, TileSize];

    /// <summary>
    /// Исходные байты тайла (16 байт)
    /// </summary>
    public byte[] RawData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Декодирование тайла из 16 байт CHR ROM
    /// </summary>
    /// <param name="data">Массив из 16 байт</param>
    /// <param name="index">Индекс тайла</param>
    public static NesTile Decode(byte[] data, int index)
    {
        if (data == null || data.Length < TileSizeInBytes)
            throw new ArgumentException($"Tile data must be at least {TileSizeInBytes} bytes", nameof(data));

        var tile = new NesTile
        {
            Index = index,
            RawData = data[..TileSizeInBytes]
        };

        // NES uses 2 bitplanes
        // First 8 bytes - low bit of color
        // Second 8 bytes - high bit of color
        for (int y = 0; y < TileSize; y++)
        {
            byte lowByte = data[y];
            byte highByte = data[y + BitPlaneSize];

            for (int x = 0; x < TileSize; x++)
            {
                // Get bits from right to left (MSB first)
                int bitPosition = MsbBitPosition - x;
                int lowBit = (lowByte >> bitPosition) & SingleBitMask;
                int highBit = (highByte >> bitPosition) & SingleBitMask;

                // Combine into 2-bit value (0-3)
                tile.Pixels[y, x] = (byte)((highBit << SingleBitMask) | lowBit);
            }
        }

        return tile;
    }

    /// <summary>
    /// Получение всех уникальных используемых цветов в тайле
    /// </summary>
    public byte[] GetUsedColors()
    {
        var colors = new System.Collections.Generic.HashSet<byte>();
        for (int y = 0; y < TileSize; y++)
        {
            for (int x = 0; x < TileSize; x++)
            {
                colors.Add(Pixels[y, x]);
            }
        }
        return colors.ToArray();
    }

    /// <summary>
    /// Проверка, является ли тайл полностью пустым (все пиксели = 0)
    /// </summary>
    public bool IsEmpty()
    {
        for (int y = 0; y < TileSize; y++)
        {
            for (int x = 0; x < TileSize; x++)
            {
                if (Pixels[y, x] != 0)
                    return false;
            }
        }
        return true;
    }
}

