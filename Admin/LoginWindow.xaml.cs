using Admin.ViewModelsAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Admin
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        
        public LoginWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModelsAdmin.AdminLoginViewModel();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем ViewModel
            if (DataContext is ViewModelsAdmin.AdminLoginViewModel vm)
            {
                string password = PasswordBox.Password; // Берем пароль напрямую из PasswordBox
                vm.LoginCommand.Execute(password);
            }
        }
    }
}
