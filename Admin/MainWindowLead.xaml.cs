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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Admin
{
    /// <summary>
    /// Логика взаимодействия для MainWindowLead.xaml
    /// </summary>
    public partial class MainWindowLead : Window
    {
        private readonly MainViewLeadModel _viewModel;
        public MainWindowLead()
        {
            InitializeComponent();

            _viewModel = new MainViewLeadModel();
            DataContext = _viewModel;

            // Передаем DataGrid во ViewModel для работы с колонками
            _viewModel.SetDataGrid(CharactersDataGrid);
        }
    }
}
