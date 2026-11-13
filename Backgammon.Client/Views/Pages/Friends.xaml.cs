using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Pages
{
    public partial class Friends : Page
    {
        public Friends()
        {
            InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) 
            {
                NavigationService.GoBack();
            }
        }
        
    }
}
