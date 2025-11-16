using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using NesExtractor.Core.Parsers;

namespace NesExtractor.Tests;

public class NesRomParserAdditionalTests
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
    public async Task ParseFileAsync_ValidFile_ShouldParseSuccessfully()
    {
        // Arrange
        var testData = CreateTestNesFile(prgSize: 1, chrSize: 1);
        var tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, testData);

        try
        {
            // Act
            var rom = await NesRomParser.ParseFileAsync(tempFile);

            // Assert
            Assert.NotNull(rom);
            Assert.True(rom.IsValid);
            Assert.Equal(tempFile, rom.FilePath);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseFileAsync_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await NesRomParser.ParseFileAsync("nonexistent.nes");
        });
    }

    [Fact]
    public async Task ParseFileAsync_NullPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await NesRomParser.ParseFileAsync(null!);
        });
    }

    [Fact]
    public async Task ParseFileAsync_EmptyPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await NesRomParser.ParseFileAsync("");
        });
    }

    [Fact]
    public async Task IsNesFileAsync_ValidNesFile_ShouldReturnTrue()
    {
        // Arrange
        var testData = CreateTestNesFile();
        var tempFile = Path.Combine(Path.GetTempPath(), "test.nes");
        File.WriteAllBytes(tempFile, testData);

        try
        {
            // Act
            var isNes = await NesRomParser.IsNesFileAsync(tempFile);

            // Assert
            Assert.True(isNes);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task IsNesFileAsync_InvalidExtension_ShouldReturnFalse()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            var isNes = await NesRomParser.IsNesFileAsync(tempFile);

            // Assert
            Assert.False(isNes);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task IsNesFileAsync_NonExistentFile_ShouldReturnFalse()
    {
        // Act
        var isNes = await NesRomParser.IsNesFileAsync("nonexistent.nes");

        // Assert
        Assert.False(isNes);
    }

    [Fact]
    public async Task IsNesFileAsync_NullPath_ShouldReturnFalse()
    {
        // Act
        var isNes = await NesRomParser.IsNesFileAsync(null!);

        // Assert
        Assert.False(isNes);
    }

    [Fact]
    public async Task IsNesFileAsync_EmptyPath_ShouldReturnFalse()
    {
        // Act
        var isNes = await NesRomParser.IsNesFileAsync("");

        // Assert
        Assert.False(isNes);
    }

    [Fact]
    public async Task IsNesFileAsync_TooSmallFile_ShouldReturnFalse()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "small.nes");
        File.WriteAllBytes(tempFile, new byte[10]); // Less than 16 bytes

        try
        {
            // Act
            var isNes = await NesRomParser.IsNesFileAsync(tempFile);

            // Assert
            Assert.False(isNes);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task IsNesFileAsync_InvalidMagicNumber_ShouldReturnFalse()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "invalid.nes");
        var invalidData = new byte[16];
        invalidData[0] = (byte)'X'; // Invalid magic
        invalidData[1] = (byte)'X';
        invalidData[2] = (byte)'X';
        invalidData[3] = 0x1A;
        File.WriteAllBytes(tempFile, invalidData);

        try
        {
            // Act
            var isNes = await NesRomParser.IsNesFileAsync(tempFile);

            // Assert
            Assert.False(isNes);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseAsync_NonReadableStream_ShouldThrowArgumentException()
    {
        // Arrange
        using var stream = new MemoryStream();
        stream.Close(); // Make it non-readable

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await NesRomParser.ParseAsync(stream);
        });
    }

    [Fact]
    public async Task ParseAsync_IncompletePrgRom_ShouldThrowInvalidDataException()
    {
        // Arrange - File says PRG ROM is 1 block, but we provide less
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Header
        writer.Write((byte)'N');
        writer.Write((byte)'E');
        writer.Write((byte)'S');
        writer.Write((byte)0x1A);
        writer.Write((byte)1); // PRG ROM size = 1 block (16 KB)
        writer.Write((byte)0); // No CHR ROM
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write(new byte[8]); // Rest of header

        // But only write 1000 bytes instead of 16384
        writer.Write(new byte[1000]);

        ms.Position = 0;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await NesRomParser.ParseAsync(ms);
        });
    }

    [Fact]
    public async Task ParseAsync_IncompleteChrRom_ShouldThrowInvalidDataException()
    {
        // Arrange - File says CHR ROM is 1 block, but we provide less
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Header
        writer.Write((byte)'N');
        writer.Write((byte)'E');
        writer.Write((byte)'S');
        writer.Write((byte)0x1A);
        writer.Write((byte)0); // No PRG ROM
        writer.Write((byte)1); // CHR ROM size = 1 block (8 KB)
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write(new byte[8]); // Rest of header

        // But only write 1000 bytes instead of 8192
        writer.Write(new byte[1000]);

        ms.Position = 0;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await NesRomParser.ParseAsync(ms);
        });
    }
}

