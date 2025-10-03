using Launcher.WPF.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Launcher.WPF.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _installPath = "";
        private bool _autoLogin = true;
        private bool _checkUpdates = true;
        private string _statusMessage = "";
        private Brush _statusBrush = Brushes.Transparent;
        private readonly string _configFilePath = "config.json";
        private readonly JsonSerializerOptions _jsonOptions;

        public string InstallPath
        {
            get => _installPath;
            set { _installPath = value; OnPropertyChanged(); }
        }

        public bool AutoLogin
        {
            get => _autoLogin;
            set { _autoLogin = value; OnPropertyChanged(); }
        }

        public bool CheckUpdates
        {
            get => _checkUpdates;
            set { _checkUpdates = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public Brush StatusBrush
        {
            get => _statusBrush;
            set { _statusBrush = value; OnPropertyChanged(); }
        }

        public RelayCommand BrowseCommand { get; }
        public RelayCommand VerifyCommand { get; }
        public RelayCommand ReinstallCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseCommand { get; }

        public SettingsViewModel()
        {
            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            BrowseCommand = new RelayCommand(OnBrowse);
            VerifyCommand = new RelayCommand(OnVerify);
            ReinstallCommand = new RelayCommand(OnReinstall);
            SaveCommand = new RelayCommand(OnSave);
            CloseCommand = new RelayCommand(OnClose);

            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<SettingsData>(json, _jsonOptions);

                    if (!string.IsNullOrEmpty(config?.InstallPath) && Directory.Exists(config.InstallPath))
                    {
                        InstallPath = config.InstallPath;
                        return;
                    }
                }

                InstallPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TurnedRolePlay");
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка загрузки настроек: {ex.Message}", Colors.OrangeRed);
                InstallPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TurnedRolePlay");
            }
        }

        private void OnBrowse()
        {
            try
            {
                using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true })
                {
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        InstallPath = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка выбора папки: {ex.Message}", Colors.OrangeRed);
            }
        }

        private void OnVerify()
        {
            if (string.IsNullOrWhiteSpace(InstallPath))
            {
                SetStatus("⚠️ Укажите путь к установке.", Colors.OrangeRed);
                return;
            }

            if (Directory.Exists(InstallPath))
            {
                string gameFile = Path.Combine(InstallPath, "gamefile.docx");
                if (File.Exists(gameFile))
                    SetStatus("✅ Файлы в порядке.", Colors.LightGreen);
                else
                    SetStatus("⚠️ Файл gamefile.docx отсутствует.", Colors.Orange);
            }
            else
            {
                SetStatus("❌ Указанная папка не найдена.", Colors.OrangeRed);
            }
        }

        private async void OnReinstall()
        {
            if (string.IsNullOrWhiteSpace(InstallPath))
            {
                SetStatus("⚠️ Укажите папку установки.", Colors.OrangeRed);
                return;
            }

            HttpClient httpClient = null;
            Stream contentStream = null;
            FileStream fileStream = null;

            try
            {
                Directory.CreateDirectory(InstallPath);
                string destFile = Path.Combine(InstallPath, "gamefile.docx");

                httpClient = new HttpClient();
                string url = "https://docs.google.com/document/d/11QxrhlqhYSNRi3zwonVI1A0RWk9_vxYEJyLkUdLSvS0/export?format=docx";

                SetStatus("📥 Загрузка...", Colors.Yellow);

                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    contentStream = await response.Content.ReadAsStreamAsync();
                    fileStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    var buffer = new byte[8192];
                    long totalRead = 0;
                    long? totalBytes = response.Content.Headers.ContentLength;
                    int read;

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        totalRead += read;

                        if (totalBytes.HasValue)
                        {
                            int progress = (int)((totalRead * 100) / totalBytes.Value);
                            SetStatus($"📥 Загрузка... {progress}%", Colors.Yellow);
                        }
                    }
                }

                SetStatus("✅ Файл скачан успешно!", Colors.LightGreen);
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Ошибка: {ex.Message}", Colors.OrangeRed);
            }
            finally
            {
                httpClient?.Dispose();
                contentStream?.Dispose();
                fileStream?.Dispose();
            }
        }

        private void OnSave()
        {
            if (string.IsNullOrWhiteSpace(InstallPath))
            {
                ShowMessage("Укажите корректный путь к установке.", "Ошибка", MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(InstallPath))
            {
                try
                {
                    Directory.CreateDirectory(InstallPath);
                }
                catch (Exception ex)
                {
                    ShowMessage($"Невозможно создать указанную директорию: {ex.Message}", "Ошибка", MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                var config = new SettingsData { InstallPath = InstallPath };
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_configFilePath, json);
                SetStatus("✅ Настройки сохранены.", Colors.LightGreen);
            }
            catch (Exception ex)
            {
                ShowMessage($"Не удалось сохранить настройки: {ex.Message}", "Ошибка", MessageBoxImage.Error);
                SetStatus("❌ Ошибка сохранения.", Colors.OrangeRed);
            }
        }

        private void OnClose()
        {
            OnSave();
            CloseWindow();
        }

        private void CloseWindow()
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }

        private void SetStatus(string message, Color color)
        {
            StatusMessage = message;
            StatusBrush = new SolidColorBrush(color);

            if (!string.IsNullOrEmpty(message))
            {
                Task.Delay(5000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (StatusMessage == message)
                            StatusMessage = "";
                    });
                });
            }
        }

        private void ShowMessage(string message, string title, MessageBoxImage image)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class SettingsData
        {
            public string InstallPath { get; set; } = string.Empty;
        }
    }
}