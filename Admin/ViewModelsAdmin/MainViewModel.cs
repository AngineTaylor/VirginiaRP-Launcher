using Admin.ServicesAdmin;
using Admin.ViewsAdmin;
using Launcher.ServiceLib.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Admin.ViewModelsAdmin
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DbManager _dbManager;
        private string _searchText;
        private DataGrid _dataGrid;

        public void SetDataGrid(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
        }

        private readonly ColumnSelectorViewModel _columnSelectorViewModel = new ColumnSelectorViewModel();

        public MainViewModel()
        {
            _dbManager = new DbManager(Database.ConnectionString);

            ToggleExtraInfoCommand = new RelayCommand<object>(_ => ShowColumnSelector());
            PlayerDoubleClickCommand = new RelayCommand<AdminCharacterViewModel>(OnPlayerDoubleClick);

            LoadCharactersFromDatabase();
            InitializeSortOptions();
        }

        public ObservableCollection<AdminCharacterViewModel> Characters { get; } = new ObservableCollection<AdminCharacterViewModel>();
        public ICollectionView CharactersView => CollectionViewSource.GetDefaultView(Characters);

        public ObservableCollection<DataGridColumn> ExtraColumns { get; } = new ObservableCollection<DataGridColumn>();

        public RelayCommand<object> ToggleExtraInfoCommand { get; }
        public RelayCommand<AdminCharacterViewModel> PlayerDoubleClickCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public class SortOption
        {
            public string DisplayName { get; set; }
            public string PropertyName { get; set; }
            public ListSortDirection Direction { get; set; }
        }

        public List<SortOption> SortOptions { get; set; } = new List<SortOption>();
        private SortOption _selectedSortOption;
        public SortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged();
                ApplySort();
            }
        }

        private void InitializeSortOptions()
        {
            SortOptions = new List<SortOption>
            {
                new SortOption { DisplayName = "ID ↑", PropertyName = "Id", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "ID ↓", PropertyName = "Id", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "ShortId ↑", PropertyName = "ShortId", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "ShortId ↓", PropertyName = "ShortId", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "Имя ↑", PropertyName = "Nickname", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "Имя ↓", PropertyName = "Nickname", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "Story ↑", PropertyName = "Story", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "Story ↓", PropertyName = "Story", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "CreatedAt ↑", PropertyName = "CreatedAt", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "CreatedAt ↓", PropertyName = "CreatedAt", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "RegIp ↑", PropertyName = "RegIp", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "RegIp ↓", PropertyName = "RegIp", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "Age ↑", PropertyName = "Age", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "Age ↓", PropertyName = "Age", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "SteamId64 ↑", PropertyName = "SteamId64", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "SteamId64 ↓", PropertyName = "SteamId64", Direction = ListSortDirection.Descending },
                new SortOption { DisplayName = "Онлайн ↑", PropertyName = "IsOnline", Direction = ListSortDirection.Ascending },
                new SortOption { DisplayName = "Онлайн ↓", PropertyName = "IsOnline", Direction = ListSortDirection.Descending }
            };

            if (SortOptions.Count > 0)
                SelectedSortOption = SortOptions[0];
        }

        private void ShowColumnSelector()
        {
            var window = new ColumnSelectorWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = _columnSelectorViewModel
            };

            void OnApply()
            {
                window.DialogResult = true;
                window.Close();
            }

            void OnCancel()
            {
                window.DialogResult = false;
                window.Close();
            }

            _columnSelectorViewModel.ApplyRequested += OnApply;
            _columnSelectorViewModel.CancelRequested += OnCancel;

            try
            {
                if (window.ShowDialog() == true)
                {
                    // Передаём все булевы флаги
                    UpdateExtraColumns(
                        _columnSelectorViewModel.ShowShortId,
                        _columnSelectorViewModel.ShowStory,
                        _columnSelectorViewModel.ShowCreatedAt,
                        _columnSelectorViewModel.ShowRegIp,
                        _columnSelectorViewModel.ShowAge,
                        _columnSelectorViewModel.ShowSteamId64
                    );
                }
            }
            finally
            {
                _columnSelectorViewModel.ApplyRequested -= OnApply;
                _columnSelectorViewModel.CancelRequested -= OnCancel;
            }
        }

        private void UpdateExtraColumns(bool showShortId, bool showStory, bool showCreatedAt, bool showRegIp, bool showAge, bool showSteamId64)
        {
            if (_dataGrid == null) return;

            // Сохраняем фиксированные колонки: ID, Имя, Онлайн
            var fixedColumns = new System.Collections.Generic.List<DataGridColumn>
    {
        _dataGrid.Columns[0], // ID
        _dataGrid.Columns[1], // Имя
        _dataGrid.Columns[2]  // Онлайн
    };

            _dataGrid.Columns.Clear();

            // Возвращаем фиксированные
            foreach (var col in fixedColumns)
                _dataGrid.Columns.Add(col);

            // Добавляем динамические колонки
            if (showShortId)
                _dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "ShortId",
                    Binding = new System.Windows.Data.Binding("ShortId"),
                    Width = 80
                });
            if (showStory)
                _dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Story",
                    Binding = new System.Windows.Data.Binding("Story"),
                    Width = 120
                });
            if (showCreatedAt)
                _dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "CreatedAt",
                    Binding = new System.Windows.Data.Binding("CreatedAt"),
                    Width = 140
                });
            if (showRegIp)
                _dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "RegIp",
                    Binding = new System.Windows.Data.Binding("RegIp"),
                    Width = 120
                });
            if (showAge)
                _dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Age",
                    Binding = new System.Windows.Data.Binding("Age"),
                    Width = 60
                });
            if (showSteamId64)
                _dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "SteamId64",
                    Binding = new System.Windows.Data.Binding("SteamId64"),
                    Width = 150
                });
        }


        private void LoadCharactersFromDatabase()
        {
            try
            {
                var allCharacters = _dbManager.GetAllCharacters();
                Characters.Clear();

                foreach (var character in allCharacters)
                    Characters.Add(new AdminCharacterViewModel(character));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки из БД: {ex.Message}", "Ошибка");
            }
        }

        private void ApplyFilter()
        {
            CharactersView.Filter = obj =>
            {
                if (string.IsNullOrEmpty(SearchText)) return true;
                if (obj is AdminCharacterViewModel vm)
                {
                    return vm.Id.ToString().Contains(SearchText) ||
                           (vm.Nickname?.ToLower().Contains(SearchText.ToLower()) ?? false);
                }
                return false;
            };
        }

        private void ApplySort()
        {
            CharactersView.SortDescriptions.Clear();

            if (SelectedSortOption != null)
            {
                CharactersView.SortDescriptions.Add(
                    new SortDescription(SelectedSortOption.PropertyName, SelectedSortOption.Direction));
            }
        }

        private void OnPlayerDoubleClick(AdminCharacterViewModel player)
        {
            if (player != null)
            {
                var status = player.IsOnline ? "ОНЛАЙН" : "ОФФЛАЙН";
                MessageBox.Show(
                    $"Игрок: {player.Nickname}\n" +
                    $"ID: {player.Id}\n" +
                    $"Age: {player.Age}\n" +
                    $"SteamId64: {player.SteamId64}\n" +
                    $"ShortId: {player.ShortId}\n" +
                    $"RegIp: {player.RegIp}\n" +
                    $"Story: {player.Story}\n" +
                    $"CreatedAt: {player.CreatedAt}\n" +
                    $"Статус: {status}",
                    "Информация об игроке");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
