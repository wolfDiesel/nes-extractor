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

