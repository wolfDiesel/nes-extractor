using Avalonia.Controls;
using NesExtractor.ViewModels;

namespace NesExtractor.Views;

public partial class FileTabView : UserControl
{
    public FileTabView()
    {
        InitializeComponent();
        
        // Передаем ссылку на главное окно в GraphicsViewModel когда DataContext установлен
        DataContextChanged += (sender, args) =>
        {
            if (DataContext is FileTabViewModel fileTab && 
                fileTab.Graphics != null)
            {
                // Находим родительское Window
                var window = TopLevel.GetTopLevel(this) as Window;
                fileTab.Graphics.ParentWindow = window;
            }
        };
    }
}

