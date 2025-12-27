using System;
using System.IO;
using System.Text.Json;

namespace SmartFolder.Core
{
    public enum MultiSelectStrategy
    {
        UseFirstItemName,
        AlwaysPrompt
    }

    public enum PropertyGroupingType
    {
        None,
        Extension,
        Author,
        CameraModel
    }

    public class SmartFolderSettings
    {
        public MultiSelectStrategy MultiSelectStrategy { get; set; } = MultiSelectStrategy.UseFirstItemName;

        public bool EnableGrouping { get; set; } = false;

        public bool GroupByYear { get; set; } = false;
        public bool GroupByMonth { get; set; } = false;
        public bool GroupByDay { get; set; } = false;

        public PropertyGroupingType SecondaryGrouping { get; set; } = PropertyGroupingType.None;

        private static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SmartFolder",
            "settings.json");

        public static SmartFolderSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<SmartFolderSettings>(json) ?? new SmartFolderSettings();
                }
            }
            catch
            {
                // Ignore errors and return default
            }
            return new SmartFolderSettings();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Handle save errors (maybe log?)
            }
        }
    }
}
