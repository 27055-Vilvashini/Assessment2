using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Assessment2.Repository
{
    internal class LogRepository
    {
        private readonly string _logFilePath;
        private readonly object _lockFile = new();

        public LogRepository()
        {
            string directory = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
            _logFilePath = Path.Combine(directory, "Boiler Log.txt");
        }

        public void EnsureLogFileExists()
        {
            try
            {
                lock (_lockFile)
                {
                    if (!File.Exists(_logFilePath))
                    {
                        using (FileStream fileStream = new FileStream(_logFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                        using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            writer.WriteLine("Timestamp,Event,Event Data");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring log file: {ex.Message}");
            }
        }

        public void WriteLogEntry(string eventName, string eventData)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
                string csvLine = $"{EscapeCsvField(timestamp)},{EscapeCsvField(eventName)},{EscapeCsvField(eventData)}";

                lock (_lockFile)
                {
                    File.AppendAllText(_logFilePath, csvLine + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log entry: {ex.Message}");
            }
        }

        public string[] GetAllLogEntries()
        {
            lock (_lockFile)
            {
                if (!File.Exists(_logFilePath))
                {
                    return Array.Empty<string>();
                }

                return File.ReadAllLines(_logFilePath, Encoding.UTF8);
            }
        }

        private static string EscapeCsvField(string? fieldValue)
        {
            if (string.IsNullOrEmpty(fieldValue))
            {
                return "\"\"";
            }

            string escaped = fieldValue.Replace("\"", "\"\"");

            if (escaped.Contains(",") || escaped.Contains("\"") || escaped.Contains("\n") || escaped.Contains("\r"))
            {
                return $"\"{escaped}\"";
            }

            return escaped;
        }
    }
}