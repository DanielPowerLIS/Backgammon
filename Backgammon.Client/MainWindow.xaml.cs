using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Backgammon.Client.Views.Pages;

namespace Backgammon.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigating += OnFrameNavigating;
            MainFrame.Navigate(new Home());
        }

        private void OnFrameNavigating(
            object eventSender,
            NavigatingCancelEventArgs navigationArguments)
        {
            if (MainFrame.Content is Page activePage &&
                !activePage.IsEnabled)
            {
                navigationArguments.Cancel = true;
            }
        }
    }
}
