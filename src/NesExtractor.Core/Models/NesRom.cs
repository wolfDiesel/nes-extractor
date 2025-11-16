using System;

namespace NesExtractor.Core.Models;

/// <summary>
/// Представление NES ROM файла
/// </summary>
public class NesRom
{
    /// <summary>
    /// Заголовок ROM
    /// </summary>
    public NesHeader Header { get; set; } = new();

    /// <summary>
    /// Trainer данные (512 байт, если присутствует)
    /// </summary>
    public byte[]? Trainer { get; set; }

    /// <summary>
    /// PRG ROM данные (программный код)
    /// </summary>
    public byte[] PrgRom { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// CHR ROM данные (графические данные)
    /// </summary>
    public byte[] ChrRom { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Путь к исходному файлу
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Имя файла
    /// </summary>
    public string FileName => FilePath != null ? System.IO.Path.GetFileName(FilePath) : "Unknown";

    /// <summary>
    /// Общий размер файла в байтах
    /// </summary>
    public long TotalFileSize
    {
        get
        {
            long size = NesHeader.HeaderSize;
            size += Header.TrainerSizeInBytes;
            size += PrgRom.Length;
            size += ChrRom.Length;
            return size;
        }
    }

    /// <summary>
    /// Валидность ROM
    /// </summary>
    public bool IsValid => Header.IsValid();

    /// <summary>
    /// Получение количества банков PRG ROM
    /// </summary>
    public int PrgRomBankCount => Header.PrgRomSize;

    /// <summary>
    /// Получение количества банков CHR ROM
    /// </summary>
    public int ChrRomBankCount => Header.ChrRomSize;

    /// <summary>
    /// Форматированная строка с информацией о ROM
    /// </summary>
    public override string ToString()
    {
        return $"{FileName} - Mapper: {Header.MapperNumber} ({Header.GetMapperName()}), " +
               $"PRG: {Header.PrgRomSize}x16KB, CHR: {Header.ChrRomSize}x8KB, " +
               $"Mirroring: {Header.Mirroring}";
    }
}

