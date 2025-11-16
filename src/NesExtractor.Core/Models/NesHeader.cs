using System;

namespace NesExtractor.Core.Models;

/// <summary>
/// NES ROM header (iNES / NES 2.0), 16 bytes total.
/// </summary>
public class NesHeader
{
    public const int HeaderSize = 16;
    public const int MagicSize = 4;
    public const int PaddingSize = 5;
    
    public const byte MagicByte = 0b0001_1010; // 0x1A
    
    public const int PrgRomBlockSize = 16384; // 16 KB
    public const int ChrRomBlockSize = 8192; // 8 KB
    public const int TrainerSize = 512;
    
    public const byte Flag6BitMirroring = 0b0000_0001; // Bit 0
    public const byte Flag6BitBattery = 0b0000_0010; // Bit 1
    public const byte Flag6BitTrainer = 0b0000_0100; // Bit 2
    public const byte Flag6BitFourScreen = 0b0000_1000; // Bit 3
    public const byte Flag6MapperLow = 0b1111_0000; // Bits 4-7
    
    public const byte Flag7Nes2IdentifierMask = 0b0000_1100; // Bits 2-3
    public const byte Flag7Nes2IdentifierValue = 0b0000_1000; // Value 10 (binary) = 2 (decimal)
    public const byte Flag7MapperHigh = 0b1111_0000; // Bits 4-7
    
    public const int Nes2IdentifierShift = 2;
    public const int Nes2IdentifierMask = 0b0000_0011; // 2 bits
    public const int Nes2IdentifierValue = 2; // NES 2.0 format
    
    public const int MapperLowShift = 4;
    
    /// <summary>
    /// Magic bytes, should be ['N','E','S', 0x1A]
    /// </summary>
    public byte[] Magic { get; set; } = new byte[MagicSize];

    /// <summary>PRG ROM size in 16KB units.</summary>
    public byte PrgRomSize { get; set; }

    /// <summary>CHR ROM size in 8KB units.</summary>
    public byte ChrRomSize { get; set; }

    /// <summary>
    /// Flags 6 bits:
    /// 0: Mirroring (0 = horizontal, 1 = vertical)
    /// 1: Battery-backed PRG RAM
    /// 2: 512-byte trainer
    /// 3: Four-screen VRAM
    /// 4-7: Lower nibble of mapper number
    /// </summary>
    public byte Flags6 { get; set; }

    /// <summary>
    /// Flags 7 bits:
    /// 0: VS Unisystem
    /// 1: PlayChoice-10
    /// 2-3: NES 2.0 identifier (if 10 then NES 2.0)
    /// 4-7: Upper nibble of mapper number
    /// </summary>
    public byte Flags7 { get; set; }

    /// <summary>Flags 8 (PRG RAM size or mapper extension).</summary>
    public byte Flags8 { get; set; }

    /// <summary>Flags 9 (TV system).</summary>
    public byte Flags9 { get; set; }

    /// <summary>Flags 10 (TV system, PRG-RAM presence).</summary>
    public byte Flags10 { get; set; }

    /// <summary>Unused padding bytes (11-15).</summary>
    public byte[] Padding { get; set; } = new byte[PaddingSize];

    /// <summary>Mapper number (lower + upper nibbles combined).</summary>
    public int MapperNumber => ((Flags7 & Flag7MapperHigh) | (Flags6 >> MapperLowShift));

    /// <summary>VRAM mirroring type.</summary>
    public MirroringType Mirroring
    {
        get
        {
            if ((Flags6 & Flag6BitFourScreen) != 0)
                return MirroringType.FourScreen;
            return (Flags6 & Flag6BitMirroring) != 0 ? MirroringType.Vertical : MirroringType.Horizontal;
        }
    }

    /// <summary>Whether battery-backed RAM is present.</summary>
    public bool HasBatteryBackedRam => (Flags6 & Flag6BitBattery) != 0;

    /// <summary>Whether 512-byte trainer is present.</summary>
    public bool HasTrainer => (Flags6 & Flag6BitTrainer) != 0;

    /// <summary>File format (iNES 1.0 or NES 2.0).</summary>
    public NesFormat Format
    {
        get
        {
            int nes2Bits = (Flags7 >> Nes2IdentifierShift) & Nes2IdentifierMask;
            return nes2Bits == Nes2IdentifierValue ? NesFormat.Nes20 : NesFormat.INes;
        }
    }

    /// <summary>PRG ROM size in bytes.</summary>
    public int PrgRomSizeInBytes => PrgRomSize * PrgRomBlockSize;

    /// <summary>CHR ROM size in bytes.</summary>
    public int ChrRomSizeInBytes => ChrRomSize * ChrRomBlockSize;

    /// <summary>Trainer size in bytes (0 or 512).</summary>
    public int TrainerSizeInBytes => HasTrainer ? TrainerSize : 0;

    /// <summary>Validate magic number.</summary>
    public bool IsValid()
    {
        return Magic.Length == MagicSize &&
               Magic[0] == 'N' &&
               Magic[1] == 'E' &&
               Magic[2] == 'S' &&
               Magic[3] == MagicByte;
    }

    /// <summary>Get mapper name if known.</summary>
    public string GetMapperName()
    {
        return MapperNumber switch
        {
            0 => "NROM",
            1 => "MMC1",
            2 => "UxROM",
            3 => "CNROM",
            4 => "MMC3",
            5 => "MMC5",
            7 => "AxROM",
            9 => "MMC2",
            10 => "MMC4",
            11 => "Color Dreams",
            _ => $"Unknown (#{MapperNumber})"
        };
    }
}

/// <summary>VRAM mirroring type.</summary>
public enum MirroringType
{
    Horizontal,
    Vertical,
    FourScreen
}

/// <summary>NES ROM file format.</summary>
public enum NesFormat
{
    INes,   // iNES 1.0
    Nes20   // NES 2.0
}

