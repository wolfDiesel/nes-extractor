using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NesExtractor.Core.Models;
using NesExtractor.Core.Services;
using SkiaSharp;
using NesExtractor.Localization;

namespace NesExtractor.ViewModels;

public partial class GraphicsViewModel : ViewModelBase
{
    private readonly FileTabViewModel _parentTab;

    [ObservableProperty]
    private Bitmap? _tileSheetImage;

    [ObservableProperty]
    private List<NesTile>? _tiles;

    [ObservableProperty]
    private double _zoom = 1.0;

    [ObservableProperty]
    private int _tilesPerRow = 16;

    [ObservableProperty]
    private int _tileScale = 2;

    [ObservableProperty]
    private int _spacing = 1;

    [ObservableProperty]
    private string _tileCountInfo = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private int _selectedPaletteIndex = 0;

    [ObservableProperty]
    private bool _useTransparency = false;

    public string[] PaletteNames => Core.Models.NesPalette.PaletteNames;

    public Window? ParentWindow { get; set; }

    public GraphicsViewModel(FileTabViewModel parentTab)
    {
        _parentTab = parentTab;
    }

    public async Task LoadGraphicsAsync()
    {
        if (_parentTab.Rom?.ChrRom == null || _parentTab.Rom.ChrRom.Length == 0)
            return;

        IsProcessing = true;

        try
        {
            await Task.Run(() =>
            {
                // Extract tiles
                Tiles = ChrRomExtractor.ExtractTiles(_parentTab.Rom.ChrRom);
                
                // Update tile count info (localized)
                var banks = Tiles.Count / 256.0;
                TileCountInfo = string.Format(LocalizationManager.GetString("Graphics.TileCount"), Tiles.Count, banks);

                // Create tile sheet
                RegenerateTileSheet();
            });
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void RegenerateTileSheet()
    {
        if (Tiles == null || Tiles.Count == 0)
            return;

        // Resolve selected palette considering transparency
        var palette = Core.Models.NesPalette.GetPalette(SelectedPaletteIndex, UseTransparency);

        // Build tile sheet with current settings
        using var skBitmap = ChrRomExtractor.CreateTileSheet(
            Tiles,
            TilesPerRow,
            TileScale,
            Spacing,
            palette,
            UseTransparency);

        // Convert SKBitmap to Avalonia Bitmap
        TileSheetImage = SKBitmapToAvaloniaBitmap(skBitmap);
    }

    partial void OnSelectedPaletteIndexChanged(int value)
    {
        RegenerateTileSheet();
    }

    partial void OnUseTransparencyChanged(bool value)
    {
        RegenerateTileSheet();
    }

    private static Bitmap SKBitmapToAvaloniaBitmap(SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Position = 0;
        return new Bitmap(stream);
    }

    [RelayCommand]
    private void ZoomIn()
    {
        Zoom = Math.Min(Zoom * 1.2, 10.0);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Zoom = Math.Max(Zoom / 1.2, 0.1);
    }

    [RelayCommand]
    private void ResetZoom()
    {
        Zoom = 1.0;
    }

    [RelayCommand]
    private void IncreaseTileScale()
    {
        TileScale = Math.Min(TileScale + 1, 8);
        RegenerateTileSheet();
    }

    [RelayCommand]
    private void DecreaseTileScale()
    {
        TileScale = Math.Max(TileScale - 1, 1);
        RegenerateTileSheet();
    }

    [RelayCommand]
    private void IncreaseSpacing()
    {
        Spacing = Math.Min(Spacing + 1, 4);
        RegenerateTileSheet();
    }

    [RelayCommand]
    private void DecreaseSpacing()
    {
        Spacing = Math.Max(Spacing - 1, 0);
        RegenerateTileSheet();
    }

    [RelayCommand]
    private void ChangeTilesPerRow(int delta)
    {
        TilesPerRow = Math.Clamp(TilesPerRow + delta, 8, 32);
        RegenerateTileSheet();
    }

    [RelayCommand]
    private async Task ExportTileSheetAsync()
    {
        if (Tiles == null || Tiles.Count == 0 || ParentWindow == null)
            return;

        try
        {
            var file = await ParentWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = LocalizationManager.GetString("Export.TileSheet.DialogTitle"),
                DefaultExtension = "png",
                SuggestedFileName = $"{_parentTab.FileName}_tiles.png",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType(LocalizationManager.GetString("Export.TileSheet.FileType")) { Patterns = new[] { "*.png" } }
                }
            });

            if (file != null)
            {
                IsProcessing = true;

                await Task.Run(() =>
                {
                    var palette = Core.Models.NesPalette.GetPalette(SelectedPaletteIndex, UseTransparency);
                    using var skBitmap = ChrRomExtractor.CreateTileSheet(
                        Tiles,
                        TilesPerRow,
                        TileScale,
                        Spacing,
                        palette,
                        UseTransparency);
                    
                    ChrRomExtractor.ExportTileSheet(skBitmap, file.Path.LocalPath);
                });

                // Success notification can be shown here if needed
            }
        }
        catch (Exception ex)
        {
            // Error handling
            Console.WriteLine(string.Format(LocalizationManager.GetString("Error.Export"), ex.Message));
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ExportIndividualTilesAsync()
    {
        if (Tiles == null || Tiles.Count == 0 || ParentWindow == null)
            return;

        try
        {
            var folder = await ParentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = LocalizationManager.GetString("Export.Tiles.DialogTitle"),
                AllowMultiple = false
            });

            if (folder.Count > 0)
            {
                IsProcessing = true;

                await Task.Run(() =>
                {
                    string directory = folder[0].Path.LocalPath;
                    string tilesDirectory = Path.Combine(directory, $"{_parentTab.FileName}_tiles");
                    var palette = Core.Models.NesPalette.GetPalette(SelectedPaletteIndex, UseTransparency);
                    
                    ChrRomExtractor.ExportTilesIndividually(
                        Tiles,
                        tilesDirectory,
                        tileScale: 4,
                        palette: palette); // Больший масштаб для отдельных файлов
                });

                // Success notification can be shown here if needed
            }
        }
        catch (Exception ex)
        {
            // Error handling
            Console.WriteLine(string.Format(LocalizationManager.GetString("Error.Export"), ex.Message));
        }
        finally
        {
            IsProcessing = false;
        }
    }
}

