using System;
using System.IO;
using System.Threading.Tasks;
using NesExtractor.Core.Models;

namespace NesExtractor.Core.Parsers;

/// <summary>
/// Parser for reading and decoding NES ROM files.
/// </summary>
public class NesRomParser
{
    /// <summary>
    /// Parse NES file from stream.
    /// </summary>
    public static async Task<NesRom> ParseAsync(Stream stream, string? filePath = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable", nameof(stream));

        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        var rom = new NesRom
        {
            FilePath = filePath
        };

        // Read header
        ReadHeader(reader, rom.Header);

        // Validate header
        if (!rom.Header.IsValid())
        {
            throw new InvalidDataException("Invalid NES file format. Magic number mismatch.");
        }

        // Read trainer (if present)
        if (rom.Header.HasTrainer)
        {
            rom.Trainer = reader.ReadBytes(NesHeader.TrainerSize);
            if (rom.Trainer.Length != NesHeader.TrainerSize)
            {
                throw new InvalidDataException($"Failed to read trainer data (expected {NesHeader.TrainerSize} bytes).");
            }
        }

        // Read PRG ROM
        int prgSize = rom.Header.PrgRomSizeInBytes;
        if (prgSize > 0)
        {
            rom.PrgRom = reader.ReadBytes(prgSize);
            if (rom.PrgRom.Length != prgSize)
            {
                throw new InvalidDataException($"Failed to read PRG ROM data (expected {prgSize} bytes, got {rom.PrgRom.Length}).");
            }
        }

        // Read CHR ROM
        int chrSize = rom.Header.ChrRomSizeInBytes;
        if (chrSize > 0)
        {
            rom.ChrRom = reader.ReadBytes(chrSize);
            if (rom.ChrRom.Length != chrSize)
            {
                throw new InvalidDataException($"Failed to read CHR ROM data (expected {chrSize} bytes, got {rom.ChrRom.Length}).");
            }
        }

        return rom;
    }

    /// <summary>
    /// Parse NES file from disk.
    /// </summary>
    public static async Task<NesRom> ParseFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("NES file not found", filePath);

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await ParseAsync(fileStream, filePath);
    }

    /// <summary>Read NES header.</summary>
    private static void ReadHeader(BinaryReader reader, NesHeader header)
    {
        // Bytes 0-3: Magic
        header.Magic = reader.ReadBytes(NesHeader.MagicSize);

        // Byte 4: PRG ROM size
        header.PrgRomSize = reader.ReadByte();

        // Byte 5: CHR ROM size
        header.ChrRomSize = reader.ReadByte();

        // Byte 6: Flags 6
        header.Flags6 = reader.ReadByte();

        // Byte 7: Flags 7
        header.Flags7 = reader.ReadByte();

        // Byte 8: Flags 8
        header.Flags8 = reader.ReadByte();

        // Byte 9: Flags 9
        header.Flags9 = reader.ReadByte();

        // Byte 10: Flags 10
        header.Flags10 = reader.ReadByte();

        // Bytes 11-15: Padding
        header.Padding = reader.ReadBytes(NesHeader.PaddingSize);
    }

    /// <summary>
    /// Check if file looks like NES ROM by extension and header.
    /// </summary>
    public static async Task<bool> IsNesFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (!File.Exists(filePath))
            return false;

        // Check extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".nes")
            return false;

        try
        {
            // Check magic number
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fileStream);

            if (fileStream.Length < NesHeader.HeaderSize)
                return false;

            var magic = reader.ReadBytes(NesHeader.MagicSize);
            return magic.Length == NesHeader.MagicSize &&
                   magic[0] == 'N' &&
                   magic[1] == 'E' &&
                   magic[2] == 'S' &&
                   magic[3] == NesHeader.MagicByte;
        }
        catch
        {
            return false;
        }
    }
}

