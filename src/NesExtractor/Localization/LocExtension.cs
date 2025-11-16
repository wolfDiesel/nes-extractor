using System;
using Avalonia.Markup.Xaml;

namespace NesExtractor.Localization;

public class LocExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public LocExtension()
    {
    }

    public LocExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return LocalizationManager.GetString(Key);
    }
}

