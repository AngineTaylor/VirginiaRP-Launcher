using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Admin.ViewModelsAdmin
{
    public class DataGridColumnInfo
    {
        public string Header { get; set; }
        public string BindingPath { get; set; }
        public double Width { get; set; }
        public bool IsVisible { get; set; }

        
    }
}
