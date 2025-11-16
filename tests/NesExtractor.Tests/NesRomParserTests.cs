using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using NesExtractor.Core.Models;
using NesExtractor.Core.Parsers;

namespace NesExtractor.Tests;

public class NesRomParserTests
{
    /// <summary>
    /// Создание минимального валидного NES файла в памяти для тестирования
    /// </summary>
    private static byte[] CreateTestNesFile(
        byte prgSize = 1,
        byte chrSize = 1,
        byte flags6 = 0,
        byte flags7 = 0,
        bool includeTrainer = false)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Заголовок (16 байт)
        writer.Write((byte)'N');
        writer.Write((byte)'E');
        writer.Write((byte)'S');
        writer.Write((byte)0x1A);
        writer.Write(prgSize);   // PRG ROM size
        writer.Write(chrSize);   // CHR ROM size
        writer.Write(flags6);    // Flags 6
        writer.Write(flags7);    // Flags 7
        writer.Write((byte)0);   // Flags 8
        writer.Write((byte)0);   // Flags 9
        writer.Write((byte)0);   // Flags 10
        writer.Write(new byte[5]); // Padding

        // Trainer (если нужен)
        if (includeTrainer)
        {
            writer.Write(new byte[512]);
        }

        // PRG ROM (16 KB на блок)
        if (prgSize > 0)
        {
            writer.Write(new byte[prgSize * 16384]);
        }

        // CHR ROM (8 KB на блок)
        if (chrSize > 0)
        {
            writer.Write(new byte[chrSize * 8192]);
        }

        return ms.ToArray();
    }

    [Fact]
    public async Task ParseAsync_ValidNesFile_ShouldParseSuccessfully()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 2, chrSize: 1);
        using var stream = new MemoryStream(testData);

        // Act
        var rom = await NesRomParser.ParseAsync(stream);

        // Assert
        Assert.NotNull(rom);
        Assert.True(rom.IsValid);
        Assert.Equal(2, rom.Header.PrgRomSize);
        Assert.Equal(1, rom.Header.ChrRomSize);
        Assert.Equal(32768, rom.PrgRom.Length); // 2 * 16 KB
        Assert.Equal(8192, rom.ChrRom.Length);  // 1 * 8 KB
    }

    [Fact]
    public async Task ParseAsync_InvalidMagicNumber_ShouldThrowException()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await NesRomParser.ParseAsync(stream);
        });
    }

    [Fact]
    public async Task ParseAsync_WithTrainer_ShouldIncludeTrainerData()
    {
        // Arrange
        byte flags6 = 0x04; // Bit 2 = trainer present
        var testData = CreateTestNesFile(prgSize: 1, chrSize: 1, flags6: flags6, includeTrainer: true);
        using var stream = new MemoryStream(testData);

        // Act
        var rom = await NesRomParser.ParseAsync(stream);

        // Assert
        Assert.True(rom.Header.HasTrainer);
        Assert.NotNull(rom.Trainer);
        Assert.Equal(512, rom.Trainer.Length);
    }

    [Fact]
    public void NesHeader_MapperNumber_ShouldCalculateCorrectly()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags6 = 0x20, // Lower nibble = 2
            Flags7 = 0x10  // Upper nibble = 1
        };

        // Act
        var mapperNumber = header.MapperNumber;

        // Assert
        Assert.Equal(18, mapperNumber); // 0x10 | 0x02 = 18
    }

    [Fact]
    public void NesHeader_Mirroring_Horizontal_ShouldReturnCorrectType()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags6 = 0x00 // Bit 0 = 0 (horizontal)
        };

        // Act
        var mirroring = header.Mirroring;

        // Assert
        Assert.Equal(MirroringType.Horizontal, mirroring);
    }

    [Fact]
    public void NesHeader_Mirroring_Vertical_ShouldReturnCorrectType()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags6 = 0x01 // Bit 0 = 1 (vertical)
        };

        // Act
        var mirroring = header.Mirroring;

        // Assert
        Assert.Equal(MirroringType.Vertical, mirroring);
    }

    [Fact]
    public void NesHeader_Mirroring_FourScreen_ShouldReturnCorrectType()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags6 = 0x08 // Bit 3 = 1 (four-screen)
        };

        // Act
        var mirroring = header.Mirroring;

        // Assert
        Assert.Equal(MirroringType.FourScreen, mirroring);
    }

    [Fact]
    public void NesHeader_BatteryBackedRam_ShouldDetectCorrectly()
    {
        // Arrange
        var headerWithBattery = new NesHeader { Flags6 = 0x02 }; // Bit 1 = 1
        var headerWithoutBattery = new NesHeader { Flags6 = 0x00 };

        // Act & Assert
        Assert.True(headerWithBattery.HasBatteryBackedRam);
        Assert.False(headerWithoutBattery.HasBatteryBackedRam);
    }

    [Fact]
    public void NesHeader_GetMapperName_ShouldReturnKnownMappers()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags6 = 0x00,
            Flags7 = 0x00
        };

        // Act
        var mapperName = header.GetMapperName();

        // Assert
        Assert.Equal("NROM", mapperName);
    }

    [Fact]
    public void NesHeader_Format_ShouldDetectINes()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags7 = 0x00 // Bits 2-3 != 10
        };

        // Act
        var format = header.Format;

        // Assert
        Assert.Equal(NesFormat.INes, format);
    }

    [Fact]
    public void NesHeader_Format_ShouldDetectNes20()
    {
        // Arrange
        var header = new NesHeader
        {
            Flags7 = 0x08 // Bits 2-3 = 10 (binary)
        };

        // Act
        var format = header.Format;

        // Assert
        Assert.Equal(NesFormat.Nes20, format);
    }

    [Fact]
    public void NesRom_TotalFileSize_ShouldCalculateCorrectly()
    {
        // Arrange
        var rom = new NesRom
        {
            Header = new NesHeader
            {
                PrgRomSize = 2,
                ChrRomSize = 1,
                Flags6 = 0x04 // Has trainer
            },
            Trainer = new byte[512],
            PrgRom = new byte[32768],
            ChrRom = new byte[8192]
        };

        // Act
        var totalSize = rom.TotalFileSize;

        // Assert
        // 16 (header) + 512 (trainer) + 32768 (PRG) + 8192 (CHR) = 41488
        Assert.Equal(41488, totalSize);
    }

    [Fact]
    public async Task ParseAsync_NullStream_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await NesRomParser.ParseAsync(null!);
        });
    }

    [Fact]
    public async Task ParseAsync_ZeroPrgRom_ShouldNotFail()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 0, chrSize: 1);
        using var stream = new MemoryStream(testData);

        // Act
        var rom = await NesRomParser.ParseAsync(stream);

        // Assert
        Assert.NotNull(rom);
        Assert.Empty(rom.PrgRom);
        Assert.Equal(8192, rom.ChrRom.Length);
    }
}

