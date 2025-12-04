using DesktopMatrix.Services;
using DesktopMatrix.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace DesktopMatrix.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TaskManager _taskManager;

        public MainWindow()
        {
            InitializeComponent();
            _taskManager = new TaskManager(new DataService(new EncryptionService()));
            this.DataContext = new MainViewModel(_taskManager);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _taskManager.SaveChangesAsync().Wait();
            base.OnClosing(e);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}