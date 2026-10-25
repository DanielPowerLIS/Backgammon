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
