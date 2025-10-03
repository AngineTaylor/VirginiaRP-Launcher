using Launcher.ServiceLib.Contracts;
using Launcher.WPF.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;

namespace Launcher.WPF.ViewModels
{
    public class NicknameViewModel : INotifyPropertyChanged
    {
        private readonly WcfClientService _service;
        private readonly string _sessionId;
        private string _fullName = "";
        private string _ageText = "";
        private string _story = "";
        private bool _agreeChecked = false;
        private string _characterType;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string AgeText
        {
            get => _ageText;
            set { _ageText = value; OnPropertyChanged(); }
        }

        public string Story
        {
            get => _story;
            set { _story = value; OnPropertyChanged(); }
        }

        public bool AgreeChecked
        {
            get => _agreeChecked;
            set { _agreeChecked = value; OnPropertyChanged(); }
        }

        public string CharacterType
        {
            get => _characterType;
            set { _characterType = value; OnPropertyChanged(); }
        }

        public string CharacterTypeDisplay
        {
            get
            {
                switch (_characterType)
                {
                    case "oligarch":
                        return "Олигарх";
                    case "sirota":
                        return "Сирота";
                    case "mafioso":
                        return "Мафиози";
                    default:
                        return "Гражданин";
                }
            }
        }

        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

        public NicknameViewModel(WcfClientService service, string sessionId, string selectedCharacter)
        {
            _service = service;
            _sessionId = sessionId;
            _characterType = selectedCharacter;

            // Установка истории по умолчанию в зависимости от типа персонажа
            SetDefaultStory(selectedCharacter);

            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void SetDefaultStory(string characterType)
        {
            switch (characterType)
            {
                case "oligarch":
                    Story = "Родился в богатой семье, унаследовал бизнес от отца. Образование получил в лучших университетах Европы. Владелец крупной корпорации, имеющий влияние на политику города.";
                    break;
                case "sirota":
                    Story = "Вырос в детском доме, не знает своих родителей. С детства привык рассчитывать только на себя. Работал на разных низкооплачиваемых работах, мечтает найти свое место в жизни.";
                    break;
                case "mafioso":
                    Story = "Вырос в криминальном районе, с молодости связан с местной группировкой. Прошел путь от мелкого исполнителя до уважаемого члена организации. Знает все темные уголки города.";
                    break;
                default:
                    Story = "Обычный житель города, работающий на стандартной работе. Мечтает о лучшей жизни и возможностях для развития.";
                    break;
            }
        }

        private void OnOk()
        {
            if (string.IsNullOrWhiteSpace(FullName) || FullName.Length < 4 ||
                !Regex.IsMatch(FullName, @"^[A-Za-zА-Яа-яёЁ]+\s+[A-Za-zА-Яа-яёЁ]+$"))
            {
                MessageBox.Show("Введите корректные имя и фамилию (минимум 4 символа, пробел между ними).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AgeText, out int age) || age < 18 || age > 50)
            {
                MessageBox.Show("Возраст должен быть числом от 18 до 50.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Story) || Story.Length < 50)
            {
                MessageBox.Show("История персонажа должна содержать не менее 50 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!AgreeChecked)
            {
                MessageBox.Show("Вы должны согласиться с условиями проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Убираем лишние пробелы, но оставляем один пробел между именем и фамилией
            string nickname = Regex.Replace(FullName.Trim(), @"\s+", " ");
            if (string.IsNullOrEmpty(nickname)) return;

            try
            {
                // Сначала создаем персонажа с базовыми данными
                var result = _service.CreateCharacter(_sessionId, nickname);
                if (result != null && result.Id > 0)
                {
                    // Обновляем персонажа с историей и возрастом
                    var updatedCharacter = new CharacterDto
                    {
                        Id = result.Id,
                        ShortId = result.ShortId,
                        Nickname = result.Nickname,
                        Age = age,
                        Story = Story,
                        SteamId64 = result.SteamId64,
                        CreatedAt = result.CreatedAt,
                        RegIp = result.RegIp
                    };

                    var updateSuccess = _service.UpdateCharacter(_sessionId, updatedCharacter);
                    if (updateSuccess)
                    {
                        CloseWindow(true);
                    }
                    else
                    {
                        MessageBox.Show("Персонаж создан, но не удалось сохранить дополнительную информацию.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        CloseWindow(true);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при создании персонажа на сервере.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании персонажа: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult)
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            if (window != null)
            {
                window.DialogResult = dialogResult;
                window.Close();
            }
        }

        // Метод для генерации случайной истории (опционально)
        public void GenerateRandomStory()
        {
            var randomStories = new[]
            {
                "Вырос в обычной семье, окончил местный университет. Работал по специальности несколько лет, но решил попробовать что-то новое. Ищет свой путь в этом городе.",
                "Переехал в город недавно в поисках лучшей жизни. Имеет небольшой опыт в разных сферах деятельности. Адаптируется к новым условиям и заводит знакомства.",
                "Местный житель, знающий город как свои пять пальцев. Работал на разных должностях, имеет широкий круг общения. Всегда готов помочь советом.",
                "Приехал из небольшого городка, мечтает построить карьеру в большом городе. Трудолюбив и амбициозен, но пока не нашел свое призвание."
            };

            var random = new Random();
            Story = randomStories[random.Next(randomStories.Length)];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}