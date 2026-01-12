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
using UniaWarehouseManagementSystem.ViewModels;

namespace UniaWarehouseManagementSystem
{
    public partial class WarehouseWindow : Window
    {
        public WarehouseWindow()
        {
            InitializeComponent();
        }

        // Opcjonalnie: konstruktor, który od razu ustawia ViewModel i podpina zamykanie
        public WarehouseWindow(WarehouseEditorViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            // Jeśli ViewModel zażąda zamknięcia, zamykamy okno
            if (viewModel.CloseAction == null)
            {
                viewModel.CloseAction = new System.Action(this.Close);
            }
        }
    }
}
