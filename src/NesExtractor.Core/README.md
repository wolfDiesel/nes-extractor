# NesExtractor.Core

Library for parsing and working with NES ROM files (iNES/NES 2.0 format).

## Features

- ✅ iNES 1.0 and NES 2.0 format parsing
- ✅ Header reading with full cartridge information
- ✅ PRG ROM extraction (program code)
- ✅ CHR ROM extraction (graphics data)
- ✅ Trainer section support
- ✅ Mapper type detection
- ✅ Mirroring type detection
- ✅ Async API

## Usage

### Basic Example

```csharp
using NesExtractor.Core.Parsers;
using NesExtractor.Core.Models;

// Parse NES file
var rom = await NesRomParser.ParseFileAsync("SuperMario.nes");

// Get information
Console.WriteLine($"File: {rom.FileName}");
Console.WriteLine($"Format: {rom.Header.Format}");
Console.WriteLine($"Mapper: #{rom.Header.MapperNumber} ({rom.Header.GetMapperName()})");
Console.WriteLine($"PRG ROM: {rom.Header.PrgRomSize} x 16 KB = {rom.Header.PrgRomSizeInBytes} bytes");
Console.WriteLine($"CHR ROM: {rom.Header.ChrRomSize} x 8 KB = {rom.Header.ChrRomSizeInBytes} bytes");
Console.WriteLine($"Mirroring: {rom.Header.Mirroring}");
Console.WriteLine($"Battery RAM: {(rom.Header.HasBatteryBackedRam ? "Yes" : "No")}");
Console.WriteLine($"Trainer: {(rom.Header.HasTrainer ? "Present" : "Absent")}");
```

### Parsing from Stream

```csharp
using var fileStream = File.OpenRead("game.nes");
var rom = await NesRomParser.ParseAsync(fileStream, "game.nes");
```

### Validation Check

```csharp
// Check by extension and magic number
if (await NesRomParser.IsNesFileAsync("game.nes"))
{
    var rom = await NesRomParser.ParseFileAsync("game.nes");
    
    // Additional validation
    if (rom.IsValid)
    {
        Console.WriteLine("ROM is valid!");
    }
}
```

### Working with ROM Data

```csharp
var rom = await NesRomParser.ParseFileAsync("game.nes");

// PRG ROM (program code)
byte[] programCode = rom.PrgRom;
Console.WriteLine($"Program size: {programCode.Length} bytes");

// CHR ROM (graphics)
byte[] graphics = rom.ChrRom;
Console.WriteLine($"Graphics size: {graphics.Length} bytes");

// Trainer (if present)
if (rom.Trainer != null)
{
    byte[] trainer = rom.Trainer;
    Console.WriteLine($"Trainer: {trainer.Length} bytes");
}
```

### Error Handling

```csharp
try
{
    var rom = await NesRomParser.ParseFileAsync("unknown.nes");
}
catch (FileNotFoundException)
{
    Console.WriteLine("File not found");
}
catch (InvalidDataException ex)
{
    Console.WriteLine($"Invalid file format: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## iNES Header Structure

```
Bytes  | Description
-------|------------------------------------------
0-3    | Magic: "NES" + 0x1A
4      | PRG ROM size (in 16 KB blocks)
5      | CHR ROM size (in 8 KB blocks)
6      | Flags 6 (mapper low, mirroring, battery, trainer)
7      | Flags 7 (mapper high, VS/PC, NES 2.0)
8      | Flags 8 (PRG-RAM size / mapper ext)
9      | Flags 9 (TV system)
10     | Flags 10 (TV system, PRG-RAM)
11-15  | Padding (usually zeros)
```

## Known Mappers

| Number | Name      | Example Games                |
|--------|-----------|-----------------------------|
| 0      | NROM      | Super Mario Bros, Donkey Kong |
| 1      | MMC1      | Mega Man 2, Legend of Zelda |
| 2      | UxROM     | Mega Man, Castlevania      |
| 3      | CNROM     | Arkanoid, Solomon's Key    |
| 4      | MMC3      | Super Mario Bros 3         |

## Mirroring Types

- **Horizontal** - horizontal mirroring
- **Vertical** - vertical mirroring  
- **FourScreen** - 4-screen VRAM

## License

TBD
