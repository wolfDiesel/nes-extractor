# NesExtractor

Cross-platform desktop utility for analyzing and extracting data from NES ROM files (Nintendo Entertainment System cartridge dumps).

## Features

### Implemented ✅
- ✅ Open and analyze .NES files (iNES/NES 2.0 format)
- ✅ Display cartridge information:
  - File size
  - PRG ROM (program code)
  - CHR ROM (graphics data)
  - Mapper number and name (NROM, MMC1, UxROM, etc.)
  - Mirroring type (Horizontal/Vertical/FourScreen)
  - Battery-backed RAM presence
  - Trainer section presence
- ✅ Support for multiple files simultaneously (tab system)
- ✅ File dialog with .nes file filtering
- ✅ Error handling with UI notifications
- ✅ Hotkeys (Ctrl+O, Ctrl+W, Ctrl+Q)
- ✅ Duplicate file detection

- ✅ **CHR ROM Graphics Visualization:**
  - Extract and decode 8×8 pixel tiles
  - Tile sheet with all tiles
  - Zoom (0.1x - 10x) and scrolling
  - Spacing between tiles
  - Export tile sheet to PNG
  - Export tiles to separate files
  - Greyscale palette

### In Development 🚧
- 🚧 NES color palette selection
- 🚧 Palette editing
- 🚧 Individual tile selection

## Technologies

- C# / .NET 8.0
- Avalonia UI 11.x
- MVVM pattern

## Target Platforms

- Linux (x64)
- Windows (x64)

## Building and Running

### Requirements

- .NET 8.0 SDK or newer
- Linux (x64) or Windows (x64)

### Quick Start

```bash
# Clone the repository (if not already done)
cd /path/to/NesExtractor

# Build
dotnet build

# Run the application
dotnet run --project src/NesExtractor/NesExtractor.csproj

# Run tests
dotnet test
```

### Release Build

```bash
# Linux
dotnet publish src/NesExtractor/NesExtractor.csproj -c Release -r linux-x64 --self-contained

# Windows
dotnet publish src/NesExtractor/NesExtractor.csproj -c Release -r win-x64 --self-contained
```

## Project Structure

```
NesExtractor/
├── src/
│   ├── NesExtractor/           # UI application (Avalonia)
│   │   ├── ViewModels/         # ViewModels
│   │   ├── Views/              # XAML views
│   │   └── Assets/             # Resources
│   └── NesExtractor.Core/      # NES processing library
│       ├── Models/             # NesHeader, NesRom
│       ├── Parsers/            # NesRomParser
│       └── Services/           # Processing services
├── tests/
│   └── NesExtractor.Tests/     # Unit tests (xUnit)
└── NesExtractor.sln            # Solution file
```

## NesExtractor.Core Library Features

- ✅ iNES 1.0 and NES 2.0 format parsing
- ✅ Header reading with full cartridge information
- ✅ PRG ROM extraction (program code)
- ✅ CHR ROM extraction (graphics data)
- ✅ Trainer section support
- ✅ Mapper type detection
- ✅ Mirroring type detection
- ✅ Fully covered by unit tests (87 tests)

## Usage

1. Launch the application
2. Click `File → Open...` (or Ctrl+O)
3. Select one or more .nes files
4. View cartridge information in the **left panel (30%)**
5. View CHR ROM graphics in the **right panel (70%)**
6. **Select color palette** from dropdown (10 options)
7. Use zoom (+/−/100%) to scale graphics
8. Export tile sheet or individual tiles via bottom buttons
9. Use tabs to switch between files
10. Close tab via Ctrl+W or menu

## Hotkeys

- `Ctrl+O` - Open file
- `Ctrl+W` - Close current tab
- `Ctrl+Q` - Exit application

## Working with Graphics

### ⚠️ Important to Understand
**Palettes are NOT stored in .NES files!** CHR ROM contains only color indices (0-3), and palettes are loaded by the game at runtime. Therefore, **Greyscale** is used by default - an honest way to show ROM contents.

### Features
- **Palette**: Default **Greyscale** (honest approach). Optionally - 9 color palettes for visualization
- **Transparency**: Checkbox to display index 0 as transparent (with checkerboard background)
- **Zoom**: +/- buttons or mouse wheel (in development)
- **Scrolling**: Use scrollbars or mouse wheel
- **Export tile sheet**: Saves all tiles to a single PNG file with selected palette
- **Export separately**: Creates a folder with PNG files for each tile

### Available Palettes
1. **Greyscale** ✅ (default, honest approach) - shows indices 0-3 as shades of gray
2. **Neutral** - gray-white from NES palette
3. **Blue** - blue shades
4. **Red** - red shades
5. **Green** - green shades
6. **Yellow** - yellow shades
7. **Purple** - purple shades
8. **Cyan** - cyan shades
9. **Orange** - orange shades
10. **Rainbow** - multicolor

## Development

The project uses:
- **Avalonia UI 11.x** for cross-platform UI
- **Fluent Design System** with custom styles
- **MVVM pattern** with CommunityToolkit.Mvvm
- **SkiaSharp** for graphics processing
- **xUnit** for unit testing
- **C# 12** and **.NET 8.0**

### UI Features
- ✨ Volumetric tabs with contrasting backgrounds and rounded corners
- ✨ Modern design with smooth animations
- 🎨 Adaptive theme (light/dark)
- 🖱️ Interactive elements with hover effects
- ⌨️ Full keyboard navigation support
- 💡 Tooltips for all actions

## License

TBD
