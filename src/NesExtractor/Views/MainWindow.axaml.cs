using Avalonia.Controls;
using NesExtractor.ViewModels;

namespace NesExtractor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Передаем ссылку на окно в ViewModel для работы с диалогами
        DataContextChanged += (sender, args) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MainWindow = this;
            }
        };
    }
}

