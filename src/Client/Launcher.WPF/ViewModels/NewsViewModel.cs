using Launcher.ServiceLib.Contracts;
using Launcher.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Launcher.WPF.ViewModels
{
    public class NewsViewModel : INotifyPropertyChanged
    {
        private readonly WcfClientService _service;
        private NewsItemDto _selectedNews;
        private bool _isLoading;

        public ObservableCollection<NewsItemDto> NewsItems { get; } = new ObservableCollection<NewsItemDto>();

        public NewsItemDto SelectedNews
        {
            get => _selectedNews;
            set
            {
                if (_selectedNews != value)
                {
                    _selectedNews = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public NewsViewModel(WcfClientService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            LoadNews();
        }

        public void LoadNews()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;
                var newsResponse = _service.GetNews();

                NewsItems.Clear();

                if (newsResponse?.News != null)
                {
                    foreach (var news in newsResponse.News)
                    {
                        NewsItems.Add(news);
                    }
                }

                if (NewsItems.Count > 0)
                {
                    SelectedNews = NewsItems[0];
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки новостей: {ex.Message}");
                // Можно добавить логирование или уведомление пользователя
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void RefreshNews()
        {
            LoadNews();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}