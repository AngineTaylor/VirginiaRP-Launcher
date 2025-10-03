using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher.WPF.Services
{
    public class DownloadManager : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public event Action<int> ProgressChanged;

        public DownloadManager()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(30)
            };
        }

        public DownloadManager(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task DownloadFileAsync(string url, string destinationPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL не может быть пустым", nameof(url));

            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Путь назначения не может быть пустым", nameof(destinationPath));

            HttpResponseMessage response = null;
            Stream contentStream = null;
            FileStream fileStream = null;

            try
            {
                response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                contentStream = await response.Content.ReadAsStreamAsync();
                fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                await CopyStreamWithProgressAsync(contentStream, fileStream, response.Content.Headers.ContentLength, cancellationToken);

                ProgressChanged?.Invoke(100);
            }
            catch (OperationCanceledException)
            {
                CleanupPartialFile(destinationPath);
                throw;
            }
            catch (Exception ex)
            {
                CleanupPartialFile(destinationPath);
                throw new DownloadException($"Ошибка загрузки файла: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
                contentStream?.Dispose();
                fileStream?.Dispose();
            }
        }

        private async Task CopyStreamWithProgressAsync(Stream source, Stream destination, long? totalBytes, CancellationToken cancellationToken)
        {
            var buffer = new byte[81920];
            long totalRead = 0;
            int read;

            while ((read = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer, 0, read, cancellationToken);
                totalRead += read;

                if (totalBytes.HasValue && totalBytes.Value > 0)
                {
                    int percent = (int)((totalRead * 100L) / totalBytes.Value);
                    ProgressChanged?.Invoke(Math.Min(percent, 99));
                }
                else
                {
                    int progress = (int)Math.Min(99, totalRead / 1024);
                    ProgressChanged?.Invoke(progress);
                }
            }
        }

        private void CleanupPartialFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Игнорируем ошибки удаления
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }

        ~DownloadManager()
        {
            Dispose(false);
        }
    }

    public class DownloadException : Exception
    {
        public DownloadException(string message) : base(message) { }
        public DownloadException(string message, Exception innerException) : base(message, innerException) { }
    }
}