# Changelog

All notable changes to the NesExtractor project will be documented in this file.

## [Unreleased]

### Added
- Internationalization using .resx resources (default: English)
- Localization markup `{l:Loc Key}` and `LocalizationManager`
- Localized main views (`MainWindow`, `FileTabView`) and graphics export dialogs
- Russian translation (`Strings.ru.resx`) added
- Language switching in Help menu (English/Russian)
- About dialog with localized content
- Comprehensive test coverage: 87 tests (was 14)
  - NesTile tests (12 tests)
  - ChrRomExtractor tests (12 tests)
  - NesPalette tests (10 tests)
  - Additional NesHeader tests (13 tests)
  - Additional NesRom tests (7 tests)
  - Additional NesRomParser tests (11 tests)
- Release build profiles for Linux and Windows (single-file)
- GitHub Actions workflow for automatic release builds
- Code cleanup: removed Russian comments, replaced hex literals with binary constants

### Implemented
- NES header reading (16 bytes)
- PRG ROM extraction (program code)
- CHR ROM extraction (graphics data)
- Trainer section support (512 bytes)
- Mapper type detection (NROM, MMC1, UxROM, etc.)
- Mirroring type detection (Horizontal/Vertical/FourScreen)
- iNES 1.0 and NES 2.0 format support

### Graphics Visualization
- ✅ CHR ROM graphics visualization
- ✅ Tile extraction and decoding (8×8 pixels, 2 bitplanes)
- ✅ Tile sheet display
- ✅ Zoom (0.1x - 10x) with +/-/100% buttons
- ✅ Tile sheet scrolling (ScrollViewer)
- ✅ Configurable spacing between tiles (0-4px)
- ✅ Tile sheet export to PNG file
- ✅ Export all tiles to separate files
- ✅ Greyscale palette (4 shades)
- ✅ Tile count information
- ✅ Nearest neighbor interpolation (no blur)

### Design Improvements
- ✅ Custom tab styles (TabStyles.axaml)
- ✅ Beautiful tab close button with hover effect
- ✅ Accent color bottom border for active tab
- ✅ Smooth transition animations (150ms)
- ✅ Enhanced export buttons with hover effects
- ✅ Styled zoom buttons
- ✅ Updated Empty State with welcome message
- ✅ Information panel with features
- ✅ Tooltips for all interactive elements
- ✅ Unified spacing and color system
- ✅ System theme adaptation (light/dark)
- ✅ Volumetric tabs with background and rounded corners
- ✅ Contrasting bar under tabs
- ✅ Active tab "protrudes" from panel (-1px margin)
- ✅ Fixed panel stretching in FileTabView (30:70)

### Palettes and Transparency
- ✅ Greyscale by default (honest way to show CHR ROM)
- ✅ Standard NES palette (64 colors PPU 2C02)
- ✅ 10 preset palettes for visualization
- ✅ "Transparency" checkbox for index 0
- ✅ Checkerboard background when transparency is enabled (like in Photoshop)
- ✅ ComboBox for palette selection in UI
- ✅ Automatic redraw on palette change
- ✅ Export with selected palette and transparency settings
- ⚠️ **Important**: Palettes are NOT stored in .NES files (like in YY-CHR)

### In Development
- Palette editing
- Individual tile selection
- Mouse wheel zoom

## [0.1.0] - 2025-11-16

### Initial Release
- Project initialization
- Basic Avalonia application structure
- Solution setup (src/, tests/)
- Documentation creation (TZ/)
