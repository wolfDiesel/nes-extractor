using SkiaSharp;

namespace NesExtractor.Core.Models;

/// <summary>
/// Палитры цветов NES
/// </summary>
public static class NesPalette
{
    public const int StandardPaletteSize = 64; // Total colors in standard NES palette
    public const int TilePaletteSize = 4; // Colors per tile palette (background + 3 colors)
    /// <summary>
    /// Стандартная палитра NES (64 цвета)
    /// Основана на спецификации 2C02 PPU
    /// </summary>
    public static readonly SKColor[] Standard = new SKColor[]
    {
        // 0x00-0x0F
        new SKColor(84, 84, 84),      // 0x00 - Dark Gray
        new SKColor(0, 30, 116),      // 0x01 - Dark Blue
        new SKColor(8, 16, 144),      // 0x02 - Purple
        new SKColor(48, 0, 136),      // 0x03 - Dark Purple
        new SKColor(68, 0, 100),      // 0x04 - Magenta
        new SKColor(92, 0, 48),       // 0x05 - Dark Red
        new SKColor(84, 4, 0),        // 0x06 - Dark Brown
        new SKColor(60, 24, 0),       // 0x07 - Brown
        new SKColor(32, 42, 0),       // 0x08 - Olive
        new SKColor(8, 58, 0),        // 0x09 - Green
        new SKColor(0, 64, 0),        // 0x0A - Dark Green
        new SKColor(0, 60, 0),        // 0x0B - Teal
        new SKColor(0, 50, 60),       // 0x0C - Cyan
        new SKColor(0, 0, 0),         // 0x0D - Black
        new SKColor(0, 0, 0),         // 0x0E - Black (duplicate)
        new SKColor(0, 0, 0),         // 0x0F - Black (duplicate)

        // 0x10-0x1F
        new SKColor(152, 150, 152),   // 0x10 - Light Gray
        new SKColor(8, 76, 196),      // 0x11 - Blue
        new SKColor(48, 50, 236),     // 0x12 - Light Purple
        new SKColor(92, 30, 228),     // 0x13 - Purple
        new SKColor(136, 20, 176),    // 0x14 - Pink
        new SKColor(160, 20, 100),    // 0x15 - Red-Pink
        new SKColor(152, 34, 32),     // 0x16 - Red
        new SKColor(120, 60, 0),      // 0x17 - Orange
        new SKColor(84, 90, 0),       // 0x18 - Yellow-Orange
        new SKColor(40, 114, 0),      // 0x19 - Light Green
        new SKColor(8, 124, 0),       // 0x1A - Green
        new SKColor(0, 118, 40),      // 0x1B - Sea Green
        new SKColor(0, 102, 120),     // 0x1C - Light Cyan
        new SKColor(0, 0, 0),         // 0x1D - Black
        new SKColor(0, 0, 0),         // 0x1E - Black (duplicate)
        new SKColor(0, 0, 0),         // 0x1F - Black (duplicate)

        // 0x20-0x2F
        new SKColor(236, 238, 236),   // 0x20 - White
        new SKColor(76, 154, 236),    // 0x21 - Light Blue
        new SKColor(120, 124, 236),   // 0x22 - Periwinkle
        new SKColor(176, 98, 236),    // 0x23 - Light Purple
        new SKColor(228, 84, 236),    // 0x24 - Light Pink
        new SKColor(236, 88, 180),    // 0x25 - Pink
        new SKColor(236, 106, 100),   // 0x26 - Light Red
        new SKColor(212, 136, 32),    // 0x27 - Light Orange
        new SKColor(160, 170, 0),     // 0x28 - Yellow
        new SKColor(116, 196, 0),     // 0x29 - Lime
        new SKColor(76, 208, 32),     // 0x2A - Light Green
        new SKColor(56, 204, 108),    // 0x2B - Mint
        new SKColor(56, 180, 204),    // 0x2C - Cyan
        new SKColor(60, 60, 60),      // 0x2D - Dark Gray
        new SKColor(0, 0, 0),         // 0x2E - Black (duplicate)
        new SKColor(0, 0, 0),         // 0x2F - Black (duplicate)

        // 0x30-0x3F
        new SKColor(236, 238, 236),   // 0x30 - White (duplicate)
        new SKColor(168, 204, 236),   // 0x31 - Pale Blue
        new SKColor(188, 188, 236),   // 0x32 - Pale Purple
        new SKColor(212, 178, 236),   // 0x33 - Pale Pink
        new SKColor(236, 174, 236),   // 0x34 - Pale Magenta
        new SKColor(236, 174, 212),   // 0x35 - Pale Red
        new SKColor(236, 180, 176),   // 0x36 - Pale Orange
        new SKColor(228, 196, 144),   // 0x37 - Pale Yellow
        new SKColor(204, 210, 120),   // 0x38 - Pale Lime
        new SKColor(180, 222, 120),   // 0x39 - Pale Green
        new SKColor(168, 226, 144),   // 0x3A - Pale Mint
        new SKColor(152, 226, 180),   // 0x3B - Pale Teal
        new SKColor(160, 214, 228),   // 0x3C - Pale Cyan
        new SKColor(160, 162, 160),   // 0x3D - Light Gray
        new SKColor(0, 0, 0),         // 0x3E - Black (duplicate)
        new SKColor(0, 0, 0),         // 0x3F - Black (duplicate)
    };

    /// <summary>
    /// Greyscale палитра для базового отображения (БЕЗ прозрачности)
    /// Это честный способ показать CHR ROM, так как палитры не хранятся в .NES файле
    /// </summary>
    public static readonly SKColor[] Greyscale = new SKColor[]
    {
        new SKColor(0, 0, 0),         // 0 - Черный (фон)
        new SKColor(85, 85, 85),      // 1 - Темно-серый
        new SKColor(170, 170, 170),   // 2 - Светло-серый
        new SKColor(255, 255, 255)    // 3 - Белый
    };

    /// <summary>
    /// Greyscale палитра С прозрачностью (для спрайтов)
    /// </summary>
    public static readonly SKColor[] GreyscaleTransparent = new SKColor[]
    {
        SKColor.Empty,                // 0 - Прозрачный
        new SKColor(85, 85, 85),      // 1 - Темно-серый
        new SKColor(170, 170, 170),   // 2 - Светло-серый
        new SKColor(255, 255, 255)    // 3 - Белый
    };

    /// <summary>
    /// Получить палитру по индексу
    /// </summary>
    /// <param name="paletteIndex">Индекс палитры 0-9</param>
    /// <param name="transparent">Использовать прозрачность для индекса 0</param>
    /// <returns>Массив из 4 цветов</returns>
    public static SKColor[] GetPalette(int paletteIndex, bool transparent = false)
    {
        // 0 - Greyscale (по умолчанию, честный способ)
        if (paletteIndex == 0)
        {
            return transparent ? GreyscaleTransparent : Greyscale;
        }

        // ВАЖНО: Палитры НЕ хранятся в .NES файле!
        // Это предустановленные палитры для визуализации (как в YY-CHR)
        var palettes = new int[][]
        {
            // Палитра 1 - нейтральная цветная
            new int[] { 0x0F, 0x00, 0x10, 0x30 },
            // Палитра 2 - синяя
            new int[] { 0x0F, 0x02, 0x12, 0x22 },
            // Палитра 3 - красная
            new int[] { 0x0F, 0x06, 0x16, 0x26 },
            // Палитра 4 - зеленая
            new int[] { 0x0F, 0x0A, 0x1A, 0x2A },
            // Палитра 5 - желтая
            new int[] { 0x0F, 0x18, 0x28, 0x38 },
            // Палитра 6 - фиолетовая
            new int[] { 0x0F, 0x03, 0x13, 0x23 },
            // Палитра 7 - бирюзовая
            new int[] { 0x0F, 0x0C, 0x1C, 0x2C },
            // Палитра 8 - оранжевая
            new int[] { 0x0F, 0x07, 0x17, 0x27 },
            // Палитра 9 - все цвета
            new int[] { 0x0F, 0x16, 0x27, 0x38 },
        };

        int idx = paletteIndex - 1;
        if (idx < 0 || idx >= palettes.Length)
            idx = 0;

        var indices = palettes[idx];
        var result = new SKColor[TilePaletteSize];
        
        // Index 0 - transparent or background
        result[0] = transparent ? SKColor.Empty : Standard[indices[0]];
        for (int i = 1; i < TilePaletteSize; i++)
        {
            result[i] = Standard[indices[i]];
        }

        return result;
    }

    /// <summary>
    /// Имена предустановленных палитр
    /// </summary>
    public static readonly string[] PaletteNames = new string[]
    {
        "Greyscale (по умолчанию)",
        "Нейтральная",
        "Синяя",
        "Красная",
        "Зеленая",
        "Желтая",
        "Фиолетовая",
        "Бирюзовая",
        "Оранжевая",
        "Радужная"
    };
}

