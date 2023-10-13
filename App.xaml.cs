using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkProgram
{
    public partial class App : Application
    {
        // Способ получить значения из JSON-файла
        private static string configFilename = "email-settings.json";
        private static JsonElement? settings = null;

        // В параметре указываем (например: "smtp:email")
        public static string? GetConfiguration(string name)
        {
            if (settings is null)
            {
                if (!File.Exists(configFilename))
                {
                    MessageBox.Show($"Файл конфигурации '{configFilename}' не найденно...",
                        "Операция не может быть выполнена", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            try { settings ??= JsonSerializer.Deserialize<dynamic>(File.ReadAllText(configFilename)); }
            catch
            {
                MessageBox.Show($"Файл конфигурации '{configFilename}' повреждён и не может быть прочитан...",
                        "Операция не может быть выполнена", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            JsonElement? jsonElement = settings;
            if (settings is not null)
            {
                try
                {
                    foreach (string key in name.Split(':'))  // цикл нужен, потому что в json может быть вложенность
                    {
                        jsonElement = jsonElement?.GetProperty(key);
                    }
                }
                catch { return null; }
                return jsonElement?.GetString();
            }
            return null;
        }
    }
}
