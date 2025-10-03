using Launcher.ServiceLib.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Launcher.WPF.Services
{
    public class NewsService : IDisposable
    {
        private readonly string _newsFilePath = "news.json";
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;

        public NewsService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public NewsResponse GetNews()
        {
            try
            {
                if (!File.Exists(_newsFilePath))
                {
                    CreateSampleNews();
                    return GetNews(); // Рекурсивный вызов после создания файла
                }

                using (var fileStream = File.OpenRead(_newsFilePath))
                {
                    var newsResponse = JsonSerializer.Deserialize<NewsResponse>(fileStream, _jsonOptions);
                    return newsResponse ?? new NewsResponse();
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка загрузки новостей: {ex.Message}");
                return new NewsResponse();
            }
        }

        public async Task<NewsResponse> GetNewsAsync()
        {
            try
            {
                if (!File.Exists(_newsFilePath))
                {
                    await CreateSampleNewsAsync();
                    return await GetNewsAsync(); // Рекурсивный вызов после создания файла
                }

                using (var fileStream = File.OpenRead(_newsFilePath))
                {
                    var newsResponse = await JsonSerializer.DeserializeAsync<NewsResponse>(fileStream, _jsonOptions);
                    return newsResponse ?? new NewsResponse();
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка асинхронной загрузки новостей: {ex.Message}");
                return new NewsResponse();
            }
        }

        public bool SaveNews(NewsResponse news)
        {
            try
            {
                var json = JsonSerializer.Serialize(news, _jsonOptions);
                File.WriteAllText(_newsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Ошибка сохранения новостей: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveNewsAsync(NewsResponse news)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(_newsFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                await JsonSerializer.SerializeAsync(fileStream, news, _jsonOptions);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Ошибка асинхронного сохранения новостей: {ex.Message}");
                return false;
            }
            finally
            {
                fileStream?.Dispose();
            }
        }

        private void CreateSampleNews()
        {
            var sampleNews = CreateSampleNewsData();
            SaveNews(sampleNews);
        }

        private async Task CreateSampleNewsAsync()
        {
            var sampleNews = CreateSampleNewsData();
            await SaveNewsAsync(sampleNews);
        }

        private NewsResponse CreateSampleNewsData()
        {
            return new NewsResponse
            {
                News = new List<NewsItemDto>
                {
                    new NewsItemDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Emoji = "🎮",
                        Title = "Добро пожаловать в UNTRN RP!",
                        Summary = "Запуск нового ролевого сервера",
                        Content = "Мы рады приветствовать вас на нашем новом ролевом сервере UNTRN RP! Присоединяйтесь к нашему сообществу и создавайте уникальные истории вместе с другими игроками.",
                        PublishDate = DateTime.Now,
                        Author = "Администрация"
                    },
                    new NewsItemDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Emoji = "⚡",
                        Title = "Обновление системы персонажей",
                        Summary = "Добавлены новые истории персонажей",
                        Content = "Теперь доступны 3 уникальные истории для ваших персонажей: Сын олигарха, Беспризорник и Мафиози. Каждая история предлагает уникальные возможности для ролевой игры.",
                        PublishDate = DateTime.Now.AddDays(-1),
                        Author = "Разработчики"
                    }
                }
            };
        }

        private void LogError(string message)
        {
            Console.WriteLine($"[NewsService] {message}");
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
                // Освобождение ресурсов если нужно
                _disposed = true;
            }
        }

        ~NewsService()
        {
            Dispose(false);
        }
    }
}