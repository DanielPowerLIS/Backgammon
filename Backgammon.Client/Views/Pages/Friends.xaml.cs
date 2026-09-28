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

namespace Backgammon.Client.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para Friends.xaml
    /// </summary>
    public partial class Friends : Page
    {
        public Friends()
        {
            InitializeComponent();

            //Solo provisional o pruebas
            FriendList.Items.Add("Amigo 1");
            FriendList.Items.Add("Amigo 2");
            FriendList.Items.Add("Amigo 3");
            FriendList.Items.Add("Amigo 4");
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
