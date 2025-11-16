using System;
using System.Globalization;
using System.Resources;

namespace NesExtractor.Localization;

public static class LocalizationManager
{
    private static readonly ResourceManager ResourceManager =
        new ResourceManager("NesExtractor.Resources.Strings", typeof(LocalizationManager).Assembly);

    public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

    public static void SetCulture(CultureInfo culture)
    {
        CurrentCulture = culture;
    }

    public static string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        try
        {
            var value = ResourceManager.GetString(key, CurrentCulture);
            return string.IsNullOrEmpty(value) ? key : value!;
        }
        catch
        {
            return key;
        }
    }
}

