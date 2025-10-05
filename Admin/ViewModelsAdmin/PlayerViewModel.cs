using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.ViewModelsAdmin
{
    public class PlayerViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public bool IsOnline { get; set; }
    }
}
