using System.Windows;
using UniaWarehouseManagementSystem.ViewModels;

namespace UniaWarehouseManagementSystem
{
    public partial class DocumentWindow : Window
    {
        public DocumentWindow()
        {
            InitializeComponent();

            // Podpinamy zamykanie okna pod ViewModel
            var vm = new DocumentEditorViewModel();
            vm.CloseAction = () => this.Close();
            this.DataContext = vm;
        }
    }
}