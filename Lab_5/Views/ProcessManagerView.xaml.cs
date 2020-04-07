using Lab_5.ViewModels;

namespace Lab_5.Views
{
    public partial class ProcessManagerView
    {
        public ProcessManagerView()
        {
            InitializeComponent();
            ManagerVM vm = new ManagerVM();
            vm.LaunchRefresh();
            DataContext = vm;
        }
    }
}