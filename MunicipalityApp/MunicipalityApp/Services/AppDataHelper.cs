using System;
using System.IO;

namespace MunicipalityApp.Helpers
{
    /// <summary>
    /// Helper class to manage a centralized App_Data folder.
    /// Ensures all JSON files are stored in MunicipalityApp\App_Data.
    /// </summary>
    public static class AppDataHelper
    {
        private static readonly string _appDataPath;

        static AppDataHelper()
        {
            // Determine project root folder.
            // Assumes running from bin/Debug/netX.0
            var baseDir = AppContext.BaseDirectory;

            // Navigate up to project root (3 levels above bin)
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

            // App_Data folder at project root
            _appDataPath = Path.Combine(projectRoot, "App_Data");

            // Ensure directory exists
            if (!Directory.Exists(_appDataPath))
            {
                Directory.CreateDirectory(_appDataPath);
            }
        }

        /// <summary>
        /// Returns the full path for a given JSON file inside App_Data.
        /// </summary>
        /// <param name="fileName">Name of the file (e.g., events.json)</param>
        /// <returns>Absolute path to the file in App_Data</returns>
        public static string GetFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));

            return Path.Combine(_appDataPath, fileName);
        }

        /// <summary>
        /// Ensures a JSON file exists with empty array content if missing.
        /// </summary>
        /// <param name="fileName">Name of the file (e.g., events.json)</param>
        public static void EnsureJsonFile(string fileName)
        {
            var path = GetFilePath(fileName);

            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                File.WriteAllText(path, "[]"); // empty list for JSON deserialization
            }
        }

    }
}
