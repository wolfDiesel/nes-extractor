using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NesExtractor.Core.Models;

namespace NesExtractor.ViewModels;

public partial class FileTabViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private NesRom? _rom;

    [ObservableProperty]
    private GraphicsViewModel? _graphics;

    // Вычисляемые свойства для отображения информации

    public string FileSize => Rom != null ? $"{Rom.TotalFileSize:N0} байт ({Rom.TotalFileSize / 1024.0:F2} КБ)" : "—";

    public string Format => Rom?.Header.Format.ToString() ?? "—";

    public string PrgRomInfo => Rom != null 
        ? $"{Rom.Header.PrgRomSize} x 16 КБ = {Rom.Header.PrgRomSizeInBytes / 1024} КБ" 
        : "—";

    public string ChrRomInfo => Rom != null 
        ? $"{Rom.Header.ChrRomSize} x 8 КБ = {Rom.Header.ChrRomSizeInBytes / 1024} КБ" 
        : "—";

    public string MapperInfo => Rom != null 
        ? $"#{Rom.Header.MapperNumber} ({Rom.Header.GetMapperName()})" 
        : "—";

    public string Mirroring => Rom?.Header.Mirroring.ToString() ?? "—";

    public string BatteryRam => Rom != null 
        ? (Rom.Header.HasBatteryBackedRam ? "Да" : "Нет") 
        : "—";

    public string Trainer => Rom != null 
        ? (Rom.Header.HasTrainer ? "Присутствует (512 байт)" : "Отсутствует") 
        : "—";

    public bool HasChrRom => Rom != null && Rom.ChrRom.Length > 0;

    partial void OnRomChanged(NesRom? value)
    {
        // Обновляем все вычисляемые свойства при изменении ROM
        OnPropertyChanged(nameof(FileSize));
        OnPropertyChanged(nameof(Format));
        OnPropertyChanged(nameof(PrgRomInfo));
        OnPropertyChanged(nameof(ChrRomInfo));
        OnPropertyChanged(nameof(MapperInfo));
        OnPropertyChanged(nameof(Mirroring));
        OnPropertyChanged(nameof(BatteryRam));
        OnPropertyChanged(nameof(Trainer));
        OnPropertyChanged(nameof(HasChrRom));

        // Инициализируем графику если есть CHR ROM
        if (HasChrRom)
        {
            Graphics = new GraphicsViewModel(this);
            _ = Graphics.LoadGraphicsAsync(); // Загружаем асинхронно
        }
    }
}

