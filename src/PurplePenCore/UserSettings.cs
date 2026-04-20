using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PurplePen
{
    // This class holds user settings. To add new settings, just add in the list of public
    // properties, and be sure to initialize to the correct default value.
    // Settings are saved in JSON format.
    public class UserSettings
    {
        public string UILanguage;
        public string UITheme = "System";
        public string LastLoadedFile;
        public float MapIntensity = 0.7F;
        public bool MapHighQuality = true;
        public bool ShowPopupInfo = true;
        public Guid ClientId = Guid.NewGuid();
        public bool ViewAllControls = false;
        public bool ShowPrintArea = true;
        public string DefaultDescriptionLanguage;
        public string NewEventMapStandard = "2017";
        public string NewEventDescriptionStandard = "2018";
        public string LiveloxSettings;
        public List<string> RecentFiles = new List<string>();

        // Cache for event titles extracted from .ppen files (file path -> title)
        private static Dictionary<string, string> titleCache = new Dictionary<string, string>();

        // Clear the title cache for a specific file
        public static void ClearTitleCache(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
                titleCache.Remove(filePath);
        }

        // Clear all title cache entries
        public static void ClearAllTitleCache()
        {
            titleCache.Clear();
        }

        public static UserSettings Current;

        public static string SettingsPath { get; private set; }

        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
            IncludeFields = true,
            WriteIndented = true
        };

        // Extract event title from a .ppen file without fully loading it
        public static string GetEventTitle(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return Path.GetFileNameWithoutExtension(filePath);

            // Check cache first
            if (titleCache.TryGetValue(filePath, out var cachedTitle))
                return cachedTitle;

            try {
                // Read only the first 10KB to find the title tag
                using (var stream = File.OpenRead(filePath)) {
                    byte[] buffer = new byte[10240];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string content = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Find title in format: <title>Event Name</title>
                    var match = System.Text.RegularExpressions.Regex.Match(content, @"<title>(.*?)</title>");
                    if (match.Success && match.Groups.Count > 1) {
                        string title = match.Groups[1].Value.Trim();
                        if (!string.IsNullOrEmpty(title)) {
                            titleCache[filePath] = title;
                            return title;
                        }
                    }
                }
            }
            catch {
                // If any error reading the file, fall back to filename
            }

            // Fallback to filename without extension
            string fallback = Path.GetFileNameWithoutExtension(filePath);
            titleCache[filePath] = fallback;
            return fallback;
        }

        // Add a file to the recent files list. Removes duplicates and keeps only the last 10.
        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            // Remove if it already exists (to move it to the top)
            RecentFiles.Remove(filePath);

            // Add to the beginning
            RecentFiles.Insert(0, filePath);

            // Keep only the last 10 files
            if (RecentFiles.Count > 10)
                RecentFiles.RemoveRange(10, RecentFiles.Count - 10);
        }

        // Save the settings to the path used in Initialize.
        public void Save()
        {
            Debug.Assert(SettingsPath != null, "Initialize hasn't been called yet.");

            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            var json = JsonSerializer.Serialize(this, jsonOptions);
            File.WriteAllText(SettingsPath, json);
        }

        // Initialize the user settings, setting them into "UserSettings.Current". If the
        // file given doesn't exist, then default settings are used. If the file does exist, but
        // can't be loaded, it is deleted and default settings are used.
        public static void Initialize(string pathName)
        {
            Debug.Assert(Current == null, "Should only call Initialize once.");

            SettingsPath = pathName;
            try {
                if (File.Exists(SettingsPath)) {
                    var json = File.ReadAllText(SettingsPath);
                    Current = JsonSerializer.Deserialize<UserSettings>(json, jsonOptions) ?? new UserSettings();
                }
                else {
                    Current = new UserSettings();
                }
            } catch {
                // use default.
                Current = new UserSettings();
            }
        }

       
    }
}
