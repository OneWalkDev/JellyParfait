using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JellyParfait.MVVM.View
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new ViewModel.MainWindowViewModel(DialogCoordinator.Instance, Player);
            DataContext = vm;
        }
    }
}
