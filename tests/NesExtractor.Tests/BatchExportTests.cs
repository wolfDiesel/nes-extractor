using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using NesExtractor.Core.Models;
using NesExtractor.Core.Parsers;
using NesExtractor.Core.Services;

namespace NesExtractor.Tests;

public class BatchExportTests
{
    private static byte[] CreateTestNesFile(
        byte prgSize = 1,
        byte chrSize = 1,
        byte flags6 = 0,
        byte flags7 = 0,
        bool includeTrainer = false)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Header (16 bytes)
        writer.Write((byte)'N');
        writer.Write((byte)'E');
        writer.Write((byte)'S');
        writer.Write((byte)0x1A);
        writer.Write(prgSize);
        writer.Write(chrSize);
        writer.Write(flags6);
        writer.Write(flags7);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write(new byte[5]);

        if (includeTrainer)
        {
            writer.Write(new byte[512]);
        }

        if (prgSize > 0)
        {
            writer.Write(new byte[prgSize * 16384]);
        }

        if (chrSize > 0)
        {
            writer.Write(new byte[chrSize * 8192]);
        }

        return ms.ToArray();
    }

    [Fact]
    public async Task BatchExport_ExportRomJson_ShouldCreateValidJsonFile()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 1, chrSize: 1);
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.nes");
        File.WriteAllBytes(tempFile, testData);

        try
        {
            var rom = await NesRomParser.ParseFileAsync(tempFile);
            var tempDir = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                ExportRomJson(rom, tempDir, "test_rom");

                // Assert
                var jsonPath = Path.Combine(tempDir, "test_rom.json");
                Assert.True(File.Exists(jsonPath));

                var jsonContent = File.ReadAllText(jsonPath);
                var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                Assert.True(jsonData.TryGetProperty("fileName", out var fileName));
                Assert.True(jsonData.TryGetProperty("format", out var format));
                Assert.True(jsonData.TryGetProperty("mapper", out var mapper));
                Assert.True(jsonData.TryGetProperty("prgRom", out var prgRom));
                Assert.True(jsonData.TryGetProperty("chrRom", out var chrRom));
                Assert.True(jsonData.TryGetProperty("mirroring", out var mirroring));
                Assert.True(jsonData.TryGetProperty("hasBatteryBackedRam", out _));
                Assert.True(jsonData.TryGetProperty("hasTrainer", out _));
                Assert.True(jsonData.TryGetProperty("totalFileSize", out _));
                Assert.True(jsonData.TryGetProperty("isValid", out _));

                // Check mapper structure
                Assert.True(mapper.TryGetProperty("number", out _));
                Assert.True(mapper.TryGetProperty("name", out _));

                // Check PRG ROM structure
                Assert.True(prgRom.TryGetProperty("banks", out _));
                Assert.True(prgRom.TryGetProperty("sizeBytes", out _));

                // Check CHR ROM structure
                Assert.True(chrRom.TryGetProperty("banks", out _));
                Assert.True(chrRom.TryGetProperty("sizeBytes", out _));
                Assert.True(chrRom.TryGetProperty("tileCount", out _));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task BatchExport_ExportRomJson_ShouldContainCorrectData()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 2, chrSize: 1, flags6: 0b0000_0010); // Battery-backed RAM
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.nes");
        File.WriteAllBytes(tempFile, testData);

        try
        {
            var rom = await NesRomParser.ParseFileAsync(tempFile);
            var tempDir = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                ExportRomJson(rom, tempDir, "test_rom");

                // Assert
                var jsonPath = Path.Combine(tempDir, "test_rom.json");
                var jsonContent = File.ReadAllText(jsonPath);
                var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                Assert.Equal(rom.FileName, jsonData.GetProperty("fileName").GetString());
                Assert.Equal(rom.Header.Format.ToString(), jsonData.GetProperty("format").GetString());
                Assert.Equal(rom.Header.MapperNumber, jsonData.GetProperty("mapper").GetProperty("number").GetInt32());
                Assert.Equal(rom.Header.PrgRomSize, jsonData.GetProperty("prgRom").GetProperty("banks").GetByte());
                Assert.Equal(rom.Header.ChrRomSize, jsonData.GetProperty("chrRom").GetProperty("banks").GetByte());
                Assert.Equal(rom.Header.Mirroring.ToString(), jsonData.GetProperty("mirroring").GetString());
                Assert.Equal(rom.Header.HasBatteryBackedRam, jsonData.GetProperty("hasBatteryBackedRam").GetBoolean());
                Assert.Equal(rom.IsValid, jsonData.GetProperty("isValid").GetBoolean());

                var expectedTileCount = rom.ChrRom.Length / NesTile.TileSizeInBytes;
                Assert.Equal(expectedTileCount, jsonData.GetProperty("chrRom").GetProperty("tileCount").GetInt32());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task BatchExport_ExportTiles_ShouldCreateCorrectNumberOfFiles()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 1, chrSize: 1); // 1 CHR bank = 256 tiles
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.nes");
        File.WriteAllBytes(tempFile, testData);

        try
        {
            var rom = await NesRomParser.ParseFileAsync(tempFile);
            var tempDir = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                var romName = Path.GetFileNameWithoutExtension(rom.FileName);
                var romDirectory = Path.Combine(tempDir, romName);
                Directory.CreateDirectory(romDirectory);

                // Act
                var tiles = ChrRomExtractor.ExtractTiles(rom.ChrRom);
                var palette = NesPalette.GetPalette(0, transparent: true);

                for (int i = 0; i < tiles.Count; i++)
                {
                    var bitmap = ChrRomExtractor.TileToBitmap(tiles[i], palette, scale: 4);
                    string fileName = Path.Combine(romDirectory, $"{romName}_{i:D3}.png");

                    using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
                    using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                    using var stream = File.OpenWrite(fileName);
                    data.SaveTo(stream);

                    bitmap.Dispose();
                }

                // Assert
                var files = Directory.GetFiles(romDirectory, $"{romName}_*.png");
                Assert.Equal(tiles.Count, files.Length);

                // Check file naming
                for (int i = 0; i < tiles.Count; i++)
                {
                    var expectedFileName = Path.Combine(romDirectory, $"{romName}_{i:D3}.png");
                    Assert.Contains(expectedFileName, files);
                    Assert.True(File.Exists(expectedFileName));
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task BatchExport_ExportFullTileSheet_ShouldCreateFullSheetFile()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 1, chrSize: 1);
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.nes");
        File.WriteAllBytes(tempFile, testData);

        try
        {
            var rom = await NesRomParser.ParseFileAsync(tempFile);
            var tempDir = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                var romName = Path.GetFileNameWithoutExtension(rom.FileName);
                var romDirectory = Path.Combine(tempDir, romName);
                Directory.CreateDirectory(romDirectory);

                // Act
                var tiles = ChrRomExtractor.ExtractTiles(rom.ChrRom);
                var palette = NesPalette.GetPalette(0, transparent: true);

                using var fullSheet = ChrRomExtractor.CreateTileSheet(
                    tiles,
                    tilesPerRow: 16,
                    tileScale: 2,
                    spacing: 1,
                    palette: palette,
                    useTransparency: true);

                string fullSheetPath = Path.Combine(romDirectory, $"{romName}_full.png");
                ChrRomExtractor.ExportTileSheet(fullSheet, fullSheetPath);

                // Assert
                Assert.True(File.Exists(fullSheetPath));
                var fileInfo = new FileInfo(fullSheetPath);
                Assert.True(fileInfo.Length > 0);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task BatchExport_ExportRomJson_WithTrainer_ShouldIncludeTrainerInfo()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 1, chrSize: 1, flags6: 0b0000_0100, includeTrainer: true);
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.nes");
        File.WriteAllBytes(tempFile, testData);

        try
        {
            var rom = await NesRomParser.ParseFileAsync(tempFile);
            var tempDir = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                ExportRomJson(rom, tempDir, "test_rom");

                // Assert
                var jsonPath = Path.Combine(tempDir, "test_rom.json");
                var jsonContent = File.ReadAllText(jsonPath);
                var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                Assert.True(jsonData.GetProperty("hasTrainer").GetBoolean());
                Assert.Equal(512, jsonData.GetProperty("trainerSize").GetInt32());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    // Helper method extracted from MainWindowViewModel for testing
    private void ExportRomJson(NesRom rom, string directory, string romName)
    {
        var jsonData = new
        {
            fileName = rom.FileName,
            filePath = rom.FilePath,
            format = rom.Header.Format.ToString(),
            mapper = new
            {
                number = rom.Header.MapperNumber,
                name = rom.Header.GetMapperName()
            },
            prgRom = new
            {
                banks = rom.Header.PrgRomSize,
                sizeBytes = rom.Header.PrgRomSizeInBytes
            },
            chrRom = new
            {
                banks = rom.Header.ChrRomSize,
                sizeBytes = rom.Header.ChrRomSizeInBytes,
                tileCount = rom.ChrRom.Length / NesTile.TileSizeInBytes
            },
            mirroring = rom.Header.Mirroring.ToString(),
            hasBatteryBackedRam = rom.Header.HasBatteryBackedRam,
            hasTrainer = rom.Header.HasTrainer,
            trainerSize = rom.Header.TrainerSizeInBytes,
            totalFileSize = rom.TotalFileSize,
            isValid = rom.IsValid
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(jsonData, options);
        string jsonPath = Path.Combine(directory, $"{romName}.json");
        File.WriteAllText(jsonPath, json);
    }
}

