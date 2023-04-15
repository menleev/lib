using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Получаем корневой диск системной директории
                var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);

                // Создаем папку AkebiPrivate на корневом диске системной директории
                var dirPath = Path.Combine(systemDrive, "AkebiPrivate");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // Проверяем наличие .exe файлов и удаляем их
                var exeFiles = Directory.GetFiles(dirPath, "*.exe");
                if (exeFiles.Length > 0)
                {
                    foreach (var exeFile in exeFiles)
                    {
                        File.Delete(exeFile);
                    }
                }

                // Загружаем архив по ссылке
                var client = new WebClient();
                var url = "https://raw.githubusercontent.com/menleev/lib/main/AkebiPrivate";
                var zipUrl = client.DownloadString(url);
                var zipPath = Path.Combine(dirPath, "AkebiPrivate.zip");
                client.DownloadFile(zipUrl, zipPath);

                // Распаковываем архив
                ZipFile.ExtractToDirectory(zipPath, dirPath);

                // Удаляем архив
                File.Delete(zipPath);

                // Скачиваем два файла без распаковки
                client.DownloadFile("https://github.com/menleev/lib/raw/main/symsrv.dll", Path.Combine(dirPath, "symsrv.dll"));
                client.DownloadFile("https://github.com/menleev/lib/raw/main/msdia140.dll", Path.Combine(dirPath, "msdia140.dll"));

                // Запрашиваем у пользователя ключ и сохраняем его в файл
                Console.Write("KEY ADD: ");
                var licenseKey = Console.ReadLine()?.Trim().Replace(" ", "");
                // Создаем новый json-файл с рандомным именем
                // Проверяем наличие json-файлов в текущей папке
                var jsonFiles = Directory.GetFiles(dirPath, "*.json");
                if (jsonFiles.Length > 0)
                {
                    // Создаем новый json-файл с рандомным именем
                    var jsonFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".json";
                    var jsonPath = Path.Combine(dirPath, jsonFileName);

                    // Записываем в новый файл данные по умолчанию
                    var json = new
                    {
                        current_profile = "default",
                        profiles = new Dictionary<string, object>
                        {
                            ["default"] = new
                            {
                                Auth = new
                                {
                                    licenseKey = string.IsNullOrEmpty(licenseKey) ? "" : licenseKey
                                }
                            }
                        }
                    };
                    File.WriteAllText(jsonPath, JsonConvert.SerializeObject(json));
                }
                else
                {
                    // Создаем новый json-файл с именем "cfg.json"
                    var json = new
                    {
                        current_profile = "default",
                        profiles = new Dictionary<string, object>
                        {
                            ["default"] = new
                            {
                                Auth = new
                                {
                                    licenseKey = string.IsNullOrEmpty(licenseKey) ? "" : licenseKey
                                }
                            }
                        }
                    };
                    var jsonPath = Path.Combine(dirPath, "cfg.json");
                    File.WriteAllText(jsonPath, JsonConvert.SerializeObject(json));
                }

                // Открываем папку в проводнике после выполнения программы
                var psi = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = dirPath,
                    UseShellExecute = true
                };
                Process.Start(psi);

                Console.WriteLine("Завершено.");
            }
            catch (Exception ex)
            {
                // Глушим все ошибки
                Console.WriteLine("Произошла ошибка: " + ex.Message);
            }
        }
    }
}
