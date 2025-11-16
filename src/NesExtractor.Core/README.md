# NesExtractor.Core

Библиотека для парсинга и работы с NES ROM файлами (формат iNES/NES 2.0).

## Возможности

- ✅ Парсинг iNES 1.0 и NES 2.0 форматов
- ✅ Чтение заголовка с полной информацией о картридже
- ✅ Извлечение PRG ROM (программный код)
- ✅ Извлечение CHR ROM (графические данные)
- ✅ Поддержка Trainer секции
- ✅ Определение типа маппера
- ✅ Определение типа mirroring
- ✅ Асинхронный API

## Использование

### Базовый пример

```csharp
using NesExtractor.Core.Parsers;
using NesExtractor.Core.Models;

// Парсинг NES файла
var rom = await NesRomParser.ParseFileAsync("SuperMario.nes");

// Получение информации
Console.WriteLine($"Файл: {rom.FileName}");
Console.WriteLine($"Формат: {rom.Header.Format}");
Console.WriteLine($"Mapper: #{rom.Header.MapperNumber} ({rom.Header.GetMapperName()})");
Console.WriteLine($"PRG ROM: {rom.Header.PrgRomSize} x 16 KB = {rom.Header.PrgRomSizeInBytes} байт");
Console.WriteLine($"CHR ROM: {rom.Header.ChrRomSize} x 8 KB = {rom.Header.ChrRomSizeInBytes} байт");
Console.WriteLine($"Mirroring: {rom.Header.Mirroring}");
Console.WriteLine($"Battery RAM: {(rom.Header.HasBatteryBackedRam ? "Да" : "Нет")}");
Console.WriteLine($"Trainer: {(rom.Header.HasTrainer ? "Присутствует" : "Отсутствует")}");
```

### Парсинг из Stream

```csharp
using var fileStream = File.OpenRead("game.nes");
var rom = await NesRomParser.ParseAsync(fileStream, "game.nes");
```

### Проверка валидности

```csharp
// Проверка по расширению и магическому числу
if (await NesRomParser.IsNesFileAsync("game.nes"))
{
    var rom = await NesRomParser.ParseFileAsync("game.nes");
    
    // Дополнительная проверка
    if (rom.IsValid)
    {
        Console.WriteLine("ROM валиден!");
    }
}
```

### Работа с данными ROM

```csharp
var rom = await NesRomParser.ParseFileAsync("game.nes");

// PRG ROM (программный код)
byte[] programCode = rom.PrgRom;
Console.WriteLine($"Размер программы: {programCode.Length} байт");

// CHR ROM (графика)
byte[] graphics = rom.ChrRom;
Console.WriteLine($"Размер графики: {graphics.Length} байт");

// Trainer (если есть)
if (rom.Trainer != null)
{
    byte[] trainer = rom.Trainer;
    Console.WriteLine($"Trainer: {trainer.Length} байт");
}
```

### Обработка ошибок

```csharp
try
{
    var rom = await NesRomParser.ParseFileAsync("unknown.nes");
}
catch (FileNotFoundException)
{
    Console.WriteLine("Файл не найден");
}
catch (InvalidDataException ex)
{
    Console.WriteLine($"Неверный формат файла: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}
```

## Структура заголовка iNES

```
Байты  | Описание
-------|------------------------------------------
0-3    | Magic: "NES" + 0x1A
4      | PRG ROM size (в 16 KB блоках)
5      | CHR ROM size (в 8 KB блоках)
6      | Flags 6 (mapper low, mirroring, battery, trainer)
7      | Flags 7 (mapper high, VS/PC, NES 2.0)
8      | Flags 8 (PRG-RAM size / mapper ext)
9      | Flags 9 (TV system)
10     | Flags 10 (TV system, PRG-RAM)
11-15  | Padding (обычно нули)
```

## Известные мапперы

| Номер | Название      | Примеры игр                |
|-------|---------------|----------------------------|
| 0     | NROM          | Super Mario Bros, Donkey Kong |
| 1     | MMC1          | Mega Man 2, Legend of Zelda |
| 2     | UxROM         | Mega Man, Castlevania      |
| 3     | CNROM         | Arkanoid, Solomon's Key    |
| 4     | MMC3          | Super Mario Bros 3         |

## Типы Mirroring

- **Horizontal** - горизонтальное зеркалирование
- **Vertical** - вертикальное зеркалирование  
- **FourScreen** - 4-screen VRAM

## Лицензия

TBD

