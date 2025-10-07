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
        public MainWindowLead()
        {
            InitializeComponent();

            // Передаем DataGrid в ViewModel для динамических колонок
            if (DataContext is ViewModelsAdmin.MainViewLeadModel vm)
            {
                vm.SetDataGrid(CharactersDataGrid);
            }
        }
    }
}
