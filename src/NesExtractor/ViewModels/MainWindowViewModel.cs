using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NesExtractor.Core.Parsers;
using NesExtractor.Localization;
using System.Globalization;
using NesExtractor.Views;
using System.Text.Json;
using NesExtractor.Core.Models;
using NesExtractor.Core.Services;
using SkiaSharp;

namespace NesExtractor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "NesExtractor";

    [ObservableProperty]
    private ObservableCollection<FileTabViewModel> _tabs = new();

    [ObservableProperty]
    private FileTabViewModel? _selectedTab;

    [ObservableProperty]
    private string? _errorMessage;

    public Window? MainWindow { get; set; }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        if (MainWindow == null)
            return;

        try
        {
            var storageProvider = MainWindow.StorageProvider;
            // Open file dialog options
            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = Localization.LocalizationManager.GetString("Dialog.OpenFile.Title"),
                AllowMultiple = true,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType(Localization.LocalizationManager.GetString("Dialog.OpenFile.Filter.Nes"))
                    {
                        Patterns = new[] { "*.nes" }
                    },
                    new FilePickerFileType(Localization.LocalizationManager.GetString("Dialog.OpenFile.Filter.All"))
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            };

            // Open dialog
            var files = await storageProvider.OpenFilePickerAsync(filePickerOptions);

            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    await LoadNesFileAsync(file.Path.LocalPath);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(Localization.LocalizationManager.GetString("Error.OpenFile"), ex.Message);
        }
    }

    private async Task LoadNesFileAsync(string filePath)
    {
        try
        {
            ErrorMessage = null;
            // Avoid duplicate tabs
            var existingTab = Tabs.FirstOrDefault(t => t.FilePath == filePath);
            if (existingTab != null)
            {
                SelectedTab = existingTab;
                return;
            }

            // Parse NES file
            var rom = await NesRomParser.ParseFileAsync(filePath);

            // Add new tab
            var tabViewModel = new FileTabViewModel
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                Rom = rom
            };

            Tabs.Add(tabViewModel);
            SelectedTab = tabViewModel;
        }
        catch (FileNotFoundException)
        {
            ErrorMessage = string.Format(Localization.LocalizationManager.GetString("Error.FileNotFound"), filePath);
        }
        catch (InvalidDataException ex)
        {
            ErrorMessage = string.Format(Localization.LocalizationManager.GetString("Error.InvalidFormat"), ex.Message);
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(Localization.LocalizationManager.GetString("Error.LoadFile"), ex.Message);
        }
    }

    [RelayCommand]
    private void CloseTab(FileTabViewModel? tab)
    {
        if (tab != null && Tabs.Contains(tab))
        {
            var index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);

            // Select neighbor tab
            if (Tabs.Count > 0)
            {
                SelectedTab = Tabs[Math.Max(0, index - 1)];
            }
        }
    }

    [RelayCommand]
    private void CloseCurrentTab()
    {
        if (SelectedTab != null)
        {
            CloseTab(SelectedTab);
        }
    }

    [RelayCommand]
    private void Exit()
    {
        if (MainWindow != null)
        {
            MainWindow.Close();
        }
    }

    [RelayCommand]
    private void ClearError()
    {
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task BatchExportAsync()
    {
        if (MainWindow == null)
            return;

        if (Tabs.Count == 0)
        {
            ErrorMessage = LocalizationManager.GetString("Error.NoFiles");
            return;
        }

        try
        {
            var storageProvider = MainWindow.StorageProvider;
            var folder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = LocalizationManager.GetString("Export.Batch.DialogTitle"),
                AllowMultiple = false
            });

            if (folder.Count == 0)
                return;

            string exportDirectory = folder[0].Path.LocalPath;

            await Task.Run(() =>
            {
                int total = Tabs.Count;
                int current = 0;

                foreach (var tab in Tabs)
                {
                    current++;
                    // Update title to show progress (optional, can be removed if not needed)
                    // Title = string.Format(LocalizationManager.GetString("Export.Batch.Processing"), current, total);

                    if (tab.Rom == null || tab.Rom.ChrRom == null || tab.Rom.ChrRom.Length == 0)
                        continue;

                    ExportRomBatch(tab, exportDirectory);
                }
            });

            // Success notification
            ErrorMessage = null; // Clear any previous errors
            // Could show success message here if needed
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(LocalizationManager.GetString("Error.BatchExport"), ex.Message);
        }
    }

    private void ExportRomBatch(FileTabViewModel tab, string baseDirectory)
    {
        if (tab.Rom == null || string.IsNullOrEmpty(tab.FileName))
            return;

        // Get ROM name without extension
        string romName = Path.GetFileNameWithoutExtension(tab.FileName);
        string romDirectory = Path.Combine(baseDirectory, romName);
        
        if (!Directory.Exists(romDirectory))
            Directory.CreateDirectory(romDirectory);

        // Extract tiles
        var tiles = ChrRomExtractor.ExtractTiles(tab.Rom.ChrRom);
        if (tiles.Count == 0)
            return;

        // Use greyscale palette with transparency for batch export
        var palette = NesPalette.GetPalette(0, transparent: true); // Greyscale with transparency

        // Export individual tiles: имя_рома_000.png, имя_рома_001.png, etc.
        for (int i = 0; i < tiles.Count; i++)
        {
            var bitmap = ChrRomExtractor.TileToBitmap(tiles[i], palette, scale: 4);
            string fileName = Path.Combine(romDirectory, $"{romName}_{i:D3}.png");
            
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(fileName);
            data.SaveTo(stream);
            
            bitmap.Dispose();
        }

        // Export full tile sheet: имя_рома_full.png (with transparency)
        using var fullSheet = ChrRomExtractor.CreateTileSheet(
            tiles,
            tilesPerRow: 16,
            tileScale: 2,
            spacing: 1,
            palette: palette,
            useTransparency: true);

        string fullSheetPath = Path.Combine(romDirectory, $"{romName}_full.png");
        ChrRomExtractor.ExportTileSheet(fullSheet, fullSheetPath);

        // Export JSON with ROM information
        ExportRomJson(tab.Rom, romDirectory, romName);
    }

    private void ExportRomJson(NesRom rom, string directory, string romName)
    {
        var jsonData = new
        {
            fileName = rom.FileName,
            filePath = rom.FilePath,
            format = rom.Header.Format.ToString(),
            mapper = new
            {
                number = rom.Header.MapperNumber,
                name = rom.Header.GetMapperName()
            },
            prgRom = new
            {
                banks = rom.Header.PrgRomSize,
                sizeBytes = rom.Header.PrgRomSizeInBytes
            },
            chrRom = new
            {
                banks = rom.Header.ChrRomSize,
                sizeBytes = rom.Header.ChrRomSizeInBytes,
                tileCount = rom.ChrRom.Length / NesTile.TileSizeInBytes
            },
            mirroring = rom.Header.Mirroring.ToString(),
            hasBatteryBackedRam = rom.Header.HasBatteryBackedRam,
            hasTrainer = rom.Header.HasTrainer,
            trainerSize = rom.Header.TrainerSizeInBytes,
            totalFileSize = rom.TotalFileSize,
            isValid = rom.IsValid
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(jsonData, options);
        string jsonPath = Path.Combine(directory, $"{romName}.json");
        File.WriteAllText(jsonPath, json);
    }

    [RelayCommand]
    private async Task ShowAboutAsync()
    {
        if (MainWindow == null)
            return;

        var about = new AboutWindow();
        await about.ShowDialog(MainWindow);
    }

    [RelayCommand]
    private void SetLanguageEnglish()
    {
        ApplyLanguage("en");
    }

    [RelayCommand]
    private void SetLanguageRussian()
    {
        ApplyLanguage("ru");
    }

    private void ApplyLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        LocalizationManager.SetCulture(culture);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            var old = lifetime.MainWindow;

            var newWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            lifetime.MainWindow = newWindow;
            newWindow.Show();

            old?.Close();
        }
    }
}

